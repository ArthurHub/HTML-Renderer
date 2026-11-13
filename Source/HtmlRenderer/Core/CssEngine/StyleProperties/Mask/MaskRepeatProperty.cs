namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MaskRepeatProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.BackgroundRepeatsConverter.FromList().OrDefault();

        internal MaskRepeatProperty()
            : base(PropertyNames.MaskRepeat)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
