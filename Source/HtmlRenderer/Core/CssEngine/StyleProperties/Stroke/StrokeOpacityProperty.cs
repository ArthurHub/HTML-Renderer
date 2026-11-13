namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StrokeOpacityProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.OptionalPercentOrNumberConverter;

        internal StrokeOpacityProperty()
            : base(PropertyNames.StrokeOpacity, PropertyFlags.Inherited | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}