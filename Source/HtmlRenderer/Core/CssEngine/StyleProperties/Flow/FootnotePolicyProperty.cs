namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FootnotePolicyProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new KeywordSetValueConverter("auto", "line", "block").OrDefault();

        internal FootnotePolicyProperty()
            : base(PropertyNames.FootnotePolicy)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
