using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// While enabled, transforming the selected object leaves its active children in world space.
    /// Inactive children follow the parent.
    /// </summary>
    public static class ChildrenTransformFreezer
    {
        private const string SessionKey = "ManeTools.FreezeChildren";
        private const int MaxOperationSnapshots = 128;

        private static readonly List<Transform> Roots = new();
        private static readonly HashSet<Transform> SelectedSet = new();
        private static readonly List<FrozenChild> ChildBuffer = new();
        private static readonly Dictionary<EntityId, FreezeState> Freezes = new();
        private static readonly Dictionary<EntityId, LocalPose> LastPoses = new();
        private static readonly Dictionary<int, Dictionary<EntityId, FreezeState>> Operations = new();
        private static readonly Queue<int> OperationOrder = new();
        private static readonly HashSet<EntityId> Dragging = new();

        private static bool _applying;

        public static event Action<bool> EnabledChanged;

        public static bool Enabled
        {
            get => SessionState.GetBool(SessionKey, false);
            set
            {
                if (Enabled == value)
                    return;

                SessionState.SetBool(SessionKey, value);
                if (value)
                    Start();
                else
                    Stop();

                EnabledChanged?.Invoke(value);
            }
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            Undo.undoRedoEvent -= OnUndoRedo;
            Undo.undoRedoEvent += OnUndoRedo;
            if (Enabled)
                Start();
        }

        private static void Start()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
            CaptureFrozenChildren();
        }

        private static void Stop()
        {
            EditorApplication.update -= Tick;
            SceneView.duringSceneGui -= OnSceneGUI;
            Selection.selectionChanged -= OnSelectionChanged;
            Roots.Clear();
            Freezes.Clear();
            LastPoses.Clear();
            Dragging.Clear();
        }

        private static void OnSelectionChanged() => CaptureFrozenChildren();

        private static void OnSceneGUI(SceneView _)
        {
            EventType type = Event.current.type;
            if (type == EventType.MouseDrag || (type == EventType.Repaint && Dragging.Count > 0))
                Tick();
        }

        private static void OnUndoRedo(in UndoRedoInfo info)
        {
            Dragging.Clear();

            if (Operations.TryGetValue(info.undoGroup, out Dictionary<EntityId, FreezeState> snapshot))
            {
                RestoreSnapshot(snapshot);
                if (Enabled)
                    AdoptSnapshot(snapshot);
                return;
            }

            if (Enabled)
                CaptureFrozenChildren();
        }

        private static void CaptureFrozenChildren()
        {
            Freezes.Clear();
            LastPoses.Clear();
            Dragging.Clear();
            RebuildRoots();

            for (int i = 0; i < Roots.Count; i++)
            {
                Transform transform = Roots[i];
                if (transform == null)
                    continue;

                transform.hasChanged = false;
                EntityId id = transform.GetEntityId();
                Freezes[id] = CreateFreezeState(transform);
                LastPoses[id] = ReadLocalPose(transform);
            }
        }

        private static void Tick()
        {
            if (_applying || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            _applying = true;
            try
            {
                Apply();
            }
            finally
            {
                _applying = false;
            }
        }

        private static void Apply()
        {
            int rootCount = Roots.Count;
            if (rootCount == 0)
                return;

            for (int i = 0; i < rootCount; i++)
            {
                Transform transform = Roots[i];
                if (transform == null)
                    continue;

                EntityId id = transform.GetEntityId();
                if (!Freezes.TryGetValue(id, out FreezeState state))
                {
                    transform.hasChanged = false;
                    Freezes[id] = CreateFreezeState(transform);
                    LastPoses[id] = ReadLocalPose(transform);
                    continue;
                }

                if (!transform.hasChanged && !Dragging.Contains(id))
                    continue;

                LocalPose pose = ReadLocalPose(transform);
                transform.hasChanged = false;

                if (LastPoses.TryGetValue(id, out LocalPose last) && pose.Equals(last))
                {
                    if (Dragging.Remove(id))
                        MarkChildrenDirty(state);
                    continue;
                }

                if (Dragging.Add(id))
                    RecordOperationSnapshot();

                RestoreChildren(state);
                LastPoses[id] = pose;
            }
        }

        private static void RecordOperationSnapshot()
        {
            int group = Undo.GetCurrentGroup();
            if (Operations.ContainsKey(group))
                return;

            Operations[group] = new Dictionary<EntityId, FreezeState>(Freezes);
            OperationOrder.Enqueue(group);

            while (OperationOrder.Count > MaxOperationSnapshots)
                Operations.Remove(OperationOrder.Dequeue());
        }

        private static void RestoreSnapshot(Dictionary<EntityId, FreezeState> snapshot)
        {
            foreach (KeyValuePair<EntityId, FreezeState> pair in snapshot)
                RestoreChildren(pair.Value);
        }

        private static void AdoptSnapshot(Dictionary<EntityId, FreezeState> snapshot)
        {
            Freezes.Clear();
            LastPoses.Clear();
            foreach (KeyValuePair<EntityId, FreezeState> pair in snapshot)
            {
                Freezes[pair.Key] = pair.Value;
                Transform parent = pair.Value.Parent;
                if (parent == null)
                    continue;

                parent.hasChanged = false;
                LastPoses[pair.Key] = ReadLocalPose(parent);
            }
        }

        private static void RebuildRoots()
        {
            Roots.Clear();
            Transform[] selected = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
            if (selected.Length == 0)
                return;

            if (selected.Length == 1)
            {
                if (selected[0] != null)
                    Roots.Add(selected[0]);
                return;
            }

            SelectedSet.Clear();
            for (int i = 0; i < selected.Length; i++)
            {
                if (selected[i] != null)
                    SelectedSet.Add(selected[i]);
            }

            for (int i = 0; i < selected.Length; i++)
            {
                Transform transform = selected[i];
                if (transform != null && !HasSelectedAncestor(transform))
                    Roots.Add(transform);
            }
        }

        private static bool HasSelectedAncestor(Transform transform)
        {
            Transform parent = transform.parent;
            while (parent != null)
            {
                if (SelectedSet.Contains(parent))
                    return true;

                parent = parent.parent;
            }

            return false;
        }

        private static FreezeState CreateFreezeState(Transform parent)
        {
            ChildBuffer.Clear();
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child == null || !child.gameObject.activeInHierarchy)
                    continue;

                ChildBuffer.Add(new FrozenChild(child, child.position, child.rotation, child.localScale));
            }

            return new FreezeState(parent, parent.localScale, ChildBuffer.ToArray());
        }

        private static void RestoreChildren(FreezeState state)
        {
            Transform parent = state.Parent;
            if (parent == null)
                return;

            Vector3 localScale = parent.localScale;
            if (localScale.x == 0f || localScale.y == 0f || localScale.z == 0f)
                return;

            Vector3 scaleShift = localScale.Divide(state.ParentLocalScale);
            FrozenChild[] children = state.Children;
            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i].Transform;
                if (child == null || !child.gameObject.activeInHierarchy)
                    continue;

                child.localScale = children[i].LocalScale.Divide(scaleShift);
                child.SetPositionAndRotation(children[i].WorldPosition, children[i].WorldRotation);
            }
        }

        private static void MarkChildrenDirty(FreezeState state)
        {
            FrozenChild[] children = state.Children;
            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i].Transform;
                if (child != null)
                    EditorUtility.SetDirty(child);
            }
        }

        private static LocalPose ReadLocalPose(Transform transform) =>
            new(transform.localPosition, transform.localRotation, transform.localScale);

        private readonly struct LocalPose
        {
            public LocalPose(Vector3 position, Quaternion rotation, Vector3 scale)
            {
                Position = position;
                Rotation = rotation;
                Scale = scale;
            }

            public Vector3 Position { get; }
            public Quaternion Rotation { get; }
            public Vector3 Scale { get; }

            public bool Equals(LocalPose other) =>
                Position == other.Position && Rotation == other.Rotation && Scale == other.Scale;
        }

        private readonly struct FreezeState
        {
            public FreezeState(Transform parent, Vector3 parentLocalScale, FrozenChild[] children)
            {
                Parent = parent;
                ParentLocalScale = parentLocalScale;
                Children = children;
            }

            public Transform Parent { get; }
            public Vector3 ParentLocalScale { get; }
            public FrozenChild[] Children { get; }
        }

        private readonly struct FrozenChild
        {
            public FrozenChild(Transform transform, Vector3 worldPosition, Quaternion worldRotation, Vector3 localScale)
            {
                Transform = transform;
                WorldPosition = worldPosition;
                WorldRotation = worldRotation;
                LocalScale = localScale;
            }

            public Transform Transform { get; }
            public Vector3 WorldPosition { get; }
            public Quaternion WorldRotation { get; }
            public Vector3 LocalScale { get; }
        }
    }
}
