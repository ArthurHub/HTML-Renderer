using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class ContainerTests
{
    [TestMethod]
    public void SimpleContainer()
    {
        const string source = "@container tall (min-width: 500px) and (min-height: 300px) {h2 { line-height: 1.6; } }";
        var result = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(source, result.StylesheetText.Text);
        var rule = result.Rules[0] as ContainerRule;
        Assert.IsNotNull(rule);
        Assert.AreEqual("@container tall (min-width: 500px) and (min-height: 300px) { h2 { line-height: 1.6 } }", rule.Text);
        Assert.AreEqual("tall", rule.Name);
        Assert.AreEqual("(min-width: 500px) and (min-height: 300px)", rule.ConditionText);
        var childRule = rule.Children.OfType<StyleRule>().First();
        Assert.AreEqual("h2 { line-height: 1.6 }", childRule.ToCss());
    }

    [TestMethod]
    public void ContainerWithoutName()
    {
        const string source = "@container (min-width: 500px) and (min-height: 300px) {h2 { line-height: 1.6; } }";
        var result = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(source, result.StylesheetText.Text);
        var rule = result.Rules[0] as ContainerRule;
        Assert.IsNotNull(rule);
        Assert.AreEqual("@container (min-width: 500px) and (min-height: 300px) { h2 { line-height: 1.6 } }", rule.Text);
        Assert.AreEqual(string.Empty, rule.Name);
        Assert.AreEqual("(min-width: 500px) and (min-height: 300px)", rule.ConditionText);
        var childRule = rule.Children.OfType<StyleRule>().First();
        Assert.AreEqual("h2 { line-height: 1.6 }", childRule.ToCss());
    }

    [TestMethod]
    public void ContainerWithoutCondition()
    {
        const string source = "@container tall {h2 { line-height: 1.6; } }";
        var result = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(source, result.StylesheetText.Text);
        var rule = result.Rules[0] as ContainerRule;
        Assert.IsNotNull(rule);
        Assert.AreEqual("@container tall { h2 { line-height: 1.6 } }", rule.Text);
        Assert.AreEqual("tall", rule.Name);
        Assert.AreEqual(string.Empty, rule.ConditionText);
        var childRule = rule.Children.OfType<StyleRule>().First();
        Assert.AreEqual("h2 { line-height: 1.6 }", childRule.ToCss());
    }

    [TestMethod]
    public void ContainerWithComparisonOperators()
    {
        const string source = "@container tall (width < 500px) and (height >= 300px) {h2 { line-height: 1.6; } }";
        var result = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(source, result.StylesheetText.Text);
        var rule = result.Rules[0] as ContainerRule;
        Assert.IsNotNull(rule);
        Assert.AreEqual("@container tall (width < 500px) and (height >= 300px) { h2 { line-height: 1.6 } }", rule.Text);
        Assert.AreEqual("tall", rule.Name);
        Assert.AreEqual("(width < 500px) and (height >= 300px)", rule.ConditionText);
        var childRule = rule.Children.OfType<StyleRule>().First();
        Assert.AreEqual("h2 { line-height: 1.6 }", childRule.ToCss());
    }

    [TestMethod]
    public void CSSWithTwoContainers()
    {
        const string source = @"li {
  container-type: inline-size;
}

@container (min-width: 45ch) {
  li span {
    color: rgb(255, 0, 0);
    font-size: 2rem !important;
  }
}

@container (min-width: 70ch) {
  li span {
    color: rgb(0, 0, 255);
    font-size: 3rem !important;
  }
}";
        var result = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(source, result.StylesheetText.Text);
        Assert.AreEqual(3, result.Rules.Length);
        var rule1 = result.Rules[0] as StyleRule;
        var rule2 = result.Rules[1] as ContainerRule;
        var rule3 = result.Rules[2] as ContainerRule;
        Assert.IsNotNull(rule1);
        Assert.IsNotNull(rule2);
        Assert.IsNotNull(rule3);
        Assert.AreEqual("li { container-type: inline-size }", rule1.ToCss());
        Assert.AreEqual("@container (min-width: 45ch) { li span { color: rgb(255, 0, 0); font-size: 2rem !important } }", rule2.ToCss());
        Assert.AreEqual("@container (min-width: 70ch) { li span { color: rgb(0, 0, 255); font-size: 3rem !important } }", rule3.ToCss());
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-conditional-5.yml")]
    [Spec("css-conditional-5", "#container-shorthand")]
    public void ContainerShorthand_BothLonghandsSet_ComposesShorthandValue()
    {
        // Exercises ContainerShorthandConverter.Construct: setting both container-name and
        // container-type individually must let the "container" shorthand serialize their combination.
        var style = CssConstructionFunctions.ParseDeclarations("container-name: sidebar; container-type: inline-size;");

        Assert.AreEqual("sidebar / inline-size", style["container"]);
    }

    [TestMethod]
    public void ContainerShorthand_OnlyOneLonghandSet_ShorthandIsEmpty()
    {
        var style = CssConstructionFunctions.ParseDeclarations("container-name: sidebar;");

        Assert.AreEqual(string.Empty, style["container"]);
    }
}
