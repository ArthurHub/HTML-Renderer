namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class VisibilityProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.VisibilityConverter.OrDefault(Visibility.Visible);

        internal VisibilityProperty()
            : base(PropertyNames.Visibility, PropertyFlags.Inherited | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}