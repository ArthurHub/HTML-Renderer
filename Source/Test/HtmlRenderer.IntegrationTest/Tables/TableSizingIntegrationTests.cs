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
    [Spec("css-tables-3", "#layout-principles")]
    public void AutoWidth_ShrinksToTheContent()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:600px'><table id='t'><tr><td>short</td></tr></table></div>"));

        Assert.IsLessThan(100, Width(LayoutHarness.FindById(root, "t")!));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#width-layout")]
    [Spec("css-tables-3", "#layout-principles")]
    public void AutoMargins_CenterTheTable()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:400px'><table id='t' style='width:200px;margin:0 auto'><tr><td>a</td></tr></table></div>"));

        Assert.AreEqual(100, LayoutHarness.FindById(root, "t")!.Location.X, 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#fixed-table-layout")]
    [Spec("css-tables-3", "#table-layout-property")]
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
    [Spec("css-tables-3", "#table-layout-property")]
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
    [Spec("css-tables-3", "#computing-column-measures")]
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
    [Spec("css-tables-3", "#computing-the-table-width")]
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
    [Spec("css-tables-3", "#computing-the-table-width")]
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
    [Spec("css-tables-3", "#computing-column-measures")]
    public void CellWidth_IsAMinimumForItsColumn()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr><td id='a' style='width:150px;padding:0'>a</td><td>b</td></tr></table>"));

        Assert.AreEqual(150, Width(LayoutHarness.FindById(root, "a")!), 1);
    }

    [TestMethod]
    [Spec("css21", "tables.html#auto-table-layout")]
    [Spec("css-tables-3", "#width-distribution-algorithm")]
    public void PercentageColumnWidth_IsRelativeToTheTableWidth()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='width:400px;border-spacing:0'><tr><td id='a' style='width:25%;padding:0'>a</td><td>b</td></tr></table>"));

        Assert.AreEqual(100, Width(LayoutHarness.FindById(root, "a")!), 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#auto-table-layout")]
    [Spec("css-tables-3", "#computing-column-measures")]
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
    [Spec("css-tables-3", "#computing-the-table-width")]
    public void CaptionMinimumWidth_WidensTheTable()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='border-spacing:0'><caption>Supercalifragilisticexpialidocious</caption><tr><td style='padding:0'>a</td></tr></table>"));

        Assert.IsGreaterThan(100, Width(LayoutHarness.FindById(root, "t")!));
    }

    [TestMethod]
    [Spec("css21", "tables.html#auto-table-layout")]
    [Spec("css-tables-3", "#computing-column-measures")]
    public void SpanningCell_TheColumnsItSpansAreTogetherAsWideAsTheCell()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='width:10px;border-spacing:4px'>"
            + "<tr><td id='a' style='padding:0'>x</td><td id='b' style='padding:0'>x</td></tr>"
            + "<tr><td id='s' colspan='2' style='padding:0'>Supercalifragilisticexpialidocious</td></tr></table>"));

        var s = LayoutHarness.FindById(root, "s")!;
        var word = LayoutHarness.Descendants(s).SelectMany(b => b.Words).Single().Width;

        Assert.AreEqual(word, Width(s), Delta);
        Assert.AreEqual(word, Width(LayoutHarness.FindById(root, "a")!) + 4 + Width(LayoutHarness.FindById(root, "b")!), Delta);
        Assert.AreEqual(word + 2 * 4, Width(LayoutHarness.FindById(root, "t")!), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#auto-table-layout")]
    [Spec("css-tables-3", "#computing-column-measures")]
    public void SpanningCellAfterAnotherCell_DoesNotNarrowThatCellsColumn()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='width:10px;border-spacing:0'>"
            + "<tr><td style='padding:0'>x</td><td style='padding:0'>x</td><td style='padding:0'>x</td></tr>"
            + "<tr><td id='d' style='padding:0'>Mississippi</td><td colspan='2' style='padding:0'>Supercalifragilisticexpialidocious</td></tr></table>"));

        var d = LayoutHarness.FindById(root, "d")!;

        Assert.IsGreaterThanOrEqualTo(LayoutHarness.Descendants(d).SelectMany(b => b.Words).Single().Width - Delta, Width(d));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#auto-table-layout")]
    [Spec("css-tables-3", "#computing-column-measures")]
    public void SpanningCellOverAWideColumn_IsAsWideAsItsContent()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='width:10px;border-spacing:0'>"
            + "<tr><td style='padding:0'>Mississippi</td><td style='padding:0'>x</td><td style='padding:0'>x</td></tr>"
            + "<tr><td id='s' colspan='2' style='padding:0'>Supercalifragilisticexpialidocious</td><td style='padding:0'>x</td></tr></table>"));

        var s = LayoutHarness.FindById(root, "s")!;

        Assert.IsGreaterThanOrEqualTo(LayoutHarness.Descendants(s).SelectMany(b => b.Words).Single().Width - Delta, Width(s));
    }

    [TestMethod]
    [Spec("css21", "visudet.html#min-max-widths")]
    [Spec("css-tables-3", "#computing-the-table-width")]
    public void MaxWidth_LimitsAnAutoWidthTable()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:600px'><table id='t' style='max-width:200px;border-spacing:0'><tr>"
            + "<td id='a' style='padding:0'>aaa bbb ccc ddd eee fff ggg hhh iii jjj</td>"
            + "<td id='b' style='padding:0'>aaa bbb ccc ddd eee fff ggg hhh iii jjj</td></tr></table></div>"));

        Assert.AreEqual(200, Width(LayoutHarness.FindById(root, "t")!), Delta);
        Assert.AreEqual(100, Width(LayoutHarness.FindById(root, "a")!), Delta);
        Assert.AreEqual(100, Width(LayoutHarness.FindById(root, "b")!), Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#min-max-widths")]
    [Spec("css-tables-3", "#computing-the-table-width")]
    public void PercentageMaxWidth_IsRelativeToTheContainingBlock()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:400px'><table id='t' style='max-width:50%;border-spacing:0'><tr>"
            + "<td style='padding:0'>aaa bbb ccc ddd eee fff ggg hhh iii jjj</td>"
            + "<td style='padding:0'>aaa bbb ccc ddd eee fff ggg hhh iii jjj</td></tr></table></div>"));

        Assert.AreEqual(200, Width(LayoutHarness.FindById(root, "t")!), Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#min-max-widths")]
    [Spec("css-tables-3", "#distributing-width-to-columns")]
    public void MaxWidth_ColumnWhoseContentFits_KeepsItsMaxContentWidth()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:600px'><table id='t' style='max-width:200px;border-spacing:0'><tr>"
            + "<td id='a' style='padding:0'>a</td>"
            + "<td id='b' style='padding:0'>aaa bbb ccc ddd eee fff ggg hhh iii jjj</td></tr></table></div>"));

        var a = LayoutHarness.FindById(root, "a")!;

        Assert.AreEqual(LayoutHarness.Descendants(a).SelectMany(b => b.Words).Single().Width, Width(a), 1);
        Assert.AreEqual(200, Width(a) + Width(LayoutHarness.FindById(root, "b")!), Delta);
        Assert.AreEqual(200, Width(LayoutHarness.FindById(root, "t")!), Delta);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#min-max-widths")]
    [Spec("css-tables-3", "#computing-the-table-width")]
    public void MaxWidthBelowTheMinimumContentWidth_DoesNotNarrowTheColumns()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:600px'><table style='max-width:30px;border-spacing:0'><tr>"
            + "<td id='a' style='padding:0'>Supercalifragilistic</td><td style='padding:0'>Supercalifragilistic</td></tr></table></div>"));

        var a = LayoutHarness.FindById(root, "a")!;

        Assert.AreEqual(LayoutHarness.Descendants(a).SelectMany(b => b.Words).Single().Width, Width(a), 1);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#auto-table-layout")]
    [Spec("css-tables-3", "#computing-the-table-width")]
    public void PercentageCellWidthsOverAHundredPercent_DoNotWidenTheTablePastTheContainingBlock()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:400px'><table id='t' style='border-spacing:0'><tr>"
            + "<td style='width:80%;padding:0'>a</td><td style='width:80%;padding:0'>b</td></tr></table></div>"));

        Assert.AreEqual(400, Width(LayoutHarness.FindById(root, "t")!), 1);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#float-width")]
    [Spec("css21", "tables.html#separated-borders")]
    public void FloatAroundATable_IsAsWideAsTheTableWithItsBorderSpacing()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:600px'><div id='f' style='float:left'><table id='t' style='border-spacing:10px'>"
            + "<tr><td style='padding:0;width:50px'>a</td><td style='padding:0;width:50px'>b</td></tr></table></div></div>"));

        Assert.AreEqual(130, Width(LayoutHarness.FindById(root, "t")!), Delta);
        Assert.AreEqual(130, Width(LayoutHarness.FindById(root, "f")!), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#float-width")]
    [Spec("css21", "tables.html#separated-borders")]
    public void FloatAroundATableWithARowGroup_IsAsWideAsTheTableWithItsBorderSpacing()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:600px'><div id='f' style='float:left'><table id='t' style='border-spacing:10px'>"
            + "<tbody><tr><td style='padding:0;width:50px'>a</td><td style='padding:0;width:50px'>b</td></tr></tbody></table></div></div>"));

        Assert.AreEqual(130, Width(LayoutHarness.FindById(root, "f")!), Delta);
    }

    [TestMethod]
    [Spec("css21", "tables.html#height-layout")]
    [Spec("css-tables-3", "#computing-the-table-height")]
    public void TableHeight_IsAMinimumHeight()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='height:200px'><tr><td>a</td></tr></table>"));

        Assert.AreEqual(200, Height(LayoutHarness.FindById(root, "t")!), 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#height-layout")]
    [Spec("css-tables-3", "#computing-the-table-height")]
    public void RowHeight_IsAMinimumHeight()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr style='height:80px'><td id='a' style='padding:0'>a</td></tr></table>"));

        Assert.AreEqual(80, Height(LayoutHarness.FindById(root, "a")!), 1);
    }

    [TestMethod]
    [Spec("css21", "tables.html#height-layout")]
    [Spec("css-tables-3", "#row-layout")]
    public void CellHeight_SetsTheRowHeight()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr><td id='a' style='height:80px;padding:0'>a</td><td id='b' style='padding:0'>b</td></tr></table>"));

        Assert.AreEqual(80, Height(LayoutHarness.FindById(root, "a")!), 1);
        Assert.AreEqual(80, Height(LayoutHarness.FindById(root, "b")!), 1);
    }

    [TestMethod]
    [Spec("css21", "tables.html#height-layout")]
    [Spec("css-tables-3", "#row-layout")]
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
    [Spec("css-tables-3", "#row-layout")]
    public void VerticalAlignTop_KeepsTheContentAtTheTopOfTheRow()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr><td style='height:100px'>tall</td><td id='v' style='vertical-align:top;padding:0'>short</td></tr></table>"));

        var v = LayoutHarness.FindById(root, "v")!;

        Assert.AreEqual(v.Location.Y, FirstWordTop(v), 1);
    }

    [TestMethod]
    [Spec("css21", "tables.html#height-layout")]
    [Spec("css-tables-3", "#row-layout")]
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
    [Spec("css-tables-3", "#row-layout")]
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
    [Spec("css-tables-3", "#row-layout")]
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
