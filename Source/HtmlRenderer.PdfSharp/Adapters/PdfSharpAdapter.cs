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
using HtmlRenderer.Entities;
using HtmlRenderer.Interfaces;
using HtmlRenderer.PdfSharp.Utilities;
using PdfSharp.Drawing;

namespace HtmlRenderer.PdfSharp.Adapters
{
    /// <summary>
    /// Adapter for general stuff for core.
    /// TODO:a add doc.
    /// </summary>
    internal sealed class PdfSharpAdapter : AdapterBase
    {
        #region Fields and Consts

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        private static readonly PdfSharpAdapter _instance = new PdfSharpAdapter();

        #endregion


        /// <summary>
        /// Init color resolve.
        /// </summary>
        private PdfSharpAdapter()
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
        public static PdfSharpAdapter Instance
        {
            get { return _instance; }
        }

        protected override RColor GetColorInt(string colorName)
        {
            return Utils.Convert(XColor.FromName(colorName));
        }

        protected override IPen CreatePen(RColor color)
        {
            return null;
        }

        protected override IBrush CreateSolidBrush(RColor color)
        {
            return null;
        }

        protected override IImage ConvertImageInt(object image)
        {
            return image != null ? new ImageAdapter((XImage)image) : null;
        }

        protected override IImage ImageFromStreamInt(Stream memoryStream)
        {
            return new ImageAdapter(XImage.FromGdiPlusImage(Image.FromStream(memoryStream)));
        }

        protected override IFont CreateFontInt(string family, double size, RFontStyle style)
        {
            var fontStyle = (XFontStyle)((int)style);
            return new FontAdapter(new XFont(family, size, fontStyle));
        }

        protected override IFont CreateFontInt(IFontFamily family, double size, RFontStyle style)
        {
            var fontStyle = (XFontStyle)((int)style);
            return new FontAdapter(new XFont(((FontFamilyAdapter)family).FontFamily.Name, size, fontStyle));
        }
    }
}
