namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AttrHyphenSelector : AttrSelectorBase
    {
        public AttrHyphenSelector(string attribute, string value)
            : base(attribute, value, $"[{attribute}|={value.StylesheetString()}]")
        {
        }
    }
}