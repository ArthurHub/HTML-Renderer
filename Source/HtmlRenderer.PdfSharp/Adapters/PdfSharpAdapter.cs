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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Network;
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
        /// Also recognizes a family registered via <see cref="AddFontFaceAsync"/> - those live entirely in
        /// <see cref="FontResolver"/>, invisible to the shared <c>FontsHandler</c> the base implementation
        /// checks (see <see cref="AddFontFaceAsync"/>'s own doc comment for why). Without this override,
        /// <see cref="Core.Parse.CssParser.ParseFontFamily"/> would never see an <c>@font-face</c>-only
        /// family as "existing" and would silently substitute <see cref="Core.Utils.CssConstants.DefaultFont"/>
        /// for it before layout ever gets a chance to resolve the real face.
        /// </summary>
        public override bool IsFontExists(string font)
        {
            return base.IsFontExists(font) || _fontResolver.HasFamily(font);
        }

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        public static PdfSharpAdapter Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Get the FontResolver instance for advanced font management.
        /// </summary>
        internal FontResolver FontResolver
        {
            get { return _fontResolver; }
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

        /// <summary>
        /// Never called: this backend overrides <see cref="AddFontFaceAsync"/> directly (see its own doc
        /// comment) rather than routing through the base's <c>LoadFontFaceFontInt</c>-based path.
        /// </summary>
        protected override RFontFamily LoadFontFaceFontInt(byte[] fontBytes, string filePath)
        {
            throw new NotSupportedException("PdfSharpAdapter overrides AddFontFaceAsync directly and never calls LoadFontFaceFontInt.");
        }

        /// <summary>
        /// Bypasses the base <see cref="RAdapter.AddFontFaceAsync"/>/shared <c>FontsHandler</c> registry
        /// entirely: PDFsharp's <see cref="IFontResolver"/> needs raw font bytes for PDF embedding, which
        /// <see cref="FontResolver.AddFont(Stream,string,int?,bool?,int?,IReadOnlyList{CodepointRange})"/>
        /// already stores and matches against directly - no platform font-family handle is involved.
        /// </summary>
        public override async Task<bool> AddFontFaceAsync(string familyName, RUri uri, int weight, bool isItalic, int stretch, IReadOnlyList<CodepointRange> ranges)
        {
            var networkResponse = await GetResourceStreamAsync(uri).ConfigureAwait(false);
            if (networkResponse?.ResourceStream == null)
            {
                return false;
            }

            try
            {
                using (networkResponse.ResourceStream)
                {
                    _fontResolver.AddFont(networkResponse.ResourceStream, familyName, weight, isItalic, stretch, ranges);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Bypasses the shared registry, for the same reason as <see cref="AddFontFaceAsync"/> - see its doc comment.</summary>
        public override bool AddFontFaceFromLocalFamily(string familyName, string localFamilyName, int weight, bool isItalic, int stretch, IReadOnlyList<CodepointRange> ranges)
        {
            return _fontResolver.AddLocalFontFamily(familyName, localFamilyName, weight, isItalic, stretch, ranges);
        }

        /// <summary>
        /// Bypasses the shared registry, for the same reason as <see cref="AddFontFaceAsync"/> - see its doc
        /// comment. PDFsharp's own <see cref="XFont(string,double,XFontStyle,XPdfFontOptions)"/>
        /// construction only ever resolves by (family name, bold, italic) internally - there is no public
        /// PDFsharp API surface to hand it an already-chosen numeric weight/stretch/face directly - so a
        /// numeric <paramref name="weight"/> can only steer face selection as far as PDFsharp's own
        /// bold/not-bold threshold allows; <paramref name="codepoint"/>, which that 2-bool resolution can't
        /// express at all, is still honored precisely by consulting the richer
        /// <see cref="FontResolver.ResolveTypeface(string,int,bool,int,int?)"/> overload up front purely to
        /// preserve the "codepoint-scoped miss returns null" contract.
        /// </summary>
        public override RFont GetFont(string family, double size, RFontStyle style, int weight, int stretch, int? codepoint)
        {
            var isItalic = (style & RFontStyle.Italic) != 0;

            if (codepoint.HasValue)
            {
                var info = _fontResolver.ResolveTypeface(family, weight, isItalic, stretch, codepoint);
                if (info == null)
                {
                    return null;
                }
            }

            var isBold = weight >= 600;
            var residualStyle = (style & (RFontStyle.Underline | RFontStyle.Strikeout))
                                 | (isBold ? RFontStyle.Bold : RFontStyle.Regular)
                                 | (isItalic ? RFontStyle.Italic : RFontStyle.Regular);

            return CreateFontInt(family, size, residualStyle);
        }
    }
}