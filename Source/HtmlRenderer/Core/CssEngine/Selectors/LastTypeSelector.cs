namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class LastTypeSelector : ChildSelector
    {
        public LastTypeSelector()
            : base(PseudoClassNames.NthLastOfType)
        {
        }
    }
}