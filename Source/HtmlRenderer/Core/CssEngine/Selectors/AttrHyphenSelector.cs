namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AttrHyphenSelector : AttrSelectorBase
    {
        public AttrHyphenSelector(string attribute, string value)
            : this(attribute, value, AttrCaseSensitivity.Default)
        {
        }

        public AttrHyphenSelector(string attribute, string value, AttrCaseSensitivity caseSensitivity)
            : base(attribute, value, $"[{attribute}|={value.StylesheetString()}{Flag(caseSensitivity)}]", caseSensitivity)
        {
        }
    }
}