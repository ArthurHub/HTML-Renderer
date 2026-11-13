namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// A feature-value entry inside a <c>@styleset</c>/<c>@swash</c>/etc. block: an author-chosen name
    /// whose value is one or more non-negative integers (<c>&lt;integer [0,inf]&gt;+</c>).
    /// </summary>
    internal sealed class FontFeatureValueProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.NaturalIntegerConverter.Many();

        internal FontFeatureValueProperty(string name)
            : base(name)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
