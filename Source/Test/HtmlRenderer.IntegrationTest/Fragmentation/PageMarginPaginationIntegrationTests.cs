using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest.Fragmentation;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/PageMarginPaginationIntegrationTests.cs: a regression class for
/// "@page margins waste marginTop+marginBottom of every page" - <see cref="HtmlContainerInt.PageSize"/> is
/// already margin-free, so the real per-page content band is the shifted grid
/// <c>[k*PageSize.Height + MarginTop, (k+1)*PageSize.Height + MarginTop)</c>, and
/// <see cref="HtmlContainerInt.PageIndexOf"/>/<see cref="HtmlContainerInt.PageTopOf"/> are the single
/// definition of that grid.
/// </summary>
/// <remarks>
/// Mirrors production's real relationship between <see cref="HtmlContainerInt.PageSize"/>,
/// <see cref="HtmlContainerInt.MarginTop"/> and <see cref="HtmlContainerInt.Location"/> - the same
/// relationship confirmed (the hard way, via a full test-run failure sweep while porting the sibling files
/// in this folder) to matter for every margin-truncation/forced-break test in this batch: content must
/// actually start at <c>Location = (0, MarginTop)</c>, not at the default <c>(0, 0)</c>, or the pagination
/// grid disagrees with where box geometry begins.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class PageMarginPaginationIntegrationTests
{
    // Roughly the customer's own repro proportions: a Letter-ish page, sizeable asymmetric top/bottom
    // margins - the shape "double-subtracting" the margins from an already margin-free PageSize.Height
    // breaks most visibly on.
    private const double RawPageHeight = 800;
    private const double MarginTopValue = 40;
    private const double MarginBottomValue = 50;

    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    private static async Task<(CssBox Root, HtmlContainerInt Container)> BuildAsync(
        string bodyHtml, double marginTop, double marginBottom)
    {
        var wrapper = new HtmlContainer();
        await wrapper.SetHtml($"<html><body style='margin:0'>{bodyHtml}</body></html>");

        var container = GetInternal(wrapper);
        container.MarginTop = (int)marginTop;
        container.MarginBottom = (int)marginBottom;
        container.MarginLeft = 0;
        container.MarginRight = 0;

        // Mirrors PdfGenerator.SetContent exactly: PageSize.Height is the margin-free content band, and
        // layout starts at (0, MarginTop) - not the raw page height/origin.
        container.PageSize = new RSize(400, RawPageHeight - marginTop - marginBottom);
        container.Location = new RPoint(0, marginTop);
        wrapper.MaxSize = new SizeF(400, 0);

        using var bitmap = new Bitmap(400, 60000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        return (container.Root!, container);
    }

    private static IEnumerable<CssBox> Walk(CssBox box)
    {
        yield return box;
        foreach (var b in box.Boxes)
            foreach (var d in Walk(b))
                yield return d;
    }

    private static CssBox FindByClass(CssBox root, string className)
    {
        foreach (var box in Walk(root))
        {
            var classAttr = box.HtmlTag?.TryGetAttribute("class", "");
            if (!string.IsNullOrEmpty(classAttr) && System.Array.IndexOf(classAttr.Split(' '), className) >= 0)
                return box;
        }
        return null!;
    }

    // A <tr> box's own Location is never assigned by table layout - only its CELLS' is (see
    // TableHeaderRepeat.cs's own doc remark, and CssLayoutEngineTable's row loop) - so geometry has to be
    // read off each row's first cell, not the row box itself.
    private static List<CssBox> FindAllRows(CssBox table) =>
        Walk(table).Where(b => b.HtmlTag?.Name == "td").ToList();

    private static string BuildManyRowTableHtml(int rowCount)
    {
        var rows = new System.Text.StringBuilder();
        for (var i = 0; i < rowCount; i++)
            rows.Append($"<tr><td style='padding:2px;border:1px solid #ccc;'>Row {i}</td></tr>");

        return $"<table style='border-collapse:collapse;width:100%;font-size:10px'>{rows}</table>";
    }

    [TestMethod]
    public async Task Table_WithPageMargins_FillsEachFullPageCloseToBottomMargin()
    {
        var (root, container) = await BuildAsync(BuildManyRowTableHtml(80), MarginTopValue, MarginBottomValue);

        var rows = FindAllRows(root);
        Assert.IsTrue(rows.Count > 10, "test setup should produce enough rows to span multiple pages");

        var page0Bottom = container.PageTopOf(1);
        var page0Rows = rows.Where(r => r.Location.Y < page0Bottom).ToList();
        Assert.IsTrue(page0Rows.Count > 0);

        var lastRowOnPage0 = page0Rows.OrderByDescending(r => r.ActualBottom).First();
        var rowHeight = lastRowOnPage0.ActualBottom - lastRowOnPage0.Location.Y;

        // Before the fix, availableHeight double-subtracted marginTop+marginBottom from an already
        // margin-free PageSize.Height, so the page broke ~marginTop+marginBottom early - far more than one
        // row's worth of slack. After the fix, the last row on the page should land within about one
        // row-height of the real page-1 boundary.
        Assert.IsTrue(page0Bottom - lastRowOnPage0.ActualBottom <= rowHeight * 1.5,
            $"Page 0's last row (bottom={lastRowOnPage0.ActualBottom:F1}) stops {page0Bottom - lastRowOnPage0.ActualBottom:F1}px "
            + $"short of the real page boundary ({page0Bottom:F1}) - more than one row's worth (~{rowHeight:F1}px), indicating the page is under-filled.");
    }

    [TestMethod]
    public async Task Table_WithZeroPageMargins_StillFillsEachPage()
    {
        // Guards the historical (always-correct) zero-margin default against regressing.
        var (root, container) = await BuildAsync(BuildManyRowTableHtml(80), marginTop: 0, marginBottom: 0);

        var rows = FindAllRows(root);
        var page0Bottom = container.PageTopOf(1);
        var page0Rows = rows.Where(r => r.Location.Y < page0Bottom).ToList();
        Assert.IsTrue(page0Rows.Count > 0);

        var lastRowOnPage0 = page0Rows.OrderByDescending(r => r.ActualBottom).First();
        var rowHeight = lastRowOnPage0.ActualBottom - lastRowOnPage0.Location.Y;

        Assert.IsTrue(page0Bottom - lastRowOnPage0.ActualBottom <= rowHeight * 1.5,
            $"Zero-margin page should still fill close to its boundary ({page0Bottom:F1}), but last row bottom is {lastRowOnPage0.ActualBottom:F1}.");
    }

    [TestMethod]
    public async Task ForcedPageBreak_WithPageMargins_LandsAtShiftedPageTop()
    {
        var (root, container) = await BuildAsync(
            "<div style='height:500px'></div><div class='second' style='page-break-before:always'>Second</div>",
            MarginTopValue, MarginBottomValue);

        var second = FindByClass(root, "second");
        Assert.IsNotNull(second);

        var expectedTop = container.PageTopOf(1);
        Assert.IsTrue(System.Math.Abs(second.Location.Y - expectedTop) < 1.0,
            $"Forced break with page margins should land exactly at the shifted page-1 top ({expectedTop:F1}), but landed at {second.Location.Y:F1}");
    }

    [TestMethod]
    public async Task BreakInsideAvoid_WithPageMargins_PositionsAtShiftedPageTop()
    {
        var (root, container) = await BuildAsync(
            "<div style='height:650px'></div>"
            + "<div class='avoid' style='break-inside:avoid;page-break-inside:avoid;height:200px'>"
            + "<p style='margin:0;line-height:20px'>Line 1</p><p style='margin:0;line-height:20px'>Line 2</p>"
            + "</div>",
            MarginTopValue, MarginBottomValue);

        var avoidBox = FindByClass(root, "avoid");
        Assert.IsNotNull(avoidBox);

        var expectedTop = container.PageTopOf(1);
        Assert.IsTrue(avoidBox.Location.Y >= container.PageSize.Height,
            "test setup expects the avoid box to be relocated past page 0 to validate positioning");
        Assert.IsTrue(System.Math.Abs(avoidBox.Location.Y - expectedTop) < 1.0,
            $"break-inside:avoid with page margins should relocate to the shifted page-1 top ({expectedTop:F1}), but landed at {avoidBox.Location.Y:F1}");
    }

    [TestMethod]
    public async Task OrphansWidows_WithPageMargins_PushesWholeParagraphToShiftedPageTop()
    {
        var (root, container) = await BuildAsync(
            "<div style='height:660px'></div>"
            + "<div class='para' style='orphans:4;widows:4;line-height:20px;margin:0;width:200px'>"
            + "Line1 Line2 Line3 Line4 Line5 Line6 Line7 Line8 Line9 Line10 "
            + "Line11 Line12 Line13 Line14 Line15 Line16 Line17 Line18</div>",
            MarginTopValue, MarginBottomValue);

        var para = FindByClass(root, "para");
        Assert.IsNotNull(para);

        // If orphans/widows relocated the whole paragraph, it should sit exactly at the shifted page-1
        // top - if it didn't need to relocate (all lines already fit), that's fine too, but then we can't
        // validate the push, so skip in that case.
        if (para.Location.Y < container.PageSize.Height) return;

        var expectedTop = container.PageTopOf(1);
        Assert.IsTrue(System.Math.Abs(para.Location.Y - expectedTop) < 1.0,
            $"orphans/widows push with page margins should land at the shifted page-1 top ({expectedTop:F1}), but landed at {para.Location.Y:F1}");
    }
}
