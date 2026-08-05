namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AttrListSelector : AttrSelectorBase
    {
        public AttrListSelector(string attribute, string value)
            : base(attribute, value, $"[{attribute}~={value.StylesheetString()}]")
        {
        }
    }
}