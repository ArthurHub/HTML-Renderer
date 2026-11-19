using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.BoxModel;

/// <summary>
/// <c>&lt;hr&gt;</c> sizes itself but is placed by the frame above it like any other block,
/// <c>CssBoxHr.PerformLayoutImp</c> has its own copy of the top placement of <c>CssBox.PerformLayoutImp</c>.
/// </summary>
/// <remarks>
/// One quirk specific to <c>CssBoxHr</c>: <c>CssBox.MarginTopCollapse</c> (<c>Core/Dom/CssBox.cs</c> ~1103-1127)
/// has an explicit "fix for hr tag" - whenever the collapsed top-margin value would come out under 0.1px AND the
/// element is an <c>&lt;hr&gt;</c>, it is forcibly replaced with <c>GetEmHeight() * 1.1</c>, regardless of
/// whether that near-zero value came from an explicit <c>margin:0</c> or just an unset default. This applies
/// identically to every fixture below that uses <c>margin:0</c> on the rule, so it does not by itself break any
/// A-vs-B comparison (it adds the same constant to both sides), but it does mean the rule never actually sits
/// flush against its predecessor even when the CSS explicitly asks for that.
/// </remarks>
[DoNotParallelize]
[TestClass]
public sealed class HrPlacementTests
{
    private const double Delta = 1.0;

    [TestMethod]
    public void ARelativelyPositionedPredecessor_DoesNotDragTheRuleWithIt()
    {
        // CSS 2.1 §9.4.3: relative positioning is purely visual - the offset must not affect where a
        // following sibling lays out. CssBoxHr.PerformLayoutImp reads prevSibling.StaticBottom (which backs
        // the offset back out), not ActualBottom, so the rule ends up in the same place whether or not its
        // predecessor is relatively positioned.
        var (staticRoot, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px'></div><hr id='h' style='margin:0'>"));
        var (offsetRoot, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px;position:relative;top:20px'></div><hr id='h' style='margin:0'>"));

        Assert.AreEqual(
            LayoutHarness.FindById(staticRoot, "h")!.Location.Y,
            LayoutHarness.FindById(offsetRoot, "h")!.Location.Y,
            Delta);
    }

    [TestMethod]
    public void AFloatedPredecessor_IsNotTheRulesMarginCollapsePartner()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px;margin-bottom:30px'></div>"
            + "<div style='float:left;width:10px;height:10px;margin-bottom:5px'></div>"
            + "<hr id='h' style='margin-top:10px'>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var h = LayoutHarness.FindById(root, "h")!;

        Assert.AreEqual(30, h.Location.Y - a.ActualBottom, Delta);
    }

    [TestMethod]
    [Spec("html", "rendering.html#the-hr-element-2")]
    [Spec("css21", "visudet.html#blockwidth")]
    public void TheRule_StartsAtTheContentEdgeOfItsContainingBlockPlusItsMargin()
    {
        var (plain, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:200px'><hr id='h' style='margin:0'></div>"));
        var (padded, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:200px;padding:0 10px;border:solid black;border-width:0 5px'><hr id='h' style='margin:0'></div>"));
        var (indented, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:200px'><hr id='h' style='margin:0 30px 0 20px'></div>"));

        var p = LayoutHarness.FindById(plain, "h")!;
        var q = LayoutHarness.FindById(padded, "h")!;
        var i = LayoutHarness.FindById(indented, "h")!;

        Assert.AreEqual(0, p.Location.X, Delta);
        Assert.AreEqual(15, q.Location.X, Delta);
        Assert.AreEqual(20, i.Location.X, Delta);
        Assert.AreEqual(p.Size.Width, q.Size.Width, Delta);
        Assert.AreEqual(p.Size.Width - 50, i.Size.Width, Delta);
    }

    [TestMethod]
    [Spec("html", "rendering.html#the-hr-element-2")]
    [Spec("css21", "visudet.html#normal-block")]
    [DataRow("", 2)]
    [DataRow("height:10px", 12)]
    [DataRow("padding:5px 0", 12)]
    [DataRow("border:3px solid", 6)]
    public void TheRule_IsAsTallAsItsHeightPaddingAndBorders(string style, double height)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            $"<div style='width:200px'><hr id='h' style='margin:0;{style}'></div>"));

        var h = LayoutHarness.FindById(root, "h")!;

        Assert.AreEqual(height, h.ActualBottom - h.Location.Y, 0.01);
    }

    [Ignore("Gap, see Spec/html.yml")]
    [TestMethod]
    [Spec("html", "rendering.html#the-hr-element-2")]
    public void TheRuleWithoutBorders_HasNoHeight()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:200px'><hr id='h' style='margin:0;border:0'></div>"));

        var h = LayoutHarness.FindById(root, "h")!;

        Assert.AreEqual(0, h.ActualBorderTopWidth);
        Assert.AreEqual(0, h.ActualBottom - h.Location.Y, 0.01);
    }

    [Ignore("Gap, see Spec/html.yml")]
    [TestMethod]
    [Spec("html", "rendering.html#the-hr-element-2")]
    public void TheRuleWithAZeroTopMargin_SitsAtTheTopOfItsContainingBlock()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:200px'><hr id='h' style='margin:0'></div>"));

        Assert.AreEqual(0, LayoutHarness.FindById(root, "h")!.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/html.yml")]
    [TestMethod]
    [Spec("html", "rendering.html#the-hr-element-2")]
    public void AutoWidthRule_BorderBoxFillsTheContainingBlock()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:200px'><hr id='h' style='margin:0'></div>"));

        var h = LayoutHarness.FindById(root, "h")!;

        Assert.AreEqual(200, h.ActualRight - h.Location.X, 0.01);
    }

    [Ignore("Gap, see Spec/html.yml")]
    [TestMethod]
    [Spec("html", "rendering.html#the-hr-element-2")]
    public void PercentageWidth_IsRelativeToTheContainingBlock()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:200px'><hr id='h' style='margin:0;width:50%'></div>"));

        var h = LayoutHarness.FindById(root, "h")!;

        Assert.AreEqual(102, h.ActualRight - h.Location.X, 0.01);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#block-formatting")]
    public void ABorderedPredecessorsBottomBorder_IsNotCountedTwice()
    {
        var (borderless, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px'></div><hr id='h' style='margin:0'>"));
        var (bordered, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px;border-bottom:10px solid black'></div><hr id='h' style='margin:0'>"));

        var plainGap = LayoutHarness.FindById(borderless, "h")!.Location.Y
                       - LayoutHarness.FindById(borderless, "a")!.ActualBottom;
        var borderedGap = LayoutHarness.FindById(bordered, "h")!.Location.Y
                          - LayoutHarness.FindById(bordered, "a")!.ActualBottom;

        Assert.AreEqual(plainGap, borderedGap, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    [Spec("css-break-3", "#break-margins")]
    public void AnOversizedTopMarginAdjoiningAnUnforcedBreak_IsTruncated()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px'></div><hr id='h' style='margin-top:300px'>"),
            maxHeight: 200, margin: 20);

        var h = LayoutHarness.FindById(root, "h")!;

        Assert.AreEqual(container.PageTopOf(1), h.Location.Y, Delta * 3);
    }

    [TestMethod]
    public void TheRule_StillSpansItsContainingBlocksContentWidth()
    {
        // 'box' uses the default box-sizing:content-box, so its own width:200px is the CONTENT width - the
        // containing block the auto-width <hr> fills is 200px wide (padding is added on top, not subtracted
        // from it). The <hr> then loses 2px off that 200px to its own UA-default 1px left/right border
        // (CssDefaults' "hr { ... border: 1px inset }"-equivalent), landing at 198, not at 200 - and NOT at
        // 180 (200 minus the parent's 20px of padding), which would only be correct under border-box sizing.
        // Verified empirically against the built assembly: box.ActualWidth is 200 (content-box), box.ClientLeft/
        // ClientRight span exactly 10..210, and h.ActualBorderLeftWidth/RightWidth are each 1.
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='box' style='width:200px;padding:0 10px'><hr id='h' style='margin:0'></div>"));

        var h = LayoutHarness.FindById(root, "h")!;
        var box = LayoutHarness.FindById(root, "box")!;

        Assert.AreEqual(box.ClientLeft, h.Location.X, Delta * 3);
        Assert.AreEqual(198, h.ActualRight - h.Location.X, Delta * 3);
    }
}
