namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class LastColumnSelector : ChildSelector
    {
        public LastColumnSelector()
            : base(PseudoClassNames.NthLastColumn)
        {
        }
    }
}