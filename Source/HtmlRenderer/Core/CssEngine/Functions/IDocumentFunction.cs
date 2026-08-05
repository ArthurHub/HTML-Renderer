namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IDocumentFunction : IStylesheetNode
    {
        string Name { get; }
        string Data { get; }
    }
}