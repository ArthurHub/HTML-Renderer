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
using System.Reflection;
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.SkiaSharp.Utilities;

namespace TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters
{
    /// <summary>
    /// Adapter for the SkiaSharp platform.
    /// </summary>
    internal sealed class SkiaSharpAdapter : RAdapter
    {
        #region Fields and Consts

        private static readonly SkiaSharpAdapter _instance = new SkiaSharpAdapter();

        /// <summary>
        /// The named colors of <see cref="SKColors"/>, e.g. for .NET names the CSS engine doesn't know.
        /// </summary>
        private static readonly Dictionary<string, SKColor> _namedColors = CreateNamedColors();

        #endregion

        private SkiaSharpAdapter()
        {
            var installed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var family in SKFontManager.Default.FontFamilies)
            {
                AddFontFamily(new FontFamilyAdapter(family));
                installed.Add(family);
            }

            // Same as the other adapters where the fonts exist, else the common Linux and macOS fonts, else the font
            // the system picks for the generic family. The NoDependencies native assets on Linux have no fontconfig
            // and can't resolve a generic family. serif isn't mapped by any adapter, see Spec/css21.yml.
            MapFamily(installed, "monospace", "monospace", "Courier New", "Liberation Mono", "DejaVu Sans Mono", "Menlo", "Courier");
            MapFamily(installed, "Helvetica", "sans-serif", "Arial", "Liberation Sans");
            MapFamily(installed, CssConstants.DefaultFont, "sans-serif", "Arial", "Liberation Sans", "DejaVu Sans", "Helvetica");
            MapFamily(installed, "Arial", "sans-serif", "Liberation Sans", "Helvetica", "DejaVu Sans");
            MapFamily(installed, "Times New Roman", "serif", "Liberation Serif", "Times", "DejaVu Serif");
            MapFamily(installed, "Courier New", "monospace", "Liberation Mono", "Courier", "DejaVu Sans Mono", "Menlo");
            MapFamily(installed, "Consolas", "monospace", "DejaVu Sans Mono", "Liberation Mono", "Menlo");
        }

        public static SkiaSharpAdapter Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Maps a missing family to the first installed candidate, or else to what the system resolves the
        /// generic family to. The core doesn't chain mappings, so the target has to be installed.
        /// </summary>
        private void MapFamily(HashSet<string> installed, string family, string generic, params string[] candidates)
        {
            if (installed.Contains(family))
                return;

            foreach (var candidate in candidates)
            {
                if (installed.Contains(candidate))
                {
                    AddFontFamilyMapping(family, candidate);
                    return;
                }
            }

            // Not disposed, SkiaSharp returns the same wrapper to everyone who asks for that typeface
            var match = SKFontManager.Default.MatchFamily(generic);
            if (match != null && installed.Contains(match.FamilyName))
                AddFontFamilyMapping(family, match.FamilyName);
        }

        protected override RColor GetColorInt(string colorName)
        {
            return _namedColors.TryGetValue(colorName, out var color) ? Utils.Convert(color) : RColor.Empty;
        }

        protected override RPen CreatePen(RColor color)
        {
            return new PenAdapter(Utils.Convert(color));
        }

        protected override RBrush CreateSolidBrush(RColor color)
        {
            return new BrushAdapter(Utils.Convert(color));
        }

        protected override RBrush CreateLinearGradientBrush(RPoint p1, RPoint p2, (RColor Color, double Position)[] stops)
        {
            var colors = new SKColor[stops.Length];
            var positions = new float[stops.Length];
            for (int i = 0; i < stops.Length; i++)
            {
                colors[i] = Utils.Convert(stops[i].Color);
                positions[i] = (float)stops[i].Position;
            }

            return new BrushAdapter(SKShader.CreateLinearGradient(Utils.Convert(p1), Utils.Convert(p2), colors, positions, SKShaderTileMode.Clamp));
        }

        protected override RImage ConvertImageInt(object image)
        {
            switch (image)
            {
                case SKImage skImage:
                    return new ImageAdapter(ToDrawableImage(skImage, false));
                case SKBitmap bitmap:
                    return new ImageAdapter(SKImage.FromBitmap(bitmap));
                default:
                    return null;
            }
        }

        protected override RImage ImageFromStreamInt(Stream memoryStream)
        {
            using (var data = SKData.Create(memoryStream))
            {
                var image = data != null ? SKImage.FromEncodedData(data) : null;
                return image != null ? new ImageAdapter(ToDrawableImage(image, true)) : null;
            }
        }

        protected override RFont CreateFontInt(string family, double size, RFontStyle style)
        {
            var fontStyle = Convert(style);
            var typeface = SKFontManager.Default.MatchFamily(family, fontStyle) ?? SKTypeface.FromFamilyName(family, fontStyle);
            return new FontAdapter(typeface, fontStyle, size);
        }

        protected override RFont CreateFontInt(RFontFamily family, double size, RFontStyle style)
        {
            var familyAdapter = (FontFamilyAdapter)family;
            if (familyAdapter.Typeface == null)
                return CreateFontInt(family.Name, size, style);

            return new FontAdapter(familyAdapter.Typeface, Convert(style), size);
        }

        protected override RFontFamily LoadFontFaceFontInt(byte[] fontBytes, string filePath)
        {
            var typeface = SKTypeface.FromData(SKData.CreateCopy(fontBytes));
            return typeface != null ? new FontFamilyAdapter(typeface) : null;
        }

        #region Private methods

        /// <summary>
        /// Decodes a lazily decoded image other than JPEG, Skia's PDF backend aborts the process when it draws one.
        /// A JPEG stays encoded, the PDF embeds it as is instead of as much larger raw pixels.
        /// </summary>
        private static SKImage ToDrawableImage(SKImage image, bool ownsImage)
        {
            if (!image.IsLazyGenerated)
                return image;

            using (var encoded = image.EncodedData)
            {
                var bytes = encoded != null && encoded.Size >= 3 ? encoded.AsSpan() : default;
                if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                    return image;
            }

            var raster = image.ToRasterImage(true);
            if (raster == null || ReferenceEquals(raster, image))
                return image;

            if (ownsImage)
                image.Dispose();
            return raster;
        }

        private static SKFontStyle Convert(RFontStyle style)
        {
            var weight = (style & RFontStyle.Bold) != 0 ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
            var slant = (style & RFontStyle.Italic) != 0 ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
            return new SKFontStyle(weight, SKFontStyleWidth.Normal, slant);
        }

        private static Dictionary<string, SKColor> CreateNamedColors()
        {
            var colors = new Dictionary<string, SKColor>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in typeof(SKColors).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(SKColor))
                    colors[field.Name] = (SKColor)field.GetValue(null);
            }
            return colors;
        }

        #endregion
    }
}
