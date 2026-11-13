namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FillOpacityProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.OptionalPercentOrNumberConverter;

        internal FillOpacityProperty()
            : base(PropertyNames.FillOpacity, PropertyFlags.Inherited | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}