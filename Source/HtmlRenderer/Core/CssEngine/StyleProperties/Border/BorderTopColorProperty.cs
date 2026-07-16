namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderTopColorProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.CurrentColorConverter.OrDefault(Color.Transparent);

        internal BorderTopColorProperty()
            : base(PropertyNames.BorderTopColor)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}