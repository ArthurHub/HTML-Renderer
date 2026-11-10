namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>CSS Box Alignment 3 §5.2</summary>
    internal sealed class PlaceContentProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.PlaceContentConverter.OrGlobalValue();

        internal PlaceContentProperty()
            : base(PropertyNames.PlaceContent)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
