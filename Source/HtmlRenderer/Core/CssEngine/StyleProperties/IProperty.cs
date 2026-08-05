namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IProperty : IStylesheetNode
    {
        string Name { get; }
        string Value { get; }
        string Original { get; }
        bool IsImportant { get; }
    }
}