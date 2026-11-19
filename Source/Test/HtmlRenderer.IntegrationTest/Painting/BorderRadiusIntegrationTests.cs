using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class BorderRadiusIntegrationTests
{
    private const double Delta = 0.5;

    // ── Circular radii (symmetric X = Y) ────────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadius_Shorthand_SetsAllCorners()
    {
        var box = FindDivBox("border-radius:10px;border:1px solid black");

        Assert.AreEqual(10.0, box.ActualBorderTopLeftRadiusX, Delta);
        Assert.AreEqual(10.0, box.ActualBorderTopLeftRadiusY, Delta);
        Assert.AreEqual(10.0, box.ActualBorderTopRightRadiusX, Delta);
        Assert.AreEqual(10.0, box.ActualBorderTopRightRadiusY, Delta);
        Assert.AreEqual(10.0, box.ActualBorderBottomRightRadiusX, Delta);
        Assert.AreEqual(10.0, box.ActualBorderBottomRightRadiusY, Delta);
        Assert.AreEqual(10.0, box.ActualBorderBottomLeftRadiusX, Delta);
        Assert.AreEqual(10.0, box.ActualBorderBottomLeftRadiusY, Delta);
        Assert.IsTrue(box.IsRounded);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadius_TwoValues_SetsOpposingCorners()
    {
        var box = FindDivBox("border-radius:10px 20px");

        Assert.AreEqual(10.0, box.ActualBorderTopLeftRadiusX, Delta);      // top-left
        Assert.AreEqual(20.0, box.ActualBorderTopRightRadiusX, Delta);     // top-right
        Assert.AreEqual(10.0, box.ActualBorderBottomRightRadiusX, Delta);  // bottom-right
        Assert.AreEqual(20.0, box.ActualBorderBottomLeftRadiusX, Delta);   // bottom-left
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    [Spec("css-borders-4", "#corner-sizing-shorthand")]
    public void BorderRadius_FourValues_SetsAllCornersIndividually()
    {
        var box = FindDivBox("border-radius:5px 10px 15px 20px");

        Assert.AreEqual(5.0, box.ActualBorderTopLeftRadiusX, Delta);       // top-left
        Assert.AreEqual(10.0, box.ActualBorderTopRightRadiusX, Delta);     // top-right
        Assert.AreEqual(15.0, box.ActualBorderBottomRightRadiusX, Delta);  // bottom-right
        Assert.AreEqual(20.0, box.ActualBorderBottomLeftRadiusX, Delta);   // bottom-left
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderTopLeftRadius_Longhand_SetsOnlyTopLeft()
    {
        var box = FindDivBox("border-top-left-radius:12px");

        Assert.AreEqual(12.0, box.ActualBorderTopLeftRadiusX, Delta);
        Assert.AreEqual(12.0, box.ActualBorderTopLeftRadiusY, Delta);
        Assert.AreEqual(0.0, box.ActualBorderTopRightRadiusX, Delta);
        Assert.AreEqual(0.0, box.ActualBorderBottomRightRadiusX, Delta);
        Assert.AreEqual(0.0, box.ActualBorderBottomLeftRadiusX, Delta);
        Assert.IsTrue(box.IsRounded);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadius_Zero_IsNotRounded()
    {
        var box = FindDivBox("");
        Assert.IsFalse(box.IsRounded);
    }

    // ── Elliptical radii (X != Y) ───────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadius_EllipticalShorthand_SetsAllCornersXAndY()
    {
        // border-radius: 40px / 15px -> each corner: X=40, Y=15
        var box = FindDivBox("border-radius:40px / 15px");

        Assert.AreEqual(40.0, box.ActualBorderTopLeftRadiusX, Delta);
        Assert.AreEqual(15.0, box.ActualBorderTopLeftRadiusY, Delta);
        Assert.AreEqual(40.0, box.ActualBorderTopRightRadiusX, Delta);
        Assert.AreEqual(15.0, box.ActualBorderTopRightRadiusY, Delta);
        Assert.AreEqual(40.0, box.ActualBorderBottomRightRadiusX, Delta);
        Assert.AreEqual(15.0, box.ActualBorderBottomRightRadiusY, Delta);
        Assert.AreEqual(40.0, box.ActualBorderBottomLeftRadiusX, Delta);
        Assert.AreEqual(15.0, box.ActualBorderBottomLeftRadiusY, Delta);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    [Spec("css-borders-4", "#border-radius")]
    public void BorderTopLeftRadius_Longhand_EllipticalValues()
    {
        // border-top-left-radius: 15px 25px -> X=15, Y=25
        var box = FindDivBox("border-top-left-radius:15px 25px");

        Assert.AreEqual(15.0, box.ActualBorderTopLeftRadiusX, Delta);
        Assert.AreEqual(25.0, box.ActualBorderTopLeftRadiusY, Delta);
        Assert.AreEqual(0.0, box.ActualBorderTopRightRadiusX, Delta);
    }

    // ── Percentage values ────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadius_Percentage_ResolvesRelativeToDimensions()
    {
        // 200x100 box; border-radius: 50% -> X = 50% of 200 = 100, Y = 50% of 100 = 50
        var box = FindDivBox("border-radius:50%");

        Assert.AreEqual(100.0, box.ActualBorderTopLeftRadiusX, 1);
        Assert.AreEqual(50.0, box.ActualBorderTopLeftRadiusY, 1);
    }

    // ── Overlapping radii reduction ─────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-backgrounds-3", "#corner-overlap")]
    [Spec("css-borders-4", "#corner-overlap")]
    public void BorderRadius_OverlappingRadii_AreReducedProportionally()
    {
        // 100x100 box; border-radius: 60px - adjacent radii sum to 120 > 100, so all must scale by
        // 100/120 (roughly 0.833), giving ~50px at the boundary.
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='c' style='width:100px;height:100px;border-radius:60px'></div>"));
        var box = LayoutHarness.FindById(root, "c")!;

        var radii = box.ComputeRadii(new RRect(0, 0, 100, 100));

        Assert.AreEqual(100.0, radii.TLX + radii.TRX, 2);
        Assert.AreEqual(100.0, radii.BLX + radii.BRX, 2);
        Assert.AreEqual(100.0, radii.TLY + radii.BLY, 2);
        Assert.AreEqual(100.0, radii.TRY + radii.BRY, 2);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#corner-overlap")]
    public void BorderRadius_NonOverlappingRadii_AreNotChanged()
    {
        // 200x200 box; border-radius: 30px - 30+30=60 < 200, no reduction.
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='c' style='width:200px;height:200px;border-radius:30px'></div>"));
        var box = LayoutHarness.FindById(root, "c")!;

        var radii = box.ComputeRadii(new RRect(0, 0, 200, 200));

        Assert.AreEqual(30.0, radii.TLX, 2);
        Assert.AreEqual(30.0, radii.TLY, 2);
    }

    [TestMethod]
    [DataRow("80px 40px 0 40px", new[] { 80.0, 40, 0, 40 })]
    [DataRow("40px 80px 40px 0", new[] { 40.0, 80, 40, 0 })]
    [DataRow("0 40px 80px 40px", new[] { 0.0, 40, 80, 40 })]
    [DataRow("40px 0 40px 80px", new[] { 40.0, 0, 40, 80 })]
    [Spec("css-backgrounds-3", "#corner-overlap")]
    [Spec("css-borders-4", "#corner-overlap")]
    public void BorderRadius_OneOversizedCorner_ScalesAllRadiiByTheSmallestRatio(string radius, double[] corners)
    {
        // 100x100 box, the two sides at the large corner sum to 120: f = 100 / 120
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            $"<div id='c' style='width:100px;height:100px;border-radius:{radius}'></div>"));
        var box = LayoutHarness.FindById(root, "c")!;

        var radii = box.ComputeRadii(new RRect(0, 0, 100, 100));

        var expected = corners.Select(c => c * 100 / 120).ToArray();
        var x = new[] { radii.TLX, radii.TRX, radii.BRX, radii.BLX };
        var y = new[] { radii.TLY, radii.TRY, radii.BRY, radii.BLY };
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], x[i], 0.01, $"x radius {i}");
            Assert.AreEqual(expected[i], y[i], 0.01, $"y radius {i}");
        }
    }

    [TestMethod]
    [DataRow("border-top-left-radius", RGraphicsPath.Corner.TopLeft)]
    [DataRow("border-top-right-radius", RGraphicsPath.Corner.TopRight)]
    [DataRow("border-bottom-right-radius", RGraphicsPath.Corner.BottomRight)]
    [DataRow("border-bottom-left-radius", RGraphicsPath.Corner.BottomLeft)]
    [Spec("css-backgrounds-3", "#corner-clipping")]
    public void SingleRoundedCorner_ClipsTheBackgroundAtThatCornerOnly(string property, RGraphicsPath.Corner corner)
    {
        var g = PaintShapes($"width:100px;height:100px;background-color:rgb(10,20,30);{property}:10px");

        var arc = g.Paths.Single(p => p.Brush == Fill).Arcs.Single();
        Assert.AreEqual(corner, arc.Corner);
        Assert.AreEqual(10, arc.RadiusX, Delta);
        Assert.AreEqual(10, arc.RadiusY, Delta);
    }

    [TestMethod]
    [DataRow(false, true)]
    [DataRow(true, false)]
    public void RoundedBackgroundAndBorder_AreAntiAliasedUnlessAvoided(bool avoid, bool antiAliased)
    {
        var g = PaintShapes("width:100px;height:100px;background-color:rgb(10,20,30);border:4px solid black;border-radius:10px", avoid);

        Assert.AreEqual(5, g.Paths.Count);
        Assert.IsTrue(g.Paths.All(p => p.AntiAliased == antiAliased));
        Assert.IsFalse(g.AntiAlias, "the previous mode is restored");
    }

    private static readonly RColor Fill = RColor.FromArgb(10, 20, 30);

    private static ShapeRecordingGraphics PaintShapes(string style, bool avoidGeometryAntialias = false)
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap($"<div id='c' style='{style}'></div>"));
        container.AvoidGeometryAntialias = avoidGeometryAntialias;
        var g = new ShapeRecordingGraphics();
        PaintHarness.PaintBox(container, PaintHarness.FindById(root, "c")!, g);
        return g;
    }

    private static CssBox FindDivBox(string css)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            $"<div id='c' style='width:200px;height:100px;{css}'></div>"));
        return LayoutHarness.FindById(root, "c")!;
    }
}
