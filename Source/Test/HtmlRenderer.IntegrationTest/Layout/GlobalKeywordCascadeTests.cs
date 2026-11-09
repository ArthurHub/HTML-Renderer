using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Layout;

[DoNotParallelize]
[TestClass]
public sealed class GlobalKeywordCascadeTests
{
    // ── inherit ────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css21", "cascade.html#value-def-inherit")]
    public void Inherit_ColorFromParent_ChildGetsParentColor()
    {
        var html = LayoutHarness.Wrap(
            "<div id='parent' style='color: red'><span id='child' style='color: inherit'>text</span></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var child = LayoutHarness.FindById(root, "child")!;

        Assert.AreEqual("rgb(255, 0, 0)", child.Color);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#value-def-inherit")]
    public void Inherit_NonInheritedProperty_ChildGetsParentValue()
    {
        var html = LayoutHarness.Wrap(
            "<div id='parent' style='margin-top: 30px'><div id='child' style='margin-top: inherit'>text</div></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var child = LayoutHarness.FindById(root, "child")!;

        Assert.AreEqual("30px", child.MarginTop);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#value-def-inherit")]
    public void Inherit_ColorWithoutExplicitParent_UsesInitialBlack()
    {
        var html = LayoutHarness.Wrap("<div id='el' style='color: inherit'>text</div>");
        var (root, _) = LayoutHarness.Layout(html);
        var el = LayoutHarness.FindById(root, "el")!;

        Assert.AreEqual("black", el.Color);
    }

    // ── initial ───────────────────────────────────────────────────────────

    [TestMethod]
    public void Initial_Color_ResetsToBlackIgnoringParent()
    {
        var html = LayoutHarness.Wrap(
            "<div id='parent' style='color: blue'><span id='child' style='color: initial'>text</span></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var child = LayoutHarness.FindById(root, "child")!;

        Assert.AreEqual("black", child.Color);
    }

    [TestMethod]
    public void Initial_FontSize_ResetsToMedium()
    {
        var html = LayoutHarness.Wrap("<h1 id='el' style='font-size: initial'>heading</h1>");
        var (root, _) = LayoutHarness.Layout(html);
        var el = LayoutHarness.FindById(root, "el")!;

        Assert.AreEqual("medium", el.FontSize);
    }

    [TestMethod]
    public void Initial_MarginTop_ResetsToZero()
    {
        var html = LayoutHarness.Wrap(
            "<style>div { margin-top: 50px; }</style><div id='el' style='margin-top: initial'>text</div>");
        var (root, _) = LayoutHarness.Layout(html);
        var el = LayoutHarness.FindById(root, "el")!;

        Assert.AreEqual("0", el.MarginTop);
    }

    // ── unset ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void Unset_InheritedProperty_BehavesLikeInherit()
    {
        var html = LayoutHarness.Wrap(
            "<div id='parent' style='color: green'><span id='child' style='color: unset'>text</span></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var child = LayoutHarness.FindById(root, "child")!;

        Assert.AreEqual("rgb(0, 128, 0)", child.Color);
    }

    [TestMethod]
    public void Unset_NonInheritedProperty_BehavesLikeInitial()
    {
        var html = LayoutHarness.Wrap(
            "<style>div { margin-top: 50px; }</style><div id='el' style='margin-top: unset'>text</div>");
        var (root, _) = LayoutHarness.Layout(html);
        var el = LayoutHarness.FindById(root, "el")!;

        Assert.AreEqual("0", el.MarginTop);
    }

    [TestMethod]
    public void Unset_InheritedPropertyWithNoParentValue_UsesInitial()
    {
        var html = LayoutHarness.Wrap("<div id='el' style='color: unset'>text</div>");
        var (root, _) = LayoutHarness.Layout(html);
        var el = LayoutHarness.FindById(root, "el")!;

        Assert.AreEqual("black", el.Color);
    }

    // ── cascade order ────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css21", "cascade.html#inheritance")]
    public void Regression_ColorInheritsFromParentWithoutKeyword()
    {
        var html = LayoutHarness.Wrap(
            "<div id='parent' style='color: purple'><span id='child'>text</span></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var child = LayoutHarness.FindById(root, "child")!;

        Assert.AreEqual("rgb(128, 0, 128)", child.Color);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#specificity")]
    public void Regression_InlineStyleOverridesAuthorRule()
    {
        var html = LayoutHarness.Wrap(
            "<style>div { color: blue; }</style><div id='el' style='color: green'>text</div>");
        var (root, _) = LayoutHarness.Layout(html);
        var el = LayoutHarness.FindById(root, "el")!;

        Assert.AreEqual("rgb(0, 128, 0)", el.Color);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#cascading-order")]
    public void Regression_AuthorRuleOverridesUaDefault()
    {
        var html = LayoutHarness.Wrap("<style>h1 { font-size: 10px; }</style><h1 id='el'>heading</h1>");
        var (root, _) = LayoutHarness.Layout(html);
        var el = LayoutHarness.FindById(root, "el")!;

        Assert.AreEqual("10px", el.FontSize);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#important-rules")]
    public void Regression_ImportantAuthorRuleBeatsInlineStyle()
    {
        var html = LayoutHarness.Wrap(
            "<style>div { color: blue !important; }</style><div id='el' style='color: red'>text</div>");
        var (root, _) = LayoutHarness.Layout(html);
        var el = LayoutHarness.FindById(root, "el")!;

        Assert.AreEqual("rgb(0, 0, 255)", el.Color);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#important-rules")]
    public void InlineImportant_BeatsAuthorImportantRule()
    {
        var html = LayoutHarness.Wrap(
            "<style>div { color: blue !important; }</style><div id='el' style='color: red !important'>text</div>");
        var (root, _) = LayoutHarness.Layout(html);
        var el = LayoutHarness.FindById(root, "el")!;

        Assert.AreEqual("rgb(255, 0, 0)", el.Color);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#cascading-order")]
    public void Regression_LaterAuthorRuleOverridesEarlierSameSpecificity()
    {
        var html = LayoutHarness.Wrap("<style>div { color: red; } div { color: blue; }</style><div id='el'>text</div>");
        var (root, _) = LayoutHarness.Layout(html);
        var el = LayoutHarness.FindById(root, "el")!;

        Assert.AreEqual("rgb(0, 0, 255)", el.Color);
    }
}
