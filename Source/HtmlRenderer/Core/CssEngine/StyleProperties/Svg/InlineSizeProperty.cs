namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class InlineSizeProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.OrDefault();

        internal InlineSizeProperty()
            : base(PropertyNames.InlineSize, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
