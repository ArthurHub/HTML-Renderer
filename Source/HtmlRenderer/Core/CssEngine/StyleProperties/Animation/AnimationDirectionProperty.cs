namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AnimationDirectionProperty : Property
    {
        private static readonly IValueConverter ListConverter =
            Converters.AnimationDirectionConverter.FromList().OrDefault(AnimationDirection.Normal);

        internal AnimationDirectionProperty()
            : base(PropertyNames.AnimationDirection)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}