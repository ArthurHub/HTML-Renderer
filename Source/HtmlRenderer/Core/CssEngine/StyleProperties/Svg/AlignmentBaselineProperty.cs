namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AlignmentBaselineProperty : Property
    {
        private static readonly IValueConverter StyleConverter = SvgConverters.AlignmentBaselineConverter.OrDefault();

        internal AlignmentBaselineProperty()
            : base(PropertyNames.AlignBaseline, PropertyFlags.None)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
