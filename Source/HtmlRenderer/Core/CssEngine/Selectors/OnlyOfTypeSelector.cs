namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OnlyOfTypeSelector : SelectorBase
    {
        public OnlyOfTypeSelector()
            : base(Priority.OneClass, $"{PseudoClassNames.Separator}{PseudoClassNames.OnlyType}")
        {
        }
    }
}
