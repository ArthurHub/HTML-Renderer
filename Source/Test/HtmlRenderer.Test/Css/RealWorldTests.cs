using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/RealWorld.cs.
/// PeachPDF's version parses a stylesheet with its CSS engine's <c>StylesheetParser</c> and reads the
/// parsed rule's properties back as CSSOM-normalized strings. HTML-Renderer's CSS engine port removed the
/// old raw-bucket <c>CssData.GetCssBlock</c> introspection API this file originally used entirely -
/// <c>CssData</c> is now purely a rule index queried during the real box-tree cascade (see
/// <c>CssData.GetStyleRules</c>), not a standalone lookup surface. So instead of parsing a bare
/// stylesheet and reading back its raw property strings, this test now drives the whole real pipeline
/// end to end via <see cref="LayoutHarness"/> - <c>SetHtml</c>, layout, then read the resolved values off
/// the laid-out <see cref="CssBox"/>es (<c>ActualBackgroundColor</c>/<c>ActualColor</c>/
/// <c>ActualMargin*</c>), which is the closest real equivalent to "does this cascade actually resolve the
/// way the stylesheet says it should".
/// The second PeachPDF test (CreateStylesheet_WithCssProperties_ExpectStandardStringBack) round-trips a
/// rule back through <c>ToCss()</c> - there is no CSS serialization (CSSOM) surface exposed on this fork,
/// so that test still has no equivalent here and remains dropped.
/// </summary>
[TestClass]
public sealed class RealWorldTests
{
    [TestMethod]
    public void ParseCss_WithStandardString_ExpectReadableProperties()
    {
        const string css = "html{ background-color: #5a5eed; color: #FFFFFF; margin: 5px; } h2{ background-color: red }";
        var html = $"<!DOCTYPE html><html><head><style>{css}</style></head><body><h2>Text</h2></body></html>";

        var (root, _) = LayoutHarness.Layout(html);

        var htmlBox = FindByTag(root, "html");
        var h2Box = FindByTag(root, "h2");

        Assert.AreEqual(RColor.FromArgb(90, 94, 237), htmlBox.ActualBackgroundColor);
        Assert.AreEqual(RColor.FromArgb(255, 255, 255), htmlBox.ActualColor);
        Assert.AreEqual(5d, htmlBox.ActualMarginLeft);
        Assert.AreEqual(5d, htmlBox.ActualMarginTop);
        Assert.AreEqual(5d, htmlBox.ActualMarginRight);
        Assert.AreEqual(5d, htmlBox.ActualMarginBottom);

        Assert.AreEqual(RColor.FromArgb(255, 0, 0), h2Box.ActualBackgroundColor);
    }

    [TestMethod]
    [DataRow("#a { --x: \"<\"; } p { color: red }")]
    [DataRow("p::before { content: \"<\" } p { color: red }")]
    [DataRow("@media (width < 100000px) { p { color: red } }")]
    public void StyleElement_TextWithLessThanSign_IsOneStyleSheet(string css)
    {
        var (root, _) = LayoutHarness.Layout($"<html><head><style>{css}</style></head><body><p>Text</p></body></html>");

        Assert.AreEqual(RColor.FromArgb(255, 0, 0), FindByTag(root, "p").ActualColor);
    }

    private static CssBox FindByTag(CssBox root, string tag)
    {
        var box = LayoutHarness.Descendants(root).FirstOrDefault(b => b.HtmlTag != null && b.HtmlTag.Name == tag);
        Assert.IsNotNull(box);
        return box!;
    }
}
