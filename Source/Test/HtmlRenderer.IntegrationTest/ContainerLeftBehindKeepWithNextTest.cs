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
/// Verifies the same css-break-3 §3.1 propagation gap as <see cref="ContainerLeftBehindTest"/>, applied
/// to <c>EnforceKeepWithNext</c>'s run-pull instead of <c>RelocateIfNeeded</c>'s relocation: a heading
/// pulled onto a paragraph's page (because they're chained by <c>break-after:avoid</c>) is also the
/// section wrapping both of them's first in-flow child - the section's own top needs to follow the
/// heading up, or the section is left spanning from its original page to the pulled-together pair's new
/// one.
/// </summary>
/// <remarks>
/// Diagnosing this surfaced a SECOND, more fundamental bug along the way: <c>CssBox.OffsetTop</c> (what
/// <c>EnforceKeepWithNext</c> uses to pull the run) kept the box's own <c>Rectangles</c> dictionary in
/// sync but never the corresponding <c>CssLineBox.Rectangles</c> entry (a separate dictionary, keyed the
/// other way, that <c>CssLineBox.LineTop</c>/<c>LineBottom</c> - and therefore <c>CssBox.EffectiveTop</c>
/// for any inline-only box - read from). <c>Location.Y</c> (this method's own last statement) was
/// correctly updated while <c>EffectiveTop</c> silently kept reporting the pre-shift position - confirmed
/// by inspecting both dictionaries directly on a real shifted heading before the fix. Fixed by having
/// <c>OffsetTop</c> also update the line's own mirror entry for each line it touches.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class ContainerLeftBehindKeepWithNextTest
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
    public async Task SectionWrappingHeadingAndParagraph_MovesWithThePulledHeading_NeverSpansBothPages()
    {
        var checkedAnyPull = false;

        for (var fillerCount = 1; fillerCount < 60; fillerCount++)
        {
            using var wrapper = new HtmlContainer();
            var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line</p>", fillerCount));
            await wrapper.SetHtml(
                $"""
                <html><body>
                    {filler}
                    <div class="section" style="margin:0;">
                        <h2 style="margin:0; break-after:avoid;">SectionHeading WordTwo WordThree WordFour WordFive WordSix WordSeven WordEight WordNine WordTen</h2>
                        <p style="margin:0;">SectionParagraph</p>
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
            var section = allBoxes.FirstOrDefault(b => b.HtmlTag?.Name == "div" && b.GetAttribute("class") == "section");
            var heading = allBoxes.FirstOrDefault(b => b.HtmlTag?.Name == "h2");
            if (section == null || heading == null)
                continue;

            var sectionSlot = container.PageIndexOf(section.Location.Y);
            var headingSlot = container.PageIndexOf(heading.EffectiveTop);

            // Only meaningful once the heading has actually been pulled forward (flush at a fresh page
            // top) - otherwise there's no run-pull for the section to have gotten left behind by.
            if (System.Math.Abs(heading.EffectiveTop - container.PageTopOf(headingSlot)) > 0.5)
                continue;

            checkedAnyPull = true;

            Assert.AreEqual(headingSlot, sectionSlot,
                $"at fillerCount={fillerCount}, the section wrapper is on page slot {sectionSlot} but its heading was pulled to slot {headingSlot}");
        }

        Assert.IsTrue(checkedAnyPull, "no filler count in range actually exercised a keep-with-next pull - test is not meaningful as written");
    }
}
