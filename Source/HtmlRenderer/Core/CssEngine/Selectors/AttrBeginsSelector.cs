namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AttrBeginsSelector : AttrSelectorBase
    {
        public AttrBeginsSelector(string attribute, string value)
            : this(attribute, value, AttrCaseSensitivity.Default)
        {
        }

        public AttrBeginsSelector(string attribute, string value, AttrCaseSensitivity caseSensitivity)
            : base(attribute, value, $"[{attribute}^={value.StylesheetString()}{Flag(caseSensitivity)}]", caseSensitivity)
        {
        }
    }
}