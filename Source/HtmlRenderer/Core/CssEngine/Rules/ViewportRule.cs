namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ViewportRule : DeclarationRule
    {
        internal ViewportRule(StylesheetParser parser)
            : base(RuleType.Viewport, RuleNames.ViewPort, parser)
        {
        }

        protected override Property CreateNewProperty(string name)
        {
            return PropertyFactory.Instance.CreateViewport(name);
        }
    }
}