using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssConditionalTests
{
    private static readonly RColor Green = RColor.FromArgb(0, 128, 0);

    // ── 2 Processing ─────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-conditional-3", "#processing")]
    public void TrueCondition_AppliesTheRulesInPlace()
    {
        Assert.AreEqual(Green, Box("<style>@media screen { p { color: green } }</style><p id=x>a</p>").ActualColor);
        Assert.AreEqual(Green, Box("<style>@media screen { p { color: red } } p { color: green }</style><p id=x>a</p>").ActualColor);
    }

    [TestMethod]
    [Spec("css-conditional-3", "#processing")]
    [Spec("css-conditional-3", "#use")]
    public void NestedConditions_ApplyOnlyWhenAllAreTrue()
    {
        var x = Box(
            "<style>p { color: green } @media screen { @media print { p { color: red } } }" +
            " @media screen { @media (min-width: 100px) { p { font-style: italic } } }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
        Assert.AreEqual("italic", x.FontStyle);
    }

    [TestMethod]
    [Spec("css-conditional-3", "#processing")]
    public void ChangedCondition_IsReflected()
    {
        var html = LayoutHarness.Wrap("<style>@media (max-width: 600px) { p { color: green } }</style><p id=x>a</p>");
        var (_, container) = LayoutHarness.Layout(html, maxWidth: 1000);
        Assert.AreNotEqual(Green, LayoutHarness.FindById(container.Root!, "x")!.ActualColor);

        container.MaxSize = new RSize(500, 4000);
        using (var graphics = new RecordingGraphics())
            container.PerformLayout(graphics);

        Assert.AreEqual(Green, LayoutHarness.FindById(container.Root!, "x")!.ActualColor);
    }

    // ── 3 Contents, 4 Placement ──────────────────────────────────────────

    [TestMethod]
    [Spec("css-conditional-3", "#contents-of")]
    public void InvalidRuleInside_DoesNotInvalidateTheGroupRule()
    {
        var x = Box("<style>@media screen { @foo bar; p { color: green } }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css-conditional-3", "#use")]
    public void SupportsRule_InsideAMediaRule_Applies()
    {
        var x = Box("<style>@media screen { @supports (color: red) { p { color: green } } }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    // ── 5 @media ─────────────────────────────────────────────────────────

    [Ignore("Gap, see Spec/css-conditional-3.yml")]
    [TestMethod]
    [Spec("css-conditional-3", "#at-media")]
    public void MediaRule_WithAnEmptyQueryList_Applies()
    {
        var x = Box("<style>@media { p { color: green } }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css-conditional-3", "#at-media")]
    public void MediaRule_AppliesWhenAnyQueryOfTheListMatches()
    {
        var x = Box("<style>@media print and (min-width: 1px), screen and (min-width: 1px) { p { color: green } }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    // ── 6 @supports ──────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-conditional-3", "#at-supports")]
    public void SupportsRule_WithATrueCondition_Applies()
    {
        var x = Box("<style>@supports (display: block) { p { color: green } }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css-conditional-3", "#at-supports")]
    public void SupportsRule_WithAFalseCondition_DoesNotApply()
    {
        var x = Box("<style>p { color: green } @supports not (display: block) { p { color: red } }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css-conditional-3", "#at-supports")]
    [DataRow("(color: red) and (color: blue)", true)]
    [DataRow("(color: red) and (foo: bar)", false)]
    [DataRow("(color: red) or (foo: bar)", true)]
    [DataRow("not (foo: bar)", true)]
    [DataRow("not ((color: red) or (foo: bar))", false)]
    public void SupportsCondition_CombinesItsTerms(string condition, bool expected)
    {
        Assert.AreEqual(expected, Supports(condition).Condition.Check());
    }

    [Ignore("Gap, see Spec/css-conditional-3.yml")]
    [TestMethod]
    [Spec("css-conditional-3", "#at-supports")]
    [DataRow("(foo bar)")]
    [DataRow("()")]
    [DataRow("foo(bar)")]
    public void GeneralEnclosed_IsFalse(string condition)
    {
        var rule = Supports(condition);

        Assert.IsNotNull(rule, "The rule is valid");
        Assert.IsFalse(rule.Condition.Check());
    }

    [TestMethod]
    [Spec("css-conditional-3", "#at-supports")]
    public void MixingAndWithOr_WithoutParentheses_IsInvalid()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@supports (color: red) or (color: blue) and (color: green) { }");

        Assert.AreEqual(0, sheet.Rules.Length);
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
