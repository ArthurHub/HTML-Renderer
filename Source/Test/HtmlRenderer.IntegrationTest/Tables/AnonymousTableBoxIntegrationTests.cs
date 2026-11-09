using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace HtmlRenderer.IntegrationTest.Tables;

[DoNotParallelize]
[TestClass]
public sealed class AnonymousTableBoxIntegrationTests
{
    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#anonymous-boxes")]
    public void MixedNonRowChildren_AllGroupIntoOneAnonymousRow_WithOnlyNonCellsWrapped()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<ul style='display:table'>"
            + "<li class='first' style='display:table-cell'></li>"
            + "<li class='second' style='display:table'></li>"
            + "<li class='third' style='display:table-cell'></li>"
            + "<li class='fourth'></li>"
            + "</ul>"));

        var ul = FindByTag(root, "ul")!;
        Assert.AreEqual(1, ul.Boxes.Count);

        var row = ul.Boxes[0];
        Assert.AreEqual(CssConstants.TableRow, row.Display);
        Assert.AreEqual(4, row.Boxes.Count);

        var first = FindByClass(root, "first")!;
        var second = FindByClass(root, "second")!;
        var third = FindByClass(root, "third")!;
        var fourth = FindByClass(root, "fourth")!;

        // Already-cell children stay direct children of the row (not re-wrapped).
        Assert.AreSame(row, first.ParentBox);
        Assert.AreSame(row, third.ParentBox);

        // Non-cell children each get their own anonymous table-cell wrapper as a direct row child.
        Assert.AreNotSame(row, second.ParentBox);
        Assert.AreEqual(CssConstants.TableCell, second.ParentBox!.Display);
        Assert.AreSame(row, second.ParentBox!.ParentBox);
        Assert.AreEqual(1, second.ParentBox!.Boxes.Count);

        Assert.AreNotSame(row, fourth.ParentBox);
        Assert.AreEqual(CssConstants.TableCell, fourth.ParentBox!.Display);
        Assert.AreSame(row, fourth.ParentBox!.ParentBox);
        Assert.AreEqual(1, fourth.ParentBox!.Boxes.Count);

        // The nested table keeps its own display inside the anonymous cell
        Assert.AreEqual(CssConstants.Table, second.Display);

        // Row order must match document order.
        CollectionAssert.AreEqual(
            new[] { first, second.ParentBox, third, fourth.ParentBox },
            row.Boxes);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#anonymous-boxes")]
    public void RowGroupWithNonRowChildren_GroupsConsecutiveSiblingsIntoOneAnonymousRow()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:table'>"
            + "<div class='rowgroup' style='display:table-row-group'>"
            + "<span class='a'></span>"
            + "<span class='b'></span>"
            + "<div class='realrow' style='display:table-row'></div>"
            + "</div>"
            + "</div>"));

        var rowGroup = FindByClass(root, "rowgroup")!;
        Assert.AreEqual(2, rowGroup.Boxes.Count);

        // The anonymous row's children aren't cells either, so they share one anonymous cell, with the
        // usual anonymous block box around the inline content in between
        var anonRow = rowGroup.Boxes[0];
        Assert.AreEqual(CssConstants.TableRow, anonRow.Display);
        Assert.AreEqual(1, anonRow.Boxes.Count);

        var anonCell = anonRow.Boxes[0];
        Assert.AreEqual(CssConstants.TableCell, anonCell.Display);

        var a = FindByClass(root, "a")!;
        var b = FindByClass(root, "b")!;
        var realRow = FindByClass(root, "realrow")!;

        Assert.AreSame(anonCell, a.ParentBox!.ParentBox);
        Assert.AreSame(anonCell, b.ParentBox!.ParentBox);

        // The anonymous row takes the place of "a", ahead of the real row
        Assert.AreSame(rowGroup, realRow.ParentBox);
        CollectionAssert.AreEqual(new[] { anonRow, realRow }, rowGroup.Boxes);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#anonymous-boxes")]
    public void MisparentedTableCell_WrappedInAnonymousRowThenTable_AtItsOriginalDocumentPosition()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div class='before'></div>"
            + "<span class='cell1' style='display:table-cell'></span>"
            + "<span class='cell2' style='display:table-cell'></span>"
            + "<div class='after'></div>"));

        var body = FindByTag(root, "body")!;
        var before = FindByClass(root, "before")!;
        var after = FindByClass(root, "after")!;
        var cell1 = FindByClass(root, "cell1")!;
        var cell2 = FindByClass(root, "cell2")!;

        // 3.1: both cells share one anonymous table-row.
        var anonRow = cell1.ParentBox!;
        Assert.AreEqual(CssConstants.TableRow, anonRow.Display);
        Assert.AreSame(anonRow, cell2.ParentBox);

        // 3.2: the anonymous row is wrapped in an anonymous table
        var anonTable = anonRow.ParentBox!;
        Assert.IsNull(anonTable.HtmlTag);
        Assert.AreEqual(CssConstants.Table, anonTable.Display);

        // The anonymous table takes the place of the cells, between "before" and "after"
        CollectionAssert.AreEqual(new[] { before, anonTable, after }, body.Boxes);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static CssBox? FindByTag(CssBox box, string tag)
    {
        if (box.HtmlTag?.Name.Equals(tag, System.StringComparison.OrdinalIgnoreCase) == true)
            return box;
        foreach (var child in box.Boxes)
        {
            var found = FindByTag(child, tag);
            if (found != null) return found;
        }
        return null;
    }

    private static CssBox? FindByClass(CssBox box, string className)
    {
        var val = box.HtmlTag?.TryGetAttribute("class", "");
        if (!string.IsNullOrEmpty(val) && val.Split(' ').Contains(className))
            return box;
        foreach (var child in box.Boxes)
        {
            var found = FindByClass(child, className);
            if (found != null) return found;
        }
        return null;
    }
}
