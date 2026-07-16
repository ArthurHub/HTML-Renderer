namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BreakAfterProperty : Property
    {
        private static readonly IValueConverter
            StyleConverter = Converters.BreakModeConverter.OrDefault(BreakMode.Auto);

        internal BreakAfterProperty()
            : base(PropertyNames.BreakAfter)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}