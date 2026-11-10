namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>CSS Box Alignment 3 §7.3</summary>
    internal sealed class PlaceItemsProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.PlaceItemsConverter.OrGlobalValue();

        internal PlaceItemsProperty()
            : base(PropertyNames.PlaceItems)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
