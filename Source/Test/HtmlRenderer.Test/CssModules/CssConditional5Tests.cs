using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssConditional5Tests
{
    private static readonly RColor Green = RColor.FromArgb(0, 128, 0);

    // ── 2 @supports extensions ───────────────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/css-conditional-5.yml")]
    [Spec("css-conditional-5", "#support-definition-ext-fonts")]
    [DataRow("font-format(truetype)", true)]
    [DataRow("font-tech(color-COLRv1)", false)]
    public void FontFunctions_TestFontSupport(string condition, bool expected)
    {
        var rule = Supports(condition);

        Assert.IsNotNull(rule, "The rule is valid");
        Assert.AreEqual(expected, rule.Condition.Check());
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-conditional-5.yml")]
    [Spec("css-conditional-5", "#support-definition-at-rules")]
    [DataRow("at-rule(@media)", true)]
    [DataRow("at-rule(@foo)", false)]
    public void AtRuleFunction_TestsAtRuleSupport(string condition, bool expected)
    {
        var rule = Supports(condition);

        Assert.IsNotNull(rule, "The rule is valid");
        Assert.AreEqual(expected, rule.Condition.Check());
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-conditional-5.yml")]
    [Spec("css-conditional-5", "#support-definition-supports-condition-name")]
    public void UnknownNamedCondition_IsFalse()
    {
        var rule = Supports("(--unknown)");

        Assert.IsNotNull(rule, "The rule is valid");
        Assert.IsFalse(rule.Condition.Check());
    }

    // ── 3 @when, 4 @else ─────────────────────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/css-conditional-5.yml")]
    [Spec("css-conditional-5", "#when-rule")]
    public void When_AppliesItsRulesWhenTrue()
    {
        var x = Box("<style>p { color: red } @when media(screen) { p { color: green } }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-conditional-5.yml")]
    [Spec("css-conditional-5", "#else-rule")]
    public void Else_AppliesWhenTheEarlierConditionsAreFalse()
    {
        var x = Box("<style>@when media(print) { p { color: red } } @else { p { color: green } }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    // ── 5 Container Queries ──────────────────────────────────────────────

    [TestMethod]
    [Spec("css-conditional-5", "#container-type")]
    [DataRow("normal")]
    [DataRow("size")]
    [DataRow("inline-size")]
    public void ContainerType_IsParsed(string value)
    {
        Assert.AreEqual(value, CssConstructionFunctions.ParseDeclaration("container-type: " + value)!.Value);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-conditional-5.yml")]
    [Spec("css-conditional-5", "#container-type")]
    public void ContainerTypeSize_AppliesSizeContainment()
    {
        var x = Box("<div id=x style='container-type:size;width:100px'><div style='height:20px'>a</div></div>");

        Assert.AreEqual(0, x.Size.Height, 0.01);
    }

    [TestMethod]
    [Spec("css-conditional-5", "#container-name")]
    public void ContainerName_TakesIdentifiers()
    {
        Assert.AreEqual("foo bar", CssConstructionFunctions.ParseDeclaration("container-name: foo bar")!.Value);
    }

    [TestMethod]
    [Spec("css-conditional-5", "#container-shorthand")]
    public void ContainerShorthand_IsParsed()
    {
        Assert.IsNotNull(CssConstructionFunctions.ParseDeclaration("container: foo / size"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-conditional-5.yml")]
    [Spec("css-conditional-5", "#container-rule")]
    [Spec("css-conditional-5", "#width")]
    public void ContainerRule_AppliesWhenTheContainerMatches()
    {
        var x = Box("<style>p { color: red } @container (min-width: 1px) { p { color: green } }</style>" +
                    "<div style='container-type:inline-size'><p id=x>a</p></div>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-conditional-5.yml")]
    [Spec("css-conditional-5", "#container-lengths")]
    public void ContainerUnits_ResolveAgainstTheContainer()
    {
        var x = Box("<div style='container-type:inline-size;width:100px'><div id=x style='width:50cqw'>a</div></div>");

        Assert.AreEqual(50, x.Size.Width, 0.01);
    }

    // ── 8 @supports-condition ────────────────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/css-conditional-5.yml")]
    [Spec("css-conditional-5", "#supports-condition-rule")]
    public void SupportsConditionRule_IsParsed()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@supports-condition --foo { (color: red) }");

        Assert.AreEqual(1, sheet.Rules.Length);
    }

    private static SupportsRule Supports(string condition)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@supports " + condition + " { }");
        return sheet.Rules.Length == 1 ? (SupportsRule)sheet.Rules[0] : null!;
    }

    private static CssBox Box(string body, string id = "x")
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(body));
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box, $"No box with id '{id}'");
        return box;
    }
}
