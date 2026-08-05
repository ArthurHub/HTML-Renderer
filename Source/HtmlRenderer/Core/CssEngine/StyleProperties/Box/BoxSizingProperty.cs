namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal class BoxSizingProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.BoxSizingConverter.OrDefault(Keywords.ContentBox);

        public BoxSizingProperty()
            : base(PropertyNames.BoxSizing, PropertyFlags.None)
        { }

        internal override IValueConverter Converter => StyleConverter;
    }
}
