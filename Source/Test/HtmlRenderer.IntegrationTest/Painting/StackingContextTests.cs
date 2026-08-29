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
    [Ignore("z-index/stacking-context paint order is not implemented on this fork: FragmentPainter.cs's own " +
            "class remarks explicitly document it as deferred (\"Stacking-context paint order and " +
            "box-decoration-break slicing are follow-on work\"), and CssBoxProperties has no ZIndex field at " +
            "all - the CSS-OM parses z-index (CssEngine/StyleProperties/Flow/ZIndexProperty.cs) but " +
            "CssUtils.SetPropertyValue never dispatches it onto a box, so it has zero effect on paint order. " +
            "This box tree currently paints in plain document order (normal flow, then absolute/fixed, per " +
            "FragmentPainter's child-iteration order) regardless of any z-index value - a position:relative " +
            "z-index:2 box painting over a LATER position:fixed sibling (this test's whole premise) is exactly " +
            "the case document order alone cannot produce, so this reliably fails rather than passing by " +
            "accident. Implementing real stacking-context ordering (a ZIndex box property, plus grouping/" +
            "sorting descendants by stacking context per CSS2.1 Appendix E) is a separate, larger feature port.")]
    [TestMethod]
    public void PositionedZIndex_PaintsOverFixedPositionedContent()
    {
        // A black position:fixed bar declared AFTER (later in the box tree than) a white
        // position:relative;z-index:2 box must still be painted BEFORE it (i.e. underneath).
        var html = LayoutHarness.Wrap(
            "<div id='fixedbar' style='position:fixed; top:0; left:0; width:50px; height:50px; background:rgb(0,0,0);'></div>"
            + "<div id='intro' style='position:relative; z-index:2; width:50px; height:50px; background:rgb(255,255,255);'></div>");

        var (root, container) = PaintHarness.Layout(html);
        var recorder = PaintHarness.PaintPage(container);

        var fixedBar = PaintHarness.FindById(root, "fixedbar")!;
        var intro = PaintHarness.FindById(root, "intro")!;

        var drawRectCalls = recorder.Log.OfType<RecordingGraphics.DrawRectCall>().ToList();
        var fixedBarPaintIndex = drawRectCalls.FindIndex(c => c.X == fixedBar.Location.X && c.Y == fixedBar.Location.Y);
        var introPaintIndex = drawRectCalls.FindIndex(c => c.X == intro.Location.X && c.Y == intro.Location.Y);

        Assert.IsTrue(fixedBarPaintIndex < introPaintIndex,
            "the z-index:2 box must paint after (on top of) the fixed bar, regardless of document order");
    }
}
