using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Text;

/// <summary>
/// The expected positions come from the real font metrics, where <c>UnderlineOffset</c> is the distance from
/// the top of the text to its baseline.
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class LineBaselineIntegrationTests
{
    private const double Delta = 0.5;

    [TestMethod]
    [Spec("css21", "visudet.html#leading")]
    public void SmallerText_SitsOnTheBaselineOfTheLargerText()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='font-size:20px'><span id='big'>Big</span> <span id='small' style='font-size:10px'>small</span></div>"));
        var big = FirstWord(LayoutHarness.FindById(root, "big")!)!;
        var small = FirstWord(LayoutHarness.FindById(root, "small")!)!;

        Assert.IsGreaterThan(big.Top, small.Top, "the smaller text must not hang from the top of the line");
        Assert.AreEqual(big.Top + Ascent(big), small.Top + Ascent(small), Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#leading")]
    public void SmallerText_SitsOnTheBaselineOfTheBlocksOwnFont()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='x' style='font-size:20px'><span id='small' style='font-size:10px'>small</span></div>"));
        var block = LayoutHarness.FindById(root, "x")!;
        var small = FirstWord(LayoutHarness.FindById(root, "small")!)!;

        Assert.AreEqual(block.Location.Y + block.ActualFont.UnderlineOffset, small.Top + Ascent(small), Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#leading")]
    public void LineWithOnlySmallerText_IsAsTallAsTheBlocksFont()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='x' style='font-size:20px'><span style='font-size:10px'>small</span></div>"));
        var block = LayoutHarness.FindById(root, "x")!;

        Assert.AreEqual(block.ActualFont.Height, block.ActualBottom - block.Location.Y, Delta);
    }

    private static double Ascent(CssRect word) => word.OwnerBox.ActualFont.UnderlineOffset;

    private static CssRect? FirstWord(CssBox box)
    {
        if (box.Words.Count > 0)
            return box.Words[0];

        foreach (var child in box.Boxes)
        {
            var word = FirstWord(child);
            if (word != null)
                return word;
        }

        return null;
    }
}
