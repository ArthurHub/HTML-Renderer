namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TransitionDurationProperty : Property
    {
        private static readonly IValueConverter
            ListConverter = Converters.TimeConverter.FromList().OrDefault(Time.Zero);

        internal TransitionDurationProperty()
            : base(PropertyNames.TransitionDuration)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}