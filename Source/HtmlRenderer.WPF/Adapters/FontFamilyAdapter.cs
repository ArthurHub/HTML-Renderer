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

using System.Globalization;
using System.Linq;
using System.Windows.Markup;
using System.Windows.Media;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.WPF.Adapters
{
    /// <summary>
    /// Adapter for WPF Font family object for core.
    /// </summary>
    internal sealed class FontFamilyAdapter : RFontFamily
    {
        /// <summary>
        /// Default language to get font family name by
        /// </summary>
        private static readonly XmlLanguage _xmlLanguage = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

        /// <summary>
        /// the underline win-forms font.
        /// </summary>
        private readonly FontFamily _fontFamily;

        /// <summary>
        /// Init.
        /// </summary>
        public FontFamilyAdapter(FontFamily fontFamily)
        {
            _fontFamily = fontFamily;
        }

        /// <summary>
        /// the underline WPF font family.
        /// </summary>
        public FontFamily FontFamily => _fontFamily;

        public override string Name => _fontFamily.FamilyNames.TryGetValue(_xmlLanguage, out var name) ? name : _fontFamily.FamilyNames.FirstOrDefault().Value;
    }
}