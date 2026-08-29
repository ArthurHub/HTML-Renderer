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
/// Verifies a real, previously-undocumented gap found while auditing this port's fragmentation engine
/// against PeachPDF a second time (a later, separate pass over what remained after the R0-R10 plan
/// completed): css-break-3 §3.1's break-point propagation was only ever applied to forced breaks
/// (<see cref="TheArtOfDev.HtmlRenderer.Core.Fragmentation.BlockFragmentation.TryGetForcedBreakTarget"/>'s
/// own "no previous sibling" check), never to <c>break-inside:avoid</c>/monolithic relocation
/// (<c>RelocateIfNeeded</c>). A box moved by that method while it's its parent's first (and here, only)
/// in-flow child - a plain wrapper div with no content before it - left the parent spanning from its
/// original page to the child's new one, its own background/border rendered as a stub-then-continuation
/// for no reason a CSS author would expect (e.g. a card/panel div wrapping a single table or figure).
/// Confirmed by temporarily reverting the fix: card and table reliably landed on different page slots at
/// several filler counts.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class ContainerLeftBehindTest
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
    public async Task WrapperDivWithOneAvoidBreakChild_MovesWithIt_NeverSpansBothPages()
    {
        var checkedAnyRelocation = false;

        // Sweep filler counts - the exact boundary where the relocation fires depends on font-metric
        // arithmetic (this session's established testing lesson: never hardcode a "just barely
        // straddles" calibration).
        for (var fillerCount = 1; fillerCount < 60; fillerCount++)
        {
            using var wrapper = new HtmlContainer();
            var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line</p>", fillerCount));
            await wrapper.SetHtml(
                $"""
                <html><body>
                    {filler}
                    <div class="card" style="margin:0;">
                        <table style="border-collapse:collapse; break-inside:avoid;">
                            <tr><td>CellOne</td></tr>
                            <tr><td>CellTwo</td></tr>
                        </table>
                    </div>
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
            var card = allBoxes.FirstOrDefault(b => b.HtmlTag?.Name == "div" && b.GetAttribute("class") == "card");
            var table = allBoxes.FirstOrDefault(b => b.HtmlTag?.Name == "table");
            if (card == null || table == null)
                continue;

            var cardSlot = container.PageIndexOf(card.Location.Y);
            var tableSlot = container.PageIndexOf(table.Location.Y);

            // Only meaningful once the table has actually been relocated (flush - within border-rounding
            // slack - at a fresh page top) - otherwise there's nothing for the card to have gotten left
            // behind by in the first place.
            if (System.Math.Abs(table.Location.Y - container.PageTopOf(tableSlot)) > 2.0)
                continue;

            checkedAnyRelocation = true;
            Assert.AreEqual(tableSlot, cardSlot,
                $"at fillerCount={fillerCount}, the card wrapper is on page slot {cardSlot} but its sole break-inside:avoid child moved to slot {tableSlot}");
        }

        Assert.IsTrue(checkedAnyRelocation, "no filler count in range actually exercised a relocation - test is not meaningful as written");
    }
}
