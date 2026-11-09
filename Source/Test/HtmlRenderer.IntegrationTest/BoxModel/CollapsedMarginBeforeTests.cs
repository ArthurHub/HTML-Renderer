using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.BoxModel;

[DoNotParallelize]
[TestClass]
public sealed class CollapsedMarginBeforeTests
{
    private const double Delta = 0.5;

    [TestMethod]
    [Spec("css21", "box.html#collapsing-margins")]
    public void AdjoiningSiblingMargins_CollapseToTheLargerPositive()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px;margin-bottom:30px'></div>"
            + "<div id='b' style='height:40px;margin-top:10px'></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(30, b.Location.Y - a.ActualBottom, Delta);
    }

    [TestMethod]
    [Spec("css21", "box.html#collapsing-margins")]
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
    [Spec("css21", "box.html#collapsing-margins")]
    public void NegativeSiblingMargins_CollapseToTheMostNegative()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px;margin-bottom:-10px'></div>"
            + "<div id='b' style='height:40px;margin-top:-30px'></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(-30, b.Location.Y - a.ActualBottom, Delta);
    }

    [TestMethod]
    [Spec("css21", "box.html#collapsing-margins")]
    public void ASelfCollapsingSiblingBetweenTwoBoxes_JoinsTheSet()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px;margin-bottom:10px'></div>"
            + "<div style='margin:40px 0'></div>"
            + "<div id='b' style='height:40px;margin-top:10px'></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(40, b.Location.Y - a.ActualBottom, Delta);
    }

    [TestMethod]
    [Spec("css21", "box.html#collapsing-margins")]
    public void AFirstInFlowChildsTopMargin_JoinsItsParentsOwnSet()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px'></div>"
            + "<div id='outer' style='margin-top:10px'>"
            + "<div id='inner' style='height:40px;margin-top:30px'></div></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var outer = LayoutHarness.FindById(root, "outer")!;
        var inner = LayoutHarness.FindById(root, "inner")!;

        Assert.AreEqual(30, outer.Location.Y - a.ActualBottom, Delta);
        Assert.AreEqual(outer.Location.Y, inner.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "box.html#collapsing-margins")]
    public void AutoHeightParent_PassesItsLastChildsBottomMarginOn()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='p'><div style='height:10px;margin-bottom:30px'></div></div>"
            + "<div id='n' style='height:10px;margin-top:10px'></div>"));

        var p = LayoutHarness.FindById(root, "p")!;
        var n = LayoutHarness.FindById(root, "n")!;

        Assert.AreEqual(30, n.Location.Y - p.ActualBottom, Delta);
    }

    [TestMethod]
    [Spec("css21", "box.html#collapsing-margins")]
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
    [Spec("css21", "box.html#collapsing-margins")]
    public void AFloatsOwnMarginDoesNotMerge_ButThePredecessorsStillOccupiesSpace()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px;margin-bottom:20px'></div>"
            + "<div id='f' style='float:left;width:40px;height:40px;margin-top:-5px'></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var f = LayoutHarness.FindById(root, "f")!;

        Assert.AreEqual(15, f.Location.Y - a.ActualBottom, Delta);
    }

    [TestMethod]
    [DataRow("overflow:hidden")]
    [DataRow("float:left;width:100px")]
    [DataRow("position:absolute;width:100px")]
    [Spec("css21", "box.html#collapsing-margins")]
    public void NewBlockFormattingContext_KeepsItsFirstChildsTopMarginInside(string parentStyle)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            $"<div id='p' style='{parentStyle}'><div id='c' style='margin-top:20px;height:10px'></div></div>"));

        var p = LayoutHarness.FindById(root, "p")!;
        var c = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(20, c.Location.Y - p.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "box.html#collapsing-margins")]
    public void FixedHeightParent_DoesNotPassItsLastChildsBottomMarginOn()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='p' style='height:50px'><div style='height:10px;margin-bottom:30px'></div></div>"
            + "<div id='n' style='height:10px'></div>"));

        var p = LayoutHarness.FindById(root, "p")!;
        var n = LayoutHarness.FindById(root, "n")!;

        Assert.AreEqual(p.ActualBottom, n.Location.Y, Delta);
    }
}
