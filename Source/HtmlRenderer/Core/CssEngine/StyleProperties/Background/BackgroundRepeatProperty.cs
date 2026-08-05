namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BackgroundRepeatProperty : Property
    {
        private static readonly IValueConverter ListConverter =
            Converters.BackgroundRepeatsConverter.FromList().OrDefault(BackgroundRepeat.Repeat);

        internal BackgroundRepeatProperty()
            : base(PropertyNames.BackgroundRepeat)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}