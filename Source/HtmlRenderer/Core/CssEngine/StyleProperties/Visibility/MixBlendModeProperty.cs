namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MixBlendModeProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Map.BlendModes.ToConverter().OrDefault(BlendMode.Normal);

        internal MixBlendModeProperty()
            : base(PropertyNames.MixBlendMode, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
