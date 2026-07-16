namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BackfaceVisibilityProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.BackfaceVisibilityConverter.OrDefault(true);

        internal BackfaceVisibilityProperty()
            : base(PropertyNames.BackfaceVisibility)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}