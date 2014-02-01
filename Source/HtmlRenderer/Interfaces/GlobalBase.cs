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


using System;
using System.IO;
using HtmlRenderer.Core;
using HtmlRenderer.Core.Entities;
using HtmlRenderer.Core.Handlers;
using HtmlRenderer.Entities;

namespace HtmlRenderer.Interfaces
{
    /// <summary>
    /// atodo: add doc
    /// </summary>
    public abstract class GlobalBase
    {
        #region Fields and Consts

        /// <summary>
        /// default CSS parsed data singleton
        /// </summary>
        private CssData _defaultCssData;

        /// <summary>
        /// image used to draw loading image icon
        /// </summary>
        private IImage _loadImage;

        /// <summary>
        /// image used to draw error image icon
        /// </summary>
        private IImage _errorImage;

        /// <summary>
        /// cache of all the font used not to create same font again and again
        /// </summary>
        private readonly FontHandler _fontHandler;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        protected GlobalBase()
        {
            _fontHandler = new FontHandler(this);
        }

        /// <summary>
        /// Get the default CSS stylesheet data.
        /// </summary>
        public CssData DefaultCssData
        {
            get { return _defaultCssData ?? ( _defaultCssData = CreateDefaultCssData(CssDefaults.DefaultStyleSheet) ); }
        }

        /// <summary>
        /// Resolve color value from given color name.
        /// </summary>
        /// <param name="colorName">the color name</param>
        /// <returns>color value</returns>
        public abstract RColor GetColor(string colorName);

        /// <summary>
        /// Convert image object returned from <see cref="HtmlImageLoadEventArgs"/> to <see cref="IImage"/>.
        /// </summary>
        /// <param name="image">the image returned from load event</param>
        /// <returns>converted image or null</returns>
        public abstract IImage ConvertImage(object image);
        
        /// <summary>
        /// Create an <see cref="IImage"/> object from the given stream.
        /// </summary>
        /// <param name="memoryStream">the stream to create image from</param>
        /// <returns>new image instance</returns>
        public abstract IImage ImageFromStream(Stream memoryStream);

        /// <summary>
        /// Check if the given font exists in the system by font family name.
        /// </summary>
        /// <param name="font">the font name to check</param>
        /// <returns>true - font exists by given family name, false - otherwise</returns>
        public bool IsFontExists(string font)
        {
            return _fontHandler.IsFontExists(font);
        }

        /// <summary>
        /// Adds a font family to be used.
        /// </summary>
        /// <param name="fontFamily">The font family to add.</param>
        public void AddFontFamily(IFontFamily fontFamily)
        {
            _fontHandler.AddFontFamily(fontFamily);
        }

        /// <summary>
        /// Adds a font mapping from <paramref name="fromFamily"/> to <paramref name="toFamily"/> iff the <paramref name="fromFamily"/> is not found.<br/>
        /// When the <paramref name="fromFamily"/> font is used in rendered html and is not found in existing 
        /// fonts (installed or added) it will be replaced by <paramref name="toFamily"/>.<br/>
        /// </summary>
        /// <param name="fromFamily">the font family to replace</param>
        /// <param name="toFamily">the font family to replace with</param>
        public void AddFontFamilyMapping(string fromFamily, string toFamily)
        {
            _fontHandler.AddFontFamilyMapping(fromFamily, toFamily);
        }

        /// <summary>
        /// Get font instance by given font family name, size and style.
        /// </summary>
        /// <param name="family">the font family name</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        public IFont GetFont(string family, float size, RFontStyle style)
        {
            return _fontHandler.GetCachedFont(family, size, style);
        }

        /// <summary>
        /// Get image to be used while HTML image is loading.
        /// </summary>
        public IImage GetLoadImage()
        {
            if( _loadImage == null )
            {
                var stream = typeof(HtmlRendererUtils).Assembly.GetManifestResourceStream("HtmlRenderer.Core.Utils.ImageLoad.png");
                if( stream != null )
                    _loadImage = ImageFromStream(stream);
            }
            return _loadImage;
        }

        /// <summary>
        /// Get image to be used if HTML image load failed.
        /// </summary>
        public IImage GetErrorImage()
        {
            if (_errorImage == null)
            {
                var stream = typeof(HtmlRendererUtils).Assembly.GetManifestResourceStream("HtmlRenderer.Core.Utils.ImageError.png");
                if (stream != null)
                    _errorImage = ImageFromStream(stream);
            }
            return _errorImage;
        }

        /// <summary>
        /// Set the given text to the clipboard
        /// </summary>
        /// <param name="text">the text to set</param>
        public virtual void SetToClipboard(string text)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the given html and plain text data to clipboard.
        /// </summary>
        /// <param name="html">the html data</param>
        /// <param name="plainText">the plain text data</param>
        public virtual void SetToClipboard(string html, string plainText)
        {
            throw new NotImplementedException();            
        }

        /// <summary>
        /// Set the given image to clipboard.
        /// </summary>
        /// <param name="image"></param>
        public virtual void SetToClipboard(IImage image)
        {
            throw new NotImplementedException();            
        }

        /// <summary>
        /// Create a context menu that can be used on the control
        /// </summary>
        /// <returns>new context menu</returns>
        public virtual IContextMenu CreateContextMenu()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Save the given image to file by showing save dialog to the client.
        /// </summary>
        /// <param name="image">the image to save</param>
        /// <param name="name">the name of the image for save dialog</param>
        /// <param name="extension">the extension of the image for save dialog</param>
        /// <param name="control">optional: the control to show the dialog on</param>
        public virtual void SaveToFile(IImage image, string name, string extension, IControl control = null)
        {
            throw new NotImplementedException();            
        }

        /// <summary>
        /// Create a default CSS data object that will be cached.
        /// </summary>
        protected abstract CssData CreateDefaultCssData(string defaultStyleSheet);

        /// <summary>
        /// Get font instance by given font family name, size and style.
        /// </summary>
        /// <param name="family">the font family name</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        protected internal abstract IFont CreateFont(string family, float size, RFontStyle style);

        /// <summary>
        /// Get font instance by given font family instance, size and style.<br/>
        /// Used to support custom fonts that require explicit font family instance to be created.
        /// </summary>
        /// <param name="family">the font family instance</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        protected internal abstract IFont CreateFont(IFontFamily family, float size, RFontStyle style);
    }
}
