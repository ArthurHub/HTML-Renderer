// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using HtmlRenderer.Entities;

namespace HtmlRenderer.Interfaces
{
    /// <summary>
    /// TODO:a add doc
    /// </summary>
    public interface IContextMenu : IDisposable
    {
        /// <summary>
        /// The total number of items in the context menu
        /// </summary>
        int ItemsCount { get; }

        /// <summary>
        /// Add divider item to the context menu.<br/>
        /// The divider is a non clickable place holder used to separate items.
        /// </summary>
        void AddDivider();

        /// <summary>
        /// Add item to the context menu with the given text that will raise the given event when clicked.
        /// </summary>
        /// <param name="text">the text to set on the new context menu item</param>
        /// <param name="enabled">if to set the item as enabled or disabled</param>
        /// <param name="onClick">the event to raise when the item is clicked</param>
        void AddItem(string text, bool enabled, EventHandler onClick);

        /// <summary>
        /// Remove the last item from the context menu iff it is a divider
        /// </summary>
        void RemoveLastDivider();

        /// <summary>
        /// Show the context menu in the given parent control at the given location.
        /// </summary>
        /// <param name="parent">the parent control to show in</param>
        /// <param name="location">the location to show at relative to the parent control</param>
        void Show(IControl parent, RPoint location);
    }
}