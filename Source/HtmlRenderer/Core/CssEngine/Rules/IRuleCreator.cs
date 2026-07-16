namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IRuleCreator
    {
        IRule AddNewRule(RuleType ruleType);
    }
}