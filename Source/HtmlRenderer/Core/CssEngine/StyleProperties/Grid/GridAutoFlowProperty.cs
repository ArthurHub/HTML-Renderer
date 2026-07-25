namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>The <c>grid-auto-flow</c> property (CSS Grid §7.7): the auto-placement direction and packing.</summary>
    internal sealed class GridAutoFlowProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new GridAutoFlowValueConverter().OrDefault();

        internal GridAutoFlowProperty()
            : base(PropertyNames.GridAutoFlow)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
