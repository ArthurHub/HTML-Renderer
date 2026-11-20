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

using Microsoft.UI.Xaml.Controls;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.WinUI.Utilities;

namespace TheArtOfDev.HtmlRenderer.WinUI.Adapters
{
    /// <summary>
    /// Adapter for WinUI Control for core.
    /// </summary>
    /// <remarks>
    /// WinUI has no global mouse state to query, the pointer state is the last one <see cref="HtmlControl"/> has seen.
    /// </remarks>
    internal sealed class ControlAdapter : RControl
    {
        /// <summary>
        /// the underline WinUI control.
        /// </summary>
        private readonly Control _control;

        /// <summary>
        /// Init.
        /// </summary>
        public ControlAdapter(Control control)
            : base(WinUIAdapter.Instance)
        {
            ArgChecker.AssertArgNotNull(control, "control");

            _control = control;
        }

        /// <summary>
        /// Get the underline WinUI control
        /// </summary>
        public Control Control
        {
            get { return _control; }
        }

        public override RPoint MouseLocation
        {
            get { return (_control as HtmlControl)?.LastPointerLocation ?? RPoint.Empty; }
        }

        public override bool LeftMouseButton
        {
            get { return (_control as HtmlControl)?.IsLeftButtonPressed ?? false; }
        }

        public override bool RightMouseButton
        {
            get { return (_control as HtmlControl)?.IsRightButtonPressed ?? false; }
        }

        public override void SetCursorDefault()
        {
            (_control as HtmlControl)?.SetCursorShape(HtmlControl.CursorShape.Default);
        }

        public override void SetCursorHand()
        {
            (_control as HtmlControl)?.SetCursorShape(HtmlControl.CursorShape.Hand);
        }

        public override void SetCursorIBeam()
        {
            (_control as HtmlControl)?.SetCursorShape(HtmlControl.CursorShape.IBeam);
        }

        public override void DoDragDropCopy(object dragDropData)
        {
            (_control as HtmlControl)?.StartDragDrop((ClipboardHelper.Content)dragDropData);
        }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            using (var g = new GraphicsAdapter(WinUIAdapter.Instance.Device))
            {
                g.MeasureString(str, font, maxWidth, out charFit, out charFitWidth);
            }
        }

        public override void Invalidate()
        {
            (_control as HtmlControl)?.InvalidateCanvas();
        }
    }
}
