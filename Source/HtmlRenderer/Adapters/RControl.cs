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

using HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.Adapters
{
    /// <summary>
    /// TODO:a add doc
    /// </summary>
    public abstract class RControl
    {
        /// <summary>
        ///  Is the left mouse button is currently in pressed state
        ///  </summary>
        public abstract bool LeftMouseButton { get; }

        /// <summary>
        ///  Is the right mouse button is currently in pressed state
        ///  </summary>
        public abstract bool RightMouseButton { get; }

        /// <summary>
        ///  Get the current location of the mouse relative to the control
        ///  </summary>
        public abstract RPoint MouseLocation { get; }

        /// <summary>
        ///  Set the cursor over the control to default cursor
        ///  </summary>
        public abstract void SetCursorDefault();

        /// <summary>
        ///  Set the cursor over the control to hand cursor
        ///  </summary>
        public abstract void SetCursorHand();

        /// <summary>
        ///  Set the cursor over the control to I beam cursor
        ///  </summary>
        public abstract void SetCursorIBeam();

        /// <summary>
        ///  Get data object for the given html and plain text data.<br />
        ///  The data object can be used for clipboard or drag-drop operation.
        ///  </summary><param name="html">the html data</param><param name="plainText">the plain text data</param><returns>drag-drop data object</returns>
        public abstract object GetDataObject(string html, string plainText);

        /// <summary>
        ///  Do drag-drop copy operation for the given data object.
        ///  </summary><param name="dragDropData">the drag-drop data object</param>
        public abstract void DoDragDropCopy(object dragDropData);

        /// <summary>
        ///  Create graphics object that can be used with the control.
        ///  </summary><returns>graphics object</returns>
        public abstract RGraphics CreateGraphics();

        /// <summary>
        ///  Invalidates the entire surface of the control and causes the control to be redrawn.
        ///  </summary>
        public abstract void Invalidate();
    }
}