namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StrokeMiterlimitProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.StrokeMiterlimitConverter.OrGlobalValue();

        public StrokeMiterlimitProperty()
            : base(PropertyNames.StrokeMiterlimit, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}