namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextAlignProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = TextFlowConverters.TextAlignConverter;

        internal TextAlignProperty()
            : base(PropertyNames.TextAlign, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}