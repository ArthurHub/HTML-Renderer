namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal abstract class AttrSelectorBase : SelectorBase, IAttrSelector
    {
        protected AttrSelectorBase(string attribute, string value, string text)
            : this(attribute, value, text, AttrCaseSensitivity.Default)
        {
        }

        protected AttrSelectorBase(string attribute, string value, string text, AttrCaseSensitivity caseSensitivity)
            : base(Priority.OneClass, text)
        {
            Attribute = attribute;
            Value = value;
            CaseSensitivity = caseSensitivity;
        }

        public string Attribute { get; }
        public string Value { get; }
        public AttrCaseSensitivity CaseSensitivity { get; }

        internal static string Flag(AttrCaseSensitivity caseSensitivity)
        {
            return caseSensitivity switch
            {
                AttrCaseSensitivity.Insensitive => " i",
                AttrCaseSensitivity.Sensitive => " s",
                _ => string.Empty
            };
        }
    }
}