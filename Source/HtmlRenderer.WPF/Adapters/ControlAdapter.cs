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

using System.Windows.Controls;
using System.Windows.Input;
using HtmlRenderer.Adapters;
using HtmlRenderer.Adapters.Entities;
using HtmlRenderer.Core.Utils;
using HtmlRenderer.WPF.Utilities;

namespace HtmlRenderer.WPF.Adapters
{
    /// <summary>
    /// Adapter for WinForms Control for core.
    /// </summary>
    internal sealed class ControlAdapter : RControl
    {
        /// <summary>
        /// the underline WPF control.
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
        public override RPoint MouseLocation
        {
            get { return Utils.Convert(_control.PointFromScreen(Mouse.GetPosition(_control))); }
        }

        /// <summary>
        /// Is the left mouse button is currently in pressed state
        /// </summary>
        public override bool LeftMouseButton
        {
            get { return Mouse.LeftButton == MouseButtonState.Pressed; }
        }

        /// <summary>
        /// Is the right mouse button is currently in pressed state
        /// </summary>
        public override bool RightMouseButton
        {
            get { return Mouse.RightButton == MouseButtonState.Pressed; }
        }

        /// <summary>
        /// Get the underline WPF control
        /// </summary>
        public Control Control
        {
            get { return _control; }
        }

        /// <summary>
        /// Set the cursor over the control to default cursor
        /// </summary>
        public override void SetCursorDefault()
        {
            _control.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Set the cursor over the control to hand cursor
        /// </summary>
        public override void SetCursorHand()
        {
            _control.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// Set the cursor over the control to I beam cursor
        /// </summary>
        public override void SetCursorIBeam()
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
        public override object GetDataObject(string html, string plainText)
        {
            // TODO:a handle WPF clipboard
//            return ClipboardHelper.CreateDataObject(html, plainText);
            return null;
        }

        /// <summary>
        /// Do drag-drop copy operation for the given data object.
        /// </summary>
        /// <param name="dragDropData">the data object</param>
        public override void DoDragDropCopy(object dragDropData)
        {
            // TODO:a handle WPF clipboard
//            _control.DoDragDrop(dragDropData, DragDropEffects.Copy);
        }

        /// <summary>
        /// Create graphics object that can be used with the control.
        /// </summary>
        /// <returns>graphics object</returns>
        public override RGraphics CreateGraphics()
        {
            // TODO:a handle it
            return null;

            // the WPF graphics object will be disposed by WinGraphics
            //            return new GraphicsAdapter(DrawingContext. _control.CreateGraphics(), _useGdiPlusTextRendering, true);
        }

        /// <summary>
        /// Invalidates the entire surface of the control and causes the control to be redrawn.
        /// </summary>
        public override void Invalidate()
        {
            _control.InvalidateVisual();
        }
    }
}