namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IAttrSelector : ISelector
    {
        string Attribute { get; }
        string Value { get; }
    }
}