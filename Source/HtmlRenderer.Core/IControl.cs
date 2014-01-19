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

using System.Drawing;
using HtmlRenderer.Core.SysEntities;

namespace HtmlRenderer.Core
{
    /// <summary>
    /// aTODO: add doc
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
        PointInt MouseLocation { get; }

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
        /// Set the given text to the clipboard
        /// </summary>
        /// <param name="text">the text to set</param>
        void SetToClipboard(string text);

        /// <summary>
        /// Set the given html and plain text data to clipboard.
        /// </summary>
        /// <param name="html">the html data</param>
        /// <param name="plainText">the plain text data</param>
        void SetToClipboard(string html, string plainText);
        
        /// <summary>
        /// Set the given image to clipboard.
        /// </summary>
        /// <param name="image"></param>
        void SetToClipboard(Image image);

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

        /// <summary>
        /// Create a context menu that can be used on the control
        /// </summary>
        /// <returns>new context menu</returns>
        IContextMenu CreateContextMenu();

        /// <summary>
        /// Save the given image to file by showing save dialog to the client.
        /// </summary>
        /// <param name="image">the image to save</param>
        /// <param name="name">the name of the image for save dialog</param>
        /// <param name="extension">the extension of the image for save dialog</param>
        void SaveToFile(Image image, string name, string extension);
    }
}
