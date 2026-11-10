using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Painting;

// Parallel full layout passes race on the adapter singletons.
[TestClass]
[DoNotParallelize]
public sealed class CssBorders4Tests
{
    [TestMethod]
    [Spec("css-borders-4", "#border-width")]
    public void LogicalBorderWidth_MapsToThePhysicalSide()
    {
        var box = Layout("<div id='b' style='border-inline-start:5px solid red;border-block-end:3px solid red'>x</div>");

        Assert.AreEqual(5, box.ActualBorderLeftWidth, 0.01);
        Assert.AreEqual(3, box.ActualBorderBottomWidth, 0.01);
    }

    [Ignore("Gap, see Spec/css-borders-4.yml")]
    [TestMethod]
    [Spec("css-borders-4", "#border-width")]
    public void LogicalBorderWidth_FollowsTheDirection()
    {
        var box = Layout("<div id='b' style='direction:rtl;border-inline-start:5px solid red'>x</div>");

        Assert.AreEqual(5, box.ActualBorderRightWidth, 0.01);
        Assert.AreEqual(0, box.ActualBorderLeftWidth, 0.01);
    }

    private static CssBox Layout(string body) =>
        LayoutHarness.FindById(LayoutHarness.Layout(LayoutHarness.Wrap(body), maxWidth: 400).Root, "b")!;
}
