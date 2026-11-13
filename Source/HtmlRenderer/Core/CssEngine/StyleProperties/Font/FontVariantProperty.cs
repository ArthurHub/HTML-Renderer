namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>font-variant</c> shorthand (CSS Fonts 4 6): <c>normal | none | [ &lt;caps&gt; || &lt;ligatures&gt;
    /// || &lt;alternates&gt; || &lt;numeric&gt; || &lt;east-asian&gt; || &lt;position&gt; || &lt;emoji&gt; ]</c>, expanding to
    /// the seven <c>font-variant-*</c> longhands.
    /// </summary>
    internal sealed class FontVariantProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter =
            new FontVariantShorthandValueConverter().OrGlobalValue();

        internal FontVariantProperty()
            : base(PropertyNames.FontVariant, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}