using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class CssUi3Tests
{
    private static readonly RColor Outline = RColor.FromArgb(51, 51, 51);

    [TestMethod]
    [DataRow("content-box", 130, 80)]
    [DataRow("border-box", 100, 50)]
    [Spec("css-ui-3", "#box-sizing")]
    [Spec("css-sizing-3", "#box-sizing")]
    public void BoxSizing_SelectsTheBoxWidthAndHeightApplyTo(string sizing, double width, double height)
    {
        var box = Layout($"<div id='b' style='box-sizing:{sizing};width:100px;height:50px;padding:10px;border:5px solid black'></div>");

        Assert.AreEqual(width, box.Size.Width, 0.5);
        Assert.AreEqual(height, box.Size.Height, 0.5);
    }

    [Ignore("Gap, see Spec/css-ui-3.yml")]
    [TestMethod]
    [Spec("css-ui-3", "#box-sizing")]
    public void BoxSizingBorderBox_ContentSizeIsNeverNegative()
    {
        var box = Layout("<div id='b' style='box-sizing:border-box;width:10px;height:10px;padding:10px;border:5px solid black'></div>");

        Assert.AreEqual(30, box.Size.Width, 0.5);
        Assert.AreEqual(30, box.Size.Height, 0.5);
    }

    [TestMethod]
    [Spec("css-ui-3", "#outline-width")]
    [Spec("css-ui-4", "#outline-width")]
    public void OutlineWidth_TakesTheBorderWidthKeywords()
    {
        var g = Paint("<div id='b' style='width:100px;height:50px;outline:thick solid rgb(51,51,51)'>x</div>");

        var top = g.Log.OfType<RecordingGraphics.DrawRectCall>().First(r => r.Color == Outline);
        Assert.AreEqual(5, top.Height, 0.1);
    }

    [Ignore("Gap, see Spec/css-ui-3.yml")]
    [TestMethod]
    [Spec("css-ui-3", "#outline-width")]
    [Spec("css-ui-4", "#outline-width")]
    public void OutlineWidth_InitialValueIsMedium()
    {
        var g = Paint("<div id='b' style='width:100px;height:50px;outline-style:solid;outline-color:rgb(51,51,51)'>x</div>");

        var top = g.Log.OfType<RecordingGraphics.DrawRectCall>().First(r => r.Color == Outline);
        Assert.AreEqual(3, top.Height, 0.1);
    }

    [Ignore("Gap, see Spec/css-ui-3.yml")]
    [TestMethod]
    [Spec("css-ui-3", "#outline-props")]
    [Spec("css-ui-4", "#outline")]
    public void Outline_FollowsTheBorderRadius()
    {
        var g = Paint("<div id='b' style='width:100px;height:50px;border-radius:10px;outline:5px solid rgb(51,51,51)'>x</div>");

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == Outline), "the outline is a ring of rectangles");
    }

    [Ignore("Gap, see Spec/css-ui-3.yml")]
    [TestMethod]
    [Spec("css-ui-3", "#text-overflow")]
    public void TextOverflowEllipsis_EndsTheClippedLineWithAnEllipsis()
    {
        var g = Paint("<div id='b' style='width:50px;overflow:hidden;white-space:nowrap;text-overflow:ellipsis'>a very long line of text</div>");

        Assert.IsTrue(g.DrawStringCalls.Any(s => s.Text.Contains('…')));
    }

    private static TheArtOfDev.HtmlRenderer.Core.Dom.CssBox Layout(string body)
    {
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap(body));
        return PaintHarness.FindById(root, "b")!;
    }

    private static RecordingGraphics Paint(string body)
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(body));
        return PaintHarness.PaintBox(container, PaintHarness.FindById(root, "b")!);
    }
}
