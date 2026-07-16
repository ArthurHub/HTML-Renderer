namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AttrMatchSelector : AttrSelectorBase
    {
        public AttrMatchSelector(string attribute, string value)
            : base(attribute, value, $"[{attribute}={value.StylesheetString()}]")
        {
        }
    }
}