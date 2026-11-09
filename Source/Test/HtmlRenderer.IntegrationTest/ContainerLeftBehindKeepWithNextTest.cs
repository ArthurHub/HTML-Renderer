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
/// A heading pulled onto its paragraph's page by <c>break-after:avoid</c> takes the section that starts
/// with it along.
/// </summary>
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
    [Spec("css21", "page.html#allowed-page-breaks")]
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

            // Only meaningful once the heading was pulled to a page top.
            if (System.Math.Abs(heading.EffectiveTop - container.PageTopOf(headingSlot)) > 0.5)
                continue;

            checkedAnyPull = true;

            Assert.AreEqual(headingSlot, sectionSlot,
                $"at fillerCount={fillerCount}, the section wrapper is on page slot {sectionSlot} but its heading was pulled to slot {headingSlot}");
        }

        Assert.IsTrue(checkedAnyPull, "no filler count in range actually exercised a keep-with-next pull - test is not meaningful as written");
    }
}
