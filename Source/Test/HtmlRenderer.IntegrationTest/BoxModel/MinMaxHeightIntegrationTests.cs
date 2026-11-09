using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.BoxModel;

[DoNotParallelize]
[TestClass]
public sealed class MinMaxHeightIntegrationTests
{
    private const double Delta = 0.5;

    [TestMethod]
    [Spec("css21", "visudet.html#min-max-heights")]
    public void MinHeightWinsOverConflictingMaxHeight()
    {
        var html = LayoutHarness.Wrap("<div id='b' style='height:8px; min-height:20px; max-height:5px;'></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(20, box.ActualBottom - box.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#min-max-heights")]
    public void MaxHeightAppliesWhenNotConflictingWithMinHeight()
    {
        var html = LayoutHarness.Wrap("<div id='b' style='height:100px; max-height:20px;'></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(20, box.ActualBottom - box.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#the-height-property")]
    public void PercentageHeight_AgainstAutoHeightContainingBlock_ComputesToAuto()
    {
        var html = LayoutHarness.Wrap(
            "<div id='cb'>"
            + "<div id='content' style='height:50px;'></div>"
            + "<div id='b' style='height:60%;'></div>"
            + "</div>");
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(0, box.ActualBottom - box.Location.Y, Delta);
    }
}
