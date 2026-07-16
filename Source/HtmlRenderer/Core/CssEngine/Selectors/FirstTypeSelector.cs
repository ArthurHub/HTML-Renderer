namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FirstTypeSelector : ChildSelector
    {
        public FirstTypeSelector()
            : base(PseudoClassNames.NthOfType)
        {
        }
    }
}