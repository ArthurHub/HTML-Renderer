namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class WritingModeProperty : Property
    {
        private static readonly IValueConverter StyleConverter = TextFlowConverters.WritingModeConverter;

        internal WritingModeProperty()
            : base(PropertyNames.WritingMode, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
