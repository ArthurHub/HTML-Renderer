namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>The <c>place-content</c> shorthand (CSS Box Alignment): <c>&lt;align-content&gt; &lt;justify-content&gt;?</c>.</summary>
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
