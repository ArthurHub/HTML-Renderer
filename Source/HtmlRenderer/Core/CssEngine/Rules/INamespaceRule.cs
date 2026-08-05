namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface INamespaceRule : IRule
    {
        string NamespaceUri { get; set; }
        string Prefix { get; set; }
    }
}