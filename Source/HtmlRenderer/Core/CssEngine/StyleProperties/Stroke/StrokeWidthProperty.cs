namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StrokeWidthProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentOrNumberConverter;

        internal StrokeWidthProperty()
            : base(PropertyNames.StrokeWidth, PropertyFlags.Inherited | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}