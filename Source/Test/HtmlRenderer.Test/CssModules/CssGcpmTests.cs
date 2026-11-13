using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssGcpmTests
{
    private const double Delta = 0.5;
    private const double PageHeight = 300;

    [TestMethod]
    [Spec("css-gcpm-3", "#running-syntax")]
    public void PositionRunning_IsAccepted()
    {
        Assert.IsTrue(CssConstructionFunctions.ParseDeclaration("position: running(header)").HasValue);
    }

    [Ignore("Gap, see Spec/css-gcpm-3.yml")]
    [TestMethod]
    [Spec("css-gcpm-3", "#running-syntax")]
    public void RunningElement_IsRemovedFromTheFlow()
    {
        var (root, _) = Paginate("<p id='h' style='position:running(header);margin:0;height:20px'>Header</p><div id='b' style='height:10px'></div>");

        Assert.AreEqual(0, Find(root, "b").Location.Y, Delta);
    }

    [TestMethod]
    [DataRow("content: element(header)")]
    [DataRow("content: element(header, last)")]
    [Spec("css-gcpm-3", "#element-syntax")]
    public void ElementFunction_IsAccepted(string declaration)
    {
        Assert.IsTrue(CssConstructionFunctions.ParseDeclaration(declaration).HasValue);
    }

    [Ignore("Gap, see Spec/css-gcpm-3.yml")]
    [TestMethod]
    [Spec("css-gcpm-3", "#creating-footnotes")]
    public void FloatFootnote_IsAccepted()
    {
        Assert.IsTrue(CssConstructionFunctions.ParseDeclaration("float: footnote").HasValue);
    }

    [Ignore("Gap, see Spec/css-gcpm-3.yml")]
    [TestMethod]
    [Spec("css-gcpm-3", "#creating-footnotes")]
    [Spec("css-gcpm-3", "#footnote-area-position")]
    public void Footnote_IsMovedToTheBottomOfThePage()
    {
        var (root, container) = Paginate("<p style='margin:0'>Text<span id='f' style='float:footnote'>Note</span></p><div id='b' style='height:10px'></div>");
        var f = Find(root, "f");

        Assert.AreEqual(0, container.PageIndexOf(f.Location.Y));
        Assert.IsTrue(f.Location.Y > PageHeight / 2);
    }

    [Ignore("Gap, see Spec/css-gcpm-3.yml")]
    [TestMethod]
    [DataRow("::footnote-call")]
    [DataRow("::footnote-marker")]
    [Spec("css-gcpm-3", "#footnote-call")]
    [Spec("css-gcpm-3", "#footnote-marker")]
    public void FootnotePseudoElements_KeepTheRuleValid(string pseudo)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($"span{pseudo}, p {{ color: red }}");

        Assert.AreEqual(1, sheet.Rules.OfType<StyleRule>().Count());
    }

    [Ignore("Gap, see Spec/css-gcpm-3.yml")]
    [TestMethod]
    [DataRow("@page :nth(1) { margin-top: 0 }")]
    [DataRow("@page :nth(2n+1 of chapter) { margin-top: 0 }")]
    [Spec("css-gcpm-3", "#document-page-selectors")]
    [Spec("css-gcpm-3", "#document-sequence-selectors")]
    public void NthPageSelector_IsAccepted(string css)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet(css);

        Assert.AreEqual(1, sheet.Rules.OfType<PageRule>().Count());
    }

    private static (CssBox Root, HtmlContainerInt Container) Paginate(string body) =>
        LayoutHarness.Layout($"<html><head></head><body style='margin:0'>{body}</body></html>", 400, pageHeight: PageHeight, margin: 0);

    private static CssBox Find(CssBox root, string id) => LayoutHarness.FindById(root, id)!;
}
