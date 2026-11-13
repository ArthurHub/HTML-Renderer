namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AttrEndsSelector : AttrSelectorBase
    {
        public AttrEndsSelector(string attribute, string value)
            : this(attribute, value, AttrCaseSensitivity.Default)
        {
        }

        public AttrEndsSelector(string attribute, string value, AttrCaseSensitivity caseSensitivity)
            : base(attribute, value, $"[{attribute}$={value.StylesheetString()}{Flag(caseSensitivity)}]", caseSensitivity)
        {
        }
    }
}