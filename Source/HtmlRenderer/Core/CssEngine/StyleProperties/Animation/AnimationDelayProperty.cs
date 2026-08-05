namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AnimationDelayProperty : Property
    {
        private static readonly IValueConverter
            ListConverter = Converters.TimeConverter.FromList().OrDefault(Time.Zero);

        internal AnimationDelayProperty()
            : base(PropertyNames.AnimationDelay)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}