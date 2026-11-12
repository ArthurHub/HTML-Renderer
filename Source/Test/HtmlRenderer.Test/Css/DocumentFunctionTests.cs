using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class DocumentFunctionTests
{
    [TestMethod]
    public void CssDocumentRuleSingleUrlFunction()
    {
        var snippet = "@document url(http://www.w3.org/) { }";
        var rule = ParseRule(snippet) as DocumentRule;
        Assert.IsNotNull(rule);
        Assert.AreEqual(RuleType.Document, rule.Type);
        Assert.AreEqual(1, rule.Conditions.Count());
        var condition = rule.Conditions.First();
        Assert.AreEqual("url", condition.Name);
        Assert.AreEqual("http://www.w3.org/", condition.Data);
    }

    [TestMethod]
    public void CssDocumentRuleSingleUrlPrefixFunction()
    {
        var snippet = "@document url-prefix(http://www.w3.org/Style/) { }";
        var rule = ParseRule(snippet) as DocumentRule;
        Assert.IsNotNull(rule);
        Assert.AreEqual(RuleType.Document, rule.Type);
        Assert.AreEqual(1, rule.Conditions.Count());
        var condition = rule.Conditions.First();
        Assert.AreEqual("url-prefix", condition.Name);
        Assert.AreEqual("http://www.w3.org/Style/", condition.Data);
    }

    [TestMethod]
    public void CssDocumentRuleSingleDomainFunction()
    {
        var snippet = "@document domain('mozilla.org') { }";
        var rule = ParseRule(snippet) as DocumentRule;
        Assert.IsNotNull(rule);
        Assert.AreEqual(RuleType.Document, rule.Type);
        Assert.AreEqual(1, rule.Conditions.Count());
        var condition = rule.Conditions.First();
        Assert.AreEqual("domain", condition.Name);
        Assert.AreEqual("mozilla.org", condition.Data);
    }

    [TestMethod]
    public void CssDocumentRuleSingleRegexpFunction()
    {
        var snippet = "@document regexp(\"https:.*\") { }";
        var rule = ParseRule(snippet) as DocumentRule;
        Assert.IsNotNull(rule);
        Assert.AreEqual(RuleType.Document, rule.Type);
        Assert.AreEqual(1, rule.Conditions.Count());
        var condition = rule.Conditions.First();
        Assert.AreEqual("regexp", condition.Name);
        Assert.AreEqual("https:.*", condition.Data);
    }

    [TestMethod]
    public void CssDocumentRuleMultipleFunctions()
    {
        var snippet = "@document url(http://www.w3.org/), url-prefix(http://www.w3.org/Style/), domain(mozilla.org), regexp(\"https:.*\") { }";
        var rule = ParseRule(snippet) as DocumentRule;
        Assert.IsNotNull(rule);
        Assert.AreEqual(RuleType.Document, rule.Type);
        Assert.AreEqual(4, rule.Conditions.Count());
    }
}
