namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FloatReferenceProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new KeywordSetValueConverter("inline", "column", "region", "page").OrDefault();

        internal FloatReferenceProperty()
            : base(PropertyNames.FloatReference)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
