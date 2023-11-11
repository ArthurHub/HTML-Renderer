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
using System;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.SkiaSharp.Utilities;

namespace TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters
{


    public class SKBrush
    {
        public enum Types
        {
            Solid,
            LinearGradient
        }


        public static SKBrush Black = new SKBrush(SKColors.Black);
        public static SKBrush White = new SKBrush(SKColors.White);
        public static SKBrush Transparent = new SKBrush(SKColors.Transparent);

        public Types Type { get; set; }

        public SKColor Color { get; set; }

        public SKPathEffect PathEffect { get; set; }

        public SKShader Shader { get; set; }


        public SKBrush(SKColor color = default, Types type = Types.Solid) 
        {
            Type = type;
            Color = color;
        }

        public SKBrush(SKRect rect, SKColor color1, SKColor color2, float angle = 0, Types type = Types.LinearGradient)
        {
            Type = type;

            var colors = new SKColor[] {
                color1,
                color2
            };

            // Leave it Horizontal for now...
            var pointA = new SKPoint(rect.Left, rect.MidY);
            var pointB = new SKPoint(rect.Right, rect.MidY);

            Shader = SKShader.CreateLinearGradient(
                pointA, pointB,
                colors,
                null,
                SKShaderTileMode.Clamp);
        }
    }


    /// <summary>
    /// Adapter for WinForms brushes objects for core.
    /// </summary>
    internal sealed class BrushAdapter : RBrush
    {
        /// <summary>
        /// The actual PdfSharp brush instance.<br/>
        /// Should be <see cref="XBrush"/> but there is some fucking issue inheriting from it =/
        /// </summary>
        private readonly Object _brush;

        /// <summary>
        /// Init.
        /// </summary>
        public BrushAdapter(Object brush)
        {
            _brush = brush;
        }

        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        public Object Brush
        {
            get { return _brush; }
        }

        public override void Dispose()
        { }
    }
}