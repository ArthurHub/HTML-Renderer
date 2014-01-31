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
using System.Windows.Forms;
using HtmlRenderer.Core;
using HtmlRenderer.Core.Utils;
using HtmlRenderer.Entities;
using HtmlRenderer.Interfaces;
using HtmlRenderer.WinForms.Utilities;

namespace HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms context menu for core.
    /// </summary>
    internal sealed class ContextMenuAdapter : IContextMenu
    {
        #region Fields and Consts

        /// <summary>
        /// the underline win forms context menu
        /// </summary>
        private readonly ContextMenuStrip _contextMenu;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public ContextMenuAdapter()
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.ShowImageMargin = false;
        }

        /// <summary>
        /// The total number of items in the context menu
        /// </summary>
        public int ItemsCount
        {
            get { return _contextMenu.Items.Count; }
        }

        /// <summary>
        /// Add divider item to the context menu.<br/>
        /// The divider is a non clickable place holder used to separate items.
        /// </summary>
        public void AddDivider()
        {
            _contextMenu.Items.Add("-");
        }

        /// <summary>
        /// Add item to the context menu with the given text that will raise the given event when clicked.
        /// </summary>
        /// <param name="text">the text to set on the new context menu item</param>
        /// <param name="enabled">if to set the item as enabled or disabled</param>
        /// <param name="onClick">the event to raise when the item is clicked</param>
        public void AddItem(string text, bool enabled, EventHandler onClick)
        {
            ArgChecker.AssertArgNotNullOrEmpty(text, "text");
            ArgChecker.AssertArgNotNull(onClick, "onClick");

            var item = _contextMenu.Items.Add(text, null, onClick);
            item.Enabled = enabled;
        }

        /// <summary>
        /// Remove the last item from the context menu iff it is a divider
        /// </summary>
        public void RemoveLastDivider()
        {
            if (_contextMenu.Items[_contextMenu.Items.Count - 1].Text == string.Empty)
                _contextMenu.Items.RemoveAt(_contextMenu.Items.Count - 1);
        }

        /// <summary>
        /// Show the context menu in the given parent control at the given location.
        /// </summary>
        /// <param name="parent">the parent control to show in</param>
        /// <param name="location">the location to show at relative to the parent control</param>
        public void Show(IControl parent, PointInt location)
        {
            _contextMenu.Show(( (ControlAdapter)parent ).Control, Utils.ConvertRound(location));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _contextMenu.Dispose();
        }
    }
}
