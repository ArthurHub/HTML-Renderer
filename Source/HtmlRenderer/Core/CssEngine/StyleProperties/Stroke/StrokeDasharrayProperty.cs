namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StrokeDasharrayProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.StrokeDasharrayConverter.OrGlobalValue();

        public StrokeDasharrayProperty()
            : base(PropertyNames.StrokeDasharray, PropertyFlags.Animatable | PropertyFlags.Unitless)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}