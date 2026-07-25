namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class InsetInlineProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.Periodic(
                PropertyNames.Left, PropertyNames.Right)
            .OrDefault(Keywords.Auto);

        internal InsetInlineProperty()
            : base(PropertyNames.InsetInline)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
