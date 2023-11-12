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

using SkiaSharp;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.SkiaSharp.Utilities;

namespace TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters
{
    /// <summary>
    /// Adapter for SkiaSharp library platform.
    /// </summary>
    internal sealed class SkiaSharpAdapter : RAdapter
    {
        #region Fields and Consts

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        private static readonly SkiaSharpAdapter _instance = new SkiaSharpAdapter();

        #endregion


        /// <summary>
        /// Init color resolve.
        /// </summary>
        private SkiaSharpAdapter()
        {
            AddFontFamilyMapping("monospace", "Courier New");
            AddFontFamilyMapping("Helvetica", "Arial");

            var manager = SKFontManager.CreateDefault();
            var families = manager.GetFontFamilies();

            foreach (var family in families)
            {
                AddFontFamily(new FontFamilyAdapter(family));
            }
        }

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        public static SkiaSharpAdapter Instance
        {
            get { return _instance; }
        }

        protected override RColor GetColorInt(string colorName)
        {
            try
            {
                var color = Color.FromKnownColor((KnownColor)System.Enum.Parse(typeof(KnownColor), colorName, true));
                return Utils.Convert(color);
            }
            catch
            {
                return RColor.Empty;
            }
        }

        protected override RPen CreatePen(RColor color)
        {
            return new PenAdapter(new SKPaint { Color = Utils.Convert(color) });
        }

        protected override RBrush CreateSolidBrush(RColor color)
        {
            SKBrush solidBrush;
            if (color == RColor.White)
                solidBrush = SKBrush.White;
            else if (color == RColor.Black)
                solidBrush = SKBrush.Black;
            else if (color.A < 1)
                solidBrush = SKBrush.Transparent;
            else
                solidBrush = new SKBrush(Utils.Convert(color));

            return new BrushAdapter(solidBrush);
        }

        protected override RBrush CreateLinearGradientBrush(RRect rect, RColor color1, RColor color2, double angle)
        {
            //XLinearGradientMode mode;
            //if (angle < 45)
            //    mode = XLinearGradientMode.ForwardDiagonal;
            //else if (angle < 90)
            //    mode = XLinearGradientMode.Vertical;
            //else if (angle < 135)
            //    mode = XLinearGradientMode.BackwardDiagonal;
            //else
            //    mode = XLinearGradientMode.Horizontal;
            return new BrushAdapter(new SKBrush(Utils.Convert(rect), Utils.Convert(color1), Utils.Convert(color2), (float)angle));
        }

        protected override RImage ConvertImageInt(object image)
        {
            return image != null ? new ImageAdapter((SKImage)image) : null;
        }

        protected override RImage ImageFromStreamInt(Stream memoryStream)
        {
            SKBitmap bitmap = SKBitmap.Decode(memoryStream);
            return new ImageAdapter(SKImage.FromBitmap(bitmap));
        }

        protected override RFont CreateFontInt(string family, double size, RFontStyle style)
        {
            SKFontStyle fontStyle;

            switch(style)
            {
                case RFontStyle.Bold: fontStyle = SKFontStyle.Bold; break;
                case RFontStyle.Italic: fontStyle = SKFontStyle.Italic; break;

                case RFontStyle.Regular:
                default:
                    fontStyle = SKFontStyle.Normal; break;
            }

            var typeface = SKTypeface.FromFamilyName(family, fontStyle);

            var skFont = new SKFont(typeface, (float)size);
            skFont.LinearMetrics = true;
            skFont.Subpixel = true;
            

            return new FontAdapter(skFont);
        }

        protected override RFont CreateFontInt(RFontFamily family, double size, RFontStyle style)
        {
            return this.CreateFontInt(((FontFamilyAdapter)family).FontFamily, size, style);
        }
    }
}