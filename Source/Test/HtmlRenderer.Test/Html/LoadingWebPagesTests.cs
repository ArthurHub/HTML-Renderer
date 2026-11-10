using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class LoadingWebPagesTests
{
    [TestMethod]
    [Spec("html", "browsing-the-web.html#scrolling-to-a-fragment")]
    public void FragmentLink_ScrollsToTheFirstElementWithTheId()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<a id=x href=#t>a</a><div id=t style='margin-top:100px'>b</div><p id=t style='margin-top:100px'>c</p>"));

        Assert.AreEqual(LayoutHarness.FindById(root, "t")!.Location.Y, ScrollTarget(container, LayoutHarness.FindById(root, "x")!));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "browsing-the-web.html#scrolling-to-a-fragment")]
    public void FragmentLink_IsPercentDecoded()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<a id=x href=#caf%C3%A9>a</a><p id=café style='margin-top:100px'>b</p>"));

        Assert.AreEqual(LayoutHarness.FindById(root, "café")!.Location.Y, ScrollTarget(container, LayoutHarness.FindById(root, "x")!));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "browsing-the-web.html#scrolling-to-a-fragment")]
    public void TopFragment_ScrollsToTheTopOfTheDocument()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<p style='margin-top:100px'><a id=x href=#top>a</a></p>"));

        Assert.AreEqual(0, ScrollTarget(container, LayoutHarness.FindById(root, "x")!));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "browsing-the-web.html#scrolling-to-a-fragment")]
    public void FragmentLink_MakesTheElementTheTargetElement()
    {
        var (root, container) = LayoutHarness.Layout(
            "<html><head><style>:target { color: red }</style></head><body><a id=x href=#t>a</a><p id=t>b</p></body></html>");

        ScrollTarget(container, LayoutHarness.FindById(root, "x")!);
        using var graphics = new RecordingGraphics();
        container.PerformLayout(graphics);

        Assert.AreEqual("rgb(255, 0, 0)", LayoutHarness.FindById(container.Root!, "t")!.Color);
    }

    /// <summary>Clicks the fragment link and returns where the container asks the host to scroll to.</summary>
    private static double? ScrollTarget(HtmlContainerInt container, CssBox link)
    {
        double? y = null;
        container.ScrollChange += (_, e) => y = e.Y;
        container.HandleLinkClicked(new MockControl(container.Adapter), RPoint.Empty, link);
        return y;
    }

    private sealed class MockControl(RAdapter adapter) : RControl(adapter)
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
