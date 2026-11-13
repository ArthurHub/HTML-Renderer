namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>The <c>:only-of-type</c> structural pseudo-class.</summary>
    internal sealed class OnlyOfTypeSelector : SelectorBase
    {
        public OnlyOfTypeSelector()
            : base(Priority.OneClass, $"{PseudoClassNames.Separator}{PseudoClassNames.OnlyType}")
        {
        }
    }
}
