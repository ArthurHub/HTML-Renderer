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

using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Adapters
{
    /// <summary>
    /// Adapter for platform specific control object - used to handle updating the control that the html is rendered on.<br/>
    /// Not relevant for platforms that don't render HTML on UI element.
    /// </summary>
    public abstract class RControl
    {
        /// <summary>
        /// The platform adapter.
        /// </summary>
        private readonly RAdapter _adapter;

        /// <summary>
        /// Init control with platform adapter.
        /// </summary>
        protected RControl(RAdapter adapter)
        {
            ArgChecker.AssertArgNotNull(adapter, "adapter");
            _adapter = adapter;
        }

        /// <summary>
        /// The platform adapter.
        /// </summary>
        public RAdapter Adapter
        {
            get { return _adapter; }
        }

        /// <summary>
        /// Is the left mouse button is currently in pressed state
        /// </summary>
        public abstract bool LeftMouseButton { get; }

        /// <summary>
        /// Is the right mouse button is currently in pressed state
        /// </summary>
        public abstract bool RightMouseButton { get; }

        /// <summary>
        /// Get the current location of the mouse relative to the control
        /// </summary>
        public abstract RPoint MouseLocation { get; }

        /// <summary>
        /// Set the cursor over the control to default cursor
        /// </summary>
        public abstract void SetCursorDefault();

        /// <summary>
        /// Set the cursor over the control to hand cursor
        /// </summary>
        public abstract void SetCursorHand();

        /// <summary>
        /// Set the cursor over the control to I beam cursor
        /// </summary>
        public abstract void SetCursorIBeam();

        /// <summary>
        /// Do drag-drop copy operation for the given data object.
        /// </summary>
        /// <param name="dragDropData">the drag-drop data object</param>
        public abstract void DoDragDropCopy(object dragDropData);

        /// <summary>
        /// Measure the width of string under max width restriction calculating the number of characters that can fit and the width those characters take.<br/>
        /// </summary>
        /// <param name="str">the string to measure</param>
        /// <param name="font">the font to measure string with</param>
        /// <param name="maxWidth">the max width to calculate fit characters</param>
        /// <param name="charFit">the number of characters that will fit under <see cref="maxWidth"/> restriction</param>
        /// <param name="charFitWidth">the width that only the characters that fit into max width take</param>
        public abstract void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth);

        /// <summary>
        /// Invalidates the entire surface of the control and causes the control to be redrawn.
        /// </summary>
        public abstract void Invalidate();
    }
}