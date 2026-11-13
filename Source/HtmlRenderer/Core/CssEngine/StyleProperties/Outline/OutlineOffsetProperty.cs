namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OutlineOffsetProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.LengthConverter.OrDefault(Length.Zero);

        internal OutlineOffsetProperty()
            : base(PropertyNames.OutlineOffset, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
