namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class VectorEffectProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new KeywordSetValueConverter("none", "non-scaling-stroke", "non-scaling-size", "non-rotation", "fixed-position").OrDefault();

        internal VectorEffectProperty()
            : base(PropertyNames.VectorEffect)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
