using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Shared layout for child fields that belong to a parent field: a branch mark in a fixed slot, then the field.
    /// Wrap each child with <see cref="CreateRow"/>; the last one gets the closing mark.
    /// </summary>
    public static class ChildFields
    {
        /// <summary>
        /// USS class on a child row.
        /// </summary>
        public const string RowClass = "mie-child-row";

        /// <summary>
        /// USS class on the branch mark.
        /// </summary>
        public const string BranchClass = "mie-child-branch";

        /// <summary>
        /// USS class on the wrapped field.
        /// </summary>
        public const string ContentClass = "mie-child-content";

        /// <summary>
        /// Mark for a child that has siblings after it.
        /// </summary>
        public const string MiddleMark = "├";

        /// <summary>
        /// Mark for the last child.
        /// </summary>
        public const string LastMark = "└";

        private const string SheetFileName = "ChildFields.uss";

        /// <summary>
        /// Wraps <paramref name="content"/> in a row with a branch mark. The row carries the stylesheet.
        /// </summary>
        /// <param name="content">The child field.</param>
        /// <param name="last">Whether no other child follows; picks the closing mark.</param>
        public static VisualElement CreateRow(VisualElement content, bool last)
        {
            VisualElement row = new();
            row.AddToClassList(RowClass);
            UIElementsTools.AddStyleSheet(row, typeof(ChildFields), SheetFileName);

            Label branch = new(last ? LastMark : MiddleMark)
            {
                pickingMode = PickingMode.Ignore
            };
            branch.AddToClassList(BranchClass);

            content.AddToClassList(ContentClass);
            row.Add(branch);
            row.Add(content);
            return row;
        }
    }
}
