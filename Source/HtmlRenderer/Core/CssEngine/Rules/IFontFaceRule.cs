namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IFontFaceRule : IRule, IProperties
    {
        string Family { get; set; }
        string Source { get; set; }
        string Style { get; set; }
        string Weight { get; set; }
        string Stretch { get; set; }
        string Range { get; set; }
        string Variant { get; set; }
        string Features { get; set; }
    }
}