using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.BoxModel;

[DoNotParallelize]
[TestClass]
public sealed class BoxDimensionsIntegrationTests
{
    private const string Box = "<div id='b' style='width:100px;height:50px;padding:10px;border:5px solid black;margin:7px;background:red'></div>";

    [TestMethod]
    [Spec("css21", "box.html#box-dimensions")]
    [Spec("css-box-3", "#box-edges")]
    [Spec("css-box-3", "#borders")]
    public void BorderBox_IsContentPlusPaddingPlusBorder_AndTheMarginSurroundsIt()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(Box));
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(7, b.Location.X);
        Assert.AreEqual(7, b.Location.Y);
        Assert.AreEqual(130, b.Size.Width);
        Assert.AreEqual(80, b.Size.Height);
    }

    [TestMethod]
    [Spec("css21", "box.html#box-dimensions")]
    [Spec("css-box-3", "#box-model")]
    public void Background_CoversThePaddingAndBorderArea()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(Box));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "b")!);

        var background = g.Log.OfType<RecordingGraphics.DrawRectCall>().Single();
        Assert.AreEqual((7d, 7d, 130d, 80d), (background.X, background.Y, background.Width, background.Height));
    }
}
