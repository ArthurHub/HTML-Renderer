namespace TheArtOfDev.HtmlRenderer.Core.CssEngine

{
    internal sealed class FirstChildSelector : ChildSelector
    {
        public FirstChildSelector()
            : base(PseudoClassNames.NthChild)
        {
        }
    }
}