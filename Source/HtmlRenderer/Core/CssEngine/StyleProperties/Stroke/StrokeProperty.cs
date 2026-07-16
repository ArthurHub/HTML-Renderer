namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StrokeProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.PaintConverter;

        internal StrokeProperty()
            : base(PropertyNames.Stroke, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}