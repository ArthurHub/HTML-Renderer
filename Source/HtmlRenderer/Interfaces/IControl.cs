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

using HtmlRenderer.Entities;

namespace HtmlRenderer.Interfaces
{
    /// <summary>
    /// TODO:a add doc
    /// </summary>
    public interface IControl
    {
        /// <summary>
        /// Is the left mouse button is currently in pressed state
        /// </summary>
        bool LeftMouseButton { get; }

        /// <summary>
        /// Is the right mouse button is currently in pressed state
        /// </summary>
        bool RightMouseButton { get; }

        /// <summary>
        /// Get the current location of the mouse relative to the control
        /// </summary>
        RPoint MouseLocation { get; }

        /// <summary>
        /// Set the cursor over the control to default cursor
        /// </summary>
        void SetCursorDefault();

        /// <summary>
        /// Set the cursor over the control to hand cursor
        /// </summary>
        void SetCursorHand();

        /// <summary>
        /// Set the cursor over the control to I beam cursor
        /// </summary>
        void SetCursorIBeam();

        /// <summary>
        /// Get data object for the given html and plain text data.<br/>
        /// The data object can be used for clipboard or drag-drop operation.
        /// </summary>
        /// <param name="html">the html data</param>
        /// <param name="plainText">the plain text data</param>
        /// <returns>drag-drop data object</returns>
        object GetDataObject(string html, string plainText);

        /// <summary>
        /// Do drag-drop copy operation for the given data object.
        /// </summary>
        /// <param name="dragDropData">the drag-drop data object</param>
        void DoDragDropCopy(object dragDropData);

        /// <summary>
        /// Create graphics object that can be used with the control.
        /// </summary>
        /// <returns>graphics object</returns>
        IGraphics CreateGraphics();

        /// <summary>
        /// Invalidates the entire surface of the control and causes the control to be redrawn.
        /// </summary>
        void Invalidate();
    }
}