namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StrokeDashoffsetProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter.OrGlobalValue();

        public StrokeDashoffsetProperty()
            : base(PropertyNames.StrokeDashoffset, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}