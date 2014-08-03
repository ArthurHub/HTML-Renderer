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
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.WinForms.Utilities;

namespace TheArtOfDev.HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms Control for core.
    /// </summary>
    internal sealed class ControlAdapter : RControl
    {
        /// <summary>
        /// the underline win forms control.
        /// </summary>
        private readonly Control _control;

        /// <summary>
        /// Use GDI+ text rendering to measure/draw text.
        /// </summary>
        private readonly bool _useGdiPlusTextRendering;

        /// <summary>
        /// Init.
        /// </summary>
        public ControlAdapter(Control control, bool useGdiPlusTextRendering)
            : base(WinFormsAdapter.Instance)
        {
            ArgChecker.AssertArgNotNull(control, "control");

            _control = control;
            _useGdiPlusTextRendering = useGdiPlusTextRendering;
        }

        /// <summary>
        /// Get the underline win forms control
        /// </summary>
        public Control Control
        {
            get { return _control; }
        }

        public override RPoint MouseLocation
        {
            get { return Utils.Convert(_control.PointToClient(Control.MousePosition)); }
        }

        public override bool LeftMouseButton
        {
            get { return (Control.MouseButtons & MouseButtons.Left) != 0; }
        }

        public override bool RightMouseButton
        {
            get { return (Control.MouseButtons & MouseButtons.Right) != 0; }
        }

        public override void SetCursorDefault()
        {
            _control.Cursor = Cursors.Default;
        }

        public override void SetCursorHand()
        {
            _control.Cursor = Cursors.Hand;
        }

        public override void SetCursorIBeam()
        {
            _control.Cursor = Cursors.IBeam;
        }

        public override void DoDragDropCopy(object dragDropData)
        {
            _control.DoDragDrop(dragDropData, DragDropEffects.Copy);
        }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            using (var g = new GraphicsAdapter(_control.CreateGraphics(), _useGdiPlusTextRendering, true))
            {
                g.MeasureString(str, font, maxWidth, out charFit, out charFitWidth);
            }
        }

        public override void Invalidate()
        {
            _control.Invalidate();
        }
    }
}