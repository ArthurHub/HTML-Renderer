namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OnlyChildSelector : SelectorBase
    {
        public OnlyChildSelector()
            : base(Priority.OneClass, $"{PseudoClassNames.Separator}{PseudoClassNames.OnlyChild}")
        {
        }
    }
}
