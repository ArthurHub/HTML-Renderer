namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MathStyleProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new KeywordSetValueConverter("normal", "compact").OrDefault();

        internal MathStyleProperty()
            : base(PropertyNames.MathStyle, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
