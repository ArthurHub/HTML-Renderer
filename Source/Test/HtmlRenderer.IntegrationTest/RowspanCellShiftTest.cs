using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest;

/// <summary>
/// Moving a row that ends a rowspan to the next page also moves the spanning cell, the row itself only holds
/// a <c>CssSpacingBox</c> placeholder for it.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class RowspanCellShiftTest
{
    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    private static IEnumerable<CssBox> Walk(CssBox box)
    {
        yield return box;
        foreach (var b in box.Boxes)
            foreach (var d in Walk(b))
                yield return d;
    }

    [TestMethod]
    public async Task RowspanCellSpanningAShiftedRow_BottomTracksTheShift_NotLeftStale()
    {
        // Sweep filler counts - the exact boundary where the row-shift fires depends on font-metric
        // arithmetic (see this session's established testing lesson: never hardcode a "just barely
        // straddles" calibration).
        var checkedAnyShift = false;

        for (var fillerCount = 1; fillerCount < 60; fillerCount++)
        {
            using var wrapper = new HtmlContainer();
            var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line</p>", fillerCount));
            // Many extra rows before the rowspan pair push the table's own total height well past one
            // page, so RelocateIfNeeded's table-level relocation (which requires the whole table to fit
            // within one page) declines, leaving CssLayoutEngineTable's own row-level shift as the ONLY
            // mechanism that can act on the straddling row - otherwise a small table gets moved wholesale
            // and never exercises this bug at all.
            var extraRows = string.Concat(Enumerable.Range(0, 40).Select(i => $"<tr><td>Extra{i}A</td><td>Extra{i}B</td></tr>"));
            await wrapper.SetHtml(
                $"""
                <html><body>
                    {filler}
                    <table style="border-collapse:collapse; break-inside:avoid;">
                        {extraRows}
                        <tr><td rowspan="2">SpanCellContent</td><td>Row1Cell2</td></tr>
                        <tr><td>Row2Cell2</td></tr>
                    </table>
                </body></html>
                """);

            var container = GetInternal(wrapper);
            container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(300, 300);
            container.MarginTop = 0;
            wrapper.MaxSize = new SizeF(300, 0);

            using var bitmap = new Bitmap(300, 20000);
            using var g = Graphics.FromImage(bitmap);
            wrapper.PerformLayout(g);

            var allBoxes = Walk(container.Root).ToList();

            CssBox? FindEnclosingTd(string text)
            {
                var wordBox = allBoxes.FirstOrDefault(b => b.Words.Any(w => w.Text.Contains(text)));
                for (var b = wordBox; b != null; b = b.ParentBox)
                    if (b.HtmlTag?.Name == "td")
                        return b;
                return null;
            }

            var spanCell = FindEnclosingTd("SpanCellContent");
            var row2Cell = FindEnclosingTd("Row2Cell2");
            if (spanCell == null || row2Cell == null)
                continue;

            // Only meaningful once the shift has actually fired for this row (row2Cell flush at a fresh
            // page top) - otherwise there's nothing to have gotten stale in the first place.
            if (System.Math.Abs(row2Cell.Location.Y - container.PageTopOf(container.PageIndexOf(row2Cell.Location.Y))) > 0.5)
                continue;

            checkedAnyShift = true;
            Assert.IsGreaterThanOrEqualTo(row2Cell.ActualBottom - 0.5, spanCell.ActualBottom,
                $"at fillerCount={fillerCount}, the rowspan cell's bottom ({spanCell.ActualBottom:F1}) fell short of its sibling's ({row2Cell.ActualBottom:F1}) after the row-shift");
        }

        Assert.IsTrue(checkedAnyShift, "no filler count in range actually exercised the row-shift - test is not meaningful as written");
    }
}
