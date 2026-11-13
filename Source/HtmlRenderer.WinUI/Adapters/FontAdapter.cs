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

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Text;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.WinUI.Adapters
{
    /// <summary>
    /// Adapter for Win2D font.
    /// </summary>
    /// <remarks>
    /// The line metrics come from laying out a probe string, the underline position is approximated from them.
    /// </remarks>
    internal sealed class FontAdapter : RFont
    {
        #region Fields and Consts

        /// <summary>
        /// Has an ascender and a descender, so the line height covers a normal line of text.
        /// </summary>
        private const string ProbeString = "Ág";

        /// <summary>
        /// the underline Win2D text format.
        /// </summary>
        private readonly CanvasTextFormat _textFormat;

        /// <summary>
        /// the size of the font, in points.
        /// </summary>
        private readonly double _size;

        /// <summary>
        /// Cached font height, in device-independent pixels.
        /// </summary>
        private readonly double _height;

        /// <summary>
        /// the vertical offset of the font underline location from the top of the font.
        /// </summary>
        private readonly double _underlineOffset;

        /// <summary>
        /// Cached font whitespace width.
        /// </summary>
        private double _whitespaceWidth = -1;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="device">the device to build the probe layout with</param>
        /// <param name="fontFamily">the font family name</param>
        /// <param name="size">the font size, in points</param>
        /// <param name="style">the font style</param>
        public FontAdapter(CanvasDevice device, string fontFamily, double size, RFontStyle style)
        {
            _size = size;

            _textFormat = new CanvasTextFormat
            {
                FontFamily = fontFamily,
                // Points to device-independent pixels
                FontSize = (float)(96d / 72d * size),
                FontWeight = (style & RFontStyle.Bold) == RFontStyle.Bold ? FontWeights.Bold : FontWeights.Normal,
                FontStyle = (style & RFontStyle.Italic) == RFontStyle.Italic ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal,
                // The core breaks the lines itself
                WordWrapping = CanvasWordWrapping.NoWrap,
            };

            using (var layout = new CanvasTextLayout(device, ProbeString, _textFormat, float.MaxValue, float.MaxValue))
            {
                var lineMetrics = layout.LineMetrics;
                if (lineMetrics.Length > 0)
                {
                    _height = lineMetrics[0].Height;
                    var baseline = lineMetrics[0].Baseline;
                    // A bit below the baseline, into the descent
                    _underlineOffset = baseline + (_height - baseline) * 0.15;
                }
                else
                {
                    _height = layout.LayoutBounds.Height;
                    _underlineOffset = _height * 0.85;
                }
            }
        }

        /// <summary>
        /// the underline Win2D text format.
        /// </summary>
        public CanvasTextFormat TextFormat
        {
            get { return _textFormat; }
        }

        public override double Size
        {
            get { return _size; }
        }

        public override double UnderlineOffset
        {
            get { return _underlineOffset; }
        }

        public override double Height
        {
            get { return _height; }
        }

        public override double LeftPadding
        {
            get { return _height / 6f; }
        }

        public override double GetWhitespaceWidth(RGraphics graphics)
        {
            if (_whitespaceWidth < 0)
            {
                _whitespaceWidth = graphics.MeasureString(" ", this).Width;
            }
            return _whitespaceWidth;
        }
    }
}
