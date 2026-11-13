namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class IsolationProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new KeywordSetValueConverter("auto", "isolate").OrDefault();

        internal IsolationProperty()
            : base(PropertyNames.Isolation)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
