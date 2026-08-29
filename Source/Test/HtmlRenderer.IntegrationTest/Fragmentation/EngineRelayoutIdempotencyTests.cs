using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest.Fragmentation;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/EngineRelayoutIdempotencyTests.cs: whether laying the same
/// subtree out again reproduces the first result.
/// </summary>
/// <remarks>
/// Reframed per the port plan: PeachPDF's version is about general resumed-pass idempotency (re-running an
/// engine's measurement phases mid-resume). This port's relevant relayout triggers are more specific -
/// <c>BlockFragmentation.RelocateIfNeeded</c>/<c>EnforceKeepWithNext</c> relaying a box out fresh within the
/// same pass (<c>child.ResumeAt</c> + <c>child.PerformLayout</c>), and
/// <c>CssLayoutEngineTable</c>'s repeated-header rebuild, which its own call site resets
/// (<c>_tableBox.RepeatedHeaderRows = null</c>) and rebuilds "from scratch" via
/// <c>TableHeaderRepeat.CloneAndPosition</c> on every table layout, per that class's own doc comment.
/// Dropped entirely: PeachPDF's flex/grid/multicol Theories (4 of 7 methods) - none of those engines exist
/// in this port (<c>MonolithicContent.RunsAnEngineOfItsOwn</c>'s own doc comment narrows "engines that
/// paginate their own content" to table only). The remaining 3 (the plain-block-flow control, and the
/// table header-repeat family) are ported, adapted to call <see cref="HtmlContainer.PerformLayout"/>
/// directly, more than once, on the SAME already-built tree - the real repeated-layout shape this port
/// actually has (<c>HtmlContainerInt.PerformLayout</c>'s own unrestricted-width double layout, and a host
/// control's own resize-driven re-layout), rather than PeachPDF's resumed-pass re-entry.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class EngineRelayoutIdempotencyTests
{
    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    /// <summary>Builds once, then lays the same tree out <paramref name="passes"/> more times, snapshotting
    /// <paramref name="snapshot"/> after each.</summary>
    private static async Task<List<string>> LayoutRepeatedlyAsync(
        string bodyHtml, int passes, System.Func<CssBox, HtmlContainerInt, string> snapshot, double pageHeight = 1000)
    {
        var wrapper = new HtmlContainer();
        await wrapper.SetHtml($"<html><body style='margin:0'>{bodyHtml}</body></html>");

        var container = GetInternal(wrapper);
        container.PageSize = new RSize(300, pageHeight);
        container.MarginTop = 0;
        container.Location = new RPoint(0, 0);
        wrapper.MaxSize = new SizeF(300, 0);

        using var bitmap = new Bitmap(300, 60000);
        using var g = Graphics.FromImage(bitmap);

        var snapshots = new List<string>();
        for (var i = 0; i < passes; i++)
        {
            wrapper.PerformLayout(g);
            snapshots.Add(snapshot(container.Root!, container));
        }

        return snapshots;
    }

    private static IEnumerable<CssBox> Walk(CssBox box)
    {
        yield return box;
        foreach (var b in box.Boxes)
            foreach (var d in Walk(b))
                yield return d;
    }

    private static string GeometryOf(CssBox root, HtmlContainerInt container)
    {
        var parts = Walk(root)
            .Where(b => !string.IsNullOrEmpty(b.HtmlTag?.TryGetAttribute("id")))
            .Select(b => string.Format(
                CultureInfo.InvariantCulture, "{0}@({1:F3},{2:F3})-({3:F3},{4:F3})",
                b.HtmlTag!.TryGetAttribute("id"), b.Location.X, b.Location.Y, b.ActualRight, b.ActualBottom));

        return string.Join("|", parts)
            + string.Format(CultureInfo.InvariantCulture, "||size={0:F3}", container.ActualSize.Height);
    }

    private static string Items(int count) =>
        string.Concat(System.Linq.Enumerable.Range(1, count).Select(i =>
            $"<div id='i{i}'>Item {i} with enough words in it to wrap onto more than a single line when the column it sits in is narrow.</div>"));

    // The control: whatever the other engines do, ordinary block flow is stable - a failure elsewhere is
    // that mechanism's own, not this harness's.
    [TestMethod]
    public async Task PlainBlockFlow_LaidOutAgain_ReproducesItsGeometry()
    {
        var snapshots = await LayoutRepeatedlyAsync($"<div id='c'>{Items(24)}</div>", passes: 3, GeometryOf, pageHeight: 300);

        Assert.AreEqual(snapshots[0], snapshots[1]);
        Assert.AreEqual(snapshots[1], snapshots[2]);
    }

    private static string TableRows(int count) =>
        string.Concat(System.Linq.Enumerable.Range(1, count).Select(i =>
            $"<tr><td>Row {i} cell one</td><td>Row {i} cell two</td></tr>"));

    private const string RepeatingHeaderTable =
        "<table id='t' style='width:100%;border-collapse:collapse'>"
        + "<thead><tr><th>Head A</th><th>Head B</th></tr></thead>"
        + "<tbody>{0}</tbody></table>";

    [TestMethod]
    public async Task ATableWithARepeatingHeader_LaidOutAgain_DoesNotThrow()
    {
        var body = string.Format(CultureInfo.InvariantCulture, RepeatingHeaderTable, TableRows(40));

        var snapshots = await LayoutRepeatedlyAsync(body, passes: 3, GeometryOf, pageHeight: 400);

        Assert.AreEqual(3, snapshots.Count);
    }

    [TestMethod]
    public async Task ATableWithARepeatingHeader_LaidOutAgain_ReproducesItsBodyRows()
    {
        var body = string.Format(CultureInfo.InvariantCulture, RepeatingHeaderTable, TableRows(40));

        var rowGeometry = await LayoutRepeatedlyAsync(
            body, passes: 3,
            (root, _) => string.Join("|", Walk(root)
                .Where(b => b.HtmlTag?.Name == "td")
                .Select(b => string.Format(CultureInfo.InvariantCulture, "({0:F3},{1:F3})", b.Location.X, b.Location.Y))),
            pageHeight: 400);

        Assert.AreEqual(rowGeometry[0], rowGeometry[1]);
        Assert.AreEqual(rowGeometry[1], rowGeometry[2]);
    }

    [TestMethod]
    [DataRow(3)]
    [DataRow(12)]
    [DataRow(40)]
    public async Task ATableWithARepeatingHeader_LaidOutAgain_ReproducesItsOwnHeight(int rows)
    {
        var body = string.Format(CultureInfo.InvariantCulture, RepeatingHeaderTable, TableRows(rows));

        var heights = await LayoutRepeatedlyAsync(
            body, passes: 3,
            (root, _) =>
            {
                var table = Walk(root).First(b => b.HtmlTag?.TryGetAttribute("id") == "t");
                return (table.ActualBottom - table.Location.Y).ToString("F3", CultureInfo.InvariantCulture);
            },
            pageHeight: 400);

        Assert.AreEqual(heights[0], heights[1]);
        Assert.AreEqual(heights[1], heights[2]);
    }

    [TestMethod]
    public async Task ATableWithARepeatingHeader_LaidOutAgain_KeepsItsHeaderGroupExactlyOnce()
    {
        var body = string.Format(CultureInfo.InvariantCulture, RepeatingHeaderTable, TableRows(12));

        var counts = await LayoutRepeatedlyAsync(
            body, passes: 3,
            (root, _) => Walk(root).Count(b => b.HtmlTag?.Name == "thead").ToString(CultureInfo.InvariantCulture),
            pageHeight: 400);

        // The source <thead> stays exactly one, on every pass - RepeatedHeaderRows' own detached clones
        // (reset to null and rebuilt fresh at the top of every CssLayoutEngineTable pass) are never part of
        // CssBox.Boxes, so they must never show up in this count regardless of how many pages the table
        // spans or how many times layout runs.
        Assert.IsTrue(counts.All(c => c == "1"));
    }
}
