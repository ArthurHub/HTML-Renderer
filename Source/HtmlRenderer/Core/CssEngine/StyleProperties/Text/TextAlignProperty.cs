namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextAlignProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.HorizontalAlignmentConverter.OrDefault(HorizontalAlignment.Left);

        internal TextAlignProperty()
            : base(PropertyNames.TextAlign, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}