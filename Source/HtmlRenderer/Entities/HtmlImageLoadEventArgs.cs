// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they bagin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;
using System.Drawing;
using HtmlRenderer.Utils;

namespace HtmlRenderer.Entities
{
    /// <summary>
    /// Callback used in <see cref="HtmlImageLoadEventArgs"/> to allow setting image externally and async.<br/>
    /// The callback can provide path to image file or uri, or the actual image to use.<br/>
    /// If <paramref name="imageRectangle"/> is not <see cref="System.Drawing.Rectangle.Empty"/> then only the specified rectangle will
    /// be used from the loaded image and not all of it.<br/> 
    /// if <paramref name="imageRectangle"/> is given it will be used for the size of the image and not the actual image size.
    /// </summary>
    /// <param name="path">the path to the image to load (file path or uri)</param>
    /// <param name="image">the image to load</param>
    /// <param name="imageRectangle">optional: limit to specific rectangle of the image and not all of it</param>
    public delegate void HtmlImageLoadCallback(string path, Image image, Rectangle imageRectangle);

    /// <summary>
    /// Raised when an image is about to be loaded by file path or URI.<br/>
    /// This event allows to provide the image manually, if not handled the image will be loaded from file or download from URI.
    /// </summary>
    public sealed class HtmlImageLoadEventArgs : EventArgs
    {
        #region Fields and Consts

        /// <summary>
        /// use to cancel the image loading by html renderer, the provided image will be used.
        /// </summary>
        private bool _handled;

        /// <summary>
        /// the source of the image (file path or uri)
        /// </summary>
        private readonly string _src;

        /// <summary>
        /// collection of all the attributes that are defined on the image element
        /// </summary>
        private readonly Dictionary<string, string> _attributes;

        /// <summary>
        /// Callback used to allow setting image externally and async.
        /// </summary>
        private readonly HtmlImageLoadCallback _callback;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="src">the source of the image (file path or uri)</param>
        /// <param name="attributes">collection of all the attributes that are defined on the image element</param>
        /// <param name="callback">Callback used to allow setting image externally and async.</param>
        public HtmlImageLoadEventArgs(string src, Dictionary<string, string> attributes, HtmlImageLoadCallback callback)
        {
            _src = src;
            _attributes = attributes;
            _callback = callback;
        }

        /// <summary>
        /// the source of the image (file path or uri)
        /// </summary>
        public string Src
        {
            get { return _src; }
        }

        /// <summary>
        /// collection of all the attributes that are defined on the image element
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get { return _attributes; }
        }

        /// <summary>
        /// use to cancel the image loading by html renderer, the provided image will be used.
        /// </summary>
        public bool Handled
        {
            get { return _handled; }
            set { _handled = value; }
        }

        /// <summary>
        /// Callback used to allow setting image externally and async.<br/>
        /// This call will set error image.
        /// </summary>
        public void Callback()
        {
            _handled = true;
            _callback(null, null, new Rectangle());
        }

        /// <summary>
        /// Callback used to allow setting image externally and async.<br/>
        /// The callback can provide path to image file or uri, or the actual image to use.<br/>
        /// If <paramref name="imageRectangle"/> is not <see cref="System.Drawing.Rectangle.Empty"/> then only the specified rectangle will
        /// be used from the loaded image and not all of it.<br/> 
        /// if <paramref name="imageRectangle"/> is given it will be used for the size of the image and not the actual image size.
        /// </summary>
        /// <param name="path">the path to the image to load (file path or uri)</param>
        /// <param name="imageRectangle">optional: limit to specific rectangle of the image and not all of it</param>
        public void Callback(string path, Rectangle imageRectangle = new Rectangle())
        {
            ArgChecker.AssertArgNotNullOrEmpty(path, "path");
            
            _handled = true;
            _callback(path, null, imageRectangle);
        }

        /// <summary>
        /// Callback used to allow setting image externally and async.<br/>
        /// The callback can provide path to image file or uri, or the actual image to use.<br/>
        /// If <paramref name="imageRectangle"/> is not <see cref="System.Drawing.Rectangle.Empty"/> then only the specified rectangle will
        /// be used from the loaded image and not all of it.<br/> 
        /// if <paramref name="imageRectangle"/> is given it will be used for the size of the image and not the actual image size.
        /// </summary>
        /// <param name="image">the image to load</param>
        /// <param name="imageRectangle">optional: limit to specific rectangle of the image and not all of it</param>
        public void Callback(Image image, Rectangle imageRectangle = new Rectangle())
        {
            ArgChecker.AssertArgNotNull(image, "image");
            
            _handled = true;
            _callback(null, image, imageRectangle);
        }
    }
}
