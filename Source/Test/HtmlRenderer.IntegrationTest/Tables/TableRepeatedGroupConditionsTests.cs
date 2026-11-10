using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// When a <c>&lt;thead&gt;</c> is repeated on the following pages (CSS Tables 3 §6.2).
/// </summary>
/// <remarks>
/// The UA's <c>thead { break-inside: avoid }</c> is in <c>@media print</c>, which the WinForms adapter
/// doesn't match, so the tests declare it themselves.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class TableRepeatedGroupConditionsTests
{
    private const double PageHeight = 300;
    private const double Margin = 20;

    private static CssBox TableOf(CssBox root) => LayoutHarness.Descendants(root).First(b => b.Display == "table");

    private static string ManyRowsTable(string theadStyle) => LayoutHarness.Wrap(
        "<table style='width:100%;border-collapse:collapse'>"
        + $"<thead style='{theadStyle}'><tr><th>Head</th></tr></thead><tbody>"
        + string.Concat(Enumerable.Range(1, 20).Select(i => $"<tr><td><div style='height:40px'>row {i}</div></td></tr>"))
        + "</tbody></table>");

    // css-break-3 §3.2: break-inside is not an inherited property, and the whole approach depends on it
    // staying that way - an inherited "avoid" on every header cell would declare its own content
    // unbreakable, not just gate the table engine's repeat decision.
    [TestMethod]
    public void BreakInsideOnAThead_DoesNotReachItsRowsOrCells()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='width:100%'><thead style='break-inside:avoid'><tr><th>H</th></tr></thead>"
            + "<tbody><tr><td>body</td></tr></tbody></table>"));

        var thead = LayoutHarness.Descendants(root).First(b => b.HtmlTag?.Name == "thead");

        Assert.AreEqual(CssConstants.Avoid, thead.BreakInside);
        Assert.IsTrue(LayoutHarness.Descendants(thead).Skip(1).All(b => b.BreakInside == CssConstants.Auto),
            "break-inside must not have cascaded from the thead onto any of its own rows or cells");
    }

    [TestMethod]
    [Spec("css-tables-3", "#repeated-headers")]
    public void AGroupOptedOutOfAvoidBreakInside_IsNeverRepeated()
    {
        var (root, container) = LayoutHarness.Layout(ManyRowsTable("break-inside:auto"), 400, PageHeight, margin: Margin);

        Assert.IsTrue(container.FragmentTree!.Fragmentainers.Count > 1, "fixture must paginate");
        Assert.IsNull(TableOf(root).RepeatedHeaderRows);
    }

    [TestMethod]
    [Spec("css-tables-3", "#repeated-headers")]
    public void AGroupWithAnExplicitAvoidBreakInside_RepeatsOnEveryPage()
    {
        var (root, container) = LayoutHarness.Layout(ManyRowsTable("break-inside:avoid"), 400, PageHeight, margin: Margin);

        var pages = container.FragmentTree!.Fragmentainers.Count;
        Assert.IsTrue(pages > 1, "fixture must paginate");

        var table = TableOf(root);
        Assert.IsNotNull(table.RepeatedHeaderRows);
        Assert.AreEqual(pages - 1, table.RepeatedHeaderRows!.Count);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#repeated-headers")]
    public void AGroupTallerThanAQuarterOfThePage_IsNotRepeated()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse'>"
            + "<thead style='break-inside:avoid'><tr><th><div style='height:200px'>Head</div></th></tr></thead>"
            + "<tbody>"
            + string.Concat(Enumerable.Range(1, 20).Select(i => $"<tr><td><div style='height:40px'>row {i}</div></td></tr>"))
            + "</tbody></table>");

        var (root, container) = LayoutHarness.Layout(html, 400, PageHeight, margin: Margin);

        Assert.IsTrue(container.FragmentTree!.Fragmentainers.Count > 2, "fixture must paginate more than once");
        Assert.IsNull(TableOf(root).RepeatedHeaderRows);
    }

    // Body rows on a continuation page start below the repeated header.
    [TestMethod]
    [Spec("css-tables-3", "#repeated-headers")]
    public void ARepeatingHeadersRoom_IsReservedOnEveryContinuationBand()
    {
        var (withHeader, containerWithHeader) = LayoutHarness.Layout(
            ManyRowsTable("break-inside:avoid"), 400, PageHeight, margin: Margin);
        var (withoutHeader, containerWithoutHeader) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse'><tbody>"
            + string.Concat(Enumerable.Range(1, 20).Select(i => $"<tr><td><div style='height:40px'>row {i}</div></td></tr>"))
            + "</tbody></table>"), 400, PageHeight, margin: Margin);

        var firstRowOnPage1WithHeader = LayoutHarness.Descendants(TableOf(withHeader))
            .Where(b => b.Display == "table-row")
            .Select(row => row.Boxes.Count > 0 ? row.Boxes[0].Location.Y : row.Location.Y)
            .First(y => containerWithHeader.PageIndexOf(y) == 1);

        var firstRowOnPage1WithoutHeader = LayoutHarness.Descendants(TableOf(withoutHeader))
            .Where(b => b.Display == "table-row")
            .Select(row => row.Boxes.Count > 0 ? row.Boxes[0].Location.Y : row.Location.Y)
            .First(y => containerWithoutHeader.PageIndexOf(y) == 1);

        Assert.AreEqual(containerWithoutHeader.PageTopOf(1), firstRowOnPage1WithoutHeader, 0.5,
            "sanity check: with no header at all, a row starting page 1 sits flush at its content top");

        Assert.IsTrue(firstRowOnPage1WithHeader > firstRowOnPage1WithoutHeader + 5,
            $"a row starting page 1 alongside a repeating header ({firstRowOnPage1WithHeader:F1}) should sit " +
            $"noticeably below where the same row would without one ({firstRowOnPage1WithoutHeader:F1})");
    }
}
