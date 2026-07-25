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

using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.IO;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution;
using TheArtOfDev.HtmlRenderer.PdfSharp.Utilities;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.Adapters
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

        /// <summary>
        /// Font resolver instance for managing font discovery and resolution.
        /// </summary>
        private FontResolver _fontResolver;

        #endregion


        /// <summary>
        /// Init color resolve.
        /// </summary>
        private PdfSharpAdapter()
        {
            _fontResolver = FontResolver.Register();

            AddFontFamilyMapping("monospace", "Courier New");
            AddFontFamilyMapping("Helvetica", "Arial");

            var fontFamilies = _fontResolver.DiscoverFontFamilies();
            
            foreach (var fontFamily in fontFamilies)
            {
                AddFontFamily(new FontFamilyAdapter(new XFontFamily(fontFamily)));
            }
        }

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        public static PdfSharpAdapter Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Paged output, so @media print applies and @media screen does not.
        /// </summary>
        public override string DefaultMediaType
        {
            get { return "print"; }
        }

        /// <summary>
        /// A PDF has no system theme to follow, so prefers-color-scheme always reports light.
        /// </summary>
        public override RColorScheme SystemColorScheme
        {
            get { return RColorScheme.Light; }
        }

        /// <summary>
        /// Get the FontResolver instance for advanced font management.
        /// </summary>
        internal FontResolver FontResolver
        {
            get { return _fontResolver; }
        }

        protected override RColor GetColorInt(string colorName)
        {
            try
            {
                var colorResourceManager = new XColorResourceManager();

                var knownColors = XColorResourceManager.GetKnownColors(true);

                foreach (var knownColor in knownColors)
                {
                    var name = colorResourceManager.ToColorName(knownColor);
                    if (!string.Equals(name, colorName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var xColor = XColor.FromKnownColor(knownColor);
                    return xColor.IsEmpty ? RColor.Empty : Utils.Convert(xColor);
                }

                return RColor.Empty;
            }
            catch
            {
                return RColor.Empty;
            }
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

        protected override RBrush CreateLinearGradientBrush(RPoint p1, RPoint p2, (RColor Color, double Position)[] stops)
        {
            return new GradientBrushAdapter(p1, p2, stops);
        }

        protected override RImage ConvertImageInt(object image)
        {
            return image != null ? new ImageAdapter((XImage)image) : null;
        }

        protected override RImage ImageFromStreamInt(Stream memoryStream)
        {
            return new ImageAdapter(XImage.FromStream(memoryStream));
        }

        protected override RFont CreateFontInt(string family, double size, RFontStyle style)
        {
            var fontStyle = Utils.Convert(style);
            var xFont = new XFont(family, size, fontStyle, new XPdfFontOptions(PdfFontEncoding.Unicode));
            return new FontAdapter(xFont);
        }

        protected override RFont CreateFontInt(RFontFamily family, double size, RFontStyle style)
        {
            var fontStyle = Utils.Convert(style);
            var xFont = new XFont(((FontFamilyAdapter)family).FontFamily.Name, size, fontStyle, new XPdfFontOptions(PdfFontEncoding.Unicode));
            return new FontAdapter(xFont);
        }
    }
}