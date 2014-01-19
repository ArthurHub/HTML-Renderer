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

namespace HtmlRenderer.Core.Dom
{
    /// <summary>
    /// Represents a word inside an inline box
    /// </summary>
    internal sealed class CssRectImage : CssRect
    {
        #region Fields and Consts

        /// <summary>
        /// the image object if it is image word (can be null if not loaded)
        /// </summary>
        private Image _image;

        /// <summary>
        /// the image rectange restriction as returned from image load event
        /// </summary>
        private Rectangle _imageRectangle;

        #endregion


        /// <summary>
        /// Creates a new BoxWord which represents an image
        /// </summary>
        /// <param name="owner">the CSS box owner of the word</param>
        public CssRectImage(CssBox owner)
            :base(owner)
        {}

        /// <summary>
        /// Gets the image this words represents (if one exists)
        /// </summary>
        public override Image Image
        {
            get { return _image; }
            set { _image = value; }
        }

        /// <summary>
        /// Gets if the word represents an image.
        /// </summary>
        public override bool IsImage
        {
            get { return true; }
        }

        /// <summary>
        /// the image rectange restriction as returned from image load event
        /// </summary>
        public Rectangle ImageRectangle
        {
            get { return _imageRectangle; }
            set { _imageRectangle = value; }
        }

        /// <summary>
        /// Represents this word for debugging purposes
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Image";
        }
    }
}
