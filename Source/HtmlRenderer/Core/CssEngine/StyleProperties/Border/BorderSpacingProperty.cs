namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BorderSpacingProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LengthConverter.Many(1, 2).OrDefault(Length.Zero);

        internal BorderSpacingProperty()
            : base(PropertyNames.BorderSpacing, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}