namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IPageRule : IRule
    {
        string SelectorText { get; set; }
        StyleDeclaration Style { get; }
    }
}