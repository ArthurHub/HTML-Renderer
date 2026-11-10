using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssScrollSnapTests
{
    [TestMethod]
    [DataRow("scroll-snap-type: y mandatory")]
    [DataRow("scroll-padding: 10px")]
    [DataRow("scroll-margin: 10px")]
    [DataRow("scroll-snap-align: start")]
    [DataRow("scroll-snap-stop: always")]
    public void ScrollSnapProperties_AreDropped(string declaration)
    {
        var style = CssConstructionFunctions.ParseDeclarations(declaration);

        Assert.AreEqual(0, style.Length);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-scroll-snap-1.yml")]
    [DataRow("scroll-snap-type: y mandatory")]
    [DataRow("scroll-snap-align: start")]
    [DataRow("scroll-snap-stop: always")]
    [Spec("css-scroll-snap-1", "#scroll-snap-type")]
    [Spec("css-scroll-snap-1", "#scroll-snap-align")]
    [Spec("css-scroll-snap-1", "#scroll-snap-stop")]
    public void SnapProperties_AreAccepted(string declaration)
    {
        var style = CssConstructionFunctions.ParseDeclarations(declaration);

        Assert.AreEqual(1, style.Length);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-scroll-snap-1.yml")]
    [Spec("css-scroll-snap-1", "#scroll-margin")]
    public void ScrollMargin_OutsetsTheFragmentTargetArea()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<a id=x href=#target>a</a><p id=target style='margin-top:100px;scroll-margin-top:30px'>b</p>"));

        Assert.AreEqual(LayoutHarness.FindById(root, "target")!.Location.Y - 30, ScrollTarget(container, "x", root));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-scroll-snap-1.yml")]
    [Spec("css-scroll-snap-1", "#scroll-padding")]
    public void ScrollPaddingOnTheRoot_InsetsTheViewportForTheFragmentTarget()
    {
        var (root, container) = LayoutHarness.Layout(
            "<html style='scroll-padding-top:30px'><head></head><body style='margin:0'>"
            + "<a id=x href=#target>a</a><p id=target style='margin-top:100px'>b</p></body></html>");

        Assert.AreEqual(LayoutHarness.FindById(root, "target")!.Location.Y - 30, ScrollTarget(container, "x", root));
    }

    private static double? ScrollTarget(HtmlContainerInt container, string linkId, TheArtOfDev.HtmlRenderer.Core.Dom.CssBox root)
    {
        double? y = null;
        container.ScrollChange += (_, e) => y = e.Y;
        container.HandleLinkClicked(new TestControl(container.Adapter), RPoint.Empty, LayoutHarness.FindById(root, linkId)!);
        return y;
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
