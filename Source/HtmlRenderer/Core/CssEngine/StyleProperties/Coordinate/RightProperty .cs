namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class RightProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.AutoLengthOrPercentConverter.OrDefault(Keywords.Auto);

        internal RightProperty()
            : base(PropertyNames.Right, PropertyFlags.Unitless | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}