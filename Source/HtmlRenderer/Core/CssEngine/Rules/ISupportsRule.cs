namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface ISupportsRule : IConditionRule
    {
        IConditionFunction Condition { get; }
    }
}