namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextWrapModeProperty : Property
    {
        private static readonly IValueConverter StyleConverter = TextFlowConverters.TextWrapModeConverter;

        internal TextWrapModeProperty()
            : base(PropertyNames.TextWrapMode, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
