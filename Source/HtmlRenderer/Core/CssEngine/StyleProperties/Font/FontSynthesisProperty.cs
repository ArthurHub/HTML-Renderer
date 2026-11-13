namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>font-synthesis</c> shorthand (CSS Fonts 4 3.5) of the four <c>font-synthesis-*</c> longhands.
    /// </summary>
    internal sealed class FontSynthesisProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter =
            new FontSynthesisShorthandValueConverter().OrGlobalValue();

        internal FontSynthesisProperty()
            : base(PropertyNames.FontSynthesis, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
