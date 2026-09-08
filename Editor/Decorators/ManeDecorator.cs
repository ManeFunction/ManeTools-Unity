using System;
using System.IO;
using UnityEditor;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    internal abstract class ManeDecorator<TDrawer> : DecoratorDrawer
        where TDrawer : ManeDecorator<TDrawer>
    {
        private static VisualTreeAsset _xml;

        protected abstract string XmlFileName { get; }

        protected VisualTreeAsset Xml
        {
            get
            {
                if (_xml != null)
                    return _xml;

                Type drawerType = typeof(TDrawer);
                string[] guids = AssetDatabase.FindAssets($"t:MonoScript {drawerType.Name}");
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    if (script == null || script.GetClass() != drawerType)
                        continue;

                    string folder = Path.GetDirectoryName(path);
                    if (string.IsNullOrEmpty(folder))
                        break;

                    string fileName = XmlFileName;
                    if (!fileName.EndsWith(".uxml", StringComparison.OrdinalIgnoreCase))
                        fileName += ".uxml";

                    string uxmlPath = Path.Combine(folder, fileName).Replace('\\', '/');
                    _xml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
                    break;
                }

                return _xml;
            }
        }
    }
}
