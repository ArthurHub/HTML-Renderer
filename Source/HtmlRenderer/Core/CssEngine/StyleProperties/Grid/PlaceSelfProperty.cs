namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>CSS Box Alignment 3 §6.3</summary>
    internal sealed class PlaceSelfProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.PlaceSelfConverter.OrGlobalValue();

        internal PlaceSelfProperty()
            : base(PropertyNames.PlaceSelf)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
