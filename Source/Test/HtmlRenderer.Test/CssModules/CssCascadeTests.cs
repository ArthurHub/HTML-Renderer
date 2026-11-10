using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssCascadeTests
{
    private static readonly RColor Green = RColor.FromArgb(0, 128, 0);

    // ── 2 @import ────────────────────────────────────────────────────────

    [Ignore("Gap, see Spec/css-cascade-5.yml")]
    [TestMethod]
    [Spec("css-cascade-5", "#at-import")]
    public void ImportPrelude_SeparatesLayerAndSupportsFromTheMediaList()
    {
        var rule = (ImportRule)CssConstructionFunctions.ParseStyleSheet(
            "@import url(a.css) layer(base) supports(display: flex) screen;").Rules.Single();

        Assert.AreEqual("a.css", rule.Href);
        Assert.AreEqual("screen", rule.Media.MediaText);
    }

    // ── 3 Shorthands ─────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-cascade-5", "#shorthand")]
    public void Shorthand_ResetsOmittedSubPropertiesToTheirInitialValue()
    {
        var x = Box("<style>p { font-style: italic; font-weight: bold } #x { font: 12px serif }</style><p id=x>a</p>");

        Assert.AreEqual("normal", x.FontStyle);
        Assert.AreEqual("normal", x.FontWeight);
        Assert.AreEqual("12px", x.FontSize);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#shorthand")]
    public void Shorthand_WithACssWideKeyword_SetsEverySubProperty()
    {
        var x = Box("<div style='margin: 7px'><div id=x style='margin: inherit'>a</div></div>");

        Assert.AreEqual("7px", x.MarginLeft);
        Assert.AreEqual("7px", x.MarginTop);
    }

    // ── 4 Value processing ───────────────────────────────────────────────

    [TestMethod]
    [Spec("css-cascade-5", "#applies-to")]
    public void PropertyThatDoesNotApply_StillHasAComputedValueThatInherits()
    {
        var x = Box("<table><tr style='text-indent: 10px'><td id=x>a</td></tr></table>");

        Assert.AreEqual("10px", x.TextIndent);
    }

    // ── 5 Filtering ──────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-cascade-5", "#filtering")]
    [Spec("css-cascade-5", "#declared")]
    public void InvalidDeclarations_AreDroppedBeforeTheCascade()
    {
        var x = Box("<style>p { color: green } p { color: bogus } p { colour: red }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    // ── 6.4 Cascade layers ───────────────────────────────────────────────

    [Ignore("Gap, see Spec/css-cascade-5.yml")]
    [TestMethod]
    [Spec("css-cascade-5", "#layer-names")]
    public void LayerName_ThatIsACssWideKeyword_InvalidatesTheRule()
    {
        var x = Box("<style>p { color: green } @layer initial { p { color: red } }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    // ── 7.3 Explicit defaulting ──────────────────────────────────────────

    [TestMethod]
    [Spec("css-cascade-5", "#initial")]
    public void Initial_IgnoresTheDefaultStyleSheet()
    {
        var x = Box("<h1 id=x style='font-weight: initial'>a</h1>");

        Assert.AreEqual("normal", x.FontWeight);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#inherit-initial")]
    public void Unset_ErasesEarlierDeclarations()
    {
        var x = Box("<div style='color: green'><p id=x style='color: red; color: unset; margin-left: 5px; margin-left: unset'>a</p></div>");

        Assert.AreEqual(Green, x.ActualColor);
        Assert.AreEqual("0", x.MarginLeft);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#default")]
    public void Revert_InAnAuthorRule_RollsBackToTheDefaultStyleSheet()
    {
        var x = Box("<style>h1 { font-weight: normal } h1 { font-weight: revert }</style><h1 id=x>a</h1>");

        Assert.AreEqual("bolder", x.FontWeight);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#default")]
    public void Revert_WithoutADefaultStyleSheetValue_InheritsOrTakesTheInitialValue()
    {
        var x = Box("<style>div { color: green } p { color: red; margin-left: 5px } p { color: revert; margin-left: revert }</style><div><p id=x>a</p></div>");

        Assert.AreEqual(Green, x.ActualColor);
        Assert.AreEqual("0", x.MarginLeft);
    }

    [Ignore("Gap, see Spec/css-cascade-5.yml")]
    [TestMethod]
    [Spec("css-cascade-5", "#default")]
    public void Revert_InAStyleAttribute_RollsBackToTheDefaultStyleSheet()
    {
        var x = Box("<style>h1 { font-weight: normal }</style><h1 id=x style='font-weight: revert'>a</h1>");

        Assert.AreEqual("bolder", x.FontWeight);
    }

    [Ignore("Gap, see Spec/css-cascade-5.yml")]
    [TestMethod]
    [Spec("css-cascade-5", "#default")]
    public void Revert_InAnImportantAuthorRule_RollsBackToTheDefaultStyleSheet()
    {
        var x = Box("<style>h1 { font-weight: normal } h1 { font-weight: revert !important }</style><h1 id=x>a</h1>");

        Assert.AreEqual("bolder", x.FontWeight);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#revert-layer")]
    public void RevertLayer_WithoutALowerLayer_RollsBackToTheDefaultStyleSheet()
    {
        var x = Box("<style>h1 { font-weight: normal } h1 { font-weight: revert-layer }</style><h1 id=x>a</h1>");

        Assert.AreEqual("bolder", x.FontWeight);
    }

    private static CssBox Box(string body, string id = "x")
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(body));
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box, $"No box with id '{id}'");
        return box;
    }
}
