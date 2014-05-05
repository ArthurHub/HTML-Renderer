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
using System.Collections.Generic;
using System.IO;
using HtmlRenderer.Core;
using HtmlRenderer.Core.Entities;
using HtmlRenderer.Core.Handlers;
using HtmlRenderer.Core.Utils;
using HtmlRenderer.Entities;

namespace HtmlRenderer.Interfaces
{
    /// <summary>
    /// TODO:a add doc
    /// </summary>
    /// <remarks>
    /// It is best to have a singleton instance of this class for concrete implementation!<br/>
    /// This is because it holds caches of default CssData, Images, Fonts and Brushes.
    /// </remarks>
    public abstract class AdapterBase
    {
        #region Fields/Consts

        /// <summary>
        /// cache of brush color to brush instance
        /// </summary>
        private static readonly Dictionary<RColor, IBrush> _brushesCache = new Dictionary<RColor, IBrush>();

        /// <summary>
        /// cache of pen color to pen instance
        /// </summary>
        private static readonly Dictionary<RColor, IPen> _penCache = new Dictionary<RColor, IPen>();

        /// <summary>
        /// cache of all the font used not to create same font again and again
        /// </summary>
        private readonly FontHandler _fontHandler;

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

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        protected AdapterBase()
        {
            _fontHandler = new FontHandler(this);
        }

        /// <summary>
        /// Get the default CSS stylesheet data.
        /// </summary>
        public CssData DefaultCssData
        {
            get { return _defaultCssData ?? (_defaultCssData = CssData.Parse(this, CssDefaults.DefaultStyleSheet, false)); }
        }

        /// <summary>
        /// Resolve color value from given color name.
        /// </summary>
        /// <param name="colorName">the color name</param>
        /// <returns>color value</returns>
        public RColor GetColor(string colorName)
        {
            ArgChecker.AssertArgNotNullOrEmpty(colorName, "colorName");
            return GetColorInt(colorName);
        }

        /// <summary>
        /// Get cached pen instance for the given color.
        /// </summary>
        /// <param name="color">the color to get pen for</param>
        /// <returns>pen instance</returns>
        public IPen GetPen(RColor color)
        {
            IPen pen;
            if (!_penCache.TryGetValue(color, out pen))
            {
                _penCache[color] = pen = CreatePen(color);
            }
            return pen;
        }

        /// <summary>
        /// Get cached solid brush instance for the given color.
        /// </summary>
        /// <param name="color">the color to get brush for</param>
        /// <returns>brush instance</returns>
        public IBrush GetSolidBrush(RColor color)
        {
            IBrush brush;
            if (!_brushesCache.TryGetValue(color, out brush))
            {
                _brushesCache[color] = brush = CreateSolidBrush(color);
            }
            return brush;
        }

        /// <summary>
        /// Convert image object returned from <see cref="HtmlImageLoadEventArgs"/> to <see cref="IImage"/>.
        /// </summary>
        /// <param name="image">the image returned from load event</param>
        /// <returns>converted image or null</returns>
        public IImage ConvertImage(object image)
        {
            return ConvertImageInt(image);
        }

        /// <summary>
        /// Create an <see cref="IImage"/> object from the given stream.
        /// </summary>
        /// <param name="memoryStream">the stream to create image from</param>
        /// <returns>new image instance</returns>
        public IImage ImageFromStream(Stream memoryStream)
        {
            return ImageFromStreamInt(memoryStream);
        }

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
        public IFont GetFont(string family, double size, RFontStyle style)
        {
            return _fontHandler.GetCachedFont(family, size, style);
        }

        /// <summary>
        /// Get image to be used while HTML image is loading.
        /// </summary>
        public IImage GetLoadImage()
        {
            if (_loadImage == null)
            {
                var stream = typeof(HtmlRendererUtils).Assembly.GetManifestResourceStream("HtmlRenderer.Core.Utils.ImageLoad.png");
                if (stream != null)
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
        public void SetToClipboard(string text)
        {
            SetToClipboardInt(text);
        }

        /// <summary>
        /// Set the given html and plain text data to clipboard.
        /// </summary>
        /// <param name="html">the html data</param>
        /// <param name="plainText">the plain text data</param>
        public void SetToClipboard(string html, string plainText)
        {
            SetToClipboardInt(html, plainText);
        }

        /// <summary>
        /// Set the given image to clipboard.
        /// </summary>
        /// <param name="image"></param>
        public void SetToClipboard(IImage image)
        {
            SetToClipboardInt(image);
        }

        /// <summary>
        /// Create a context menu that can be used on the control
        /// </summary>
        /// <returns>new context menu</returns>
        public IContextMenu GetContextMenu()
        {
            return CreateContextMenuInt();
        }

        /// <summary>
        /// Save the given image to file by showing save dialog to the client.
        /// </summary>
        /// <param name="image">the image to save</param>
        /// <param name="name">the name of the image for save dialog</param>
        /// <param name="extension">the extension of the image for save dialog</param>
        /// <param name="control">optional: the control to show the dialog on</param>
        public void SaveToFile(IImage image, string name, string extension, IControl control = null)
        {
            SaveToFileInt(image, name, extension, control);
        }


        #region Private/Protected methods

        /// <summary>
        /// Resolve color value from given color name.
        /// </summary>
        /// <param name="colorName">the color name</param>
        /// <returns>color value</returns>
        protected abstract RColor GetColorInt(string colorName);

        /// <summary>
        /// Get cached pen instance for the given color.
        /// </summary>
        /// <param name="color">the color to get pen for</param>
        /// <returns>pen instance</returns>
        protected abstract IPen CreatePen(RColor color);

        /// <summary>
        /// Get cached solid brush instance for the given color.
        /// </summary>
        /// <param name="color">the color to get brush for</param>
        /// <returns>brush instance</returns>
        protected abstract IBrush CreateSolidBrush(RColor color);

        /// <summary>
        /// Convert image object returned from <see cref="HtmlImageLoadEventArgs"/> to <see cref="IImage"/>.
        /// </summary>
        /// <param name="image">the image returned from load event</param>
        /// <returns>converted image or null</returns>
        protected abstract IImage ConvertImageInt(object image);

        /// <summary>
        /// Create an <see cref="IImage"/> object from the given stream.
        /// </summary>
        /// <param name="memoryStream">the stream to create image from</param>
        /// <returns>new image instance</returns>
        protected abstract IImage ImageFromStreamInt(Stream memoryStream);

        /// <summary>
        /// Get font instance by given font family name, size and style.
        /// </summary>
        /// <param name="family">the font family name</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        protected internal abstract IFont CreateFontInt(string family, double size, RFontStyle style);

        /// <summary>
        /// Get font instance by given font family instance, size and style.<br/>
        /// Used to support custom fonts that require explicit font family instance to be created.
        /// </summary>
        /// <param name="family">the font family instance</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        protected internal abstract IFont CreateFontInt(IFontFamily family, double size, RFontStyle style);

        /// <summary>
        /// Set the given text to the clipboard
        /// </summary>
        /// <param name="text">the text to set</param>
        protected virtual void SetToClipboardInt(string text)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the given html and plain text data to clipboard.
        /// </summary>
        /// <param name="html">the html data</param>
        /// <param name="plainText">the plain text data</param>
        protected virtual void SetToClipboardInt(string html, string plainText)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the given image to clipboard.
        /// </summary>
        /// <param name="image"></param>
        protected virtual void SetToClipboardInt(IImage image)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a context menu that can be used on the control
        /// </summary>
        /// <returns>new context menu</returns>
        protected virtual IContextMenu CreateContextMenuInt()
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
        protected virtual void SaveToFileInt(IImage image, string name, string extension, IControl control = null)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}