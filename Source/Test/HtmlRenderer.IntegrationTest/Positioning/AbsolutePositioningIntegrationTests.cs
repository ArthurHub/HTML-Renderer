using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Positioning;

/// <summary>
/// Ported from PeachPDF.Tests' Acid2FeatureVerificationTests.cs (the position:relative/absolute/fixed offset
/// section) and AbsolutePositioningIntegrationTests.cs's shrink-to-fit cases. CSS 2.1 §9.4.3: for each axis of
/// a relatively/absolutely positioned box, the "near" offset (<c>left</c>/<c>top</c>) wins when set; if it's
/// <c>auto</c> and the "far" offset (<c>right</c>/<c>bottom</c>) isn't, the far offset applies with its sign
/// flipped. §9.4.3 also requires relative positioning to be purely visual - it must not affect the parent's
/// content-driven height or any following sibling's layout. §10.3.7: an absolutely positioned box's offsets
/// are measured from its nearest positioned ancestor's PADDING edge, and (like every other positioning scheme)
/// its own margin still applies on top of that offset; with no explicit width, it shrinks to fit its content.
/// <para>
/// The PeachPDF source file's flexbox/grid blockification cases and detached-&lt;thead&gt;/&lt;tfoot&gt;
/// containing-block cases are not ported: this fork has neither a flex/grid layout engine nor the notion of a
/// detached header/footer proxy box those target.
/// </para>
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class AbsolutePositioningIntegrationTests
{
    private const double Delta = 1.0;

    [TestMethod]
    public void PositionRelative_BottomOffset_MovesBoxOppositeDirection()
    {
        // top is auto, bottom is set - a positive "bottom" pulls the box UP, i.e. subtracts from Y.
        var html = LayoutHarness.Wrap("<div id='t' style='position:relative; bottom:10px; width:10px; height:10px;'></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.FindById(root, "t")!;

        // Static-flow position is Y=0 (LayoutHarness.Wrap sets body margin:0); "bottom:10px" must move it to Y=-10.
        Assert.AreEqual(-10, box.Location.Y, Delta);
    }

    [TestMethod]
    public void PositionRelative_Offset_DoesNotAffectParentHeightOrFollowingSibling()
    {
        // The offset box (and its descendants) move, but the parent's content-driven height and every
        // following sibling must lay out against the STATIC position.
        var html = LayoutHarness.Wrap(
            "<div id='parent'>"
            + "<div id='shifted' style='position:relative; bottom:-30px; height:40px;'></div>"
            + "</div>"
            + "<div id='after' style='height:10px;'></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var parent = LayoutHarness.FindById(root, "parent")!;
        var shifted = LayoutHarness.FindById(root, "shifted")!;
        var after = LayoutHarness.FindById(root, "after")!;

        // The offset itself is applied visually: the child sits 30px below the parent's top...
        Assert.AreEqual(30, shifted.Location.Y - parent.Location.Y, Delta);

        // ...but the parent is still exactly 40px tall (the child's static extent)...
        Assert.AreEqual(40, parent.ActualBottom - parent.Location.Y, Delta);

        // ...and the following sibling starts at the parent's un-inflated bottom.
        Assert.AreEqual(parent.ActualBottom, after.Location.Y, Delta);
    }

    [TestMethod]
    public void PositionRelative_OffsetOnBoxItself_DoesNotShiftFollowingSibling()
    {
        // "after" must lay out against "shifted"'s static bottom, not its visually offset bottom 25px lower.
        var html = LayoutHarness.Wrap(
            "<div id='shifted' style='position:relative; top:25px; height:40px;'></div>"
            + "<div id='after' style='height:10px;'></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var shifted = LayoutHarness.FindById(root, "shifted")!;
        var after = LayoutHarness.FindById(root, "after")!;

        Assert.AreEqual(25, shifted.RelativeOffsetY, Delta);
        Assert.AreEqual(shifted.ActualBottom - 25, after.Location.Y, Delta);
    }

    [TestMethod]
    public void PositionAbsolute_BottomOffset_PositionsRelativeToContainingBlockBottomEdge()
    {
        var html = LayoutHarness.Wrap(
            "<div id='cb' style='position:relative; width:100px; height:100px;'>"
            + "<div id='t' style='position:absolute; bottom:10px; width:10px; height:10px;'></div></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var cb = LayoutHarness.FindById(root, "cb")!;
        var box = LayoutHarness.FindById(root, "t")!;

        // Box's bottom edge must sit 10px above the containing block's own bottom (padding) edge.
        Assert.AreEqual(cb.ActualBottom - 10, box.ActualBottom, Delta);
    }

    [TestMethod]
    public void PositionAbsolute_WithMarginAndBorderedContainingBlock_AppliesBothCorrectly()
    {
        var html = LayoutHarness.Wrap(
            "<div id='cb' style='position:relative; border:16px solid black; width:100px; height:100px;'>"
            + "<div id='t' style='position:absolute; top:0; left:0; margin:36px 0 0 60px; width:10px; height:10px;'></div></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var cb = LayoutHarness.FindById(root, "cb")!;
        var box = LayoutHarness.FindById(root, "t")!;

        // Expected: containing block's PADDING edge (border-box + 16px border) + the box's own margin.
        Assert.AreEqual(cb.Location.X + 16 + 60, box.Location.X, Delta);
        Assert.AreEqual(cb.Location.Y + 16 + 36, box.Location.Y, Delta);
    }

    [TestMethod]
    public void PositionFixed_WithMargin_AppliesMarginOnTopOfOffset()
    {
        var html = LayoutHarness.Wrap(
            "<div id='t' style='position:fixed; top:10px; left:20px; margin:5px 0 0 8px; width:10px; height:10px;'></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.FindById(root, "t")!;

        Assert.AreEqual(20 + 8, box.Location.X, Delta);
        Assert.AreEqual(10 + 5, box.Location.Y, Delta);
    }

    [TestMethod]
    public void PositionAbsoluteAutoWidth_ShrinksToWidestChild_NotSumOfSiblingBorders()
    {
        // Three siblings under an absolutely-positioned, auto-width parent: #text (real content, ~short),
        // #border1 (80px combined border, no content), #border2 (60px combined border, no content) - the
        // correct shrink-to-fit width is #border1's own ~80px (the widest single line), not #border1 +
        // #border2's borders summed together (~140px).
        var html = LayoutHarness.Wrap(
            "<div id='container' style='position:relative; width:400px;'>"
            + "<div id='target' style='position:absolute;'>"
            + "<div id='text'>Hi</div>"
            + "<div id='border1' style='border-left:40px solid black; border-right:40px solid black;'></div>"
            + "<div id='border2' style='border-left:30px solid black; border-right:30px solid black;'></div>"
            + "</div></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var target = LayoutHarness.FindById(root, "target")!;

        var targetWidth = target.ActualRight - target.Location.X;

        // Allow a little headroom above 80 for #text's own (much smaller) content contribution.
        Assert.IsTrue(targetWidth is >= 79 and <= 100,
            $"expected shrink-to-fit width near 80px (the widest single sibling), was {targetWidth}");
    }

    [TestMethod]
    public void PositionAbsoluteAutoWidth_MultipleExplicitWidthSiblings_TakesWidestNotSum()
    {
        var html = LayoutHarness.Wrap(
            "<div id='container' style='position:relative; width:400px;'>"
            + "<div id='target' style='position:absolute;'>"
            + "<div id='wide1' style='width:100px; height:10px;'></div>"
            + "<div id='wide2' style='width:90px; height:10px;'></div>"
            + "</div></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var target = LayoutHarness.FindById(root, "target")!;

        var targetWidth = target.ActualRight - target.Location.X;

        // The widest single sibling (100px) should win - the buggy summed-across-siblings result would be
        // at least 100+90=190px.
        Assert.IsTrue(targetWidth is >= 99 and <= 105,
            $"expected shrink-to-fit width near 100px (the widest single sibling), was {targetWidth}");
    }

    [TestMethod]
    public void PositionAbsoluteAutoWidth_NonReplacedInlineChildsExplicitWidth_HasNoEffect()
    {
        // Per CSS2.1 10.3.3, `width` has no effect on a non-replaced inline-level box - its explicit width
        // must not be folded into an ancestor's shrink-to-fit computation.
        var html = LayoutHarness.Wrap(
            "<div id='container' style='position:relative; width:400px;'>"
            + "<div id='target' style='position:absolute;'>"
            + "<span id='inlineWide' style='display:inline; width:200px;'></span>"
            + "</div></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var target = LayoutHarness.FindById(root, "target")!;

        var targetWidth = target.ActualRight - target.Location.X;

        // The inline child's own "width:200px" must be ignored - target should shrink to ~0 (no real
        // content), not inflate to 200px.
        Assert.IsTrue(targetWidth is >= 0 and <= 20,
            $"expected shrink-to-fit width near 0px (inline width has no effect), was {targetWidth}");
    }
}
