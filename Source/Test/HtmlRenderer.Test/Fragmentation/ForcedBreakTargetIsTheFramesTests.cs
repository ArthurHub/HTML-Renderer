using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragmentation;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace HtmlRenderer.Test.Fragmentation;

/// <summary>
/// Where <see cref="BlockFragmentation.TryGetForcedBreakTarget"/> puts a box with a forced break, compared
/// with where layout actually placed it.
/// </summary>
[TestClass]
public sealed class ForcedBreakTargetIsTheFramesTests
{
    // Sheet height 300 with 20 margin top and bottom, LayoutHarness takes the 260 content band.
    private const double PageHeight = 300;
    private const double Margin = 20;
    private const double Band = PageHeight - 2 * Margin;

    private static double SlotTop(int slot) => Margin + slot * Band;

    /// <summary>The target the frame resolves for <paramref name="id"/>, asked after layout.</summary>
    private static double? TargetFor(CssBox root, string id)
    {
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box);

        var prevSibling = DomUtils.GetPreviousSibling(box!);
        if (prevSibling is null)
            return null;

        var baseTopWithoutMargin = prevSibling.ActualBottom;
        return BlockFragmentation.TryGetForcedBreakTarget(box!, prevSibling, baseTopWithoutMargin, out _, out var targetTop)
            ? targetTop
            : null;
    }

    [TestMethod]
    [Spec("css21", "page.html#forced")]
    [Spec("css-break-3", "#forced-breaks")]
    [Spec("css-break-4", "#forced-breaks")]
    public void PlainForcedBreak_TargetsTheNextSlotsContentTop_AndIsWhereTheBoxLanded()
    {
        var (root, container) = LayoutHarness.Layout(
            LayoutHarness.Wrap(
                "<div id='first' style='height:50px;margin:0'>first</div>"
                + "<div id='second' style='height:50px;margin:0;break-before:page'>second</div>"),
            pageHeight: Band, margin: Margin);

        Assert.AreEqual(container.PageTopOf(1), TargetFor(root, "second")!.Value, 1e-6);
        Assert.AreEqual(SlotTop(1), TargetFor(root, "second")!.Value, 1e-6);
        Assert.AreEqual(SlotTop(1), LayoutHarness.FindById(root, "second")!.Location.Y, 1e-6);
    }

    // A predecessor ending flush on a page boundary already satisfies the break, no target is reported.
    [TestMethod]
    [Spec("css21", "page.html#forced")]
    [Spec("css-break-3", "#forced-breaks")]
    [Spec("css-break-4", "#forced-breaks")]
    public void PredecessorEndingFlushOnABoundary_TargetsThatBoundary_NotTheSlotAfterIt()
    {
        var (root, container) = LayoutHarness.Layout(
            LayoutHarness.Wrap(
                $"<div id='first' style='height:{Band}px;margin:0'>first</div>"
                + "<div id='second' style='height:50px;margin:0;break-before:page'>second</div>"),
            pageHeight: Band, margin: Margin);

        // The first box occupies the whole of slot 0 and ends exactly where slot 1 begins.
        Assert.AreEqual(SlotTop(1), LayoutHarness.FindById(root, "first")!.ActualBottom, 1e-6);

        // Slot 1, not slot 2, no page is skipped.
        Assert.IsNull(TargetFor(root, "second"));
        Assert.AreEqual(SlotTop(1), LayoutHarness.FindById(root, "second")!.Location.Y, 1e-6);
        Assert.AreEqual(2, container.FragmentTree!.Fragmentainers.Count);
    }

    // A zero-height marker moved to a page top by its own forced break, followed by another forced
    // break, should leave a blank page.
    [Ignore("Gap, see Spec/css-break-3.yml")]
    [TestMethod]
    [Spec("css-break-3", "#forced-breaks")]
    [Spec("css-break-4", "#forced-breaks")]
    public void ConsecutiveForcedBreaks_StepPastTheMarkerRatherThanCollapsingOntoIt()
    {
        var (root, container) = LayoutHarness.Layout(
            LayoutHarness.Wrap(
                "<div id='first' style='height:50px;margin:0'>first</div>"
                + "<div id='marker' style='margin:0;break-before:page'></div>"
                + "<div id='second' style='height:50px;margin:0;break-before:page'>second</div>"),
            pageHeight: Band, margin: Margin);

        var marker = LayoutHarness.FindById(root, "marker");
        Assert.IsNotNull(marker);

        // The marker took its own break to slot 1's top and contributes no height of its own.
        Assert.AreEqual(SlotTop(1), marker!.Location.Y, 1e-6);
        Assert.AreEqual(marker.Location.Y, marker.ActualBottom, 1e-6);

        Assert.AreEqual(container.PageTopOf(2), TargetFor(root, "second")!.Value, 1e-6);
        Assert.AreEqual(SlotTop(2), LayoutHarness.FindById(root, "second")!.Location.Y, 1e-6);
    }

    // The margin-top after a forced break is retained.
    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    [Spec("css-break-3", "#break-margins")]
    [Spec("css-break-4", "#break-margins")]
    public void TargetIsTheBoundary_AndThePreservedMarginIsAddedToIt()
    {
        var (root, container) = LayoutHarness.Layout(
            LayoutHarness.Wrap(
                "<div id='first' style='height:50px;margin:0'>first</div>"
                + "<div id='second' style='height:50px;margin:0;margin-top:30px;break-before:page'>second</div>"),
            pageHeight: Band, margin: Margin);

        Assert.AreEqual(container.PageTopOf(1), TargetFor(root, "second")!.Value, 1e-6);
        Assert.AreEqual(SlotTop(1) + 30, LayoutHarness.FindById(root, "second")!.Location.Y, 1e-6);
    }

    // The break before a container's first in-flow child is also the one before the container.
    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "page.html#forced")]
    [Spec("css-break-3", "#break-propagation")]
    [Spec("css-break-4", "#break-propagation")]
    public void BreakBeforeAFirstInFlowChild_IsTakenByTheContainerItBegins()
    {
        var (root, container) = LayoutHarness.Layout(
            LayoutHarness.Wrap(
                "<div id='first' style='height:50px;margin:0'>first</div>"
                + "<div id='wrapper' style='margin:0'>"
                + "<div id='second' style='height:50px;margin:0;break-before:page'>second</div>"
                + "</div>"),
            pageHeight: Band, margin: Margin);

        Assert.AreEqual(container.PageTopOf(1), TargetFor(root, "wrapper")!.Value, 1e-6);
        Assert.IsNull(TargetFor(root, "second"));
        Assert.AreEqual(SlotTop(1), LayoutHarness.FindById(root, "wrapper")!.Location.Y, 1e-6);
        Assert.AreEqual(SlotTop(1), LayoutHarness.FindById(root, "second")!.Location.Y, 1e-6);
    }

    // Nothing precedes the box, so there is no break to take and no leading blank page.
    [TestMethod]
    [Spec("css-break-3", "#break-propagation")]
    [Spec("css-break-4", "#break-propagation")]
    public void BoxThatBeginsTheFlow_HasNoTargetAtAll()
    {
        var (root, container) = LayoutHarness.Layout(
            LayoutHarness.Wrap(
                "<div id='second' style='height:50px;margin:0;break-before:page'>second</div>"),
            pageHeight: Band, margin: Margin);

        Assert.IsNull(TargetFor(root, "second"));
        Assert.AreEqual(1, container.FragmentTree!.Fragmentainers.Count);
    }

    [TestMethod]
    public void BoxWithNoForcedBreak_HasNoTarget()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap(
                "<div id='first' style='height:50px;margin:0'>first</div>"
                + "<div id='second' style='height:50px;margin:0'>second</div>"),
            pageHeight: Band, margin: Margin);

        Assert.IsNull(TargetFor(root, "second"));
    }

    // CSS 2.1 §9.4.3: a relative offset doesn't affect the flow, so it doesn't move the break either.
    [TestMethod]
    [DataRow("top: -40px")]
    [DataRow("top: 40px")]
    public void RelativelyOffsetPredecessor_DoesNotMoveTheTarget(string offset)
    {
        var (root, container) = LayoutHarness.Layout(
            LayoutHarness.Wrap(
                $"<div id='first' style='height:50px;margin:0;position:relative;{offset}'>first</div>"
                + "<div id='second' style='height:50px;margin:0;break-before:page'>second</div>"),
            pageHeight: Band, margin: Margin);

        Assert.AreEqual(container.PageTopOf(1), TargetFor(root, "second")!.Value, 1e-6);
        Assert.AreEqual(SlotTop(1), LayoutHarness.FindById(root, "second")!.Location.Y, 1e-6);
    }
}
