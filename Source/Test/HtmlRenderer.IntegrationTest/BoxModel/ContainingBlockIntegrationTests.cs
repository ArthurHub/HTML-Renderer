using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.BoxModel;

[DoNotParallelize]
[TestClass]
public sealed class ContainingBlockIntegrationTests
{
    private const double Delta = 0.5;

    [TestMethod]
    [Spec("css21", "visudet.html#containing-block-details")]
    public void InFlowBlock_IsContainedByTheContentEdgeOfItsParent()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:200px;padding:10px;border:5px solid'><div id='c' style='height:10px'></div></div>"));
        var c = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(15, c.Location.X, Delta);
        Assert.AreEqual(15, c.Location.Y, Delta);
        Assert.AreEqual(200, c.Size.Width, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#containing-block-details")]
    public void BlockInsideAnInline_IsContainedByTheNearestBlockContainer()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:200px'><span style='padding-left:30px'><div id='c' style='height:5px'></div></span></div>"));
        var c = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(0, c.Location.X, Delta);
        Assert.AreEqual(200, c.Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#containing-block-details")]
    public void AbsoluteBox_IsPlacedInTheNearestPositionedAncestor()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:300px;height:300px'><div style='position:relative;margin-left:40px;width:200px;height:100px'>"
            + "<div style='width:100px;padding-top:5px'><div id='a' style='position:absolute;left:10px;top:20px;width:30px;height:10px'></div></div></div></div>"));
        var a = LayoutHarness.FindById(root, "a")!;

        Assert.AreEqual(50, a.Location.X, Delta);
        Assert.AreEqual(20, a.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#containing-block-details")]
    public void AbsoluteBox_PercentageWidth_ResolvesAgainstTheNearestPositionedAncestor()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='position:relative;width:200px;height:100px'>"
            + "<div style='width:100px'><div id='a' style='position:absolute;left:0;top:0;width:50%;height:10px'></div></div></div>"));
        var a = LayoutHarness.FindById(root, "a")!;

        Assert.AreEqual(100, a.Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#containing-block-details")]
    public void AbsoluteBox_WithoutAPositionedAncestor_IsPlacedInTheInitialContainingBlock()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='margin:30px;width:100px'><div id='a' style='position:absolute;left:10px;top:20px;width:30px;height:10px'></div></div>"));
        var a = LayoutHarness.FindById(root, "a")!;

        Assert.AreEqual(10, a.Location.X, Delta);
        Assert.AreEqual(20, a.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#containing-block-details")]
    public void AbsoluteBox_IsContainedByThePaddingEdgeOfThePositionedAncestor()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='position:relative;width:200px;height:100px;padding:20px;border:5px solid'>"
            + "<div id='a' style='position:absolute;left:0;top:0;width:50%;height:10px'></div></div>"));
        var a = LayoutHarness.FindById(root, "a")!;

        Assert.AreEqual(5, a.Location.X, Delta);
        Assert.AreEqual(5, a.Location.Y, Delta);
        Assert.AreEqual(120, a.Size.Width, Delta);
    }
}
