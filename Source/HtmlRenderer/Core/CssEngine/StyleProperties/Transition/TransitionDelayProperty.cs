namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TransitionDelayProperty : Property
    {
        private static readonly IValueConverter
            ListConverter = Converters.TimeConverter.FromList().OrDefault(Time.Zero);

        internal TransitionDelayProperty()
            : base(PropertyNames.TransitionDelay)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}