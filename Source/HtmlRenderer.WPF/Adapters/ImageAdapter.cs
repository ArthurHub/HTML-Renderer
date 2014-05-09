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
using System.Windows.Media.Imaging;
using HtmlRenderer.Adapters;

namespace HtmlRenderer.WPF.Adapters
{
    /// <summary>
    /// Adapter for WinForms Image object for core.
    /// </summary>
    internal sealed class ImageAdapter : RImage
    {
        /// <summary>
        /// the underline win-forms image.
        /// </summary>
        private readonly BitmapImage _image;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ImageAdapter(BitmapImage image)
        {
            _image = image;
        }

        /// <summary>
        /// the underline win-forms image.
        /// </summary>
        public BitmapImage Image
        {
            get { return _image; }
        }

        /// <summary>
        /// Get the width, in pixels, of the image.
        /// </summary>
        public override double Width
        {
            get { return _image.Width; }
        }

        /// <summary>
        /// Get the height, in pixels, of the image.
        /// </summary>
        public override double Height
        {
            get { return _image.Height; }
        }

        /// <summary>
        /// Saves this image to the specified stream in PNG format.
        /// </summary>
        /// <param name="stream">The Stream where the image will be saved. </param>
        public override void Save(MemoryStream stream)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(_image));
            encoder.Save(stream);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            _image.StreamSource.Dispose();
        }
    }
}