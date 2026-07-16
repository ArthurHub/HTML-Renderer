namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BreakInsideProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.BreakInsideModeConverter.OrDefault(BreakMode.Auto);

        internal BreakInsideProperty()
            : base(PropertyNames.BreakInside)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}