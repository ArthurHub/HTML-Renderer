namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StrokeLinejoinProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.StrokeLinejoinConverter.OrGlobalValue();

        public StrokeLinejoinProperty()
            : base(PropertyNames.StrokeLinejoin, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}