using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class CssBackgrounds4Tests
{
    // MockAdapter decodes every image as 40x30
    private const string Image = "url(data:image/png;base64,iVBORw0KGgo=)";

    [Ignore("Gap, see Spec/css-backgrounds-4.yml")]
    [TestMethod]
    [DataRow("background-position-x:right", 60, 0)]
    [DataRow("background-position-y:bottom", 0, 70)]
    [DataRow("background-position:right bottom;background-position-x:left", 0, 70)]
    [Spec("css-backgrounds-4", "#background-position-longhands")]
    public void BackgroundPositionLonghands_PositionTheImage(string position, double x, double y)
    {
        var g = Paint($"width:100px;height:100px;background-image:{Image};background-repeat:no-repeat;{position}");

        var image = g.DrawImageCalls.Single();
        Assert.AreEqual(x, image.DestRect.X, 0.5);
        Assert.AreEqual(y, image.DestRect.Y, 0.5);
    }

    [Ignore("Gap, see Spec/css-backgrounds-4.yml")]
    [TestMethod]
    [Spec("css-backgrounds-4", "#background-repeat-longhands")]
    public void BackgroundRepeatY_NoRepeatTilesOnlyHorizontally()
    {
        var g = Paint($"width:100px;height:50px;background-image:{Image};background-repeat-y:no-repeat");

        var tiles = g.Log.OfType<RecordingGraphics.DrawRectCall>().Single(r => r.Color == RColor.Empty);
        Assert.AreEqual(new RRect(0, 0, 100, 30), new RRect(tiles.X, tiles.Y, tiles.Width, tiles.Height));
    }

    private static RecordingGraphics Paint(string style)
    {
        var container = new HtmlContainerInt(new MockAdapter())
        {
            MaxSize = new RSize(1000, 4000),
            Location = RPoint.Empty,
            PageSize = new RSize(1000, 4000),
            AvoidAsyncImagesLoading = true
        };
        container.SetHtml(PaintHarness.Wrap($"<div id='b' style='{style}'></div>")).GetAwaiter().GetResult();

        using (var layoutGraphics = new RecordingGraphics())
            container.PerformLayout(layoutGraphics);

        return PaintHarness.PaintBox(container, PaintHarness.FindById(container.Root!, "b")!);
    }
}
