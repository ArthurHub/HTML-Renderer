namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class InsetInlineProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.Periodic(
                PropertyNames.InsetInlineStart, PropertyNames.InsetInlineEnd)
            .OrDefault(Keywords.Auto);

        internal InsetInlineProperty()
            : base(PropertyNames.InsetInline)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
