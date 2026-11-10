namespace TheArtOfDev.HtmlRenderer.Core.Utils
{
    /// <summary>
    /// Resolves a <c>font-stretch</c> keyword (CSS Fonts 4 §2.3) to the 1-9 scale of the OpenType OS/2
    /// <c>usWidthClass</c>, 1 is ultra-condensed, 5 normal and 9 ultra-expanded.
    /// </summary>
    internal static class FontStretchResolver
    {
        internal const int Normal = 5;

        /// <summary>
        /// Anything that isn't a width keyword resolves to normal.
        /// </summary>
        internal static int Resolve(string fontStretchValue) => fontStretchValue switch
        {
            CssConstants.UltraCondensed => 1,
            CssConstants.ExtraCondensed => 2,
            CssConstants.Condensed => 3,
            CssConstants.SemiCondensed => 4,
            CssConstants.SemiExpanded => 6,
            CssConstants.Expanded => 7,
            CssConstants.ExtraExpanded => 8,
            CssConstants.UltraExpanded => 9,
            _ => Normal
        };
    }
}
