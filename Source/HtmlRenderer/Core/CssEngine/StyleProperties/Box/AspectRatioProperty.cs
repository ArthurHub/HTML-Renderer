namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>aspect-ratio</c> property (CSS Box Sizing Level 4). The value is validated by the shared
    /// <see cref="AspectRatioGrammar"/> and the authored text preserved for the layout engine, which computes
    /// an auto dimension from the definite one.
    /// </summary>
    internal sealed class AspectRatioProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new AspectRatioValueConverter().OrDefault();

        internal AspectRatioProperty()
            : base(PropertyNames.AspectRatio)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
