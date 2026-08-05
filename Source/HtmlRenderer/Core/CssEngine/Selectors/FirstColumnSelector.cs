namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FirstColumnSelector : ChildSelector
    {
        public FirstColumnSelector()
            : base(PseudoClassNames.NthColumn)
        {
        }
    }
}