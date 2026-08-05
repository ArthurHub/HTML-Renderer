namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BackgroundColorProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.CurrentColorConverter.OrDefault();

        internal BackgroundColorProperty()
            : base(PropertyNames.BackgroundColor, PropertyFlags.Hashless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}