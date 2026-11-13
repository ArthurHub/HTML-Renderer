namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>The <c>justify-items</c> property (CSS Box Alignment): default inline-axis alignment of grid items.</summary>
    internal sealed class JustifyItemsProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.JustifyItemsConverter;

        internal JustifyItemsProperty()
            : base(PropertyNames.JustifyItems)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
