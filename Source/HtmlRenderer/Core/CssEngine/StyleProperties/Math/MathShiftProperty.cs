namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MathShiftProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new KeywordSetValueConverter("normal", "compact").OrDefault();

        internal MathShiftProperty()
            : base(PropertyNames.MathShift, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
