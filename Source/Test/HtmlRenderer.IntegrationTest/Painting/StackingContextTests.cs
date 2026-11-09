using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Painting;

/// <summary>
/// Ported from PeachPDF.Tests' Acid2FeatureVerificationTests.cs (the z-index/stacking-context section).
/// CSS 2.1 §9.9/Appendix E: a positioned box's <c>z-index</c> establishes a stacking context, and boxes in a
/// higher stacking context paint after (on top of) boxes in a lower one, regardless of document/source order -
/// e.g. a <c>position:relative; z-index:2</c> box must paint over a <c>position:fixed</c> box declared later
/// in the document.
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class StackingContextTests
{
    [Ignore("z-index and stacking-context paint order aren't implemented yet, boxes paint in document order")]
    [TestMethod]
    public void PositionedZIndex_PaintsOverFixedPositionedContent()
    {
        // The white z-index:2 box must paint on top of the black fixed bar, even though the painter
        // currently draws the fixed bar after the normal flow.
        var html = LayoutHarness.Wrap(
            "<div id='fixedbar' style='position:fixed; top:0; left:0; width:50px; height:50px; background:rgb(0,0,0);'></div>"
            + "<div id='intro' style='position:relative; z-index:2; width:50px; height:50px; background:rgb(255,255,255);'></div>");

        var (_, container) = PaintHarness.Layout(html);
        var recorder = PaintHarness.PaintPage(container);

        var drawRectCalls = recorder.Log.OfType<RecordingGraphics.DrawRectCall>().ToList();
        var fixedBarPaintIndex = drawRectCalls.FindIndex(c => c.Color.R == 0 && c.Color.G == 0 && c.Color.B == 0);
        var introPaintIndex = drawRectCalls.FindIndex(c => c.Color.R == 255 && c.Color.G == 255 && c.Color.B == 255);

        Assert.IsGreaterThanOrEqualTo(0, fixedBarPaintIndex);
        Assert.IsGreaterThanOrEqualTo(0, introPaintIndex);
        Assert.IsTrue(fixedBarPaintIndex < introPaintIndex,
            "the z-index:2 box must paint after (on top of) the fixed bar, regardless of document order");
    }
}
