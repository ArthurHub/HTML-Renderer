using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.BoxModel;

/// <summary>
/// Ported from PeachPDF.Tests' Acid2 min-height/max-height regression cases (CSS 2.1 §10.7).
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class MinMaxHeightIntegrationTests
{
    private const double Delta = 0.5;

    [TestMethod]
    public void MinHeightWinsOverConflictingMaxHeight()
    {
        // min-height (20px) and max-height (5px) conflict; min-height must win per §10.7, so the box's
        // actual height must be 20px, not clamped down to 5px.
        var html = LayoutHarness.Wrap("<div id='b' style='height:8px; min-height:20px; max-height:5px;'></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(20, box.ActualBottom - box.Location.Y, Delta);
    }

    [TestMethod]
    public void MaxHeightAppliesWhenNotConflictingWithMinHeight()
    {
        var html = LayoutHarness.Wrap("<div id='b' style='height:100px; max-height:20px;'></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(20, box.ActualBottom - box.Location.Y, Delta);
    }

    [TestMethod]
    public void PercentageHeight_AgainstAutoHeightContainingBlock_ResolvesToAuto_LettingMaxHeightWin()
    {
        // "height: 60%" must resolve to auto (zero intrinsic content) against an auto-height containing
        // block (CSS 2.1 §10.5/§10.7), not against the container's own content-driven height - so
        // max-height:20px is the only thing constraining #b's height.
        var html = LayoutHarness.Wrap(
            "<div id='cb'>"
            + "<div id='content' style='height:50px;'></div>"
            + "<div id='b' style='height:60%; max-height:20px;'></div>"
            + "</div>");
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.FindById(root, "b")!;

        var height = box.ActualBottom - box.Location.Y;
        Assert.IsTrue(height is >= 0 and <= 20.5, $"expected height clamped to max-height:20px, was {height}");
    }
}
