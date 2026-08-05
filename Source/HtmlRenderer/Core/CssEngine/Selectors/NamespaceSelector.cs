namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class NamespaceSelector : SelectorBase
    {
        private NamespaceSelector(string prefix) : base(Priority.Zero, prefix)
        {
        }

        public static NamespaceSelector Create(string prefix)
        {
            return new NamespaceSelector(prefix);
        }
    }
}