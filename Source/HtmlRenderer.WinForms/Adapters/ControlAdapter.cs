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

using System.Windows.Forms;
using HtmlRenderer.Core;
using HtmlRenderer.Core.Entities;
using HtmlRenderer.Core.Utils;
using HtmlRenderer.WinForms.Utilities;

namespace HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms Control for core.
    /// </summary>
    internal sealed class ControlAdapter : IControl
    {
        /// <summary>
        /// the underline win forms control.
        /// </summary>
        private readonly Control _control;

        /// <summary>
        /// Init.
        /// </summary>
        public ControlAdapter(Control control)
        {
            ArgChecker.AssertArgNotNull(control, "control");

            _control = control;
        }

        /// <summary>
        /// Get the current location of the mouse relative to the control
        /// </summary>
        public PointInt MouseLocation
        {
            get { return Utils.Convert(_control.PointToClient(Control.MousePosition)); }
        }

        /// <summary>
        /// Is the left mouse button is currently in pressed state
        /// </summary>
        public bool LeftMouseButton
        {
            get { return (Control.MouseButtons & MouseButtons.Left) != 0; }
        }

        /// <summary>
        /// Is the right mouse button is currently in pressed state
        /// </summary>
        public bool RightMouseButton
        {
            get { return (Control.MouseButtons & MouseButtons.Right) != 0; }
        }

        /// <summary>
        /// Get the underline win forms control
        /// </summary>
        public Control Control
        {
            get { return _control; }
        }

        /// <summary>
        /// Set the cursor over the control to default cursor
        /// </summary>
        public void SetCursorDefault()
        {
            _control.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Set the cursor over the control to hand cursor
        /// </summary>
        public void SetCursorHand()
        {
            _control.Cursor = Cursors.Hand;            
        }

        /// <summary>
        /// Set the cursor over the control to I beam cursor
        /// </summary>
        public void SetCursorIBeam()
        {
            _control.Cursor = Cursors.IBeam;            
        }

        /// <summary>
        /// Get data object for the given html and plain text data.<br/>
        /// The data object can be used for clipboard or drag-drop operation.
        /// </summary>
        /// <param name="html">the html data</param>
        /// <param name="plainText">the plain text data</param>
        /// <returns>drag-drop data object</returns>
        public object GetDataObject(string html, string plainText)
        {
            return HtmlClipboardUtils.GetDataObject(html, plainText);
        }

        /// <summary>
        /// Do drag-drop copy operation for the given data object.
        /// </summary>
        /// <param name="dragDropData">the data object</param>
        public void DoDragDropCopy(object dragDropData)
        {
            _control.DoDragDrop(dragDropData, DragDropEffects.Copy);
        }

        /// <summary>
        /// Create graphics object that can be used with the control.
        /// </summary>
        /// <returns>graphics object</returns>
        public IGraphics CreateGraphics()
        {
            // the win forms graphics object will be disposed by WinGraphics
            return new GraphicsAdapter(_control.CreateGraphics(), false, true);
        }

        /// <summary>
        /// Invalidates the entire surface of the control and causes the control to be redrawn.
        /// </summary>
        public void Invalidate()
        {
            _control.Invalidate();
        }
    }
}
