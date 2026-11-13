namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextAlignAllProperty : Property
    {
        private static readonly IValueConverter StyleConverter = TextFlowConverters.TextAlignAllConverter;

        internal TextAlignAllProperty()
            : base(PropertyNames.TextAlignAll, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
