namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TextCombineUprightProperty : Property
    {
        private static readonly IValueConverter StyleConverter = TextFlowConverters.TextCombineUprightConverter;

        internal TextCombineUprightProperty()
            : base(PropertyNames.TextCombineUpright, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
