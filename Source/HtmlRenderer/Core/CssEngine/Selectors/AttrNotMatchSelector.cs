namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AttrNotMatchSelector : AttrSelectorBase
    {
        public AttrNotMatchSelector(string attribute, string value)
            : base(attribute, value, $"[{attribute}!={value.StylesheetString()}]")
        {
        }
    }
}