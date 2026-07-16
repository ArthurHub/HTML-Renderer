namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IPropertyValue
    {
        string CssText { get; }
        TokenValue Original { get; }
        TokenValue ExtractFor(string name);
    }
}