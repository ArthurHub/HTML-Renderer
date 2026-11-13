namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextUnderlineOffsetProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LengthOrPercentConverter.OrAuto().OrDefault();

        internal TextUnderlineOffsetProperty()
            : base(PropertyNames.TextUnderlineOffset, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
