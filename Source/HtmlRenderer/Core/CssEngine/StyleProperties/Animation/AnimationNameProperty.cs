namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AnimationNameProperty : Property
    {
        private static readonly IValueConverter ListConverter =
            Converters.AnimationNameConverter.FromList().OrNone().OrDefault();

        internal AnimationNameProperty()
            : base(PropertyNames.AnimationName)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}