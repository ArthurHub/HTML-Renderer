using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class AtPropertyTests
{
    [TestMethod]
    public void AtProperty_ParsesNameAndDescriptors()
    {
        var src = "@property --my-color { syntax: \"<color>\"; inherits: false; initial-value: #c0ffee; }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(src);

        Assert.IsNotNull(sheet);
        var rule = sheet.Rules.OfType<PropertyRule>().Single();
        Assert.AreEqual("--my-color", rule.Name);
        StringAssert.Contains(rule.Syntax, "color");
        Assert.AreEqual("false", rule.Inherits);
        Assert.AreEqual("#c0ffee", rule.InitialValue);
    }

    [TestMethod]
    public void AtProperty_UniversalSyntax_NoInitialValue()
    {
        var src = "@property --x { syntax: \"*\"; inherits: true; }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(src);

        var rule = sheet.Rules.OfType<PropertyRule>().Single();
        Assert.AreEqual("--x", rule.Name);
        StringAssert.Contains(rule.Syntax, "*");
        Assert.AreEqual("true", rule.Inherits);
        Assert.AreEqual("", rule.InitialValue);
    }

    [TestMethod]
    public void AtProperty_DoesNotDerailFollowingRules()
    {
        // Regression: an @property rule must not swallow or drop the rules that follow it. Before real
        // @property parsing, the rule routed to CreateUnknown and (the whole point of this feature) was
        // silently dropped; the following style rule must still parse and apply.
        var src = "@property --gap { syntax: \"<length>\"; inherits: false; initial-value: 4px; } .after { color: red; }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(src);

        Assert.AreEqual(1, sheet.Rules.OfType<PropertyRule>().Count());
        var styleRule = sheet.Rules.OfType<StyleRule>().Single();
        Assert.AreEqual(".after", styleRule.SelectorText);
        // Named colors are normalized to rgb() at parse time.
        Assert.AreEqual("rgb(255, 0, 0)", styleRule.Style.GetPropertyValue("color"));
    }

    [TestMethod]
    public void AtProperty_NoDeclarationBlock_DoesNotCrashAndFollowingRuleApplies()
    {
        // A malformed @property with no { } block must be skipped without derailing the next rule.
        var src = "@property --x; .after { color: red; }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(src);

        var styleRule = sheet.Rules.OfType<StyleRule>().Single();
        Assert.AreEqual(".after", styleRule.SelectorText);
        Assert.AreEqual("rgb(255, 0, 0)", styleRule.Style.GetPropertyValue("color"));
    }

    [TestMethod]
    public void AtProperty_Setters_And_ToCss_RoundTrip()
    {
        // Start from a rule that only declares `syntax`, so setting initial-value/inherits exercises the
        // create-new-descriptor path, and re-setting syntax exercises the replace-existing path.
        var src = "@property --p { syntax: \"<length>\"; }";
        var rule = (PropertyRule)CssConstructionFunctions.ParseStyleSheet(src).Rules.OfType<PropertyRule>().Single();

        rule.InitialValue = "1px";              // new descriptor
        rule.Inherits = "true";                 // new descriptor
        rule.Syntax = "\"<color>\"";            // replace existing descriptor
        StringAssert.Contains(rule.Syntax, "color");
        Assert.AreEqual("1px", rule.InitialValue);
        Assert.AreEqual("true", rule.Inherits);

        var css = rule.ToCss();
        StringAssert.Contains(css, "@property --p");
        StringAssert.Contains(css, "initial-value");
    }

    [TestMethod]
    public void AtProperty_MultipleRules_AllRegister()
    {
        var src = "@property --a { syntax: \"<number>\"; inherits: false; initial-value: 0; }" +
                  "@property --b { syntax: \"<percentage>\"; inherits: true; initial-value: 50%; }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(src);

        var rules = sheet.Rules.OfType<PropertyRule>().ToList();
        Assert.AreEqual(2, rules.Count);
        Assert.AreEqual("--a", rules[0].Name);
        Assert.AreEqual("--b", rules[1].Name);
    }
}
