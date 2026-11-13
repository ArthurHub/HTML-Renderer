namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MathDepthProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new MathDepthValueConverter().OrDefault();

        internal MathDepthProperty()
            : base(PropertyNames.MathDepth, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
