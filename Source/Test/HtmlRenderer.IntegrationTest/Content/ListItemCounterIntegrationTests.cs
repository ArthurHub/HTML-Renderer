using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Content;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/ListItemCounterIntegrationTests.cs - the CSS2.1-relevant subset
/// this fork's simpler, non-CSS-counter-driven list-marker mechanism (<c>CssBox.GetIndexForList</c>) can
/// support: <c>&lt;ol start&gt;</c>/<c>reversed</c> (already implemented) and <c>&lt;li value&gt;</c> (newly
/// added by this port). CSS3 marker-styling extras from the source file are not ported.
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class ListItemCounterIntegrationTests
{
    [TestMethod]
    public void OlStart_OffsetsEveryItem()
    {
        var html = LayoutHarness.Wrap("<ol start='5'><li id='a'>x</li><li id='b'>y</li></ol>");
        var (root, _) = LayoutHarness.Layout(html);

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual("5.", a.ListItemBox.Words[0].Text);
        Assert.AreEqual("6.", b.ListItemBox.Words[0].Text);
    }

    [TestMethod]
    public void OlReversed_CountsDownFromItemCount()
    {
        var html = LayoutHarness.Wrap("<ol reversed='reversed'><li id='a'>x</li><li id='b'>y</li><li id='c'>z</li></ol>");
        var (root, _) = LayoutHarness.Layout(html);

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;
        var c = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual("3.", a.ListItemBox.Words[0].Text);
        Assert.AreEqual("2.", b.ListItemBox.Words[0].Text);
        Assert.AreEqual("1.", c.ListItemBox.Words[0].Text);
    }

    [TestMethod]
    public void LiValue_OverridesThatItemAndContinuesFromThere()
    {
        var html = LayoutHarness.Wrap(
            "<ol><li id='a'>x</li><li id='b' value='10'>y</li><li id='c'>z</li></ol>");
        var (root, _) = LayoutHarness.Layout(html);

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;
        var c = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual("1.", a.ListItemBox.Words[0].Text);
        Assert.AreEqual("10.", b.ListItemBox.Words[0].Text);
        Assert.AreEqual("11.", c.ListItemBox.Words[0].Text);
    }

    [TestMethod]
    public void ListStyleTypeSquare_UsesAFilledSquareGlyph_NotASpadeSuitSymbol()
    {
        // Regression: this used to render "♠" (U+2660 BLACK SPADE SUIT), not a square at all.
        var html = LayoutHarness.Wrap("<ul style='list-style-type:square'><li id='a'>x</li></ul>");
        var (root, _) = LayoutHarness.Layout(html);
        var a = LayoutHarness.FindById(root, "a")!;

        Assert.AreEqual("▪", a.ListItemBox.Words[0].Text);
    }
}
