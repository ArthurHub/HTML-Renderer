namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BackgroundBlendModeProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Map.BlendModes.ToConverter().FromList().OrDefault(BlendMode.Normal);

        internal BackgroundBlendModeProperty()
            : base(PropertyNames.BackgroundBlendMode)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
