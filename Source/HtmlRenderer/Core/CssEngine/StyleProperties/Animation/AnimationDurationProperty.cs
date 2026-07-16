namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AnimationDurationProperty : Property
    {
        private static readonly IValueConverter
            ListConverter = Converters.TimeConverter.FromList().OrDefault(Time.Zero);

        internal AnimationDurationProperty() : base(PropertyNames.AnimationDuration)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}