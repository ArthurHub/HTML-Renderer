namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextTransformProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.TextTransformConverter.OrDefault(TextTransform.None);

        internal TextTransformProperty()
            : base(PropertyNames.TextTransform, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}