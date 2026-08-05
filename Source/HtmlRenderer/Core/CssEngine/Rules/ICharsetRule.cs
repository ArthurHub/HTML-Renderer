namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface ICharsetRule : IRule
    {
        string CharacterSet { get; set; }
    }
}