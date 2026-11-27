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
using System.Globalization;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters
{
    /// <summary>
    /// Adapter for a SkiaSharp font, including the fonts it falls back to for characters it has no glyph for.
    /// </summary>
    /// <remarks>
    /// Skia has no font linking like GDI or DirectWrite, without the fallback CJK text or emoji end up as boxes.
    /// </remarks>
    internal sealed class FontAdapter : RFont
    {
        #region Fields and Consts

        private readonly SKFont _font;

        private readonly SKFontStyle _style;

        /// <summary>
        /// The size in points, the core's unit for font sizes.
        /// </summary>
        private readonly double _size;

        private readonly double _height;

        /// <summary>
        /// Distance from the top of the line to the baseline.
        /// </summary>
        private readonly float _ascent;

        private readonly double _underlineOffset;

        /// <summary>
        /// The fonts found so far for characters the primary font doesn't cover. Fonts are shared between layouts.
        /// </summary>
        private readonly List<SKFont> _fallbacks = new List<SKFont>();

        /// <summary>
        /// The shapers by typeface handle, typefaces live as long as the process.
        /// </summary>
        private static readonly Dictionary<IntPtr, SKShaper> _shapers = new Dictionary<IntPtr, SKShaper>();

        private double _whitespaceWidth = -1;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="typeface">the typeface to draw with</param>
        /// <param name="style">the requested style, bold and italic are synthesized when the typeface lacks them</param>
        /// <param name="size">the font size, in points</param>
        public FontAdapter(SKTypeface typeface, SKFontStyle style, double size)
        {
            _style = style;
            _size = size;
            _font = CreateFont(typeface, style, (float)(size * 96d / 72d));

            _font.GetFontMetrics(out var metrics);
            _ascent = -metrics.Ascent;
            _height = -metrics.Ascent + metrics.Descent;

            // The core takes this as the baseline for vertical alignment too, so it stays close to it
            _underlineOffset = _ascent + metrics.Descent * 0.15;
        }

        public SKFont Font
        {
            get { return _font; }
        }

        /// <summary>
        /// Distance from the top of the line to the baseline.
        /// </summary>
        public float Ascent
        {
            get { return _ascent; }
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

        /// <summary>
        /// The advance width of the text.
        /// </summary>
        public float MeasureText(string text)
        {
            float width = 0;
            foreach (var run in GetRuns(text))
                width += Shape(text, run).Width;
            return width;
        }

        /// <summary>
        /// The number of characters from the start of the text that fit into the width, and the width they take.
        /// </summary>
        public int BreakText(string text, float maxWidth, out float measuredWidth)
        {
            measuredWidth = 0;
            var count = 0;
            foreach (var run in GetRuns(text))
            {
                var result = Shape(text, run);
                if (measuredWidth + result.Width <= maxWidth)
                {
                    measuredWidth += result.Width;
                    count += run.Length;
                    continue;
                }

                // The glyphs are in visual order, the clusters map them back to the characters
                var advances = new float[run.Length];
                var glyphCount = result.Codepoints.Length;
                for (var i = 0; i < glyphCount; i++)
                {
                    var end = i + 1 < glyphCount ? result.Points[i + 1].X : result.Width;
                    advances[result.Clusters[i]] += Math.Abs(end - result.Points[i].X);
                }

                foreach (var advance in advances)
                {
                    if (measuredWidth + advance > maxWidth)
                        return count;
                    measuredWidth += advance;
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Draws the text with its baseline at the given position, <paramref name="rtl"/> is the direction of
        /// text without a strong directional character.
        /// </summary>
        public void DrawText(SKCanvas canvas, string text, float x, float baseline, SKPaint paint, bool rtl = false)
        {
            // HarfBuzz orders the glyphs within a run, the runs of a right-to-left word go from right to left
            var runs = GetRuns(text);
            if (runs.Count > 1 && IsRightToLeft(text, rtl))
                runs.Reverse();

            foreach (var run in runs)
            {
                var result = Shape(text, run);
                var glyphCount = result.Codepoints.Length;
                if (glyphCount > 0)
                {
                    var glyphs = new ushort[glyphCount];
                    for (var i = 0; i < glyphCount; i++)
                        glyphs[i] = (ushort)result.Codepoints[i];

                    using (var builder = new SKTextBlobBuilder())
                    {
                        builder.AddPositionedRun(glyphs, run.Font, result.Points);
                        using (var blob = builder.Build())
                        {
                            if (blob != null)
                                canvas.DrawText(blob, x, baseline, paint);
                        }
                    }
                }
                x += result.Width;
            }
        }

        #region Private methods

        private static SKFont CreateFont(SKTypeface typeface, SKFontStyle style, float pixelSize)
        {
            return new SKFont(typeface, pixelSize)
            {
                Edging = SKFontEdging.Antialias,
                Subpixel = true,
                LinearMetrics = true,
                Hinting = SKFontHinting.Slight,
                // Like browsers, a family without a bold or italic face still gets bold or slanted text
                Embolden = style.Weight >= (int)SKFontStyleWeight.SemiBold && typeface.FontWeight < (int)SKFontStyleWeight.SemiBold,
                SkewX = style.Slant != SKFontStyleSlant.Upright && typeface.FontSlant == SKFontStyleSlant.Upright ? -0.25f : 0f,
            };
        }

        /// <summary>
        /// Shapes one run with HarfBuzz, for ligatures, kerning, joining scripts and right-to-left text.
        /// </summary>
        private static SKShaper.Result Shape(string text, TextRun run)
        {
            var runText = run.Start == 0 && run.Length == text.Length ? text : text.Substring(run.Start, run.Length);
            return GetShaper(run.Font.Typeface).Shape(ReplaceControlCharacters(runText), run.Font);
        }

        /// <summary>
        /// Control characters such as a tab in <c>white-space: pre</c> become spaces, fonts map them to the missing glyph box.
        /// </summary>
        private static string ReplaceControlCharacters(string text)
        {
            var index = 0;
            while (index < text.Length && text[index] >= 0x20)
                index++;
            if (index == text.Length)
                return text;

            var chars = text.ToCharArray();
            for (; index < chars.Length; index++)
            {
                if (chars[index] < 0x20)
                    chars[index] = ' ';
            }
            return new string(chars);
        }

        /// <summary>
        /// A shaper per typeface, creating one copies the whole font file.
        /// </summary>
        private static SKShaper GetShaper(SKTypeface typeface)
        {
            lock (_shapers)
            {
                if (!_shapers.TryGetValue(typeface.Handle, out var shaper))
                    _shapers[typeface.Handle] = shaper = new SKShaper(typeface);
                return shaper;
            }
        }

        /// <summary>
        /// Splits the text into runs of the same font, the fallback fonts cover what the primary font lacks.
        /// </summary>
        private List<TextRun> GetRuns(string text)
        {
            if (string.IsNullOrEmpty(text) || _font.ContainsGlyphs(text))
                return new List<TextRun>(1) { new TextRun(0, text?.Length ?? 0, _font) };

            var runs = new List<TextRun>();
            var runStart = 0;
            SKFont runFont = null;
            for (var i = 0; i < text.Length;)
            {
                var codepoint = CodepointAt(text, i, out var charCount);
                var category = CharUnicodeInfo.GetUnicodeCategory(text, i);

                // Marks, joiners and variation selectors belong to the character before them
                var attaches = runFont != null
                               && (category == UnicodeCategory.NonSpacingMark
                                   || category == UnicodeCategory.EnclosingMark
                                   || category == UnicodeCategory.SpacingCombiningMark
                                   || category == UnicodeCategory.Format);

                var font = attaches ? runFont : GetFontFor(codepoint);
                if (runFont != null && font != runFont)
                {
                    runs.Add(new TextRun(runStart, i - runStart, runFont));
                    runStart = i;
                }
                runFont = font;
                i += charCount;
            }
            runs.Add(new TextRun(runStart, text.Length - runStart, runFont));
            return runs;
        }

        private SKFont GetFontFor(int codepoint)
        {
            if (codepoint < 0x20 || _font.ContainsGlyph(codepoint))
                return _font;

            lock (_fallbacks)
            {
                foreach (var fallback in _fallbacks)
                {
                    if (fallback.ContainsGlyph(codepoint))
                        return fallback;
                }

                var typeface = SKFontManager.Default.MatchCharacter(_font.Typeface.FamilyName, _style, null, codepoint);
                if (typeface == null)
                    return _font;

                // A typeface that lacks the glyph after all would be matched again for every character
                foreach (var fallback in _fallbacks)
                {
                    if (fallback.Typeface.Handle == typeface.Handle)
                        return fallback;
                }

                var font = CreateFont(typeface, _style, _font.Size);
                _fallbacks.Add(font);
                return font;
            }
        }

        /// <summary>
        /// Whether the first strong character is right-to-left (UAX #9 P2 and P3). Letters in the Hebrew, Arabic,
        /// Syriac, Thaana, NKo, Samaritan and Mandaic blocks and their presentation forms count as right-to-left.
        /// </summary>
        private static bool IsRightToLeft(string text, bool defaultRtl)
        {
            for (var i = 0; i < text.Length;)
            {
                var codepoint = CodepointAt(text, i, out var charCount);
                var isLetter = char.IsLetter(text, i);
                i += charCount;

                if (!isLetter)
                    continue;

                return (codepoint >= 0x0590 && codepoint <= 0x08FF)
                       || (codepoint >= 0xFB1D && codepoint <= 0xFDFF)
                       || (codepoint >= 0xFE70 && codepoint <= 0xFEFF)
                       || (codepoint >= 0x10800 && codepoint <= 0x10FFF)
                       || (codepoint >= 0x1E800 && codepoint <= 0x1EFFF);
            }
            return defaultRtl;
        }

        /// <summary>
        /// The code point at the index, a lone surrogate is returned as is.
        /// </summary>
        private static int CodepointAt(string text, int index, out int charCount)
        {
            if (char.IsHighSurrogate(text[index]) && index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]))
            {
                charCount = 2;
                return char.ConvertToUtf32(text[index], text[index + 1]);
            }

            charCount = 1;
            return text[index];
        }

        #endregion

        private readonly struct TextRun
        {
            public TextRun(int start, int length, SKFont font)
            {
                Start = start;
                Length = length;
                Font = font;
            }

            public int Start { get; }

            public int Length { get; }

            public SKFont Font { get; }
        }
    }
}
