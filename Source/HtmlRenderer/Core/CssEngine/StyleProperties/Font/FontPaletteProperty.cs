namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>font-palette</c> property (CSS Fonts Module Level 4): selects a color palette for a COLR/CPAL
    /// color font. Inherited. The value grammar (<c>normal | light | dark | &lt;dashed-ident&gt;</c>) is
    /// validated by <see cref="FontPaletteValueConverter"/>; the authored text is preserved verbatim.
    /// </summary>
    internal sealed class FontPaletteProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new FontPaletteValueConverter().OrDefault();

        internal FontPaletteProperty()
            : base(PropertyNames.FontPalette, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
