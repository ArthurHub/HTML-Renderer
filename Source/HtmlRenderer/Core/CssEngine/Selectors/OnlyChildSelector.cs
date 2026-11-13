namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>The <c>:only-child</c> structural pseudo-class.</summary>
    internal sealed class OnlyChildSelector : SelectorBase
    {
        public OnlyChildSelector()
            : base(Priority.OneClass, $"{PseudoClassNames.Separator}{PseudoClassNames.OnlyChild}")
        {
        }
    }
}
