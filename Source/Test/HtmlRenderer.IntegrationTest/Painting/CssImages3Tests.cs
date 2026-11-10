using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class CssImages3Tests
{
    // The adapter decodes every image as 40x30
    private const string Image = "url(data:image/png;base64,iVBORw0KGgo=)";

    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);
    private static readonly RColor Blue = RColor.FromArgb(0, 0, 255);

    [TestMethod]
    [Spec("css-images-3", "#url-notation")]
    [Spec("css-images-3", "#default-sizing")]
    [Spec("css-images-4", "#image-values")]
    [Spec("css-images-4", "#url-notation")]
    [Spec("css-images-4", "#sizing")]
    public void UrlImage_IsDrawnAtItsNaturalSize()
    {
        var (g, _) = Paint($"<div id='b' style='width:100px;height:100px;background-image:{Image};background-repeat:no-repeat'></div>");

        Assert.AreEqual(new RRect(0, 0, 40, 30), g.DrawImageCalls.Single().DestRect);
    }

    [TestMethod]
    [Spec("css-images-3", "#default-sizing")]
    [Spec("css-images-4", "#sizing")]
    public void ImageWithOnlyAWidth_KeepsItsNaturalAspectRatio()
    {
        var root = TestImages.Layout($"<img id='i' src='{TestImages.Png(40, 30)}' style='width:80px'>");

        var rect = TestImages.ImageRect(root, "i");

        Assert.AreEqual(80, rect.Width, 0.5);
        Assert.AreEqual(60, rect.Height, 0.5);
    }

    [Ignore("Gap, see Spec/css-images-3.yml")]
    [TestMethod]
    [Spec("css-images-3", "#default-sizing")]
    public void Gradient_IsSizedToThePaddingBox()
    {
        var (_, gradient) = Paint("<div id='b' style='width:100px;height:50px;border:10px solid black;background-image:linear-gradient(to right, red, blue)'></div>");

        Assert.AreEqual(10, gradient!.P1.X, 0.5);
        Assert.AreEqual(110, gradient.P2.X, 0.5);
    }

    [TestMethod]
    [DataRow("linear-gradient(red, blue)", 50, 0, 50, 50)]
    [DataRow("linear-gradient(to right, red, blue)", 0, 25, 100, 25)]
    [DataRow("linear-gradient(to top, red, blue)", 50, 50, 50, 0)]
    [DataRow("linear-gradient(90deg, red, blue)", 0, 25, 100, 25)]
    [DataRow("linear-gradient(0.25turn, red, blue)", 0, 25, 100, 25)]
    [Spec("css-images-3", "#linear-gradient-syntax")]
    [Spec("css-images-4", "#linear-gradients")]
    public void LinearGradient_DirectionSetsTheGradientLine(string image, double x1, double y1, double x2, double y2)
    {
        var (_, gradient) = Paint($"<div id='b' style='width:100px;height:50px;background-image:{image}'></div>");

        Assert.AreEqual(x1, gradient!.P1.X, 0.5);
        Assert.AreEqual(y1, gradient.P1.Y, 0.5);
        Assert.AreEqual(x2, gradient.P2.X, 0.5);
        Assert.AreEqual(y2, gradient.P2.Y, 0.5);
    }

    [TestMethod]
    [Spec("css-images-3", "#linear-gradient-syntax")]
    public void LinearGradient_AngledLineSpansTheBoxCorners()
    {
        // 45deg on 100x50: the line has length 100 sin 45 + 50 cos 45
        var (_, gradient) = Paint("<div id='b' style='width:100px;height:50px;background-image:linear-gradient(45deg, red, blue)'></div>");

        var dx = gradient!.P2.X - gradient.P1.X;
        var dy = gradient.P2.Y - gradient.P1.Y;
        Assert.AreEqual(150 * Math.Sqrt(0.5), Math.Sqrt(dx * dx + dy * dy), 0.5);
        Assert.AreEqual(dx, -dy, 0.01);
    }

    [Ignore("Gap, see Spec/css-images-3.yml")]
    [TestMethod]
    [Spec("css-images-3", "#linear-gradient-syntax")]
    public void LinearGradient_CornerDirectionDependsOnTheBoxShape()
    {
        var (_, gradient) = Paint("<div id='b' style='width:200px;height:100px;background-image:linear-gradient(to bottom right, red, blue)'></div>");

        // Perpendicular to the top right to bottom left diagonal: direction (1, 2), length 400 / sqrt 5
        Assert.AreEqual(60, gradient!.P1.X, 0.5);
        Assert.AreEqual(-30, gradient.P1.Y, 0.5);
        Assert.AreEqual(140, gradient.P2.X, 0.5);
        Assert.AreEqual(130, gradient.P2.Y, 0.5);
    }

    [TestMethod]
    [Spec("css-images-3", "#radial-gradient-syntax")]
    [Spec("css-images-4", "#radial-gradients")]
    public void RadialGradient_IsAccepted()
    {
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap("<div id='b' style='background-image:radial-gradient(circle at top left, red, blue)'></div>"));

        StringAssert.StartsWith(PaintHarness.FindById(root, "b")!.BackgroundImage, "radial-gradient(circle");
    }

    [Ignore("Gap, see Spec/css-images-3.yml")]
    [TestMethod]
    [DataRow("radial-gradient(red, blue)")]
    [DataRow("repeating-radial-gradient(red, blue 10px)")]
    [Spec("css-images-3", "#radial-gradients")]
    [Spec("css-images-4", "#radial-gradients")]
    public void RadialGradient_IsPainted(string image)
    {
        var (g, _) = Paint($"<div id='b' style='width:100px;height:50px;background-image:{image}'></div>");

        Assert.IsTrue(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any());
    }

    [TestMethod]
    [Spec("css-images-3", "#repeating-gradients")]
    [Spec("css-images-4", "#repeating-gradients")]
    public void RepeatingLinearGradient_RepeatsTheStopsAlongTheLine()
    {
        var (_, gradient) = Paint("<div id='b' style='width:100px;height:50px;background-image:repeating-linear-gradient(to right, red, blue 25px)'></div>");

        var reds = gradient!.Stops.Where(s => s.Color == Red).Select(s => s.Position).ToList();
        CollectionAssert.AreEqual(new[] { 0, 0.25, 0.5, 0.75, 1 }, reds.Select(p => Math.Round(p, 3)).ToArray());
    }

    [TestMethod]
    [Spec("css-images-3", "#color-stop-syntax")]
    [Spec("css-images-4", "#color-stop-syntax")]
    public void ColorStops_TakeLengthsPercentagesAndTwoPositions()
    {
        var (_, gradient) = Paint("<div id='b' style='width:100px;height:50px;background-image:linear-gradient(to right, red 10px, blue 40% 60%, red)'></div>");

        CollectionAssert.AreEqual(new[] { 0.1, 0.4, 0.6, 1 }, gradient!.Stops.Select(s => Math.Round(s.Position, 3)).ToArray());
        CollectionAssert.AreEqual(new[] { Red, Blue, Blue, Red }, gradient.Stops.Select(s => s.Color).ToArray());
    }

    [Ignore("Gap, see Spec/css-images-3.yml")]
    [TestMethod]
    [Spec("css-images-3", "#color-stop-syntax")]
    public void ColorStops_ResolveEmAgainstTheElementsFontSize()
    {
        var (_, gradient) = Paint("<div id='b' style='width:100px;height:50px;font-size:20px;background-image:linear-gradient(to right, red 1em, blue 2em)'></div>");

        CollectionAssert.AreEqual(new[] { 0.2, 0.4 }, gradient!.Stops.Select(s => Math.Round(s.Position, 3)).ToArray());
    }

    [TestMethod]
    [Spec("css-images-3", "#coloring-gradient-line")]
    [Spec("css-images-4", "#coloring-gradient-line")]
    public void ColorHint_ShiftsTheMidpointOfTheTransition()
    {
        var (_, gradient) = Paint("<div id='b' style='width:100px;height:50px;background-image:linear-gradient(to right, rgb(0,0,0), 25%, rgb(200,200,200))'></div>");

        // Half the color is reached at the hint, at 25% instead of 50%
        var atHint = gradient!.Stops.Where(s => s.Position > 0 && s.Position < 1).OrderBy(s => Math.Abs(s.Color.R - 100)).First();
        Assert.IsTrue(atHint.Position < 0.3, $"half the color at {atHint.Position}");
    }

    [Ignore("Gap, see Spec/css-images-3.yml")]
    [TestMethod]
    [Spec("css-images-3", "#coloring-gradient-line")]
    [Spec("css-images-4", "#coloring-gradient-line")]
    public void ColorsAreInterpolatedPremultiplied()
    {
        var (_, gradient) = Paint("<div id='b' style='width:100px;height:50px;background-image:linear-gradient(to right, red, 50%, transparent)'></div>");

        // Fading red into transparent black keeps the red, only the alpha changes
        foreach (var stop in gradient!.Stops.Where(s => s.Color.A > 0))
            Assert.AreEqual(255, stop.Color.R, $"stop at {stop.Position}");
    }

    [TestMethod]
    [Spec("css-images-3", "#color-stop-fixup")]
    [Spec("css-images-4", "#color-stop-fixup")]
    public void ColorStopFixup_DefaultsTheEndsAndSpacesStopsEvenly()
    {
        var (_, gradient) = Paint("<div id='b' style='width:100px;height:50px;background-image:linear-gradient(to right, red, blue, red 60%, blue, red)'></div>");

        CollectionAssert.AreEqual(new[] { 0, 0.3, 0.6, 0.8, 1 }, gradient!.Stops.Select(s => Math.Round(s.Position, 3)).ToArray());
    }

    [Ignore("Gap, see Spec/css-images-3.yml")]
    [TestMethod]
    [Spec("css-images-3", "#color-stop-fixup")]
    [Spec("css-images-4", "#color-stop-fixup")]
    public void ColorStopFixup_ClampsAStopBeforeThePreviousOne()
    {
        var (_, gradient) = Paint("<div id='b' style='width:100px;height:50px;background-image:linear-gradient(to right, red 50%, blue 20%)'></div>");

        CollectionAssert.AreEqual(new[] { 0.5, 0.5 }, gradient!.Stops.Select(s => Math.Round(s.Position, 3)).ToArray());
    }

    [Ignore("Gap, see Spec/css-images-3.yml")]
    [TestMethod]
    [DataRow("contain", 0, 12.5, 100, 75)]
    [DataRow("cover", -16.7, 0, 133.3, 100)]
    [DataRow("none", 30, 35, 40, 30)]
    [DataRow("scale-down", 30, 35, 40, 30)]
    [Spec("css-images-3", "#the-object-fit")]
    [Spec("css-images-4", "#the-object-fit")]
    public void ObjectFit_SizesTheImageInTheContentBox(string fit, double x, double y, double width, double height)
    {
        var (g, _) = Paint($"<img id='b' src='{TestImages.Png(40, 30)}' style='display:block;width:100px;height:100px;object-fit:{fit}'>");

        var image = g.DrawImageCalls.Single().DestRect;
        Assert.AreEqual(new RRect(x, y, width, height).ToString(), new RRect(Math.Round(image.X, 1), Math.Round(image.Y, 1), Math.Round(image.Width, 1), Math.Round(image.Height, 1)).ToString());
    }

    [Ignore("Gap, see Spec/css-images-3.yml")]
    [TestMethod]
    [Spec("css-images-3", "#the-object-position")]
    public void ObjectPosition_PlacesTheImageInTheContentBox()
    {
        var (g, _) = Paint($"<img id='b' src='{TestImages.Png(40, 30)}' style='display:block;width:100px;height:100px;object-fit:none;object-position:right 10px top'>");

        var image = g.DrawImageCalls.Single().DestRect;
        Assert.AreEqual(new RRect(50, 0, 40, 30), image);
    }

    /// <summary>Lays out and paints the element with id "b", returns the draw calls and the last gradient brush.</summary>
    private static (RecordingGraphics Graphics, GradientAdapter.Gradient? Gradient) Paint(string body)
    {
        var adapter = new GradientAdapter();
        var container = new HtmlContainerInt(adapter)
        {
            MaxSize = new RSize(1000, 4000),
            Location = RPoint.Empty,
            PageSize = new RSize(1000, 4000),
            AvoidAsyncImagesLoading = true,
            AvoidImagesLateLoading = true
        };
        container.SetHtml(PaintHarness.Wrap(body)).GetAwaiter().GetResult();

        using (var layoutGraphics = new RecordingGraphics(adapter))
            container.PerformLayout(layoutGraphics);

        var g = new RecordingGraphics(adapter);
        PaintHarness.PaintBox(container, PaintHarness.FindById(container.Root!, "b")!, g);
        return (g, adapter.Gradients.LastOrDefault());
    }

    /// <summary>Like <see cref="MockAdapter"/>, but remembers the linear gradient brushes it creates.</summary>
    internal sealed class GradientAdapter : RAdapter
    {
        internal sealed record Gradient(RPoint P1, RPoint P2, (RColor Color, double Position)[] Stops);

        public List<Gradient> Gradients { get; } = [];

        protected override RColor GetColorInt(string colorName)
        {
            var color = System.Drawing.Color.FromName(colorName);
            return RColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        protected override RPen CreatePen(RColor color) => new MockPen(color);

        protected override RBrush CreateSolidBrush(RColor color) => new MockBrush(color);

        protected override RBrush CreateLinearGradientBrush(RPoint p1, RPoint p2, (RColor Color, double Position)[] stops)
        {
            Gradients.Add(new Gradient(p1, p2, stops));
            return new MockBrush(stops.Length > 0 ? stops[0].Color : RColor.Black);
        }

        protected override RImage ConvertImageInt(object image) => image as RImage ?? new MockImage(0, 0);

        protected override RImage ImageFromStreamInt(System.IO.Stream memoryStream) => new MockImage(40, 30);

        protected override RFont CreateFontInt(string family, double size, RFontStyle style) => new MockFont(size);

        protected override RFont CreateFontInt(RFontFamily family, double size, RFontStyle style) => new MockFont(size);

        protected override RFontFamily LoadFontFaceFontInt(byte[] fontBytes, string filePath) => new MockFontFamily(filePath);
    }
}
