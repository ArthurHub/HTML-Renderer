namespace TheArtOfDev.HtmlRenderer.Core.Entities
{
    /// <summary>
    /// The parsed <c>background-image</c>, a <c>url()</c> or a linear gradient (CSS Images 3 §2). Only
    /// one layer.
    /// </summary>
    internal abstract class CssImage
    {
        internal sealed class Url : CssImage
        {
            public Url(string href)
            {
                Href = href;
            }

            public string Href { get; }
        }

        internal sealed class LinearGradient : CssImage
        {
            public LinearGradient(ParsedLinearGradient gradient)
            {
                Gradient = gradient;
            }

            public ParsedLinearGradient Gradient { get; }
        }
    }
}
