namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>The <c>place-items</c> shorthand (CSS Box Alignment): <c>&lt;align-items&gt; &lt;justify-items&gt;?</c>.</summary>
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
