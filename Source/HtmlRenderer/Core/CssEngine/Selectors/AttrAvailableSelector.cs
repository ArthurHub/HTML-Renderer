namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AttrAvailableSelector : AttrSelectorBase
    {
        public AttrAvailableSelector(string attribute, string value)
            : base(attribute, value, $"[{attribute}]")
        {
        }
    }
}