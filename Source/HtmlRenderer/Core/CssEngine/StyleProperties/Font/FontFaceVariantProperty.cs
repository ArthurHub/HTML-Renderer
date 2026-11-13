namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>font-variant</c> descriptor of <c>@font-face</c>: a flat declaration restricted to
    /// <c>normal | small-caps</c>. It cannot reuse <see cref="FontVariantProperty"/>, which is the
    /// <c>font-variant</c> shorthand of the style properties.
    /// </summary>
    internal sealed class FontFaceVariantProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.FontVariantConverter.OrDefault(FontVariant.Normal);

        internal FontFaceVariantProperty()
            : base(PropertyNames.FontVariant, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
