using UnityEditor;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    public abstract class ManeDecorator<TDrawer> : DecoratorDrawer
        where TDrawer : ManeDecorator<TDrawer>
    {
        private static VisualTreeAsset _xml;

        protected abstract string XmlFileName { get; }

        protected VisualTreeAsset Xml =>
            _xml ??= UIElementsTools.LoadUXML(typeof(TDrawer), XmlFileName);
    }
}
