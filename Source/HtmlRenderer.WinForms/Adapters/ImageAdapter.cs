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
using System.Drawing.Imaging;
using System.IO;
using HtmlRenderer.Core;
using HtmlRenderer.Interfaces;

namespace HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms Image object for core.
    /// </summary>
    internal sealed class ImageAdapter : IImage
    {
        /// <summary>
        /// the underline win-forms image.
        /// </summary>
        private readonly Image _image;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ImageAdapter(Image image)
        {
            _image = image;
        }

        /// <summary>
        /// the underline win-forms image.
        /// </summary>
        public Image Image
        {
            get { return _image; }
        }

        /// <summary>
        /// Get the width, in pixels, of the image.
        /// </summary>
        public float Width
        {
            get { return _image.Width; }
        }

        /// <summary>
        /// Get the height, in pixels, of the image.
        /// </summary>
        public float Height
        {
            get { return _image.Height; }
        }

        /// <summary>
        /// Saves this image to the specified stream in PNG format.
        /// </summary>
        /// <param name="stream">The Stream where the image will be saved. </param>
        public void Save(MemoryStream stream)
        {
            _image.Save(stream, ImageFormat.Png);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _image.Dispose();
        }
    }
}
