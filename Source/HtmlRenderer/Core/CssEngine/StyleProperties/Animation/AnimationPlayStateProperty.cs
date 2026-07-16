namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AnimationPlayStateProperty : Property
    {
        private static readonly IValueConverter ListConverter =
            Converters.PlayStateConverter.FromList().OrDefault(PlayState.Running);

        internal AnimationPlayStateProperty()
            : base(PropertyNames.AnimationPlayState)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}