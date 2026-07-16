namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderImageRepeatProperty : Property
    {
        internal static readonly IValueConverter TheConverter = Map.BorderRepeatModes.ToConverter().Many(1, 2);
        private static readonly IValueConverter StyleConverter = TheConverter.OrDefault(BorderRepeat.Stretch);

        internal BorderImageRepeatProperty()
            : base(PropertyNames.BorderImageRepeat)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}