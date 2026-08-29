using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/TableRepeatedGroupConditionsTests.cs: css-tables-3 §6.2's
/// conditions on whether to repeat a <c>&lt;thead&gt;</c>/<c>&lt;tfoot&gt;</c> at all.
/// </summary>
/// <remarks>
/// A drastic reduction from PeachPDF's 16 (thead half only, per the port plan's tfoot-drop rule), not a
/// rename - confirmed by direct source read:
/// <list type="bullet">
/// <item><see cref="TheArtOfDev.HtmlRenderer.Core.Dom.CssLayoutEngineTable"/>'s <c>repeatsHeader</c> gate
/// (<c>Core/Dom/CssLayoutEngineTable.cs</c> ~647-648) checks only
/// <c>BreakValues.AvoidsBreak(_headerBox.BreakInside)</c> and <c>HasRealPageGrid</c> - §6.2's SECOND
/// condition (repeat only if the group's own height is under a quarter of the page) is not implemented at
/// all, confirmed by reading the gate in full: there is no height comparison anywhere in it. Every test
/// built on that cap (<c>AGroupExactlyAQuarterOfThePage_DoesNotRepeat</c>,
/// <c>ATallHeaderThatDoesNotRepeat_LeavesTheLaterBandsToTheRows</c> and their footer siblings) is dropped
/// outright rather than force-fit; <see cref="AGroupTallerThanAQuarterOfThePage_StillRepeats_UnlikeCssTables3"/>
/// pins the real, inverted behavior instead.</item>
/// <item>The UA stylesheet's <c>thead, tfoot { break-inside: avoid }</c> default
/// (<c>Core/CssDefaults.cs</c>) lives under <c>@media print</c>, which only <c>PdfSharpAdapter</c> ever
/// matches (confirmed: <c>RAdapter.DefaultMediaType</c>'s base default is not <c>"print"</c>, and this
/// file's harness lays out over <c>WinFormsAdapter</c>, whose reported type is <c>"screen"</c> - the same
/// established convention documented in <c>StageD4RepeatedHeaderTest.cs</c>). So
/// <c>TheUaStylesheet_GivesATheadAndTfootAvoidBreakInside</c> and
/// <c>TheUaStylesheet_StillGivesHeadingsAnAvoidingBreakAfter</c> are dropped, not ported Ignored - what
/// they'd actually be testing (whether the print stylesheet text itself contains the rule) is a
/// stylesheet-parsing concern for a unit test, not a layout-behavior one, and is already directly
/// confirmed by reading <c>Core/CssDefaults.cs</c>, quoted above; the OBSERVABLE effect of that default (a
/// plain <c>&lt;thead&gt;</c> repeating) is already covered by every other test in this file, each of
/// which declares <c>break-inside:avoid</c> explicitly rather than relying on the print-scoped default, per
/// this repo's own established convention for WinForms-harness tests.</item>
/// <item><c>&lt;tfoot&gt;</c> repeat has no implementation at all (only <c>&lt;thead&gt;</c> - confirmed:
/// <c>TableHeaderRepeat</c> is thead-only, no footer equivalent), so every <c>ADeclinedFooter_*</c>/
/// <c>AFooterCarriedOntoTheNextPage_*</c>/<c>ARepeatingFooter_*</c>/<c>ThePageACarriedFooterOpens_*</c> test
/// and the footer arm of every <c>[Theory]</c> is dropped, per the port plan's tfoot-drop rule.</item>
/// <item><c>DetachedRowGroup.Repeats</c>/<c>TableSetup.Header</c>/<c>Footer</c> have no counterpart -
/// <see cref="CssBox.RepeatedHeaderRows"/> (null vs non-null, and its count) is this fork's own equivalent
/// signal, used throughout below instead.</item>
/// </list>
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

    // A group whose author opts back out of the UA default is laid out once, in flow, and never repeated -
    // css-tables-3 6.2's first condition, and the opt-out the UA default exists to be taken away from.
    [TestMethod]
    public void AGroupOptedOutOfAvoidBreakInside_IsNeverRepeated()
    {
        var (root, container) = LayoutHarness.Layout(ManyRowsTable("break-inside:auto"), 400, PageHeight, margin: Margin);

        Assert.IsTrue(container.FragmentTree!.Fragmentainers.Count > 1, "fixture must paginate");
        Assert.IsNull(TableOf(root).RepeatedHeaderRows);
    }

    // With the UA default's effect declared explicitly instead, the same shape of fixture repeats on every
    // page the table covers - the behavior the opt-out test above is a negative control for.
    [TestMethod]
    public void AGroupWithAnExplicitAvoidBreakInside_RepeatsOnEveryPage()
    {
        var (root, container) = LayoutHarness.Layout(ManyRowsTable("break-inside:avoid"), 400, PageHeight, margin: Margin);

        var pages = container.FragmentTree!.Fragmentainers.Count;
        Assert.IsTrue(pages > 1, "fixture must paginate");

        var table = TableOf(root);
        Assert.IsNotNull(table.RepeatedHeaderRows);
        Assert.AreEqual(pages - 1, table.RepeatedHeaderRows!.Count);
    }

    // Confirmed gap (see class remarks): css-tables-3 6.2's "under a quarter of the page" cap does not
    // exist in this fork - a header far taller than a quarter of the page still repeats on every page,
    // unlike the spec (and unlike PeachPDF, which declines to repeat it).
    [TestMethod]
    public void AGroupTallerThanAQuarterOfThePage_StillRepeats_UnlikeCssTables3()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse'>"
            + "<thead style='break-inside:avoid'><tr><th><div style='height:200px'>Head</div></th></tr></thead>"
            + "<tbody>"
            + string.Concat(Enumerable.Range(1, 20).Select(i => $"<tr><td><div style='height:40px'>row {i}</div></td></tr>"))
            + "</tbody></table>");

        // The header alone (200px) is already well over a quarter of PageHeight (75px) - comfortably past
        // the spec's cap, if this fork implemented it.
        var (root, container) = LayoutHarness.Layout(html, 400, PageHeight, margin: Margin);

        Assert.IsTrue(container.FragmentTree!.Fragmentainers.Count > 2, "fixture must paginate more than once");

        var table = TableOf(root);
        Assert.IsNotNull(table.RepeatedHeaderRows,
            "unlike css-tables-3 6.2, this fork's repeatsHeader gate has no height cap - confirmed by reading it in full");
        Assert.IsTrue(table.RepeatedHeaderRows!.Count > 0);
    }

    // The room a repeating header needs IS genuinely reserved on every continuation band it appears on
    // (CssLayoutEngineTable.cs's own "reserving the room here... is what keeps that row from being drawn
    // underneath the repeated header instead of below it" remark) - body content on a continuation page
    // starts below the header's own height, not at the page's bare content top.
    [TestMethod]
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
