namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class PageNameProperty : Property
    {
        internal PageNameProperty() : base(PropertyNames.PageName)
        {
        }

        internal override IValueConverter Converter => IdentifierConverter.OrDefault();
    }
}
