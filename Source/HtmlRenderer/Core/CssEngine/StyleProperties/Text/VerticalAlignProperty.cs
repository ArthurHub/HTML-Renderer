namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class VerticalAlignProperty : Property
    {
        private static readonly IValueConverter StyleConverter = LengthOrPercentConverter.Or(
            VerticalAlignmentConverter).OrDefault(VerticalAlignment.Baseline);

        internal VerticalAlignProperty()
            : base(PropertyNames.VerticalAlign, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}