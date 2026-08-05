namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IMarginRule : IRule
    {
        string Name { get; }
        StyleDeclaration Style { get; }
    }
}