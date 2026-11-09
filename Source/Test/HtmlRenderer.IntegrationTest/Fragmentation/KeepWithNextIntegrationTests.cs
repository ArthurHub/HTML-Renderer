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
/// Keep-with-next through <c>break-after: avoid</c> or <c>break-before: avoid</c>. Headings set it explicitly,
/// the UA default only applies to print media.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class KeepWithNextIntegrationTests
{
    private const double PageHeight = 1000;
    private const int MarginTop = 0;

    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    private static async Task<(CssBox Root, HtmlContainerInt Container)> BuildAsync(string sectionHtml, double fillerHeight = 900)
    {
        var wrapper = new HtmlContainer();
        await wrapper.SetHtml(
            $"<html><body style='margin:0'>"
            + $"<div class='filler' style='height:{fillerHeight}px'>filler</div>"
            + sectionHtml
            + "</body></html>");

        var container = GetInternal(wrapper);
        container.PageSize = new RSize(400, PageHeight);
        container.MarginTop = MarginTop;
        container.Location = new RPoint(0, MarginTop);
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

    // A moved row shifts its cells, the table box itself doesn't always follow, so read the cell.
    private static CssBox FindFirstCell(CssBox table) => Walk(table).FirstOrDefault(b => b.HtmlTag?.Name == "td")!;

    // The table's only row doesn't fit, so the whole table moves to the next page.
    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task TableMovedToNextPage_PullsAvoidChainedHeadingAlong()
    {
        var (root, container) = await BuildAsync(
            "<h2 class='heading' style='margin:0;break-after:avoid'>Section heading</h2>"
            + "<table class='keep' style='border-collapse:collapse'><tr><td><div style='height:150px'>swatch</div></td></tr></table>");

        var heading = FindByClass(root, "heading");
        var table = FindByClass(root, "keep");
        Assert.IsNotNull(heading);
        Assert.IsNotNull(table);
        var cell = FindFirstCell(table);
        Assert.IsNotNull(cell);

        var tablePage = container.PageIndexOf(cell.Location.Y);
        Assert.IsTrue(tablePage >= 1, $"Test setup expects the table to be moved to page 2+, but it is at y={cell.Location.Y}");
        Assert.AreEqual(tablePage, container.PageIndexOf(heading.Location.Y));
        Assert.IsTrue(heading.ActualBottom <= cell.Location.Y + 1.0,
            $"Heading (bottom={heading.ActualBottom}) must sit above the table (top={cell.Location.Y}) after both moved");
    }

    // Without an avoid the heading stays behind.
    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task TableMovedToNextPage_LeavesNonAvoidHeadingBehind()
    {
        var (root, container) = await BuildAsync(
            "<h2 class='heading' style='margin:0;break-after:auto;page-break-after:auto'>Section heading</h2>"
            + "<table class='keep' style='border-collapse:collapse'><tr><td><div style='height:150px'>swatch</div></td></tr></table>");

        var heading = FindByClass(root, "heading");
        var table = FindByClass(root, "keep");
        Assert.IsNotNull(heading);
        Assert.IsNotNull(table);
        var cell = FindFirstCell(table);
        Assert.IsNotNull(cell);

        Assert.IsTrue(container.PageIndexOf(cell.Location.Y) >= 1,
            $"Test setup expects the table to be moved to page 2+, but it is at y={cell.Location.Y}");
        Assert.AreEqual(0, container.PageIndexOf(heading.Location.Y));
    }

    // The chain skips a display:none sibling.
    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task TableMovedToNextPage_ChainSkipsDisplayNoneSibling_PullsHeadingAndIntroAlong()
    {
        var (root, container) = await BuildAsync(
            "<h2 class='heading' style='margin:0;break-after:avoid'>Section heading</h2>"
            + "<p class='intro' style='margin:0;break-after:avoid'>Intro paragraph kept with the content below.</p>"
            + "<style>.card { color: #222 }</style>"
            + "<table class='keep' style='border-collapse:collapse'><tr><td><div style='height:150px'>swatch</div></td></tr></table>");

        var heading = FindByClass(root, "heading");
        var intro = FindByClass(root, "intro");
        var table = FindByClass(root, "keep");
        Assert.IsNotNull(heading);
        Assert.IsNotNull(intro);
        Assert.IsNotNull(table);
        var cell = FindFirstCell(table);
        Assert.IsNotNull(cell);

        var tablePage = container.PageIndexOf(cell.Location.Y);
        Assert.IsTrue(tablePage >= 1, $"Test setup expects the table to be moved to page 2+, but it is at y={cell.Location.Y}");
        Assert.AreEqual(tablePage, container.PageIndexOf(heading.Location.Y));
        Assert.AreEqual(tablePage, container.PageIndexOf(intro.Location.Y));
        Assert.IsTrue(heading.ActualBottom <= intro.Location.Y + 1.0);
        Assert.IsTrue(intro.ActualBottom <= cell.Location.Y + 1.0);
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task BreakInsideAvoidBox_PullsAvoidChainedHeadingAlong()
    {
        var (root, container) = await BuildAsync(
            "<h2 class='heading' style='margin:0;break-after:avoid'>Section heading</h2>"
            + "<div class='keep' style='break-inside:avoid;page-break-inside:avoid'>"
            + "<div style='height:850px'>Keep together</div></div>");

        var heading = FindByClass(root, "heading");
        var keep = FindByClass(root, "keep");
        Assert.IsNotNull(heading);
        Assert.IsNotNull(keep);

        var keepPage = container.PageIndexOf(keep.Location.Y);
        Assert.IsTrue(keepPage >= 1, $"Test setup expects the avoid box to be moved to page 2+, but it is at y={keep.Location.Y}");
        Assert.AreEqual(keepPage, container.PageIndexOf(heading.Location.Y));
        Assert.IsTrue(heading.ActualBottom <= keep.Location.Y + 1.0);
    }

    // Only the paragraph's first line is pushed to the next page, not the whole paragraph.
    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task ParagraphFirstLinePushedByWordFlow_PullsAvoidChainedHeadingAlong()
    {
        var (root, container) = await BuildAsync(
            "<h2 class='heading' style='margin:0;break-after:avoid'>Section heading</h2>"
            + "<p class='para' style='margin:0'>A plain paragraph of body text that follows the heading and whose first line lands across the page boundary because filler pushed it there.</p>",
            fillerHeight: 965);

        var heading = FindByClass(root, "heading");
        var para = FindByClass(root, "para");
        Assert.IsNotNull(heading);
        Assert.IsNotNull(para);

        var paraPage = container.PageIndexOf(para.Location.Y);
        Assert.IsTrue(paraPage >= 1, $"Test setup expects the paragraph to start on page 2+, but it is at y={para.Location.Y}");
        Assert.AreEqual(paraPage, container.PageIndexOf(heading.Location.Y));
        Assert.IsTrue(heading.ActualBottom <= para.Location.Y + 1.0);
    }

    // css-break-3 §4.3: a chain too tall to travel whole is trimmed from its front.
    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task ChainedAvoidHeadings_TooTallToTravelWhole_AreTrimmedFromTheFront()
    {
        var (root, container) = await BuildAsync(
            "<h2 class='outer' style='margin:0;break-after:avoid;height:970px'>Chapter heading</h2>"
            + "<h3 class='inner' style='margin:0;break-after:avoid'>Section heading</h3>"
            + "<div class='keep' style='break-inside:avoid;page-break-inside:avoid'>"
            + "<div style='height:400px'>Keep together</div></div>",
            fillerHeight: 100);

        var outer = FindByClass(root, "outer");
        var inner = FindByClass(root, "inner");
        var keep = FindByClass(root, "keep");
        Assert.IsNotNull(outer);
        Assert.IsNotNull(inner);
        Assert.IsNotNull(keep);

        var keepPage = container.PageIndexOf(keep.Location.Y);
        Assert.IsTrue(keepPage >= 1, $"Test setup expects the avoid box to move, but it is at y={keep.Location.Y}");

        Assert.AreEqual(keepPage, container.PageIndexOf(inner.Location.Y));
        Assert.IsTrue(inner.ActualBottom <= keep.Location.Y + 1.0);

        Assert.IsTrue(container.PageIndexOf(outer.Location.Y) < keepPage,
            $"expected the chapter heading to stay behind, it is at y={outer.Location.Y}");
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task ChainedAvoidHeadings_AllMoveTogether()
    {
        var (root, container) = await BuildAsync(
            "<h2 class='outer' style='margin:0;break-after:avoid'>Chapter heading</h2>"
            + "<h3 class='inner' style='margin:0;break-after:avoid'>Section heading</h3>"
            + "<div class='keep' style='break-inside:avoid;page-break-inside:avoid'>"
            + "<div style='height:850px'>Keep together</div></div>");

        var outer = FindByClass(root, "outer");
        var inner = FindByClass(root, "inner");
        var keep = FindByClass(root, "keep");
        Assert.IsNotNull(outer);
        Assert.IsNotNull(inner);
        Assert.IsNotNull(keep);

        var keepPage = container.PageIndexOf(keep.Location.Y);
        Assert.IsTrue(keepPage >= 1, $"Test setup expects the avoid box to be moved to page 2+, but it is at y={keep.Location.Y}");
        Assert.AreEqual(keepPage, container.PageIndexOf(outer.Location.Y));
        Assert.AreEqual(keepPage, container.PageIndexOf(inner.Location.Y));
        Assert.IsTrue(outer.ActualBottom <= inner.Location.Y + 1.0);
        Assert.IsTrue(inner.ActualBottom <= keep.Location.Y + 1.0);
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task OrphansPushedParagraph_PullsAvoidChainedHeadingAlong()
    {
        var (root, container) = await BuildAsync(
            "<h2 class='heading' style='margin:0;break-after:avoid'>Section heading</h2>"
            + "<p class='para' style='font-size:9px;line-height:12px;margin:0;orphans:2;widows:2'>one<br>two<br>three<br>four<br>five<br>six</p>",
            fillerHeight: 965);

        var heading = FindByClass(root, "heading");
        var para = FindByClass(root, "para");
        Assert.IsNotNull(heading);
        Assert.IsNotNull(para);

        var paraPage = container.PageIndexOf(para.Location.Y);
        Assert.IsTrue(paraPage >= 1, $"Test setup expects the paragraph to be relocated to page 2+, but it is at y={para.Location.Y}");
        Assert.AreEqual(paraPage, container.PageIndexOf(heading.Location.Y));
    }

    [TestMethod]
    [Spec("css21", "page.html#page-break-props")]
    public async Task ForcedBreakAfter_TakesPrecedenceOverAvoid_HeadingIsNotPulled()
    {
        var (root, container) = await BuildAsync(
            "<h2 class='heading' style='margin:0;break-after:page;page-break-after:always'>Chapter heading</h2>"
            + "<div class='keep' style='break-inside:avoid;page-break-inside:avoid;break-before:avoid'>"
            + "<div style='height:1900px'>tall keep-together content</div></div>",
            fillerHeight: 100);

        var heading = FindByClass(root, "heading");
        var keep = FindByClass(root, "keep");
        Assert.IsNotNull(heading);
        Assert.IsNotNull(keep);

        Assert.AreEqual(1, container.PageIndexOf(keep.Location.Y),
            $"Test setup expects the forced break to place the keep box on page 2, but it is at y={keep.Location.Y}");
        Assert.AreEqual(0, container.PageIndexOf(heading.Location.Y));
    }

    // css-break-3 §3.2: avoid-column and avoid-region don't avoid page breaks.
    [TestMethod]
    [DataRow("break-after:avoid", true)]
    [DataRow("break-after:avoid-page", true)]
    [DataRow("break-after:avoid-column", false)]
    [DataRow("break-after:avoid-region", false)]
    public async Task KeepWithNext_ChainsOnlyOnPageContextAvoidance(string headingDeclaration, bool shouldChain)
    {
        var (root, container) = await BuildAsync(
            $"<h2 class='heading' style='margin:0;{headingDeclaration}'>Section heading</h2>"
            + "<div class='keep' style='break-inside:avoid'><div style='height:850px'>Keep together</div></div>");

        var heading = FindByClass(root, "heading");
        var keep = FindByClass(root, "keep");
        Assert.IsNotNull(heading);
        Assert.IsNotNull(keep);

        var keepPage = container.PageIndexOf(keep.Location.Y);
        Assert.IsTrue(keepPage >= 1, $"Test setup expects the avoid box to be moved to page 2+, but it is at y={keep.Location.Y}");

        if (shouldChain)
            Assert.AreEqual(keepPage, container.PageIndexOf(heading.Location.Y));
        else
            Assert.AreEqual(0, container.PageIndexOf(heading.Location.Y));
    }

    // A keep box taller than a page fits nowhere, so neither it nor the heading moves.
    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task UnsatisfiableAvoidAtBreakInsideSite_IsRelaxed_NeitherIsMoved()
    {
        var (root, container) = await BuildAsync(
            "<h2 class='heading' style='margin:0;break-after:avoid'>Section heading</h2>"
            + "<div class='keep' style='break-inside:avoid;page-break-inside:avoid'>"
            + "<div style='height:1900px'>taller than the space a heading would leave</div></div>",
            fillerHeight: 100);

        var heading = FindByClass(root, "heading");
        var keep = FindByClass(root, "keep");
        Assert.IsNotNull(heading);
        Assert.IsNotNull(keep);

        Assert.AreEqual(0, container.PageIndexOf(heading.Location.Y));
        Assert.AreEqual(0, container.PageIndexOf(keep.Location.Y),
            "content taller than a page fits nowhere, so RelocateIfNeeded must leave it exactly where ordinary flow placed it");
    }

    // Heading plus content taller than a page: the avoid is dropped and the heading stays.
    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task UnsatisfiableAvoid_IsRelaxed_ContentMovesAlone()
    {
        var rows = string.Concat(Enumerable.Range(1, 60).Select(i => $"<tr><td>row {i}</td></tr>"));
        var (root, container) = await BuildAsync(
            "<h2 class='heading' style='margin:0;break-after:avoid'>Section heading</h2>"
            + $"<table class='keep' style='border-collapse:collapse'>{rows}</table>",
            fillerHeight: 100);

        var heading = FindByClass(root, "heading");
        Assert.IsNotNull(heading);

        Assert.AreEqual(0, container.PageIndexOf(heading.Location.Y));
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task BreakBeforeAvoid_OnMovedBox_PullsPrecedingSiblingAlong()
    {
        var (root, container) = await BuildAsync(
            "<div class='lead' style='margin:0;break-after:auto'>Lead-in paragraph</div>"
            + "<div class='keep' style='break-inside:avoid;page-break-inside:avoid;break-before:avoid'>"
            + "<div style='height:850px'>Keep together</div></div>");

        var lead = FindByClass(root, "lead");
        var keep = FindByClass(root, "keep");
        Assert.IsNotNull(lead);
        Assert.IsNotNull(keep);

        var keepPage = container.PageIndexOf(keep.Location.Y);
        Assert.IsTrue(keepPage >= 1, $"Test setup expects the avoid box to be moved to page 2+, but it is at y={keep.Location.Y}");
        Assert.AreEqual(keepPage, container.PageIndexOf(lead.Location.Y));
    }
}
