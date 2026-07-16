namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IImportRule : IRule
    {
        string Href { get; set; }
        MediaList Media { get; }
    }
}