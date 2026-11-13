namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FootnoteDisplayProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new KeywordSetValueConverter("block", "inline", "compact").OrDefault();

        internal FootnoteDisplayProperty()
            : base(PropertyNames.FootnoteDisplay)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
