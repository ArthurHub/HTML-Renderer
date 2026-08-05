namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class LastChildSelector : ChildSelector
    {
        public LastChildSelector()
            : base(PseudoClassNames.NthLastChild)
        {
        }
    }
}