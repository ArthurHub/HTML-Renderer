namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>CSS Box Alignment 3 §7.1</summary>
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
