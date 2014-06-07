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
    /// Adapter for WPF Control for core.
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
        /// Get the underline WPF control
        /// </summary>
        public Control Control
        {
            get { return _control; }
        }

        public override RPoint MouseLocation
        {
            get { return Utils.Convert(_control.PointFromScreen(Mouse.GetPosition(_control))); }
        }

        public override bool LeftMouseButton
        {
            get { return Mouse.LeftButton == MouseButtonState.Pressed; }
        }

        public override bool RightMouseButton
        {
            get { return Mouse.RightButton == MouseButtonState.Pressed; }
        }

        public override void SetCursorDefault()
        {
            _control.Cursor = Cursors.Arrow;
        }

        public override void SetCursorHand()
        {
            _control.Cursor = Cursors.Hand;
        }

        public override void SetCursorIBeam()
        {
            _control.Cursor = Cursors.IBeam;
        }

        public override object GetDataObject(string html, string plainText)
        {
            // TODO:a handle WPF clipboard
//            return ClipboardHelper.CreateDataObject(html, plainText);
            return null;
        }

        public override void DoDragDropCopy(object dragDropData)
        {
            // TODO:a handle WPF clipboard
//            _control.DoDragDrop(dragDropData, DragDropEffects.Copy);
        }

        public override RGraphics CreateGraphics()
        {
            // TODO:a handle it
            return null;

            // the WPF graphics object will be disposed by WinGraphics
            //            return new GraphicsAdapter(DrawingContext. _control.CreateGraphics(), _useGdiPlusTextRendering, true);
        }

        public override void Invalidate()
        {
            _control.InvalidateVisual();
        }
    }
}