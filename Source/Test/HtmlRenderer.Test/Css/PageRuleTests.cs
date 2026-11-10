using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Parsing of <c>@page</c> rules.
/// </summary>
[TestClass]
public sealed class PageRuleTests
{
    [TestMethod]
    [Spec("css-page-3", "#syntax-page-selector")]
    [DataRow("@page :blank { margin: 0 }", ":blank")]
    [DataRow("@page:first { margin: 0 }", ":first")]
    [DataRow("@page toc, index { margin: 0 }", "toc, index")]
    public void AtPage_SpecSelector_IsParsed(string css, string selector)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet(css);
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual(selector, rule.SelectorText);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-page-3.yml")]
    [Spec("css-page-3", "#syntax-page-selector")]
    [DataRow("@page :blank:first { margin: 0 }", ":blank:first")]
    [DataRow("@page chapter:first:left { margin: 0 }", "chapter:first:left")]
    public void AtPage_SeveralPseudoClasses_AreParsed(string css, string selector)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet(css);
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual(selector, rule.SelectorText);
    }

    [TestMethod]
    [Spec("css-page-3", "#syntax-page-selector")]
    [DataRow("@page : first { margin: 0 }")]
    [DataRow("@page :first chapter { margin: 0 }")]
    public void AtPage_InvalidSelector_DropsTheRule(string css)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet(css);
        Assert.AreEqual(0, sheet.Rules.OfType<PageRule>().Count());
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-page-3.yml")]
    [Spec("css-page-3", "#syntax-page-selector")]
    public void AtPage_UnknownPseudoClass_DropsTheRule()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page :middle { margin: 0 }");
        Assert.AreEqual(0, sheet.Rules.OfType<PageRule>().Count());
    }

    // ── @page { size: ... } ────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-page-3", "#page-size-prop")]
    public void AtPage_SizeA4_IsStoredOnStyleDeclaration()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { size: A4; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("A4", rule.Style.Size);
    }

    [TestMethod]
    [Spec("css-page-3", "#page-size-prop")]
    public void AtPage_SizeLetter_IsStoredOnStyleDeclaration()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { size: letter; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("letter", rule.Style.Size);
    }

    [TestMethod]
    [Spec("css-page-3", "#page-size-prop")]
    public void AtPage_SizeA4Landscape_IsStoredOnStyleDeclaration()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { size: A4 landscape; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("A4 landscape", rule.Style.Size);
    }

    [TestMethod]
    [Spec("css-page-3", "#page-size-prop")]
    public void AtPage_SizeExplicitDimensions_IsStoredOnStyleDeclaration()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { size: 210mm 297mm; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("210mm 297mm", rule.Style.Size);
    }

    // ── @page pseudo-selectors ─────────────────────────────────────────────

    [TestMethod]
    [Spec("css21", "page.html#page-box")]
    [Spec("css-page-3", "#at-page-rule")]
    public void AtPage_NoSelector_HasNullSelector()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { margin: 20mm; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.IsNull(rule.Selector);
    }

    [TestMethod]
    [Spec("css21", "page.html#page-selectors")]
    [Spec("css-page-3", "#first-pseudo")]
    public void AtPage_FirstPseudoSelector_SelectorTextIsFirst()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page :first { margin-top: 0; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual(":first", rule.SelectorText);
    }

    [TestMethod]
    [Spec("css21", "page.html#page-selectors")]
    [Spec("css-page-3", "#spread-pseudos")]
    public void AtPage_LeftPseudoSelector_SelectorTextIsLeft()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page :left { margin-left: 30mm; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual(":left", rule.SelectorText);
    }

    [TestMethod]
    [Spec("css21", "page.html#page-selectors")]
    [Spec("css-page-3", "#spread-pseudos")]
    public void AtPage_RightPseudoSelector_SelectorTextIsRight()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page :right { margin-right: 30mm; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual(":right", rule.SelectorText);
    }

    // ── @page named selectors ──────────────────────────────────────────────

    [TestMethod]
    [Spec("css-page-3", "#page-selectors")]
    public void AtPage_NamedSelector_SelectorTextIsName()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page chapter { margin-top: 0; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("chapter", rule.SelectorText);
    }

    [TestMethod]
    [Spec("css-page-3", "#page-selectors")]
    public void AtPage_NamedSelector_IsNotColonPrefixed()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page chapter { margin-top: 0; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.IsFalse(rule.SelectorText.StartsWith(':'));
    }

    [TestMethod]
    [Spec("css-page-3", "#syntax-page-selector")]
    public void AtPage_CommaSeparatedNamedSelectors_AllNamesPresent()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page chapter1, chapter2, chapter3 { margin-top: 0; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("chapter1, chapter2, chapter3", rule.SelectorText);
    }

    [TestMethod]
    [Spec("css-page-3", "#page-selectors")]
    public void AtPage_NamedSelector_PreservesCase()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page Chapter { margin-top: 0; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("Chapter", rule.SelectorText);
    }

    // ── @page compound name:pseudo selectors ───────────────────────────────

    [TestMethod]
    [Spec("css-page-3", "#syntax-page-selector")]
    public void AtPage_CompoundNamePseudoSelector_ParsesAsOneRule()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page chapter1:left { margin-top: 0; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("chapter1:left", rule.SelectorText);
    }

    [TestMethod]
    [Spec("css-page-3", "#syntax-page-selector")]
    public void AtPage_CompoundCommaSeparatedSelectors_AllEntriesPresent()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page chapter1:left, chapter2:left { margin-top: 0; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("chapter1:left, chapter2:left", rule.SelectorText);
    }

    // ── @page margin boxes ─────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-page-3", "#margin-at-rules")]
    public void AtPage_TopCenterMarginBox_IsParsedIntoMargins()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { @top-center { content: \"Page\"; } }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        var margin = rule.Margins.FirstOrDefault();
        Assert.IsNotNull(margin);
    }

    [TestMethod]
    [Spec("css-page-3", "#margin-at-rules")]
    public void AtPage_TopCenterMarginBox_HasTopCenterSelector()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { @top-center { content: counter(page); } }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        var margin = rule.Margins.FirstOrDefault(m => m.Selector?.Text?.Contains("top-center") == true);
        Assert.IsNotNull(margin);
    }

    [TestMethod]
    [Spec("css-page-3", "#populating-margin-boxes")]
    public void AtPage_TopCenterMarginBox_ContentIsNonEmpty()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { @top-center { content: \"Header\"; } }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        var margin = rule.Margins.FirstOrDefault();
        Assert.IsNotNull(margin);
        Assert.IsFalse(string.IsNullOrEmpty(margin.Style.Content));
    }

    [TestMethod]
    [Spec("css-page-3", "#margin-at-rules")]
    public void AtPage_MultipleMarginBoxes_AllParsed()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet(@"
            @page {
                @top-left   { content: ""Left""; }
                @top-center { content: ""Center""; }
                @top-right  { content: ""Right""; }
            }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual(3, rule.Margins.Count());
    }

    // ── named pages ────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-page-3", "#using-named-pages")]
    public void PageNameProperty_Identifier_ParsesCorrectly()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("div { page: chapter; }");
        var block = sheet.Rules.OfType<StyleRule>().FirstOrDefault();
        Assert.IsNotNull(block);
        Assert.AreEqual("chapter", block.Style.PageName);
    }

    [TestMethod]
    [Spec("css-page-3", "#using-named-pages")]
    public void PageNameProperty_Auto_ParsesCorrectly()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("div { page: auto; }");
        var block = sheet.Rules.OfType<StyleRule>().FirstOrDefault();
        Assert.IsNotNull(block);
        Assert.AreEqual("auto", block.Style.PageName);
    }

    // ── margin box explicit width ───────────────────────────────────────────

    [TestMethod]
    [Spec("css-page-3", "#margin-at-rules")]
    public void AtPage_MarginBox_ExplicitWidthIsParsedFromStyle()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { @top-left { content: \"L\"; width: 100pt; } }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        var margin = rule.Margins.FirstOrDefault(m => m.Selector?.Text?.Contains("top-left") == true);
        Assert.IsNotNull(margin);
        Assert.AreEqual("100pt", margin.Style.Width);
    }
}
