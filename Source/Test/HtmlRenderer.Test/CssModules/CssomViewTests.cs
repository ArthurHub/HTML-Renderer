using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssomViewTests
{
    [TestMethod]
    [Spec("cssom-view-1", "#terminology")]
    [DataRow("<div style='height:300px'></div>", 300)]
    [DataRow("<div style='height:100px'></div><div style='position:absolute;top:400px;height:100px'></div>", 500)]
    [DataRow("<div style='height:100px'><div style='position:relative;top:300px;height:100px'></div></div>", 400)]
    public void ViewportScrollingArea_ReachesTheBottomOfEveryBox(string body, double height)
    {
        var (_, container) = LayoutHarness.Layout(LayoutHarness.Wrap(body), maxWidth: 500);

        Assert.AreEqual(height, container.ActualSize.Height, 0.5);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/cssom-view-1.yml")]
    [Spec("cssom-view-1", "#terminology")]
    public void ViewportScrollingArea_ReachesTheBottomMarginEdge()
    {
        var (_, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='height:100px'><div style='position:absolute;top:0;height:100px;margin-bottom:50px'></div></div>"), maxWidth: 500);

        Assert.AreEqual(150, container.ActualSize.Height, 0.5);
    }

    [TestMethod]
    [Spec("cssom-view-1", "#scrolling")]
    [Spec("cssom-view-1", "#element-scrolling-members")]
    public void FragmentNavigation_AlignsTheTargetWithTheTopOfTheViewport()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<a id=x href=#target>a</a><p id=target style='margin-top:300px'>b</p>"));

        Assert.AreEqual(LayoutHarness.FindById(root, "target")!.Location.Y, ScrollTarget(container, root, "x")?.Y);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/cssom-view-1.yml")]
    [Spec("cssom-view-1", "#element-scrolling-members")]
    [Spec("html", "browsing-the-web.html#scrolling-to-a-fragment")]
    public void FragmentNavigation_ScrollsTheInlineAxisOnlyAsFarAsNeeded()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<a id=x href=#target>a</a><p id=target style='margin:300px 0 0 300px'>b</p>"));

        Assert.AreEqual(0d, ScrollTarget(container, root, "x")?.X);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/cssom-view-1.yml")]
    [Spec("cssom-view-1", "#scrolling")]
    public void TopFragment_ScrollsToTheBeginningOfTheDocument()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='height:2000px'></div><a id=x href=#top>top</a>"));

        Assert.AreEqual<RPoint?>(RPoint.Empty, ScrollTarget(container, root, "x"));
    }

    private static RPoint? ScrollTarget(HtmlContainerInt container, TheArtOfDev.HtmlRenderer.Core.Dom.CssBox root, string linkId)
    {
        RPoint? location = null;
        container.ScrollChange += (_, e) => location = new RPoint(e.X, e.Y);
        container.HandleLinkClicked(new TestControl(container.Adapter), RPoint.Empty, LayoutHarness.FindById(root, linkId)!);
        return location;
    }

    private sealed class TestControl(RAdapter adapter) : RControl(adapter)
    {
        public override bool LeftMouseButton => false;
        public override bool RightMouseButton => false;
        public override RPoint MouseLocation => RPoint.Empty;
        public override void SetCursorDefault() { }
        public override void SetCursorHand() { }
        public override void SetCursorIBeam() { }
        public override void DoDragDropCopy(object dragDropData) { }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            charFit = str.Length;
            charFitWidth = 0;
        }

        public override void Invalidate() { }
    }
}
