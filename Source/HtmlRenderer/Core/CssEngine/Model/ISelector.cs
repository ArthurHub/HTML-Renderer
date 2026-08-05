namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface ISelector : IStylesheetNode
    {
        Priority Specificity { get; }
        string Text { get; }
    }
}