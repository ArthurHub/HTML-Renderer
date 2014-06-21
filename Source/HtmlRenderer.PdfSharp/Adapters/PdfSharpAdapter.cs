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
using HtmlRenderer.Adapters;
using HtmlRenderer.Adapters.Entities;
using HtmlRenderer.PdfSharp.Utilities;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace HtmlRenderer.PdfSharp.Adapters
{
    /// <summary>
    /// Adapter for PdfSharp library platform.
    /// </summary>
    internal sealed class PdfSharpAdapter : RAdapter
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

        protected override RPen CreatePen(RColor color)
        {
            return new PenAdapter(new XPen(Utils.Convert(color)));
        }

        protected override RBrush CreateSolidBrush(RColor color)
        {
            XBrush solidBrush;
            if (color == RColor.White)
                solidBrush = XBrushes.White;
            else if (color == RColor.Black)
                solidBrush = XBrushes.Black;
            else if (color.A < 1)
                solidBrush = XBrushes.Transparent;
            else
                solidBrush = new XSolidBrush(Utils.Convert(color));

            return new BrushAdapter(solidBrush);
        }

        protected override RImage ConvertImageInt(object image)
        {
            return image != null ? new ImageAdapter((XImage)image) : null;
        }

        protected override RImage ImageFromStreamInt(Stream memoryStream)
        {
            return new ImageAdapter(XImage.FromGdiPlusImage(Image.FromStream(memoryStream)));
        }

        protected override RFont CreateFontInt(string family, double size, RFontStyle style)
        {
            var fontStyle = (XFontStyle)((int)style);
            var xFont = new XFont(family, size, fontStyle, new XPdfFontOptions(PdfFontEncoding.Unicode));
            return new FontAdapter(xFont);
        }

        protected override RFont CreateFontInt(RFontFamily family, double size, RFontStyle style)
        {
            var fontStyle = (XFontStyle)((int)style);
            var xFont = new XFont(((FontFamilyAdapter)family).FontFamily.Name, size, fontStyle, new XPdfFontOptions(PdfFontEncoding.Unicode));
            return new FontAdapter(xFont);
        }
    }
}