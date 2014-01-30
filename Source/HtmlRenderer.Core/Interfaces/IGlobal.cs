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

using System.IO;
using HtmlRenderer.Core.Entities;

namespace HtmlRenderer.Core.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGlobal
    {
        CssData GetDefaultCssData();

        /// <summary>
        /// Resolve color value from given color name.
        /// </summary>
        /// <param name="colorName">the color name</param>
        /// <returns>color value</returns>
        ColorInt ResolveColorFromName(string colorName);

        /// <summary>
        /// Get image to be used while HTML image is loading.
        /// </summary>
        IImage GetLoadImage();

        /// <summary>
        /// Get image to be used if HTML image load failed.
        /// </summary>
        IImage GetErrorImage();

        /// <summary>
        /// Convert image object returned from <see cref="HtmlImageLoadEventArgs"/> to <see cref="IImage"/>.
        /// </summary>
        /// <param name="image">the image returned from load event</param>
        /// <returns>converted image or null</returns>
        IImage ConvertImage(object image);

        /// <summary>
        /// Create an <see cref="IImage"/> object from the given stream.
        /// </summary>
        /// <param name="memoryStream">the stream to create image from</param>
        /// <returns>new image instance</returns>
        IImage ImageFromStream(Stream memoryStream);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="family"></param>
        /// <param name="size"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IFont CreateFont(string family, float size, FontStyleInt style);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="family"></param>
        /// <param name="size"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IFont CreateFont(IFontFamily family, float size, FontStyleInt style);

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
        void SetToClipboard(IImage image);

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
        /// <param name="control">optional: the control to show the dialog on</param>
        void SaveToFile(IImage image, string name, string extension, IControl control = null);
    }
}
