using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace HtmlRenderer.IntegrationTest.Tables;

[DoNotParallelize]
[TestClass]
public sealed class TableModelIntegrationTests
{
    private const double Delta = 0.5;

    [TestMethod]
    [Spec("css21", "tables.html#table-display")]
    [Spec("css-tables-3", "#table-structure")]
    public void DisplayValues_TurnArbitraryElementsIntoATable()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:table;border-spacing:0'>"
            + "<div style='display:table-row'><div id='a' style='display:table-cell;width:40px'>a</div><div id='b' style='display:table-cell;width:40px'>b</div></div>"
            + "<div style='display:table-row'><div id='c' style='display:table-cell'>c</div><div id='d' style='display:table-cell'>d</div></div>"
            + "</div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;
        var c = LayoutHarness.FindById(root, "c")!;
        var d = LayoutHarness.FindById(root, "d")!;

        Assert.AreEqual(a.Location.Y, b.Location.Y, Delta);
        Assert.AreEqual(a.ActualRight, b.Location.X, Delta);
        Assert.AreEqual(a.Location.X, c.Location.X, Delta);
        Assert.AreEqual(b.Location.X, d.Location.X, Delta);
        Assert.AreEqual(a.ActualBottom, c.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "tables.html#table-display")]
    [Spec("css-tables-3", "#table-structure")]
    public void HeaderGroup_IsDisplayedBeforeAndFooterGroupAfterTheBody()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table>"
            + "<tfoot><tr><td id='foot'>foot</td></tr></tfoot>"
            + "<tbody><tr><td id='body'>body</td></tr></tbody>"
            + "<thead><tr><td id='head'>head</td></tr></thead>"
            + "</table>"));

        var head = LayoutHarness.FindById(root, "head")!;
        var body = LayoutHarness.FindById(root, "body")!;
        var foot = LayoutHarness.FindById(root, "foot")!;

        Assert.IsLessThan(body.Location.Y, head.Location.Y);
        Assert.IsLessThan(foot.Location.Y, body.Location.Y);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#table-display")]
    [Spec("css-tables-3", "#table-structure")]
    public void SecondHeaderGroup_IsTreatedAsARowGroupInSourceOrder()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:table'>"
            + "<div style='display:table-header-group'><div style='display:table-row'><div id='head' style='display:table-cell'>head</div></div></div>"
            + "<div style='display:table-row-group'><div style='display:table-row'><div id='body' style='display:table-cell'>body</div></div></div>"
            + "<div style='display:table-header-group'><div style='display:table-row'><div id='second' style='display:table-cell;width:30px'>second</div></div></div>"
            + "</div>"));

        var head = LayoutHarness.FindById(root, "head")!;
        var body = LayoutHarness.FindById(root, "body")!;
        var second = LayoutHarness.FindById(root, "second")!;

        Assert.IsLessThan(body.Location.Y, head.Location.Y);
        Assert.IsGreaterThan(body.Location.Y, second.Location.Y);
        Assert.AreEqual(head.Location.X, second.Location.X, Delta);
    }

    [TestMethod]
    [Spec("css21", "tables.html#table-display")]
    [Spec("css-tables-3", "#table-structure")]
    public void ColumnElements_AreNotRendered()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='t' style='display:table'>"
            + "<div style='display:table-column'>column text</div>"
            + "<div style='display:table-row'><div style='display:table-cell'>cell</div></div>"
            + "</div>"));

        var words = LayoutHarness.Descendants(LayoutHarness.FindById(root, "t")!)
            .Where(b => b.Display != CssConstants.None)
            .SelectMany(b => b.Words)
            .Select(w => w.Text.Trim())
            .ToList();

        CollectionAssert.DoesNotContain(words, "column");
        CollectionAssert.Contains(words, "cell");
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#table-display")]
    [Spec("css-tables-3", "#table-structure")]
    public void InlineTable_SitsOnTheLineBetweenTheSurroundingText()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div><span id='before'>before</span><span id='t' style='display:inline-table'><span style='display:table-row'><span style='display:table-cell'>cell</span></span></span><span id='after'>after</span></div>"));

        var before = LayoutHarness.Descendants(LayoutHarness.FindById(root, "before")!).SelectMany(b => b.Words).First();
        var after = LayoutHarness.Descendants(LayoutHarness.FindById(root, "after")!).SelectMany(b => b.Words).First();
        var table = LayoutHarness.FindById(root, "t")!;

        Assert.AreEqual(before.Top, after.Top, Delta);
        Assert.IsGreaterThanOrEqualTo(before.Right - Delta, table.Location.X);
        Assert.IsLessThanOrEqualTo(after.Left + Delta, table.ActualRight);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#anonymous-boxes")]
    [Spec("css-tables-3", "#fixup-algorithm")]
    public void CellsWithoutARow_ShareOneAnonymousRow()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='t' style='display:table'><div id='a' style='display:table-cell'>a</div><div id='b' style='display:table-cell'>b</div></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreSame(a.ParentBox, b.ParentBox);
        Assert.AreEqual(CssConstants.TableRow, a.ParentBox!.Display);
        Assert.IsNull(a.ParentBox.HtmlTag);
        Assert.AreEqual(a.Location.Y, b.Location.Y, Delta);
        Assert.IsGreaterThan(a.Location.X, b.Location.X);
    }

    [TestMethod]
    [Spec("css21", "tables.html#anonymous-boxes")]
    [Spec("css-tables-3", "#fixup-algorithm")]
    public void NonCellContentOfARow_GetsAnAnonymousCell()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:table'><div id='r' style='display:table-row'><span id='s'>text</span></div></div>"));

        var s = LayoutHarness.FindById(root, "s")!;
        var row = LayoutHarness.FindById(root, "r")!;

        var cell = row.Boxes.Single();
        Assert.AreEqual(CssConstants.TableCell, cell.Display);
        Assert.IsNull(cell.HtmlTag);
        Assert.IsTrue(LayoutHarness.Descendants(cell).Contains(s));
    }

    [TestMethod]
    [Spec("css21", "tables.html#anonymous-boxes")]
    [Spec("css-tables-3", "#fixup-algorithm")]
    public void ChildrenOfAColumn_AreNotRendered()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:table'>"
            + "<div style='display:table-column-group'><span id='g'>group text</span><div style='display:table-column'><span id='c'>column text</span></div></div>"
            + "<div style='display:table-row'><div style='display:table-cell'>cell</div></div>"
            + "</div>"));

        Assert.IsTrue(IsHidden(LayoutHarness.FindById(root, "g")!));
        Assert.IsTrue(IsHidden(LayoutHarness.FindById(root, "c")!));
    }

    private static bool IsHidden(CssBox box)
    {
        for (var b = box; b != null; b = b.ParentBox)
        {
            if (b.Display == CssConstants.None)
                return true;
        }

        return false;
    }

    [TestMethod]
    [Spec("css21", "tables.html#anonymous-boxes")]
    [Spec("css-tables-3", "#fixup-algorithm")]
    public void WhiteSpaceBetweenRows_DoesNotCreateARow()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='t' style='display:table'>\n"
            + "  <div style='display:table-row'><div style='display:table-cell'>a</div></div>\n"
            + "  <div style='display:table-row'><div style='display:table-cell'>b</div></div>\n"
            + "</div>"));

        var table = LayoutHarness.FindById(root, "t")!;

        Assert.AreEqual(2, table.Boxes.Count(b => b.Display == CssConstants.TableRow));
    }

    [TestMethod]
    [Spec("css21", "tables.html#columns")]
    [Spec("css-tables-3", "#computing-column-measures")]
    public void ColumnWidth_IsAMinimumForTheColumn()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><col style='width:120px'><col>"
            + "<tr><td id='a' style='padding:0'>a</td><td id='b' style='padding:0'>b</td></tr></table>"));

        var a = LayoutHarness.FindById(root, "a")!;

        Assert.AreEqual(120, a.Size.Width, 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#columns")]
    [Spec("css-tables-3", "#drawing-cell-backgrounds")]
    public void ColumnBackground_IsPaintedBehindItsCells()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t' style='border-spacing:0'><col style='background-color:rgb(0,0,255)'><col>"
            + "<tr><td style='width:40px;height:20px'></td><td style='width:40px;height:20px'></td></tr></table>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!);

        Assert.IsTrue(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColor.FromArgb(0, 0, 255)));
    }

    [TestMethod]
    [Spec("css21", "tables.html#model")]
    [Spec("css-tables-3", "#global-style-overrides")]
    public void Margin_IsAppliedOutsideTheTable()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='before' style='height:10px'></div><table id='t' style='margin:20px 0 0 30px'><tr><td>a</td></tr></table>"));

        var t = LayoutHarness.FindById(root, "t")!;

        Assert.AreEqual(30, t.Location.X, Delta);
        Assert.AreEqual(30, t.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#model")]
    public void InheritedTextAlign_DoesNotMoveTheTable()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:400px;text-align:center'><table id='t'><tr><td>a</td></tr></table></div>"));

        Assert.AreEqual(0, LayoutHarness.FindById(root, "t")!.Location.X, Delta);
    }

    [TestMethod]
    [Spec("css21", "tables.html#model")]
    public void PercentageWidth_IsRelativeToTheContainingBlock()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:400px'><table id='t' style='width:50%'><tr><td>a</td></tr></table></div>"));

        var t = LayoutHarness.FindById(root, "t")!;

        Assert.AreEqual(200, t.ActualRight - t.Location.X, 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#model")]
    [Spec("css-tables-3", "#bounding-box-assignment")]
    public void Caption_IsRenderedAboveTheTable()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table><caption id='c'>caption</caption><tr><td id='a'>a</td></tr></table><div id='next'>next</div>"));

        var c = LayoutHarness.FindById(root, "c")!;
        var a = LayoutHarness.FindById(root, "a")!;
        var next = LayoutHarness.FindById(root, "next")!;

        Assert.IsGreaterThan(c.Location.Y, c.ActualBottom);
        Assert.IsLessThanOrEqualTo(a.Location.Y + Delta, c.ActualBottom);
        Assert.IsLessThanOrEqualTo(next.Location.Y + Delta, c.ActualBottom);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#caption-position")]
    [Spec("css-tables-3", "#caption-side-property")]
    public void CaptionSideBottom_PutsTheCaptionBelowTheTable()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table><caption id='c' style='caption-side:bottom'>caption</caption><tr><td id='a'>a</td></tr></table>"));

        var c = LayoutHarness.FindById(root, "c")!;
        var a = LayoutHarness.FindById(root, "a")!;

        Assert.IsGreaterThanOrEqualTo(a.ActualBottom - Delta, c.Location.Y);
    }

    [TestMethod]
    [Spec("css21", "tables.html#table-layout")]
    [Spec("css-tables-3", "#dimensioning-the-row-column-grid--step1")]
    public void RowspanCell_PushesTheNextRowsCellIntoTheFollowingColumn()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'>"
            + "<tr><td id='span' rowspan='2' style='width:50px'>x</td><td id='b' style='width:50px'>b</td></tr>"
            + "<tr><td id='c'>c</td></tr></table>"));

        var span = LayoutHarness.FindById(root, "span")!;
        var b = LayoutHarness.FindById(root, "b")!;
        var c = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(b.Location.X, c.Location.X, Delta);
        Assert.AreEqual(c.ActualBottom, span.ActualBottom, Delta);
    }

    [TestMethod]
    [Spec("css21", "tables.html#table-layout")]
    [Spec("css-tables-3", "#dimensioning-the-row-column-grid--step1")]
    [Spec("css-tables-3", "#bounding-box-assignment")]
    public void ColspanCell_CoversTheColumnsItSpans()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'>"
            + "<tr><td id='wide' colspan='2'>wide</td></tr>"
            + "<tr><td id='a' style='width:50px'>a</td><td id='b' style='width:70px'>b</td></tr></table>"));

        var wide = LayoutHarness.FindById(root, "wide")!;
        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(a.Location.X, wide.Location.X, Delta);
        Assert.AreEqual(b.ActualRight, wide.ActualRight, Delta);
    }

    [TestMethod]
    [Spec("css21", "tables.html#table-layout")]
    [Spec("css-tables-3", "#global-style-overrides")]
    public void CellMargins_AreIgnored()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr><td id='a' style='width:50px'>a</td><td id='b' style='margin-left:30px'>b</td></tr></table>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(a.ActualRight, b.Location.X, Delta);
        Assert.AreEqual(a.ActualRight, LayoutHarness.Descendants(b).SelectMany(x => x.Words).First().Left - b.ActualPaddingLeft - b.ActualBorderLeftWidth, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#table-layers")]
    [Spec("css-tables-3", "#drawing-cell-backgrounds")]
    public void RowBackground_IsPaintedBehindItsCells()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t' style='border-spacing:0'><tr style='background-color:rgb(255,0,0)'><td id='a' style='width:40px;height:20px'></td></tr></table>"));

        var a = PaintHarness.FindById(root, "a")!;
        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!);

        var red = g.Log.OfType<RecordingGraphics.DrawRectCall>().Where(r => r.Color == RColor.FromArgb(255, 0, 0)).ToList();
        Assert.IsTrue(red.Any(r => Math.Abs(r.X - a.Location.X) < 1 && Math.Abs(r.Y - a.Location.Y) < 1 && r.Width >= a.Size.Width - 1));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#table-layers")]
    [Spec("css-tables-3", "#drawing-cell-backgrounds")]
    public void RowGroupBackground_IsPaintedBehindItsCells()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t' style='border-spacing:0'><tbody style='background-color:rgb(255,0,0)'><tr><td style='width:40px;height:20px'></td></tr></tbody></table>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!);

        Assert.IsTrue(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColor.FromArgb(255, 0, 0) && r.Width >= 39 && r.Height >= 19));
    }

    [TestMethod]
    [Spec("css21", "tables.html#table-layers")]
    [Spec("css-tables-3", "#drawing-table-backgrounds-and-borders")]
    [Spec("css-tables-3", "#drawing-cell-backgrounds")]
    public void CellBackground_IsPaintedOverTheTableBackground()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t' style='background-color:rgb(255,255,0);border-spacing:4px'><tr><td style='background-color:rgb(0,128,0);width:40px;height:20px'></td></tr></table>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!);
        var rects = g.Log.OfType<RecordingGraphics.DrawRectCall>().ToList();

        var table = rects.FindIndex(r => r.Color == RColor.FromArgb(255, 255, 0));
        var cell = rects.FindIndex(r => r.Color == RColor.FromArgb(0, 128, 0));
        Assert.IsGreaterThanOrEqualTo(0, table);
        Assert.IsGreaterThan(table, cell);
    }
}
