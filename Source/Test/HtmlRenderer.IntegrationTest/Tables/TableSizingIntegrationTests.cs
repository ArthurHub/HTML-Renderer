using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Tables;

[DoNotParallelize]
[TestClass]
public sealed class TableSizingIntegrationTests
{
    private const double Delta = 0.5;

    private static double Width(CssBox box) => box.ActualRight - box.Location.X;

    private static double Height(CssBox box) => box.ActualBottom - box.Location.Y;

    private static double FirstWordTop(CssBox box) => LayoutHarness.Descendants(box).SelectMany(b => b.Words).First().Top;

    private static double FirstWordBottom(CssBox box) => LayoutHarness.Descendants(box).SelectMany(b => b.Words).First().Bottom;

    [TestMethod]
    [Spec("css21", "tables.html#width-layout")]
    public void AutoWidth_ShrinksToTheContent()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:600px'><table id='t'><tr><td>short</td></tr></table></div>"));

        Assert.IsLessThan(100, Width(LayoutHarness.FindById(root, "t")!));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#width-layout")]
    public void AutoMargins_CenterTheTable()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:400px'><table id='t' style='width:200px;margin:0 auto'><tr><td>a</td></tr></table></div>"));

        Assert.AreEqual(100, LayoutHarness.FindById(root, "t")!.Location.X, 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#fixed-table-layout")]
    public void FixedLayout_ColumnWidthsComeFromTheFirstRowOnly()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='table-layout:fixed;width:300px;border-spacing:0'>"
            + "<tr><td id='a' style='width:100px;padding:0'>a</td><td id='b' style='padding:0'>b</td></tr>"
            + "<tr><td style='width:250px;padding:0'>wide</td><td style='padding:0'>c</td></tr></table>"));

        Assert.AreEqual(100, Width(LayoutHarness.FindById(root, "a")!), 1);
        Assert.AreEqual(200, Width(LayoutHarness.FindById(root, "b")!), 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#fixed-table-layout")]
    public void FixedLayout_ContentDoesNotWidenAColumn()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='table-layout:fixed;width:200px;border-spacing:0'>"
            + "<tr><td id='a' style='padding:0'>Supercalifragilisticexpialidocious</td><td id='b' style='padding:0'>b</td></tr></table>"));

        Assert.AreEqual(100, Width(LayoutHarness.FindById(root, "a")!), 1);
        Assert.AreEqual(100, Width(LayoutHarness.FindById(root, "b")!), 1);
    }

    [TestMethod]
    [Spec("css21", "tables.html#auto-table-layout")]
    public void ColumnWidth_IsTheWidestCellInTheColumn()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'>"
            + "<tr><td id='a' style='padding:0'>a</td></tr>"
            + "<tr><td id='b' style='padding:0'>a much longer cell</td></tr></table>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(Width(b), Width(a), Delta);
        Assert.IsGreaterThan(50, Width(a));
    }

    [TestMethod]
    [Spec("css21", "tables.html#auto-table-layout")]
    public void SpecifiedTableWidth_IsDistributedOverTheColumns()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='width:400px;border-spacing:0'><tr><td id='a' style='padding:0'>a</td><td id='b' style='padding:0'>b</td></tr></table>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(400, Width(a) + Width(b), 1);
        Assert.AreEqual(400, Width(LayoutHarness.FindById(root, "t")!), 1);
    }

    [TestMethod]
    [Spec("css21", "tables.html#auto-table-layout")]
    public void SpecifiedTableWidth_GrowsToTheMinimumContentWidth()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='width:20px;border-spacing:0'><tr><td id='a' style='padding:0'>Supercalifragilisticexpialidocious</td></tr></table>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var word = LayoutHarness.Descendants(a).SelectMany(b => b.Words).First();

        Assert.IsGreaterThanOrEqualTo(word.Width - 1, Width(a));
        Assert.IsGreaterThanOrEqualTo(Width(a) - 1, Width(LayoutHarness.FindById(root, "t")!));
    }

    [TestMethod]
    [Spec("css21", "tables.html#auto-table-layout")]
    public void CellWidth_IsAMinimumForItsColumn()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr><td id='a' style='width:150px;padding:0'>a</td><td>b</td></tr></table>"));

        Assert.AreEqual(150, Width(LayoutHarness.FindById(root, "a")!), 1);
    }

    [TestMethod]
    [Spec("css21", "tables.html#auto-table-layout")]
    public void PercentageColumnWidth_IsRelativeToTheTableWidth()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='width:400px;border-spacing:0'><tr><td id='a' style='width:25%;padding:0'>a</td><td>b</td></tr></table>"));

        Assert.AreEqual(100, Width(LayoutHarness.FindById(root, "a")!), 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#auto-table-layout")]
    public void ColumnGroupWidth_IsAMinimumForItsColumns()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><colgroup style='width:200px'><col><col></colgroup>"
            + "<tr><td id='a' style='padding:0'>a</td><td id='b' style='padding:0'>b</td></tr></table>"));

        Assert.AreEqual(200, Width(LayoutHarness.FindById(root, "a")!) + Width(LayoutHarness.FindById(root, "b")!), 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#auto-table-layout")]
    public void CaptionMinimumWidth_WidensTheTable()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='border-spacing:0'><caption>Supercalifragilisticexpialidocious</caption><tr><td style='padding:0'>a</td></tr></table>"));

        Assert.IsGreaterThan(100, Width(LayoutHarness.FindById(root, "t")!));
    }

    [TestMethod]
    [Spec("css21", "tables.html#height-layout")]
    public void TableHeight_IsAMinimumHeight()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='height:200px'><tr><td>a</td></tr></table>"));

        Assert.AreEqual(200, Height(LayoutHarness.FindById(root, "t")!), 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#height-layout")]
    public void RowHeight_IsAMinimumHeight()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr style='height:80px'><td id='a' style='padding:0'>a</td></tr></table>"));

        Assert.AreEqual(80, Height(LayoutHarness.FindById(root, "a")!), 1);
    }

    [TestMethod]
    [Spec("css21", "tables.html#height-layout")]
    public void CellHeight_SetsTheRowHeight()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr><td id='a' style='height:80px;padding:0'>a</td><td id='b' style='padding:0'>b</td></tr></table>"));

        Assert.AreEqual(80, Height(LayoutHarness.FindById(root, "a")!), 1);
        Assert.AreEqual(80, Height(LayoutHarness.FindById(root, "b")!), 1);
    }

    [TestMethod]
    [Spec("css21", "tables.html#height-layout")]
    public void RowspanCell_IsCoveredByTheRowsItSpans()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'>"
            + "<tr><td id='span' rowspan='2' style='height:100px'>x</td><td id='a'>a</td></tr>"
            + "<tr><td id='b'>b</td></tr></table>"));

        var span = LayoutHarness.FindById(root, "span")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.IsGreaterThanOrEqualTo(span.ActualBottom - Delta, b.ActualBottom);
    }

    [TestMethod]
    [Spec("css21", "tables.html#height-layout")]
    public void VerticalAlignTop_KeepsTheContentAtTheTopOfTheRow()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr><td style='height:100px'>tall</td><td id='v' style='vertical-align:top;padding:0'>short</td></tr></table>"));

        var v = LayoutHarness.FindById(root, "v")!;

        Assert.AreEqual(v.Location.Y, FirstWordTop(v), 1);
    }

    [TestMethod]
    [Spec("css21", "tables.html#height-layout")]
    public void VerticalAlignBottom_PutsTheContentAtTheBottomOfTheRow()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr><td style='height:100px'>tall</td><td id='v' style='vertical-align:bottom;padding:0'>short</td></tr></table>"));

        var v = LayoutHarness.FindById(root, "v")!;

        Assert.AreEqual(v.ActualBottom, FirstWordBottom(v), 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#height-layout")]
    public void VerticalAlignBaseline_AlignsTheFirstLinesOfTheCells()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr>"
            + "<td id='big' style='vertical-align:baseline;font-size:40px;padding:0'>Big</td>"
            + "<td id='small' style='vertical-align:baseline;font-size:10px;padding:0'>small</td></tr></table>"));

        Assert.AreEqual(FirstWordBottom(LayoutHarness.FindById(root, "big")!), FirstWordBottom(LayoutHarness.FindById(root, "small")!), 3);
    }

    [TestMethod]
    [Spec("css21", "tables.html#height-layout")]
    public void VerticalAlignSub_IsTreatedAsBaseline()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr><td style='height:100px'>tall</td>"
            + "<td id='v' style='vertical-align:sub;padding:0'>short</td><td id='w' style='vertical-align:baseline;padding:0'>short</td></tr></table>"));

        Assert.AreEqual(FirstWordTop(LayoutHarness.FindById(root, "w")!), FirstWordTop(LayoutHarness.FindById(root, "v")!), Delta);
    }

    [TestMethod]
    [Spec("css21", "tables.html#column-alignment")]
    public void TextAlign_AlignsTheContentInsideTheCell()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr><td id='l' style='width:200px;padding:0;text-align:left'>x</td></tr>"
            + "<tr><td id='r' style='padding:0;text-align:right'>x</td></tr></table>"));

        var l = LayoutHarness.FindById(root, "l")!;
        var r = LayoutHarness.FindById(root, "r")!;

        Assert.AreEqual(l.Location.X, LayoutHarness.Descendants(l).SelectMany(b => b.Words).First().Left, 1);
        Assert.AreEqual(r.ActualRight, LayoutHarness.Descendants(r).SelectMany(b => b.Words).First().Right, 1);
    }
}
