using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Tables;

[DoNotParallelize]
[TestClass]
public sealed class TableBorderIntegrationTests
{
    private const double Delta = 0.5;

    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);
    private static readonly RColor Blue = RColor.FromArgb(0, 0, 255);

    private static List<RecordingGraphics.DrawLineCall> VerticalLines(RecordingGraphics g, RColor color) =>
        g.Log.OfType<RecordingGraphics.DrawLineCall>().Where(l => l.Color == color && Math.Abs(l.X1 - l.X2) < 0.01).ToList();

    [TestMethod]
    [Spec("css21", "tables.html#borders")]
    [Spec("css-tables-3", "#border-collapse-property")]
    public void BorderCollapse_IsSeparateByDefault()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:10px'><tr><td id='a'>a</td><td id='b'>b</td></tr></table>"));

        Assert.AreEqual(10, LayoutHarness.FindById(root, "b")!.Location.X - LayoutHarness.FindById(root, "a")!.ActualRight, Delta);
    }

    [TestMethod]
    [Spec("css21", "tables.html#separated-borders")]
    [Spec("css-tables-3", "#computing-undistributable-space")]
    [Spec("css-tables-3", "#bounding-box-assignment")]
    public void BorderSpacing_SeparatesAdjoiningCellsAndTheTableBorder()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='border:3px solid;border-spacing:10px'><tr><td id='a'>a</td><td id='b'>b</td></tr><tr><td id='c'>c</td><td>d</td></tr></table>"));

        var t = LayoutHarness.FindById(root, "t")!;
        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;
        var c = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(10, b.Location.X - a.ActualRight, Delta);
        Assert.AreEqual(10, c.Location.Y - a.ActualBottom, Delta);
        Assert.AreEqual(t.Location.X + 3 + 10, a.Location.X, Delta);
        Assert.AreEqual(t.Location.Y + 3 + 10, a.Location.Y, Delta);
        Assert.AreEqual(b.ActualRight + 10 + 3, t.ActualRight, Delta);
    }

    [TestMethod]
    [Spec("css21", "tables.html#separated-borders")]
    [Spec("css-tables-3", "#border-spacing-property")]
    public void BorderSpacing_TwoLengthsAreHorizontalThenVertical()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:4px 12px'><tr><td id='a'>a</td><td id='b'>b</td></tr><tr><td id='c'>c</td><td>d</td></tr></table>"));

        var a = LayoutHarness.FindById(root, "a")!;

        Assert.AreEqual(4, LayoutHarness.FindById(root, "b")!.Location.X - a.ActualRight, Delta);
        Assert.AreEqual(12, LayoutHarness.FindById(root, "c")!.Location.Y - a.ActualBottom, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#separated-borders")]
    [Spec("css-tables-3", "#computing-undistributable-space")]
    public void TablePadding_AddsToTheBorderSpacingAtTheEdges()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='padding:7px;border-spacing:10px'><tr><td id='a'>a</td></tr></table>"));

        var t = LayoutHarness.FindById(root, "t")!;

        Assert.AreEqual(t.Location.X + 7 + 10, LayoutHarness.FindById(root, "a")!.Location.X, Delta);
    }

    [TestMethod]
    [Spec("css21", "tables.html#separated-borders")]
    public void RowBorders_AreIgnored()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t' style='border-spacing:0'><tr style='border:3px solid rgb(255,0,0)'><td style='width:40px;height:20px'></td></tr></table>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawLineCall>().Any(l => l.Color == Red));
        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawPolygonCall>().Any(p => p.Color == Red));
    }

    [TestMethod]
    [Spec("css21", "tables.html#empty-cells")]
    [Spec("css-tables-3", "#empty-cell-rendering")]
    public void EmptyCellsHide_PaintsNoBackgroundOrBorderAroundAnEmptyCell()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t' style='empty-cells:hide'><tr><td style='background-color:rgb(255,0,0);border:2px solid rgb(255,0,0);width:40px;height:20px'></td></tr></table>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == Red));
        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawLineCall>().Any(l => l.Color == Red));
    }

    [TestMethod]
    [Spec("css21", "tables.html#empty-cells")]
    [Spec("css-tables-3", "#empty-cell-rendering")]
    public void EmptyCellsShow_PaintsTheBackgroundOfAnEmptyCell()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t'><tr><td style='background-color:rgb(255,0,0);width:40px;height:20px'></td></tr></table>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!);

        Assert.IsTrue(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == Red));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#empty-cells")]
    [Spec("css-tables-3", "#empty-cell-rendering")]
    public void EmptyCellsHide_StillPaintsTheBackgroundOfACellWithText()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t' style='empty-cells:hide'><tr><td style='background-color:rgb(0,0,255)'>x</td></tr></table>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!);

        Assert.IsTrue(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == Blue));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#empty-cells")]
    public void EmptyCellsHide_RowOfOnlyEmptyCellsHasZeroHeight()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='empty-cells:hide;border-spacing:10px'><tr><td id='a'>a</td></tr><tr><td style='padding:5px'></td></tr><tr><td id='c'>c</td></tr></table>"));

        var a = LayoutHarness.FindById(root, "a")!;

        Assert.AreEqual(a.ActualBottom + 10, LayoutHarness.FindById(root, "c")!.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#collapsing-borders")]
    [Spec("css-tables-3", "#drawing-collapsed-borders-2")]
    public void CollapsedBorders_AreCenteredOnTheGridLineBetweenCells()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-collapse:collapse'><tr>"
            + "<td id='a' style='border:4px solid;padding:0;width:40px'>a</td>"
            + "<td id='b' style='border:4px solid;padding:0;width:40px'>b</td></tr></table>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(4, b.ClientLeft - a.ClientRight, Delta);
    }

    [TestMethod]
    [Spec("css21", "tables.html#collapsing-borders")]
    [Spec("css-tables-3", "#border-collapse-property")]
    [Spec("css-tables-3", "#collapsed-style-overrides")]
    public void Collapse_IgnoresTheBorderSpacing()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-collapse:collapse;border-spacing:20px'><tr>"
            + "<td id='a' style='padding:0;width:40px'>a</td>"
            + "<td id='b' style='padding:0;width:40px'>b</td></tr></table>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.IsLessThanOrEqualTo(1, Math.Abs(b.Location.X - a.ActualRight));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#border-conflict-resolution")]
    [Spec("css-tables-3", "#border-specificity")]
    public void WiderBorder_WinsTheSharedEdge()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t' style='border-collapse:collapse'><tr>"
            + "<td style='border:1px solid rgb(255,0,0);width:40px;height:20px'></td>"
            + "<td id='b' style='border:5px solid rgb(0,0,255);width:40px;height:20px'></td></tr></table>"));

        var b = PaintHarness.FindById(root, "b")!;
        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!);

        Assert.IsFalse(VerticalLines(g, Red).Any(l => l.X1 > b.Location.X - 3));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#border-conflict-resolution")]
    [Spec("css-tables-3", "#border-conflict-resolution-algorithm")]
    public void TableBorder_WinsOverANarrowerCellBorder()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t' style='border-collapse:collapse;border:6px solid rgb(0,0,255)'><tr><td style='border:2px solid rgb(255,0,0);width:40px;height:20px'></td></tr></table>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawLineCall>().Any(l => l.Color == Red));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#table-border-styles")]
    [Spec("css-tables-3", "#border-style-overrides")]
    public void HiddenBorder_SuppressesTheNeighboursBorder()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t' style='border-collapse:collapse'><tr>"
            + "<td style='border-right:hidden;width:40px;height:20px'></td>"
            + "<td id='b' style='border:3px solid rgb(255,0,0);width:40px;height:20px'></td></tr></table>"));

        var b = PaintHarness.FindById(root, "b")!;
        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!);

        Assert.IsFalse(VerticalLines(g, Red).Any(l => l.X1 < b.Location.X + 5));
    }
}
