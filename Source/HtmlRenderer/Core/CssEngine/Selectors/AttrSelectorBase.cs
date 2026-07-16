namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal abstract class AttrSelectorBase : SelectorBase, IAttrSelector
    {
        protected AttrSelectorBase(string attribute, string value, string text) : base(Priority.OneClass, text)
        {
            Attribute = attribute;
            Value = value;
        }
        public string Attribute { get; }
        public string Value { get; }
    }
}