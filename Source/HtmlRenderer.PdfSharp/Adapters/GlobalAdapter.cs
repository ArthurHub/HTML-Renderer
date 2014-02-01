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
using System.IO;
using HtmlRenderer.Core;
using HtmlRenderer.Entities;
using HtmlRenderer.Interfaces;
using HtmlRenderer.PdfSharp.Utilities;
using PdfSharp.Drawing;

namespace HtmlRenderer.PdfSharp.Adapters
{
    /// <summary>
    /// Adapter for general stuff for core.
    /// </summary>
    internal sealed class GlobalAdapter : GlobalBase
    {
        #region Fields and Consts

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        private static readonly GlobalAdapter _instance = new GlobalAdapter();

        #endregion


        /// <summary>
        /// Init color resolve.
        /// </summary>
        private GlobalAdapter()
        {
            AddFontFamilyMapping("monospace", "Courier New");
            AddFontFamilyMapping("Helvetica", "Arial");

            foreach (var family in XFontFamily.Families)
            {
                AddFontFamily(new FontFamilyAdapter(family));
            }
        }

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        public static GlobalAdapter Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Create a default CSS data object that will be cached.
        /// </summary>
        protected override CssData CreateDefaultCssData(string defaultStyleSheet)
        {
            return CssData.Parse(this, defaultStyleSheet, false);
        }

        /// <summary>
        /// Resolve color value from given color name.
        /// </summary>
        /// <param name="colorName">the color name</param>
        /// <returns>color value</returns>
        public override RColor GetColor(string colorName)
        {
            var color = XColor.FromName(colorName);
            return Utils.Convert(color);
        }

        /// <summary>
        /// Convert image object returned from <see cref="HtmlImageLoadEventArgs"/> to <see cref="IImage"/>.
        /// </summary>
        /// <param name="image">the image returned from load event</param>
        /// <returns>converted image or null</returns>
        public override IImage ConvertImage(object image)
        {
            return image != null ? new ImageAdapter((XImage)image) : null;
        }

        /// <summary>
        /// Create an <see cref="IImage"/> object from the given stream.
        /// </summary>
        /// <param name="memoryStream">the stream to create image from</param>
        /// <returns>new image instance</returns>
        public override IImage ImageFromStream(Stream memoryStream)
        {
            return new ImageAdapter(XImage.FromGdiPlusImage(Image.FromStream(memoryStream)));
        }

        /// <summary>
        /// Get font instance by given font family name, size and style.
        /// </summary>
        /// <param name="family">the font family name</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        protected override IFont CreateFont(string family, float size, RFontStyle style)
        {
            var fontStyle = (XFontStyle)((int)style);
            return new FontAdapter(new XFont(family, size, fontStyle));
        }

        /// <summary>
        /// Get font instance by given font family instance, size and style.<br/>
        /// Used to support custom fonts that require explicit font family instance to be created.
        /// </summary>
        /// <param name="family">the font family instance</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        protected override IFont CreateFont(IFontFamily family, float size, RFontStyle style)
        {
            var fontStyle = (XFontStyle)((int)style);
            return new FontAdapter(new XFont(((FontFamilyAdapter)family).FontFamily.Name, size, fontStyle));
        }
    }
}
