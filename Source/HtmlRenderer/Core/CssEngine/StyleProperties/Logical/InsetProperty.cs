namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class InsetProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.AutoLengthOrPercentConverter.Periodic(
                PropertyNames.Top, PropertyNames.Right, PropertyNames.Bottom, PropertyNames.Left)
            .OrDefault(Keywords.Auto);

        internal InsetProperty()
            : base(PropertyNames.Inset)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
