using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssConditional4Tests
{
    [TestMethod]
    [Ignore("Gap, see Spec/css-conditional-4.yml")]
    [Spec("css-conditional-4", "#at-supports-ext")]
    [DataRow("selector(p)")]
    [DataRow("selector(p > a)")]
    public void SelectorFunction_IsTrueForASupportedSelector(string condition)
    {
        var rule = Supports(condition);

        Assert.IsNotNull(rule, "The rule is valid");
        Assert.IsTrue(rule.Condition.Check());
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-conditional-4.yml")]
    [Spec("css-conditional-4", "#support-definition-ext")]
    [DataRow("selector(p:foo)")]
    [DataRow("selector(p::-webkit-foo)")]
    public void SelectorFunction_IsFalseForAnUnsupportedSelector(string condition)
    {
        var rule = Supports(condition);

        Assert.IsNotNull(rule, "The rule is valid");
        Assert.IsFalse(rule.Condition.Check());
    }

    private static SupportsRule Supports(string condition)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@supports " + condition + " { }");
        return sheet.Rules.Length == 1 ? (SupportsRule)sheet.Rules[0] : null!;
    }
}
