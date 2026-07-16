namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ClearProperty : Property
    {
        private static readonly IValueConverter
            StyleConverter = Converters.ClearModeConverter.OrDefault(ClearMode.None);

        internal ClearProperty()
            : base(PropertyNames.Clear)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}