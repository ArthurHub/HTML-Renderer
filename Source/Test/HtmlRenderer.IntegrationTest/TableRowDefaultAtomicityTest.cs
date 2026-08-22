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
/// Verifies a real, spec-confirmed default-behavior gap found while auditing this port's fragmentation
/// engine against PeachPDF a third time, then checking the actual W3C text directly
/// (<see href="https://drafts.csswg.org/css-tables-3/#breaking-rules">css-tables-3 §6.1</see>, current
/// Editor's Draft): "When fragmenting a table, user agents <b>must</b> attempt to preserve the table rows
/// unfragmented if the cells spanning the row do not span any subsequent row, and their height is at
/// least twice smaller than both the fragmentainer height and width. Other rows are said <i>freely
/// fragmentable</i>." This is phrased as a required UA default, not something an author opts into -
/// <c>CssLayoutEngineTable.LayoutCells</c> previously only preserved a row when the TABLE had explicit
/// <c>break-inside:avoid</c>, meaning an ordinary multi-page table with no special markup at all rendered
/// rows split across page boundaries by default, which the spec does not permit as the default.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class TableRowDefaultAtomicityTest
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
    public async Task OrdinaryRowWithNoBreakInsideAvoid_IsStillPreservedUnfragmented_ByDefault()
    {
        var checkedAnyStraddleCandidate = false;

        for (var fillerCount = 1; fillerCount < 60; fillerCount++)
        {
            using var wrapper = new HtmlContainer();
            var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line</p>", fillerCount));
            // Deliberately no break-inside:avoid anywhere - this is the plain, no-special-markup case
            // css-tables-3 §6.1 says every conformant UA must handle this way by default.
            await wrapper.SetHtml(
                $"""
                <html><body>
                    {filler}
                    <table style="border-collapse:collapse;">
                        <tr><td>RowOneCell</td></tr>
                        <tr><td>TargetRowCellText with several words giving it real, non-trivial height</td></tr>
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

            var tds = Walk(container.Root).Where(b => b.HtmlTag?.Name == "td").ToList();
            if (tds.Count < 2)
                continue;
            var targetCell = tds[1];

            var topSlot = container.PageIndexOf(targetCell.Location.Y);
            var bottomSlot = container.PageIndexOf(System.Math.Max(targetCell.Location.Y, targetCell.ActualBottom - 0.01));

            checkedAnyStraddleCandidate = true;
            Assert.AreEqual(topSlot, bottomSlot,
                $"at fillerCount={fillerCount}, the second row straddles page slots {topSlot}->{bottomSlot} with no break-inside:avoid anywhere - css-tables-3 6.1 requires it stay whole by default");
        }

        Assert.IsTrue(checkedAnyStraddleCandidate, "no filler count in range produced a target cell - test is not meaningful as written");
    }

    [TestMethod]
    public async Task RowSpanningIntoASubsequentRow_RemainsFreelyFragmentable()
    {
        // css-tables-3 6.1's own carve-out: a row a rowspan cell only STARTS in (spanning further rows)
        // is explicitly excluded from the "preserve unfragmented" default - confirming the new default
        // atomicity doesn't overreach into content the spec says must stay freely fragmentable.
        var foundAStraddle = false;

        for (var fillerCount = 1; fillerCount < 30; fillerCount++)
        {
            using var wrapper = new HtmlContainer();
            var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line</p>", fillerCount));
            await wrapper.SetHtml(
                $"""
                <html><body>
                    {filler}
                    <table style="border-collapse:collapse;">
                        <tr><td rowspan="2">SpanCell</td><td>Row1Cell2 with enough words to make this row meaningfully tall for the straddle test to matter</td></tr>
                        <tr><td>Row2Cell</td></tr>
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

            var row1Cell2 = Walk(container.Root)
                .FirstOrDefault(b => b.Words.Any(w => w.Text.Contains("Row1Cell2")))
                ?.ParentBox;
            if (row1Cell2 == null)
                continue;

            var topSlot = container.PageIndexOf(row1Cell2.Location.Y);
            var bottomSlot = container.PageIndexOf(System.Math.Max(row1Cell2.Location.Y, row1Cell2.ActualBottom - 0.01));
            if (topSlot != bottomSlot)
                foundAStraddle = true;
        }

        Assert.IsTrue(foundAStraddle, "expected at least one filler count where the rowspan-starting row straddles a page boundary - if none do, this test isn't exercising the carve-out");
    }
}
