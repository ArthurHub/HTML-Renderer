namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextWrapStyleProperty : Property
    {
        private static readonly IValueConverter StyleConverter = TextFlowConverters.TextWrapStyleConverter;

        internal TextWrapStyleProperty()
            : base(PropertyNames.TextWrapStyle, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
