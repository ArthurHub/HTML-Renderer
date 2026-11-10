using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class BackgroundPaintIntegrationTests
{
    // MockAdapter decodes every image as 40x30
    private const string Image = "url(data:image/png;base64,iVBORw0KGgo=)";
    private const double ImageWidth = 40;
    private const double ImageHeight = 30;

    private static readonly RColor Fill = RColor.FromArgb(10, 20, 30);

    [TestMethod]
    [Spec("css21", "colors.html#background-properties")]
    public void BackgroundColor_PaintsASolidFillRect()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;background-color:rgb(10,20,30)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);
        var fills = g.Log.OfType<RecordingGraphics.DrawRectCall>()
            .Where(r => r.Color == RColor.FromArgb(10, 20, 30)).ToList();

        Assert.AreEqual(1, fills.Count);
        Assert.AreEqual(100, fills[0].Width, 0.1);
        Assert.AreEqual(50, fills[0].Height, 0.1);
    }

    [TestMethod]
    [Spec("css21", "colors.html#background-properties")]
    public void BackgroundColorTransparent_PaintsNothing()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;background-color:transparent'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any());
    }

    [TestMethod]
    [Spec("css21", "colors.html#background-properties")]
    public void BackgroundColorDefault_PaintsNothing()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any());
    }

    [TestMethod]
    [Spec("css21", "zindex.html#painting-order")]
    public void BackgroundColor_PaintsBeforeBorderAndOutline()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;background-color:rgb(10,20,30);"
            + "border:3px solid rgb(1,2,3);outline:5px solid rgb(4,5,6)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        var backgroundIndex = g.Log.FindIndex(c => c is RecordingGraphics.DrawRectCall r && r.Color == RColor.FromArgb(10, 20, 30));
        var borderIndex = g.Log.FindIndex(c => c is RecordingGraphics.DrawLineCall l && l.Color == RColor.FromArgb(1, 2, 3));
        var outlineIndex = g.Log.FindIndex(c => c is RecordingGraphics.DrawRectCall r && r.Color == RColor.FromArgb(4, 5, 6));

        Assert.IsTrue(backgroundIndex >= 0 && borderIndex >= 0 && outlineIndex >= 0);
        Assert.IsTrue(backgroundIndex < borderIndex, "background must paint before the border");
        Assert.IsTrue(borderIndex < outlineIndex, "border must paint before the outline");
    }

    [TestMethod]
    [Spec("css21", "colors.html#background")]
    public void Background_CoversContentPaddingAndBorderButNotMargin()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='margin:10px;width:100px;height:50px;padding:5px;border:3px solid black;background-color:rgb(10,20,30)'>x</div>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "b")!);
        var fill = Fills(g).Single();

        Assert.AreEqual(10, fill.X, 0.5);
        Assert.AreEqual(10, fill.Y, 0.5);
        Assert.AreEqual(116, fill.Width, 0.1);
        Assert.AreEqual(66, fill.Height, 0.1);
    }

    [TestMethod]
    [Spec("css21", "colors.html#background")]
    public void Background_IsNotInherited()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div style='background-color:rgb(10,20,30)'><p id='c' style='margin:0'>x</p></div>"));
        var child = PaintHarness.FindById(root, "c")!;

        var g = PaintHarness.PaintBox(container, child);

        Assert.AreEqual("transparent", child.BackgroundColor);
        Assert.IsFalse(Fills(g).Any());
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "colors.html#background")]
    public void RootBackground_CoversTheWholeCanvas()
    {
        var (_, container) = PaintHarness.Layout(
            "<!DOCTYPE html><html style='background-color:rgb(10,20,30)'><body style='margin:0'><div style='height:10px'></div></body></html>",
            500, 300);

        var g = PaintHarness.PaintPage(container);
        var fill = Fills(g).Single();

        Assert.IsTrue(fill.Width >= 500 && fill.Height >= 300, $"canvas fill was {fill.Width}x{fill.Height}");
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "colors.html#background")]
    public void BodyBackground_PropagatesToTheCanvas()
    {
        var (_, container) = PaintHarness.Layout(
            "<!DOCTYPE html><html><body style='margin:20px;background-color:rgb(10,20,30)'><div style='height:10px'></div></body></html>",
            500, 300);

        var g = PaintHarness.PaintPage(container);
        var fill = Fills(g).Single();

        Assert.AreEqual(0, fill.X, 0.5);
        Assert.AreEqual(0, fill.Y, 0.5);
        Assert.IsTrue(fill.Width >= 500 && fill.Height >= 300, $"canvas fill was {fill.Width}x{fill.Height}");
    }

    [TestMethod]
    [Spec("css21", "colors.html#background-properties")]
    public void BackgroundImage_PaintsOnTopOfTheBackgroundColor()
    {
        var g = PaintBackground($"width:100px;height:50px;background-color:rgb(10,20,30);background-image:{Image};background-repeat:no-repeat");

        var fillIndex = g.Log.FindIndex(c => c is RecordingGraphics.DrawRectCall r && r.Color == Fill);
        var imageIndex = g.Log.FindIndex(c => c is RecordingGraphics.DrawImageCall);

        Assert.IsTrue(fillIndex >= 0 && imageIndex >= 0);
        Assert.IsTrue(fillIndex < imageIndex);
    }

    [TestMethod]
    [Spec("css21", "colors.html#background-properties")]
    public void BackgroundImageNone_PaintsNoImage()
    {
        var g = PaintBackground("width:100px;height:50px;background-image:none;background-repeat:no-repeat");

        Assert.IsFalse(g.DrawImageCalls.Any());
    }

    [TestMethod]
    [Spec("css21", "colors.html#background-properties")]
    public void BackgroundRepeatNoRepeat_DrawsOneCopyAtTheTopLeft()
    {
        var g = PaintBackground($"width:100px;height:50px;background-image:{Image};background-repeat:no-repeat");

        var image = g.DrawImageCalls.Single();
        Assert.AreEqual(new RRect(0, 0, ImageWidth, ImageHeight), image.DestRect);
    }

    [TestMethod]
    [DataRow("repeat", 0, 0, 100, 50)]
    [DataRow("repeat-x", 0, 0, 100, ImageHeight)]
    [DataRow("repeat-y", 0, 0, ImageWidth, 50)]
    [Spec("css21", "colors.html#background-properties")]
    public void BackgroundRepeat_TilesAlongTheGivenAxes(string repeat, double x, double y, double width, double height)
    {
        var g = PaintBackground($"width:100px;height:50px;background-image:{Image};background-repeat:{repeat}");

        // Tiling paints the area with a texture brush, which RecordingGraphics records with an empty color
        var tiles = g.Log.OfType<RecordingGraphics.DrawRectCall>().Single(r => r.Color == RColor.Empty);
        Assert.AreEqual(x, tiles.X, 0.5);
        Assert.AreEqual(y, tiles.Y, 0.5);
        Assert.AreEqual(width, tiles.Width, 0.5);
        Assert.AreEqual(height, tiles.Height, 0.5);
        Assert.IsFalse(g.DrawImageCalls.Any());
    }

    [TestMethod]
    [Spec("css21", "colors.html#background-properties")]
    public void BackgroundRepeat_TilingCoversTheBorderArea()
    {
        var g = PaintBackground($"width:100px;height:50px;border:5px solid black;background-image:{Image}");

        var tiles = g.Log.OfType<RecordingGraphics.DrawRectCall>().Single(r => r.Color == RColor.Empty);
        Assert.AreEqual(new RRect(0, 0, 110, 60), new RRect(tiles.X, tiles.Y, tiles.Width, tiles.Height));
    }

    [TestMethod]
    [DataRow("left top", 0, 0)]
    [DataRow("right bottom", 60, 70)]
    [DataRow("center", 30, 35)]
    [DataRow("right", 60, 35)]
    [DataRow("bottom", 30, 70)]
    [DataRow("top center", 30, 0)]
    [DataRow("0% 0%", 0, 0)]
    [Spec("css21", "colors.html#background-properties")]
    [Spec("css-values-4", "#position-parsing")]
    public void BackgroundPosition_Keywords(string position, double x, double y)
    {
        var g = PaintBackground($"width:100px;height:100px;background-image:{Image};background-repeat:no-repeat;background-position:{position}");

        var image = g.DrawImageCalls.Single();
        Assert.AreEqual(x, image.DestRect.X, 0.5);
        Assert.AreEqual(y, image.DestRect.Y, 0.5);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [DataRow("50% 50%", 30, 35)]
    [DataRow("100% 100%", 60, 70)]
    [DataRow("25% 50%", 15, 35)]
    [DataRow("10% 90%", 6, 63)]
    [Spec("css21", "colors.html#background-properties")]
    [Spec("css-values-4", "#position-parsing")]
    public void BackgroundPosition_Percentages(string position, double x, double y)
    {
        var g = PaintBackground($"width:100px;height:100px;background-image:{Image};background-repeat:no-repeat;background-position:{position}");

        var image = g.DrawImageCalls.Single();
        Assert.AreEqual(x, image.DestRect.X, 0.5);
        Assert.AreEqual(y, image.DestRect.Y, 0.5);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [DataRow("10px 20px", 10, 20)]
    [DataRow("-5px 7px", -5, 7)]
    [DataRow("right 15px", 60, 15)]
    [Spec("css21", "colors.html#background-properties")]
    [Spec("css-values-4", "#position-parsing")]
    public void BackgroundPosition_Lengths(string position, double x, double y)
    {
        var g = PaintBackground($"width:100px;height:100px;background-image:{Image};background-repeat:no-repeat;background-position:{position}");

        var image = g.DrawImageCalls.Single();
        Assert.AreEqual(x, image.DestRect.X, 0.5);
        Assert.AreEqual(y, image.DestRect.Y, 0.5);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "colors.html#background-properties")]
    public void BackgroundPosition_IsRelativeToThePaddingBox()
    {
        var g = PaintBackground($"width:100px;height:100px;border:5px solid black;background-image:{Image};background-repeat:no-repeat");

        var image = g.DrawImageCalls.Single();
        Assert.AreEqual(5, image.DestRect.X, 0.5);
        Assert.AreEqual(5, image.DestRect.Y, 0.5);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "colors.html#background-properties")]
    public void BackgroundAttachmentFixed_PositionsTheImageAgainstTheViewport()
    {
        var g = PaintBackground($"margin-top:100px;width:100px;height:100px;background-image:{Image};background-repeat:no-repeat;background-attachment:fixed");

        var image = g.DrawImageCalls.Single();
        Assert.AreEqual(0, image.DestRect.Y, 0.5);
    }

    [TestMethod]
    [Spec("css21", "colors.html#background-properties")]
    public void BackgroundShorthand_SetsAllGivenLonghands()
    {
        var g = PaintBackground($"width:100px;height:100px;background:rgb(10,20,30) {Image} no-repeat right bottom");

        Assert.AreEqual(1, Fills(g).Count);
        var image = g.DrawImageCalls.Single();
        Assert.AreEqual(60, image.DestRect.X, 0.5);
        Assert.AreEqual(70, image.DestRect.Y, 0.5);
    }

    [TestMethod]
    [Spec("css21", "colors.html#background-properties")]
    public void BackgroundShorthand_ResetsOmittedLonghandsToTheirInitialValues()
    {
        var g = PaintBackground($"width:100px;height:100px;background-image:{Image};background-repeat:no-repeat;background:rgb(10,20,30)");

        Assert.AreEqual(1, Fills(g).Count);
        Assert.IsFalse(g.DrawImageCalls.Any());
        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColor.Empty));
    }

    private static List<RecordingGraphics.DrawRectCall> Fills(RecordingGraphics g) =>
        g.Log.OfType<RecordingGraphics.DrawRectCall>().Where(r => r.Color == Fill).ToList();

    /// <summary>Lays out a div with <paramref name="style"/> at the origin, loading images synchronously, and paints it.</summary>
    private static RecordingGraphics PaintBackground(string style)
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
