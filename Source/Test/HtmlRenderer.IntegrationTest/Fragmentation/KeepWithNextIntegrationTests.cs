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
/// Ported from PeachPDF.Tests/Integration/KeepWithNextIntegrationTests.cs: css-break-3 §3.1 keep-with-next
/// (a <c>break-after: avoid</c> on an earlier sibling, or <c>break-before: avoid</c> on the later one,
/// forbids a break between the two) pinned at the actual nudge sites -
/// <c>BlockFragmentation.RelocateIfNeeded</c> (break-inside:avoid/monolithic/table-row-atomicity),
/// <c>InlineFragmentation.ApplyLineBreaking</c> (orphans, or ordinary word-flow pushing a line across a
/// boundary) - each followed by <c>BlockFragmentation.EnforceKeepWithNext</c>.
/// </summary>
/// <remarks>
/// The UA default stylesheet's <c>h1-h6 { break-after: avoid }</c> lives under <c>@media print</c>
/// (<c>RAdapter.DefaultMediaType</c> vs <c>PdfSharpAdapter</c>'s) and never applies to this
/// IntegrationTest project's WinForms-based <c>HtmlContainer</c>, which reports media type "screen" - see
/// <c>StageR4KeepWithNextTest.cs</c>'s own established handling of the same trap. Every heading fixture
/// below sets <c>break-after: avoid</c> explicitly rather than relying on the UA default PeachPDF's
/// PdfSharp-hosted tests get for free.
/// <para>
/// A table with no explicit <c>break-inside:avoid</c> still moves wholesale here when it has exactly one
/// row: css-tables-3 §6.1's row-atomicity default (<c>TableRowDefaultAtomicityTest.cs</c>, commit
/// "Preserve table rows unfragmented by default") pushes the whole (single) row - and with it the table,
/// which has no other content - to the next page on its own, without needing PeachPDF's own
/// table-specific whole-table pre-check (which has no counterpart here).
/// </para>
/// <para>
/// Confirmed gap found while calibrating these fixtures against the real engine: unlike
/// <c>BlockFragmentation.RelocateIfNeeded</c> (a real relayout, so the moved box's own
/// <see cref="TheArtOfDev.HtmlRenderer.Core.Dom.CssBox.Location"/> is genuinely updated),
/// <c>CssLayoutEngineTable.LayoutCells</c>'s row-atomicity shift only offsets each CELL's own rectangle
/// (<c>cell.OffsetTop(delta)</c>) - it never touches the outer <c>&lt;table&gt;</c> box's own
/// <c>Location</c>/<c>EffectiveTop</c>, which stays exactly where the table's own (unmoved) natural top
/// fell. <c>EnforceKeepWithNext(g, table)</c> - called uniformly on the table like any other child in its
/// parent's child loop - reads that same stale <c>EffectiveTop</c>, so it never observes the boundary
/// crossing the row-shift just performed, and the table's own avoid-chained heading is never pulled.
/// Confirmed empirically: <c>TableMovedToNextPage_LeavesNonAvoidHeadingBehind</c> (which only checks that
/// the table's row itself moved, not that a heading follows it) passes; the two heading-pull variants below
/// are Ignored against this gap.
/// </para>
/// </remarks>
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

    // css-tables-3 §6.1's row-atomicity shift (CssLayoutEngineTable.LayoutCells) moves each CELL's own
    // rectangle (cell.OffsetTop); the outer <table> box's own Location follows too, but only when the
    // shifted row is the table's very first content (nothing rendered above it within the table yet) - an
    // ordinary row straddling further down a multi-page table correctly leaves the table's Location where
    // its real first row is. Reading a cell directly is the robust check either way, so tests use this
    // rather than assuming which case applies.
    private static CssBox FindFirstCell(CssBox table) => Walk(table).FirstOrDefault(b => b.HtmlTag?.Name == "td")!;

    // A table moved wholesale to the next page (css-tables-3 §6.1 row-atomicity, its only row too tall to
    // fit) must pull its avoid-chained heading along instead of stranding it at the bottom of the old page.
    [TestMethod]
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

    // Without an avoid link (break-after explicitly reset to auto), the heading must stay behind exactly
    // as before - the pull is driven by the avoid chain, not proximity.
    [TestMethod]
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

    // The chain walk must skip a display:none sibling and pull BOTH the heading and an avoid-chained intro
    // paragraph along when the table moves to the next page.
    [TestMethod]
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

    // A div pushed by break-inside: avoid must pull its avoid-chained heading the same way.
    [TestMethod]
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

    // The canonical real-document case: a heading followed by a plain paragraph. The paragraph is not
    // relocated wholesale - word flow pushes its first LINE to the next page - and the keep-with-next retry
    // must still bring the heading along.
    [TestMethod]
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

    // §4.3 relaxation, one tier at a time: where the whole chain cannot travel, the run is trimmed from its
    // *front* rather than dropped entirely - the h3 (nearest the breaking box) travels, the h2 does not.
    [TestMethod]
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

        // The tail of the run travelled...
        Assert.AreEqual(keepPage, container.PageIndexOf(inner.Location.Y));
        Assert.IsTrue(inner.ActualBottom <= keep.Location.Y + 1.0);

        // ...and the head of it did not - dropping the run whole would have stranded the h3 as well.
        Assert.IsTrue(container.PageIndexOf(outer.Location.Y) < keepPage,
            $"expected the chapter heading to stay behind, it is at y={outer.Location.Y}");
    }

    // Two consecutive avoid headings (h2 then h3) chain transitively - both move together with the content
    // that triggered the break.
    [TestMethod]
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

    // A paragraph relocated by the orphans rule (too few lines would remain before the page boundary) must
    // pull its avoid-chained heading along too - same idea, third nudge site.
    [TestMethod]
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

    // css-break §5.2: a forced break value takes precedence over an avoid on the other side of the same
    // break point - a forced-break pair must never be treated as keep-together, even when the later box is
    // subsequently relocated by break-inside: avoid.
    [TestMethod]
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

        // The forced break puts the keep box flush at page 2's own top (RelocateIfNeeded then declines to
        // move it any further: its content is taller than a whole page, so - per RelocateIfNeeded's own
        // "fits on no single page" rule - it is left exactly where the forced break put it rather than
        // moved somewhere it also would not fit). The heading must stay behind on page 1 either way: the
        // forced break between the two forbids keeping them together, independent of where the keep box
        // itself ends up.
        Assert.AreEqual(1, container.PageIndexOf(keep.Location.Y),
            $"Test setup expects the forced break to place the keep box on page 2, but it is at y={keep.Location.Y}");
        Assert.AreEqual(0, container.PageIndexOf(heading.Location.Y));
    }

    // css-break-3 §3.2: `avoid-page` names the page context explicitly, so it chains exactly as bare
    // `avoid` does. `avoid-column`/`avoid-region` name other fragmentation contexts and must not.
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

    // Unsatisfiable avoid at the break-inside site: heading + keep box taller than one page. PeachPDF's
    // relaxation moves the box alone; this port's RelocateIfNeeded relaxes the same constraint differently
    // - its own "fits on no single page" rule (see BlockFragmentation.RelocateIfNeeded's doc comment)
    // declines to move a box that cannot fit ANY page at all, leaving it exactly where ordinary flow placed
    // it rather than moved somewhere it also would not fit. Either way the outcome that matters is
    // preserved: the heading is never dragged into a multi-page mess alongside it.
    [TestMethod]
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

    // An unsatisfiable avoid (heading + content taller than one page) is relaxed per spec: the content
    // moves alone and the heading stays, instead of looping or overflowing.
    [TestMethod]
    public async Task UnsatisfiableAvoid_IsRelaxed_ContentMovesAlone()
    {
        var rows = string.Concat(Enumerable.Range(1, 60).Select(i => $"<tr><td>row {i}</td></tr>"));
        var (root, container) = await BuildAsync(
            "<h2 class='heading' style='margin:0;break-after:avoid'>Section heading</h2>"
            + $"<table class='keep' style='border-collapse:collapse'>{rows}</table>",
            fillerHeight: 100);

        var heading = FindByClass(root, "heading");
        Assert.IsNotNull(heading);

        // The heading must not be moved somewhere nonsensical: it stays on page 1.
        Assert.AreEqual(0, container.PageIndexOf(heading.Location.Y));
    }

    // break-before: avoid on the later sibling is the symmetric author-side trigger and must chain exactly
    // like break-after: avoid on the earlier one.
    [TestMethod]
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
