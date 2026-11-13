namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextWrapProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = TextFlowConverters.TextWrapConverter;

        internal TextWrapProperty()
            : base(PropertyNames.TextWrap, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
