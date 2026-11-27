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
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters
{
    /// <summary>
    /// Adapter for a font family, either installed and found by name, or a single loaded typeface.
    /// </summary>
    internal sealed class FontFamilyAdapter : RFontFamily
    {
        private readonly string _name;

        private readonly SKTypeface _typeface;

        /// <summary>
        /// An installed family, the font manager picks the typeface for each style.
        /// </summary>
        public FontFamilyAdapter(string name)
        {
            _name = name;
        }

        /// <summary>
        /// A typeface that isn't installed, e.g. from a font file or an <c>@font-face</c> rule.
        /// </summary>
        public FontFamilyAdapter(SKTypeface typeface)
        {
            _name = typeface.FamilyName;
            _typeface = typeface;
        }

        public override string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// The loaded typeface, null for an installed family.
        /// </summary>
        public SKTypeface Typeface
        {
            get { return _typeface; }
        }
    }
}
