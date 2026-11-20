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
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.WinUI.Adapters
{
    /// <summary>
    /// Adapter for Win2D/DirectWrite font family object for core.
    /// </summary>
    internal sealed class FontFamilyAdapter : RFontFamily
    {
        /// <summary>
        /// the family name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The font file of an <c>@font-face</c> or custom family, null for system families.
        /// </summary>
        private readonly Uri _fontFile;

        /// <summary>
        /// Init.
        /// </summary>
        public FontFamilyAdapter(string name, Uri fontFile = null)
        {
            _name = name;
            _fontFile = fontFile;
        }

        public override string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// The family for <see cref="Microsoft.Graphics.Canvas.Text.CanvasTextFormat.FontFamily"/>, DirectWrite
        /// only finds a family that isn't installed with the font file in front of the name.
        /// </summary>
        public string TextFormatFamily
        {
            get { return _fontFile != null ? _fontFile.AbsoluteUri + "#" + _name : _name; }
        }
    }
}
