namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AlignItemsProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.AlignItemsConverter
                                                                           .OrDefault(Keywords.Normal);

        internal AlignItemsProperty()
            : base(PropertyNames.AlignItems)
        { }

        internal override IValueConverter Converter => StyleConverter;
    }
}