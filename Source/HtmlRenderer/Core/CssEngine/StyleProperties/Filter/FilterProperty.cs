namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FilterProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new FilterValueConverter().OrDefault();

        internal FilterProperty()
            : base(PropertyNames.Filter, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
