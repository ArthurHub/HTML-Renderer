namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StrokeWidthProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter;

        internal StrokeWidthProperty()
            : base(PropertyNames.StrokeWidth, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}