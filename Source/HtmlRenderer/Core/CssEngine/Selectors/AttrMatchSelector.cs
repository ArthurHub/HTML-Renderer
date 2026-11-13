namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AttrMatchSelector : AttrSelectorBase
    {
        public AttrMatchSelector(string attribute, string value)
            : this(attribute, value, AttrCaseSensitivity.Default)
        {
        }

        public AttrMatchSelector(string attribute, string value, AttrCaseSensitivity caseSensitivity)
            : base(attribute, value, $"[{attribute}={value.StylesheetString()}{Flag(caseSensitivity)}]", caseSensitivity)
        {
        }
    }
}