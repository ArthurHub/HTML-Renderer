using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest;

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
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task WrapperDivWithOneAvoidBreakChild_MovesWithIt_NeverSpansBothPages()
    {
        var checkedAnyRelocation = false;

        // Where the relocation fires depends on font metrics, so sweep instead of calibrating.
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

            // Only meaningful once the table was relocated to a page top (within border rounding).
            if (System.Math.Abs(table.Location.Y - container.PageTopOf(tableSlot)) > 2.0)
                continue;

            checkedAnyRelocation = true;
            Assert.AreEqual(tableSlot, cardSlot,
                $"at fillerCount={fillerCount}, the card wrapper is on page slot {cardSlot} but its sole break-inside:avoid child moved to slot {tableSlot}");
        }

        Assert.IsTrue(checkedAnyRelocation, "no filler count in range actually exercised a relocation - test is not meaningful as written");
    }
}
