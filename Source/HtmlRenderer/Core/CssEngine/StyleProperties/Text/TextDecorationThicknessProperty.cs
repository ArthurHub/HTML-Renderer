namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextDecorationThicknessProperty : Property
    {
        // auto | from-font | <length-percentage>; shared with the text-decoration shorthand.
        internal static readonly IValueConverter ValueConverter =
            Converters.LengthOrPercentConverter.OrAuto().Or("from-font");

        private static readonly IValueConverter StyleConverter = ValueConverter.OrDefault();

        internal TextDecorationThicknessProperty()
            : base(PropertyNames.TextDecorationThickness)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
