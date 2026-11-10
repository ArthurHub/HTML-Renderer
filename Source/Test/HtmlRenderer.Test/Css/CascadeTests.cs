using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class CascadeTests
{
    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);
    private static readonly RColor Green = RColor.FromArgb(0, 128, 0);

    // ── 6.1 Specified, computed, used values ─────────────────────────────

    [TestMethod]
    [Spec("css21", "cascade.html#specified-value")]
    [Spec("css-cascade-5", "#cascaded")]
    [Spec("css-cascade-5", "#specified")]
    public void CascadedValue_WinsOverTheInheritedValue()
    {
        var x = Box("<div style='color:red'><p id=x style='color:green'>a</p></div>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#specified-value")]
    [Spec("css-cascade-5", "#inheriting")]
    public void InheritedProperty_WithoutCascadedValue_TakesTheParentsValue()
    {
        var x = Box("<div style='font-style:italic;white-space:pre'><p id=x>a</p></div>");

        Assert.AreEqual("italic", x.FontStyle);
        Assert.AreEqual("pre", x.WhiteSpace);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#specified-value")]
    [Spec("css-cascade-5", "#initial-values")]
    public void NonInheritedProperty_WithoutCascadedValue_TakesTheInitialValue()
    {
        var x = Box("<div style='margin-top:30px;padding-left:5px;background-color:red'><div id=x>a</div></div>");

        Assert.AreEqual("0", x.MarginTop);
        Assert.AreEqual("0", x.PaddingLeft);
        Assert.AreEqual("transparent", x.BackgroundColor);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#computed-value")]
    [Spec("css-cascade-5", "#computed")]
    public void EmLength_IsComputedAgainstTheElementsFontSize()
    {
        var x = Box("<div id=x style='font-size:20px;text-indent:2em'>a</div>");

        Assert.AreEqual(40, ParsePx(x.TextIndent), 0.01);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "cascade.html#computed-value")]
    [Spec("css-cascade-5", "#computed")]
    public void EmLength_DeclaredBeforeTheFontSize_IsComputedAgainstTheElementsFontSize()
    {
        var x = Box("<div id=x style='text-indent:2em;font-size:20px'>a</div>");

        Assert.AreEqual(40, ParsePx(x.TextIndent), 0.01);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#used-value")]
    [Spec("css-cascade-5", "#used")]
    public void PercentageWidth_IsResolvedAgainstTheContainingBlock()
    {
        var x = Box("<div style='width:200px'><div id=x style='width:50%'>a</div></div>");

        Assert.AreEqual("50%", x.Width);
        Assert.AreEqual(100, x.Size.Width, 0.5);
    }

    // ── 6.2 Inheritance ──────────────────────────────────────────────────

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "cascade.html#inheritance")]
    [Spec("css-cascade-5", "#inheriting")]
    public void PercentageFontSize_IsInheritedAsTheComputedSize()
    {
        var (root, _) = LayoutHarness.Layout(
            "<html><head><style>body { font-size: 10pt } h1 { font-size: 130% }</style></head>" +
            "<body><h1 id=h1>A <em id=em>large</em> heading</h1></body></html>");
        var h1 = LayoutHarness.FindById(root, "h1")!;
        var em = LayoutHarness.FindById(root, "em")!;

        Assert.AreEqual(13, h1.ActualFont.Size, 0.01);
        Assert.AreEqual(h1.ActualFont.Size, em.ActualFont.Size, 0.01);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#inheritance")]
    [Spec("css-cascade-5", "#inheriting")]
    public void EmLength_IsInheritedAsTheComputedLength()
    {
        var x = Box("<div style='font-size:20px;text-indent:2em'><div id=x style='font-size:10px'>a</div></div>");

        Assert.AreEqual(40, ParsePx(x.TextIndent), 0.01);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#inheritance")]
    [Spec("css-cascade-5", "#inheriting")]
    public void Inheritance_FollowsTheDocumentTree_NotAnonymousBoxes()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<span style='color:red;margin-left:7px'>a<div id=x style='margin-left:inherit'>b</div>c</span>"));
        var x = LayoutHarness.FindById(root, "x")!;

        Assert.AreEqual(Red, x.ActualColor);
        Assert.AreEqual("7px", x.MarginLeft);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "cascade.html#inheritance")]
    [Spec("css-cascade-5", "#inheriting")]
    public void WordSpacing_IsInherited()
    {
        var x = Box("<div style='word-spacing:10px'><p id=x>a b</p></div>");

        Assert.AreEqual("10px", x.WordSpacing);
    }

    // ── 6.2.1 'inherit' ──────────────────────────────────────────────────

    [TestMethod]
    [Spec("css21", "cascade.html#value-def-inherit")]
    [Spec("css-cascade-5", "#inherit")]
    public void Inherit_OnANonInheritedProperty_TakesTheParentsValue()
    {
        var x = Box("<div style='border-top:3px solid red'><div id=x style='border-top-style:inherit;background-color:inherit'>a</div></div>");

        Assert.AreEqual("solid", x.BorderTopStyle);
        Assert.AreEqual("transparent", x.BackgroundColor);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "cascade.html#value-def-inherit")]
    [Spec("css-cascade-5", "#inherit")]
    public void Inherit_OnTheRootElement_TakesTheInitialValue()
    {
        var (root, _) = LayoutHarness.Layout("<html id=x style='display:inherit'><body>a</body></html>");
        var x = LayoutHarness.FindById(root, "x")!;

        Assert.AreEqual("inline", x.Display);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "cascade.html#value-def-inherit")]
    [Spec("css-cascade-5", "#inherit")]
    public void Inherit_TakesTheParentsComputedEmLength()
    {
        var x = Box("<div style='font-size:20px;margin-left:2em'><div id=x style='font-size:10px;margin-left:inherit'>a</div></div>");

        Assert.AreEqual(40, x.ActualMarginLeft, 0.01);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#value-def-inherit")]
    [Spec("css-cascade-5", "#inherit")]
    public void InheritImportant_EnforcesTheParentsValue()
    {
        var x = Box(
            "<style>div { color: green !important } * { color: inherit !important }</style>" +
            "<div><p style='color:red'><span id=x style='color:red'>a</span></p></div>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    // ── 6.3 @import ──────────────────────────────────────────────────────

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "cascade.html#at-import")]
    [Spec("css-cascade-5", "#at-import")]
    public void Import_AppliesTheImportedStyleSheet_BeforeTheImportingOne()
    {
        var x = BoxWithStylesheets(
            "<style>@import 'a.css'; @import url('b.css'); p { font-style: italic }</style><p id=x>a</p>",
            new() { ["a.css"] = "p { color: red; font-style: normal }", ["b.css"] = "p { color: green }" });

        Assert.AreEqual(Green, x.ActualColor);
        Assert.AreEqual("italic", x.FontStyle);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "cascade.html#at-import")]
    [Spec("css-cascade-5", "#conditional-import")]
    public void Import_WithANonMatchingMediaList_IsIgnored()
    {
        var x = BoxWithStylesheets(
            "<style>@import 'screen.css' screen, projection; @import 'print.css' print;</style><p id=x>a</p>",
            new() { ["screen.css"] = "p { color: green }", ["print.css"] = "p { font-style: italic }" });

        Assert.AreEqual(Green, x.ActualColor);
        Assert.AreEqual("normal", x.FontStyle);
    }

    // ── 6.4 The cascade ──────────────────────────────────────────────────

    [TestMethod]
    [Spec("css21", "cascade.html#cascading-order")]
    [Spec("css-cascade-5", "#cascading-origins")]
    public void AuthorRule_BeatsTheDefaultStyleSheet_RegardlessOfSpecificity()
    {
        var x = Box("<style>* { font-weight: normal }</style><b id=x>a</b>");

        Assert.AreEqual("normal", x.FontWeight);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#cascading-order")]
    [Spec("css-cascade-5", "#filtering")]
    public void RuleInANonMatchingMediaBlock_DoesNotApply()
    {
        var x = Box("<style>p { color: green } @media print { p { color: red } }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "cascade.html#cascading-order")]
    [Spec("css-cascade-5", "#filtering")]
    public void StyleSheet_WithANonMatchingMediaAttribute_DoesNotApply()
    {
        var x = BoxWithStylesheets(
            "<link rel=stylesheet href='screen.css' media=screen><link rel=stylesheet href='print.css' media=print><p id=x>a</p>",
            new() { ["screen.css"] = "p { color: green }", ["print.css"] = "p { color: red }" });

        Assert.AreEqual(Green, x.ActualColor);
        Assert.AreEqual(RColor.Black, Box("<style media=print>p { color: red }</style><p id=x>a</p>").ActualColor);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#cascading-order")]
    [Spec("css-cascade-5", "#cascade-sort")]
    public void LaterStyleSheet_WinsAtEqualSpecificity()
    {
        var x = BoxWithStylesheets(
            "<style>p { color: red }</style><link rel=stylesheet href='a.css'><p id=x>a</p>",
            new() { ["a.css"] = "p { color: green }" });

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#important-rules")]
    [Spec("css-cascade-5", "#importance")]
    public void ImportantDeclaration_BeatsAMoreSpecificNormalOne()
    {
        var x = Box("<style>div { color: green !important } #x { color: red }</style><div id=x>a</div>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#important-rules")]
    [Spec("css-cascade-5", "#shorthand")]
    public void ImportantShorthand_MakesEverySubPropertyImportant()
    {
        var x = Box("<style>div { margin: 5px !important } #x { margin-left: 20px }</style><div id=x>a</div>");

        Assert.AreEqual("5px", x.MarginLeft);
        Assert.AreEqual("5px", x.MarginTop);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#important-rules")]
    [Spec("css-cascade-5", "#importance")]
    public void ImportantDeclaration_WinsWithinTheAuthorStyleSheet()
    {
        var x = Box(
            "<style>p { font: normal 12pt sans-serif !important } p { font-size: 24pt }</style><p id=x>a</p>");

        Assert.AreEqual("12pt", x.FontSize);
    }

    // ── 6.4.3 Specificity ────────────────────────────────────────────────

    [TestMethod]
    [Spec("css21", "cascade.html#specificity")]
    [DataRow("*", 0, 0, 0)]
    [DataRow("li", 0, 0, 1)]
    [DataRow("li:first-line", 0, 0, 2)]
    [DataRow("ul li", 0, 0, 2)]
    [DataRow("ul ol+li", 0, 0, 3)]
    [DataRow("h1 + *[rel=up]", 0, 1, 1)]
    [DataRow("ul ol li.red", 0, 1, 3)]
    [DataRow("li.red.level", 0, 2, 1)]
    [DataRow("#x34y", 1, 0, 0)]
    [DataRow("[id=p33]", 0, 1, 0)]
    [DataRow("a:link", 0, 1, 1)]
    public void Specificity_MatchesTheSpecExamples(string selector, int ids, int classes, int tags)
    {
        var rule = CssParser.ParseStyleSheetText(selector + " {}").StyleRules.Single();

        Assert.AreEqual(new Priority(0, (byte)ids, (byte)classes, (byte)tags), rule.Selector.Specificity);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#specificity")]
    [Spec("css-style-attr", "#interpret")]
    [Spec("css-cascade-5", "#cascade-sort")]
    public void StyleAttribute_BeatsAnIdSelector()
    {
        var x = Box("<style>#x { color: red }</style><p id=x style='color: green'>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#specificity")]
    public void IdAttributeSelector_CountsAsAnAttribute_NotAsAnId()
    {
        Assert.AreEqual(Green, Box("<style>#x { color: green } [id=x] { color: red }</style><p id=x>a</p>").ActualColor);
        Assert.AreEqual(Green, Box("<style>[id=x] { color: red } .c { color: green }</style><p id=x class=c>a</p>").ActualColor);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#specificity")]
    public void PseudoClass_CountsAsAClass()
    {
        Assert.AreEqual(Green, Box("<style>p:first-child { color: green } div p { color: red }</style><div><p id=x>a</p></div>").ActualColor);
        Assert.AreEqual(Green, Box("<style>p:first-child { color: red } p.c { color: green }</style><div><p id=x class=c>a</p></div>").ActualColor);
    }

    // ── 6.4.4 Presentational hints ───────────────────────────────────────

    [TestMethod]
    [Spec("css21", "cascade.html#preshint")]
    [Spec("html", "rendering.html#the-css-user-agent-style-sheet-and-presentational-hints")]
    [Spec("css-cascade-5", "#preshint")]
    public void PresentationalHint_BeatsTheDefaultStyleSheet()
    {
        var x = Box("<table><tr><th id=x align=left>a</th></tr></table>");

        Assert.AreEqual("left", x.TextAlign);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#preshint")]
    [Spec("html", "rendering.html#the-css-user-agent-style-sheet-and-presentational-hints")]
    [Spec("css-cascade-5", "#preshint")]
    public void StyleAttribute_BeatsAPresentationalHint()
    {
        var x = Box("<font id=x color=red style='color:green'>a</font>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "cascade.html#preshint")]
    [Spec("html", "rendering.html#the-css-user-agent-style-sheet-and-presentational-hints")]
    [Spec("css-cascade-5", "#preshint")]
    public void PresentationalHint_LosesToAnAuthorRule()
    {
        Assert.AreEqual(Green, Box("<style>* { color: green }</style><font id=x color=red>a</font>").ActualColor);
        Assert.AreEqual("100px", Box("<style>table { width: 100px }</style><table id=x width=300><tr><td>a</td></tr></table>").Width);
    }

    private static CssBox Box(string body, string id = "x")
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(body));
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box, $"No box with id '{id}'");
        return box;
    }

    /// <summary>Lays out <paramref name="body"/> with external style sheets served from memory.</summary>
    private static CssBox BoxWithStylesheets(string body, Dictionary<string, string> stylesheets, string id = "x")
    {
        var container = new HtmlContainerInt(new MockAdapter()) { MaxSize = new RSize(1000, 4000) };
        container.StylesheetLoad += (_, e) =>
        {
            if (stylesheets.TryGetValue(e.Src, out var css))
                e.SetStyleSheet = css;
        };

        container.SetHtml(LayoutHarness.Wrap(body));
        using (var graphics = new RecordingGraphics())
            container.PerformLayout(graphics);

        var box = LayoutHarness.FindById(container.Root!, id);
        Assert.IsNotNull(box, $"No box with id '{id}'");
        return box;
    }

    private static double ParsePx(string length) =>
        double.Parse(length.Replace("px", ""), System.Globalization.CultureInfo.InvariantCulture);
}
