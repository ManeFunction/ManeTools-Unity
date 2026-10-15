using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// A <see cref="Quaternion"/> property edited as Euler angles, like Unity's own Quaternion field, but built with
    /// UI Toolkit. Unity draws Quaternion fields with IMGUI, which puts their inputs in its own label column;
    /// ManeStyle inspectors swap them for this field so they line up with the other fields.
    /// </summary>
    internal sealed class QuaternionField : Vector3Field
    {
        private const string ImguiPropertyClass = "unity-property-field__imgui-container-property";
        // Unity rebuilds a PropertyField's content on rebind, sometimes without a geometry change to notice it by.
        private const long ReplaceCheckIntervalMs = 300;
        private const float SameRotationAngle = 0.001f;

        private SerializedProperty _property;

        private QuaternionField(string label) : base(label)
        {
            this.RegisterValueChangedCallback(evt =>
            {
                if (_property == null)
                    return;

                _property.quaternionValue = Quaternion.Euler(evt.newValue);
                _property.serializedObject.ApplyModifiedProperties();
            });
        }

        /// <summary>
        /// Swaps every IMGUI-drawn Quaternion field under <paramref name="root"/> for a <see cref="QuaternionField"/>,
        /// now and whenever the fields are rebuilt.
        /// </summary>
        public static void ReplaceImguiFields(VisualElement root)
        {
            root.RegisterCallback<GeometryChangedEvent>(_ => Replace(root));
            root.schedule.Execute(() => Replace(root)).Every(ReplaceCheckIntervalMs);
        }

        private static void Replace(VisualElement root)
        {
            root.Query<PropertyField>(className: ImguiPropertyClass).ForEach(host =>
            {
                IMGUIContainer imgui = null;
                foreach (VisualElement child in host.Children())
                {
                    if (child is IMGUIContainer container)
                        imgui = container;
                }

                SerializedProperty property = host.GetBoundSerializedProperty();
                if (imgui == null || property is not { propertyType: SerializedPropertyType.Quaternion })
                    return;

                QuaternionField field = new(host.label ?? property.displayName)
                {
                    tooltip = property.tooltip
                };
                field.Bind(property);
                imgui.SetVisible(false);
                // Unity's stylesheet shifts an IMGUI field's host left to cancel IMGUI's own padding. The host now
                // holds a regular field, so it drops that class. A rebuild adds it back, and the swap runs again.
                host.RemoveFromClassList(ImguiPropertyClass);
                host.Add(field);
            });
        }

        private void Bind(SerializedProperty property)
        {
            _property = property.Copy();
            Refresh();
            this.TrackPropertyValue(_property, _ => Refresh());
        }

        private void Refresh()
        {
            _property.serializedObject.UpdateIfRequiredOrScript();
            showMixedValue = _property.hasMultipleDifferentValues;

            // Keep the angles as typed while they still give this rotation, so 350 does not turn into -10.
            Quaternion rotation = _property.quaternionValue;
            if (Quaternion.Angle(Quaternion.Euler(value), rotation) > SameRotationAngle)
                SetValueWithoutNotify(rotation.eulerAngles);
        }
    }
}
