using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// The stylesheet is checked through the cascaded values of the laid-out boxes.
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
