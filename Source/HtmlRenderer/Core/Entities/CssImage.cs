namespace TheArtOfDev.HtmlRenderer.Core.Entities
{
    /// <summary>
    /// The parsed form of a single <c>background-image</c> value: either a <c>url()</c> reference or a
    /// gradient function. Unlike the fuller model this is trimmed down from (same
    /// name for an easy diff), <c>background-image</c> here is a single value, not a comma-separated
    /// layered list - this project only backports enough to replace the old non-standard
    /// "background-gradient" property.
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
