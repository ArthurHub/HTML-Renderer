namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-auto-flow</c>, CSS Grid 2 §7.7.</summary>
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
