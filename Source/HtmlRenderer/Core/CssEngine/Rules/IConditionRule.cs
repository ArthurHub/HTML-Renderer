namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IConditionRule : IGroupingRule
    {
        string ConditionText { get; set; }
    }
}