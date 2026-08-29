using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.BoxModel;

/// <summary>
/// <see href="https://www.w3.org/TR/CSS21/box.html#collapsing-margins">CSS 2.1 §8.3.1</see>'s adjoining-margin
/// set, resolved at one break point by the frame the boxes are children of.
/// </summary>
/// <remarks>
/// <c>CssBox.MarginTopCollapse</c> (Dom/CssBox.cs) implements the real adjoining-margin-set algorithm:
/// <c>CollapsedMarginTopChain</c>/<c>CollapsedMarginBottomChain</c> walk forward through an unblocked run of
/// first/last-in-flow children (so a nested child sits flush with its parent instead of being pushed down a
/// second time), <c>CollectAdjoiningMarginsBeforeSibling</c> walks backward through however many self-collapsing
/// (empty) siblings precede a box - reaching all the way to the parent's own top margin if the whole run turns
/// out self-collapsing - and <c>CollapseMarginSet</c> collapses the resulting set to
/// <c>max(positives) + min(negatives)</c>, not a plain <c>Math.Max</c> between just two values (which only
/// happens to match when both margins share a sign). A floated box's own margin never participates in
/// collapsing at all (CSS 2.1 §8.3.1: floats are out of flow).
/// </remarks>
[DoNotParallelize]
[TestClass]
public sealed class CollapsedMarginBeforeTests
{
    private const double Delta = 0.5;

    [TestMethod]
    public void AdjoiningSiblingMargins_CollapseToTheLargerPositive()
    {
        // Same-sign case: Math.Max(30, 10) = 30 happens to equal the spec result (max of the positives), so this
        // one genuinely passes on this fork.
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px;margin-bottom:30px'></div>"
            + "<div id='b' style='height:40px;margin-top:10px'></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(30, b.Location.Y - a.ActualBottom, Delta);
    }

    [TestMethod]
    public void MixedSignSiblingMargins_Sum()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px;margin-bottom:30px'></div>"
            + "<div id='b' style='height:40px;margin-top:-10px'></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(20, b.Location.Y - a.ActualBottom, Delta);
    }

    [TestMethod]
    public void ASelfCollapsingSiblingBetweenTwoBoxes_JoinsTheSet()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px;margin-bottom:10px'></div>"
            + "<div style='margin:40px 0'></div>"
            + "<div id='b' style='height:40px;margin-top:10px'></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        // The whole run should collapse to one value - the largest member, not a sum of the pairs.
        Assert.AreEqual(40, b.Location.Y - a.ActualBottom, Delta);
    }

    [TestMethod]
    public void AFirstInFlowChildsTopMargin_JoinsItsParentsOwnSet()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px'></div>"
            + "<div id='outer' style='margin-top:10px'>"
            + "<div id='inner' style='height:40px;margin-top:30px'></div></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var outer = LayoutHarness.FindById(root, "outer")!;
        var inner = LayoutHarness.FindById(root, "inner")!;

        // One collapsed value for the whole chain, taken once: the outer box should move down by it and the
        // inner box should sit at its parent's content top rather than being pushed down again.
        Assert.AreEqual(30, outer.Location.Y - a.ActualBottom, Delta);
        Assert.AreEqual(outer.Location.Y, inner.Location.Y, Delta);
    }

    [TestMethod]
    public void ABorderOnTheParent_BlocksTheChainAndLeavesBothMarginsStanding()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px'></div>"
            + "<div id='outer' style='margin-top:10px;border-top:1px solid black'>"
            + "<div id='inner' style='height:40px;margin-top:30px'></div></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var outer = LayoutHarness.FindById(root, "outer")!;
        var inner = LayoutHarness.FindById(root, "inner")!;

        Assert.AreEqual(10, outer.Location.Y - a.ActualBottom, Delta);
        Assert.AreEqual(31, inner.Location.Y - outer.Location.Y, Delta);
    }

    [TestMethod]
    public void AFloatsOwnMarginDoesNotMerge_ButThePredecessorsStillOccupiesSpace()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px;margin-bottom:20px'></div>"
            + "<div id='f' style='float:left;width:40px;height:40px;margin-top:-5px'></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var f = LayoutHarness.FindById(root, "f")!;

        // Summed, not merged: a float is out of flow so its margin should never adjoin anything, but the
        // predecessor's trailing margin is real space it must sit after.
        Assert.AreEqual(15, f.Location.Y - a.ActualBottom, Delta);
    }
}
