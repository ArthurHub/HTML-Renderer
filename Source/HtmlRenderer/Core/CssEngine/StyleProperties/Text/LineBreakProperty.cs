namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class LineBreakProperty : Property
    {
        private static readonly IValueConverter StyleConverter = TextFlowConverters.LineBreakConverter;

        internal LineBreakProperty()
            : base(PropertyNames.LineBreak, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
