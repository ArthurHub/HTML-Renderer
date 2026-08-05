namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal abstract class ConditionRule : GroupingRule
    {
        internal ConditionRule(RuleType type, StylesheetParser parser)
            : base(type, parser)
        {
        }
    }
}