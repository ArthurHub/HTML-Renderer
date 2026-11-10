using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class CssBackgrounds3Tests
{
    // MockAdapter decodes every image as 40x30
    private const string Image = "url(data:image/png;base64,iVBORw0KGgo=)";

    private static readonly RColor Fill = RColor.FromArgb(10, 20, 30);
    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);
    private static readonly RColor Blue = RColor.FromArgb(0, 0, 255);

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [Spec("css-backgrounds-3", "#layering")]
    [Spec("css-backgrounds-4", "#layering")]
    public void MultipleLayers_PaintEveryImage()
    {
        var g = Paint($"width:100px;height:100px;background-image:{Image}, {Image};background-repeat:no-repeat;background-position:left top, right bottom");

        Assert.AreEqual(2, g.DrawImageCalls.Count);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-color")]
    public void BackgroundColor_IsPaintedBeneathTheImage()
    {
        var g = Paint($"width:100px;height:50px;background-color:rgb(10,20,30);background-image:{Image};background-repeat:no-repeat");

        var fill = g.Log.FindIndex(c => c is RecordingGraphics.DrawRectCall r && r.Color == Fill);
        var image = g.Log.FindIndex(c => c is RecordingGraphics.DrawImageCall);
        Assert.IsTrue(fill >= 0 && fill < image);
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [Spec("css-backgrounds-3", "#background-color")]
    public void BackgroundColor_IsPaintedBeneathAGradient()
    {
        var g = Paint("width:100px;height:50px;background-color:rgb(10,20,30);background-image:linear-gradient(red, blue)");

        Assert.IsTrue(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == Fill));
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-image")]
    public void BackgroundImage_LinearGradientFillsTheBox()
    {
        var g = Paint("width:100px;height:50px;background-image:linear-gradient(red, blue)");

        var fill = g.Log.OfType<RecordingGraphics.DrawRectCall>().Single();
        Assert.AreEqual(Red, fill.Color);
        Assert.AreEqual(new RRect(0, 0, 100, 50), new RRect(fill.X, fill.Y, fill.Width, fill.Height));
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [Spec("css-backgrounds-3", "#background-image")]
    public void BackgroundImage_RadialGradientIsPainted()
    {
        var g = Paint("width:100px;height:50px;background-image:radial-gradient(red, blue)");

        Assert.IsTrue(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any());
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [DataRow("repeat no-repeat", 0, 0, 100, 30)]
    [DataRow("no-repeat repeat", 0, 0, 40, 50)]
    [Spec("css-backgrounds-3", "#background-repeat")]
    public void BackgroundRepeat_TwoValueSyntax(string repeat, double x, double y, double width, double height)
    {
        var g = Paint($"width:100px;height:50px;background-image:{Image};background-repeat:{repeat}");

        var tiles = g.Log.OfType<RecordingGraphics.DrawRectCall>().Single(r => r.Color == RColor.Empty);
        Assert.AreEqual(new RRect(x, y, width, height), new RRect(tiles.X, tiles.Y, tiles.Width, tiles.Height));
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [Spec("css-backgrounds-3", "#background-repeat")]
    public void BackgroundRepeatSpace_DistributesWholeTilesWithSpaceBetween()
    {
        var g = Paint($"width:100px;height:30px;background-image:{Image};background-repeat:space no-repeat");

        var xs = g.DrawImageCalls.Select(c => c.DestRect.X).OrderBy(x => x).ToList();
        CollectionAssert.AreEqual(new[] { 0.0, 60.0 }, xs);
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [Spec("css-backgrounds-3", "#background-repeat")]
    public void BackgroundRepeatRound_ScalesTheTileToFitAWholeNumberOfTimes()
    {
        var g = Paint($"width:110px;height:30px;background-image:{Image};background-repeat:round no-repeat");

        Assert.AreEqual(3, g.DrawImageCalls.Count);
        Assert.AreEqual(110.0 / 3, g.DrawImageCalls[0].DestRect.Width, 0.5);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-attachment")]
    [Spec("css-backgrounds-4", "#background-attachment")]
    public void BackgroundAttachmentScroll_FixesTheImageToTheBox()
    {
        var g = Paint($"margin-top:100px;width:100px;height:100px;background-image:{Image};background-repeat:no-repeat;background-attachment:scroll");

        Assert.AreEqual(100, g.DrawImageCalls.Single().DestRect.Y, 0.5);
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [DataRow("right 10px bottom 20px", 50, 50)]
    [DataRow("left 10px top 15px", 10, 15)]
    [DataRow("right 10px top", 50, 0)]
    [DataRow("center bottom 10px", 30, 60)]
    [Spec("css-backgrounds-3", "#background-position")]
    public void BackgroundPosition_EdgeOffsets(string position, double x, double y)
    {
        var g = Paint($"width:100px;height:100px;background-image:{Image};background-repeat:no-repeat;background-position:{position}");

        var image = g.DrawImageCalls.Single();
        Assert.AreEqual(x, image.DestRect.X, 0.5);
        Assert.AreEqual(y, image.DestRect.Y, 0.5);
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [DataRow("padding-box", 5, 5, 100, 50)]
    [DataRow("content-box", 15, 15, 80, 30)]
    [Spec("css-backgrounds-3", "#background-clip")]
    [Spec("css-backgrounds-4", "#background-clip")]
    public void BackgroundClip_ClipsTheBackgroundToTheGivenBox(string clip, double x, double y, double width, double height)
    {
        var g = Paint($"width:80px;height:30px;padding:10px;border:5px solid black;background-color:rgb(10,20,30);background-clip:{clip}");

        var fill = g.Log.OfType<RecordingGraphics.DrawRectCall>().Single(r => r.Color == Fill);
        Assert.AreEqual(new RRect(x, y, width, height), new RRect(fill.X, fill.Y, fill.Width, fill.Height));
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [DataRow("content-box", 15, 15)]
    [Spec("css-backgrounds-3", "#background-origin")]
    public void BackgroundOrigin_PositionsTheImageInTheGivenBox(string origin, double x, double y)
    {
        var g = Paint($"width:80px;height:80px;padding:10px;border:5px solid black;background-image:{Image};background-repeat:no-repeat;background-origin:{origin}");

        var image = g.DrawImageCalls.Single();
        Assert.AreEqual(x, image.DestRect.X, 0.5);
        Assert.AreEqual(y, image.DestRect.Y, 0.5);
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [DataRow("20px 10px", 20, 10)]
    [DataRow("50%", 50, 37.5)]
    [DataRow("auto 60px", 80, 60)]
    [DataRow("contain", 100, 75)]
    [DataRow("cover", 133.3, 100)]
    [Spec("css-backgrounds-3", "#background-size")]
    public void BackgroundSize_ScalesTheImage(string size, double width, double height)
    {
        var g = Paint($"width:100px;height:100px;background-image:{Image};background-repeat:no-repeat;background-size:{size}");

        var image = g.DrawImageCalls.Single();
        Assert.AreEqual(width, image.DestRect.Width, 0.5);
        Assert.AreEqual(height, image.DestRect.Height, 0.5);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background")]
    public void BackgroundShorthand_AcceptsPositionAndSize()
    {
        var g = Paint($"width:100px;height:100px;background:{Image} no-repeat right bottom / 20px 10px rgb(10,20,30)");

        Assert.IsTrue(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == Fill));
        Assert.AreEqual(1, g.DrawImageCalls.Count);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#root-background")]
    [Spec("css-backgrounds-4", "#root-background")]
    public void RootBackground_IsPainted()
    {
        var (_, container) = PaintHarness.Layout(
            "<!DOCTYPE html><html style='background-color:rgb(10,20,30)'><body style='margin:0'><div style='height:10px'></div></body></html>",
            500, 300);

        var g = PaintHarness.PaintPage(container);

        Assert.AreEqual(1, g.Log.OfType<RecordingGraphics.DrawRectCall>().Count(r => r.Color == Fill));
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadius_PercentagesReferToTheBorderBox()
    {
        var box = LayoutDiv("width:100px;height:50px;padding:10px;border:5px solid black;border-radius:50%");

        Assert.AreEqual(65, box.ActualBorderTopLeftRadiusX, 0.5);
        Assert.AreEqual(40, box.ActualBorderTopLeftRadiusY, 0.5);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#corner-shaping")]
    public void RoundedBackground_FollowsTheOuterBorderEdgeCurve()
    {
        var g = PaintShapes("width:100px;height:100px;border:4px solid black;background-color:rgb(10,20,30);border-radius:10px 20px");

        var path = g.Paths.Single(p => p.Brush == Fill);
        Assert.AreEqual(new RPoint(10, 0), path.Points[0]);
        CollectionAssert.AreEqual(new[] { 20.0, 10.0, 20.0, 10.0 }, path.Arcs.Select(a => a.RadiusX).ToArray());
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [Spec("css-backgrounds-3", "#corner-shaping")]
    public void RoundedBorder_FollowsTheOuterAndInnerBorderCurves()
    {
        var g = PaintShapes("width:100px;height:100px;border:4px solid black;border-radius:10px");

        // The stroke runs along the middle of the border, a circle of 10 - 4/2 around the corner's center at (10,10)
        var top = g.Paths.First(p => p.Brush == null);
        Assert.AreEqual(new RPoint(2, 10), top.Points[0]);
        Assert.AreEqual(8, top.Arcs[0].RadiusX, 0.01);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#corner-clipping")]
    public void RoundedCorners_ClipTheBackgroundColor()
    {
        var g = PaintShapes("width:100px;height:100px;background-color:rgb(10,20,30);border-radius:10px");

        var path = g.Paths.Single(p => p.Brush == Fill);
        Assert.AreEqual(4, path.Arcs.Count);
        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == Fill));
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [Spec("css-backgrounds-3", "#corner-clipping")]
    public void RoundedCorners_ClipTheBackgroundImage()
    {
        var g = PaintShapes($"width:100px;height:100px;background-image:{Image};border-radius:10px");

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColor.Empty),
            "the tiled image is painted as a plain rectangle");
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [Spec("css-backgrounds-3", "#corner-clipping")]
    public void RoundedCorners_ClipOverflowingContentToThePaddingEdgeCurve()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:100px;overflow:hidden;border-radius:50px'>"
            + "<div style='height:100px;background-color:rgb(10,20,30)'></div></div>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "b")!);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == Fill),
            "the child's background is only clipped to the rectangular padding box");
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#corner-transitions")]
    public void AdjoiningInsetBorders_MeetAtTheDiagonal()
    {
        var g = PaintShapes("width:20px;height:20px;border-style:inset;border-width:10px;border-color:red blue");

        var top = g.Log.OfType<RecordingGraphics.DrawPolygonCall>().Single(p => p.Points.All(pt => pt.Y <= 10));
        CollectionAssert.AreEquivalent(new[] { new RPoint(0, 0), new RPoint(40, 0), new RPoint(30, 10), new RPoint(10, 10) }, top.Points);
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [Spec("css-backgrounds-3", "#corner-transitions")]
    public void AdjoiningBorders_MeetAtTheDiagonal()
    {
        var g = PaintShapes("width:20px;height:20px;border-style:solid;border-width:10px;border-color:red blue");

        // The top border is a trapezoid from the outer corners to the inner ones
        var top = g.Log.OfType<RecordingGraphics.DrawPolygonCall>().Single(p => p.Color == Red && p.Points.All(pt => pt.Y <= 10));
        CollectionAssert.AreEquivalent(new[] { new RPoint(0, 0), new RPoint(40, 0), new RPoint(30, 10), new RPoint(10, 10) }, top.Points);
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [Spec("css-backgrounds-3", "#corner-transitions")]
    public void RoundedCorner_TransitionsBetweenTheAdjoiningBorderColors()
    {
        var g = PaintShapes("width:100px;height:100px;border-style:solid;border-width:4px;border-color:red blue;border-radius:10px");

        // Each border's path ends halfway around the corner, the top one doesn't run the full arc
        var top = g.Paths.First(p => p.Pen == Red);
        Assert.IsTrue(top.Points[0].X > 2, $"the top border starts at the left edge, {top.Points[0]}");
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [Spec("css-backgrounds-3", "#corner-overlap")]
    public void OverlappingRadii_AreAllScaledByTheSameFactor()
    {
        var box = LayoutDiv("width:200px;height:100px;border-radius:60px");

        var radii = box.ComputeRadii(new RRect(0, 0, 200, 100));

        Assert.AreEqual(50, radii.TLX, 0.5);
        Assert.AreEqual(50, radii.TLY, 0.5);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius-tables")]
    public void BorderRadius_RoundsASeparatedBordersTable()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t' style='border-collapse:separate;border-radius:10px;background-color:rgb(10,20,30)'><tr><td>x</td></tr></table>"));
        var g = new ShapeRecordingGraphics();

        PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!, g);

        Assert.AreEqual(4, g.Paths.Single(p => p.Brush == Fill).Arcs.Count);
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius-tables")]
    public void BorderRadius_DoesNotApplyToACollapsingBordersTable()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t' style='border-collapse:collapse;border-radius:10px;background-color:rgb(10,20,30)'><tr><td>x</td></tr></table>"));
        var g = new ShapeRecordingGraphics();

        PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!, g);

        Assert.IsFalse(g.Paths.Any(p => p.Brush == Fill));
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [Spec("css-backgrounds-3", "#border-images")]
    public void BorderImage_ReplacesTheBorderStyle()
    {
        var g = Paint($"width:100px;height:100px;border:10px solid red;border-image:{Image} 10");

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawLineCall>().Any(l => l.Color == Red));
        Assert.IsTrue(g.DrawImageCalls.Any());
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [Spec("css-backgrounds-3", "#box-shadow")]
    public void BoxShadow_PaintsAShadowBeneathTheBackground()
    {
        var g = Paint("width:100px;height:50px;background-color:rgb(10,20,30);box-shadow:5px 5px blue");

        var shadow = g.Log.FindIndex(c => c is RecordingGraphics.DrawRectCall r && r.Color == Blue);
        var background = g.Log.FindIndex(c => c is RecordingGraphics.DrawRectCall r && r.Color == Fill);
        Assert.IsTrue(shadow >= 0 && shadow < background);
    }

    /// <summary>Lays out a div with <paramref name="style"/> at the origin, loading images synchronously.</summary>
    private static (HtmlContainerInt Container, TheArtOfDev.HtmlRenderer.Core.Dom.CssBox Box) LayoutDivWithContainer(string style)
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

        return (container, PaintHarness.FindById(container.Root!, "b")!);
    }

    private static TheArtOfDev.HtmlRenderer.Core.Dom.CssBox LayoutDiv(string style) => LayoutDivWithContainer(style).Box;

    private static RecordingGraphics Paint(string style)
    {
        var (container, box) = LayoutDivWithContainer(style);
        return PaintHarness.PaintBox(container, box);
    }

    private static ShapeRecordingGraphics PaintShapes(string style)
    {
        var (container, box) = LayoutDivWithContainer(style);
        var g = new ShapeRecordingGraphics();
        PaintHarness.PaintBox(container, box, g);
        return g;
    }

    internal sealed record Arc(RPoint End, double RadiusX, double RadiusY);

    internal sealed class ShapePath : RGraphicsPath
    {
        public List<RPoint> Points { get; } = [];
        public List<Arc> Arcs { get; } = [];
        public RColor? Brush { get; set; }
        public RColor? Pen { get; set; }

        public override void Start(double x, double y) => Points.Add(new RPoint(x, y));
        public override void LineTo(double x, double y) => Points.Add(new RPoint(x, y));

        public override void ArcTo(double x, double y, double radiusX, double radiusY, Corner corner)
        {
            Points.Add(new RPoint(x, y));
            Arcs.Add(new Arc(new RPoint(x, y), radiusX, radiusY));
        }

        public override void Dispose() { }
    }

    /// <summary>Also records the paths, which <see cref="RecordingGraphics"/> drops.</summary>
    internal sealed class ShapeRecordingGraphics : RecordingGraphics
    {
        public List<ShapePath> Paths { get; } = [];

        public override RGraphicsPath GetGraphicsPath() => new ShapePath();

        public override void DrawPath(RPen pen, RGraphicsPath path)
        {
            var shape = (ShapePath)path;
            shape.Pen = pen is MockPen p ? p.Color : RColor.Empty;
            Paths.Add(shape);
        }

        public override void DrawPath(RBrush brush, RGraphicsPath path)
        {
            var shape = (ShapePath)path;
            shape.Brush = brush is MockBrush b ? b.Color : RColor.Empty;
            Paths.Add(shape);
        }
    }
}
