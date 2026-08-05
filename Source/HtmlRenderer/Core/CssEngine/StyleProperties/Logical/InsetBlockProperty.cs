namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class InsetBlockProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.Periodic(
                PropertyNames.Top, PropertyNames.Bottom)
            .OrDefault(Keywords.Auto);

        internal InsetBlockProperty()
            : base(PropertyNames.InsetBlock)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
