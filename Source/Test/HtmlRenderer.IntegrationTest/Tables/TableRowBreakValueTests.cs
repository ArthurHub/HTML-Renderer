using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// Forced breaks between two table rows, css-break-3 §3.1.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class TableRowBreakValueTests
{
    private const double PageHeight = 300;
    private const double Margin = 20;

    private static string Table(string rowCss, string extraRowAttrs = "") => LayoutHarness.Wrap(
        "<table id='t' style='width:100%'>"
        + "<tbody>"
        + "<tr id='r1'><td>Row one</td></tr>"
        + $"<tr id='r2' {extraRowAttrs}><td>Row two</td></tr>"
        + $"<tr id='r3' style='{rowCss}'><td>Row three</td></tr>"
        + "</tbody></table>");

    [TestMethod]
    public void WithoutABreakValue_EveryRowStaysOnTheFirstPage()
    {
        var (root, container) = LayoutHarness.Layout(Table(""), 400, PageHeight, margin: Margin);

        foreach (var id in new[] { "r1", "r2", "r3" })
        {
            var row = LayoutHarness.FindById(root, id)!;
            Assert.AreEqual(0, container.PageIndexOf(row.Boxes[0].Location.Y));
        }
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#breaking-rules")]
    public void BreakBeforeOnARow_StartsItOnTheNextPage()
    {
        var (root, container) = LayoutHarness.Layout(Table("break-before:page"), 400, PageHeight, margin: Margin);

        var r2 = LayoutHarness.FindById(root, "r2")!;
        var r3 = LayoutHarness.FindById(root, "r3")!;
        Assert.AreEqual(0, container.PageIndexOf(r2.Boxes[0].Location.Y));
        Assert.AreEqual(1, container.PageIndexOf(r3.Boxes[0].Location.Y));
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#breaking-rules")]
    public void BreakAfterOnTheRowBefore_StartsTheNextOneOnTheNextPage()
    {
        var (root, container) = LayoutHarness.Layout(Table("", "style='break-after:page'"), 400, PageHeight, margin: Margin);

        var r2 = LayoutHarness.FindById(root, "r2")!;
        var r3 = LayoutHarness.FindById(root, "r3")!;
        Assert.AreEqual(0, container.PageIndexOf(r2.Boxes[0].Location.Y));
        Assert.AreEqual(1, container.PageIndexOf(r3.Boxes[0].Location.Y));
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#breaking-rules")]
    public void BreakBeforeOnARowGroup_IsSeenAtItsFirstRow()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='width:100%'>"
            + "<tbody><tr id='r1'><td>Row one</td></tr></tbody>"
            + "<tbody style='break-before:page'><tr id='r2'><td>Row two</td></tr></tbody>"
            + "</table>"), 400, PageHeight, margin: Margin);

        var r1 = LayoutHarness.FindById(root, "r1")!;
        var r2 = LayoutHarness.FindById(root, "r2")!;
        Assert.AreEqual(0, container.PageIndexOf(r1.Boxes[0].Location.Y));
        Assert.AreEqual(1, container.PageIndexOf(r2.Boxes[0].Location.Y));
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#breaking-rules")]
    public void AForcedRowBreak_StillRepeatsTheHeaderOnTheNewPage()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='width:100%'>"
            + "<thead style='break-inside:avoid'><tr><th>Head</th></tr></thead>"
            + "<tbody><tr id='r1'><td>Row one</td></tr>"
            + "<tr id='r2' style='break-before:page'><td>Row two</td></tr></tbody>"
            + "</table>"), 400, PageHeight, margin: Margin);

        var r2 = LayoutHarness.FindById(root, "r2")!;
        Assert.AreEqual(1, container.PageIndexOf(r2.Boxes[0].Location.Y));
    }
}
