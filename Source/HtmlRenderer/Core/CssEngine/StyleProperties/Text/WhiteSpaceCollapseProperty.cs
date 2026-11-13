namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class WhiteSpaceCollapseProperty : Property
    {
        private static readonly IValueConverter StyleConverter = TextFlowConverters.WhiteSpaceCollapseConverter;

        internal WhiteSpaceCollapseProperty()
            : base(PropertyNames.WhiteSpaceCollapse, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
