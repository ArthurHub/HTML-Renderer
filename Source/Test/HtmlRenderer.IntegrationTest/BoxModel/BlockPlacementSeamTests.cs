using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.BoxModel;

/// <summary>
/// A block-level box's position is assigned by the frame above it, from what that frame placed before it - the
/// seam <c>CssBox.PlaceBlockChild</c>-equivalent logic in <c>CssBox.PerformLayoutImp</c> names (search "prevSibling").
/// </summary>
/// <remarks>
/// <para>
/// What these pin is the <i>inputs</i> the seam resolves against, which is the half a box cannot answer alone:
/// which box counts as "the one before this one" is a question about the frame's own child list, and the answer
/// skips every child that is not in the flow the frame is placing.
/// </para>
/// <para>
/// The root is the one box with no frame above it, so it stands in for its own - and the whole of what that has
/// to mean is that it finds nothing before it, exactly as a box with no parent always did.
/// </para>
/// </remarks>
// The regex cache in RegexParserUtils isn't thread-safe, parallel SetHtml calls can corrupt it.
[TestClass]
[DoNotParallelize]
public sealed class BlockPlacementSeamTests
{
    private const double Margin = 20;
    private const double Delta = 0.5;

    [TestMethod]
    public void BlockChild_TakesItsTop_FromThePreviousInFlowSiblingItsFrameHeld()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px'></div><div id='b' style='height:40px'></div>"),
            margin: Margin);

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(a.ActualBottom, b.Location.Y, Delta);
    }

    [TestMethod]
    public void ADisplayNoneSibling_IsNotThePredecessorTheFrameResolvesAgainst()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px'></div>"
            + "<div id='hidden' style='display:none;height:200px'></div>"
            + "<div id='b' style='height:40px'></div>"),
            margin: Margin);

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(a.ActualBottom, b.Location.Y, Delta);
    }

    [TestMethod]
    public void AnOutOfFlowSibling_IsNotThePredecessorTheFrameResolvesAgainst()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px'></div>"
            + "<div id='abs' style='position:absolute;top:300px;height:40px'></div>"
            + "<div id='b' style='height:40px'></div>"),
            margin: Margin);

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(a.ActualBottom, b.Location.Y, Delta);
    }

    [TestMethod]
    public void AFloatedSibling_IsNotThePredecessorTheFrameResolvesAgainst()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px'></div>"
            + "<div id='f' style='float:left;width:40px;height:40px'></div>"
            + "<div id='b' style='height:40px'></div>"),
            margin: Margin);

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(a.ActualBottom, b.Location.Y, Delta);
    }

    [TestMethod]
    public void TheRoot_HasNothingBeforeIt_AndIsPlacedAtTheInitialContainingBlocksTop()
    {
        var (root, container) = LayoutHarness.Layout(
            "<html style='margin:0'><body style='margin:0'>"
            + "<div style='height:40px'></div></body></html>",
            margin: Margin);

        Assert.AreEqual(container.Location.Y, root.Location.Y, Delta);
    }
}
