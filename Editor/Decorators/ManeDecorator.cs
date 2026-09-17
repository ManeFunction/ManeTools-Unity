using UnityEditor;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Decorator drawer that loads UXML next to the concrete drawer type.
    /// </summary>
    /// <typeparam name="TDrawer">Concrete drawer type used to locate the UXML.</typeparam>
    public abstract class ManeDecorator<TDrawer> : DecoratorDrawer
        where TDrawer : ManeDecorator<TDrawer>
    {
        private static VisualTreeAsset _xml;

        protected abstract string XmlFileName { get; }

        protected VisualTreeAsset Xml =>
            _xml ??= UIElementsTools.LoadUXML(typeof(TDrawer), XmlFileName);
    }
}
