namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>The <c>place-self</c> shorthand (CSS Box Alignment): <c>&lt;align-self&gt; &lt;justify-self&gt;?</c>.</summary>
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
