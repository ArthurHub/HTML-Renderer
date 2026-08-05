namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IMediaFeature : IStylesheetNode
    {
        string Name { get; }
        string Value { get; }
        bool HasValue { get; }
    }
}