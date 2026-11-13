namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AttrListSelector : AttrSelectorBase
    {
        public AttrListSelector(string attribute, string value)
            : this(attribute, value, AttrCaseSensitivity.Default)
        {
        }

        public AttrListSelector(string attribute, string value, AttrCaseSensitivity caseSensitivity)
            : base(attribute, value, $"[{attribute}~={value.StylesheetString()}{Flag(caseSensitivity)}]", caseSensitivity)
        {
        }
    }
}