namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderRightColorProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.CurrentColorConverter.OrDefault(Color.Transparent);

        internal BorderRightColorProperty()
            : base(PropertyNames.BorderRightColor)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}