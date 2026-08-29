using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HtmlRenderer.IntegrationTest.Painting;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/GhostTextOnPreviousPageIntegrationTests.cs (issue #113): a box
/// relocated to the next page's content top (forced breaks, <c>break-inside:avoid</c>) must not also leave a
/// clipped-but-still-logged duplicate behind on the page it left, when it lands flush against exactly that
/// page's own boundary.
/// </summary>
/// <remarks>
/// PeachPDF's underlying bug was a paint-time clip-intersection check (<c>RRect.Intersect</c> treating two
/// rects that merely touch at an edge as non-empty) in a live-tree paint walk that re-painted the WHOLE
/// document, translated, once per page and relied on that check alone to cull content that belonged to a
/// different page.
/// <para>
/// That specific mechanism does not exist in this port. Paint here is driven from the immutable
/// <see cref="TheArtOfDev.HtmlRenderer.Core.Fragments.FragmentTree"/> (<c>Core/Paint/FragmentPainter.cs</c>),
/// which is built once by <c>FragmentEmitter.Finish</c> - and a box only gets a
/// <see cref="TheArtOfDev.HtmlRenderer.Core.Fragments.BoxFragment"/>/<see cref="TheArtOfDev.HtmlRenderer.Core.Fragments.TextFragment"/>
/// on a given page's band at all if <c>FragmentEmitter.HasContentInBand</c> finds its geometry actually
/// overlapping that band (a strict <c>rect.Top &lt; band.Bottom &amp;&amp; rect.Bottom &gt; band.Top</c> test -
/// exactly touching a boundary, as a relocated box does by construction, does not overlap the band it left).
/// So a box relocated flush to the very next page's top structurally has nothing built for the previous
/// page's fragment to paint in the first place - there is no separate paint-time clip check left to get
/// wrong. Ported anyway as a direct regression pin of the same observable, user-facing invariant PeachPDF's
/// fix targets, using the real multi-page harness plus <see cref="PaintHarness.PaintPage(TheArtOfDev.HtmlRenderer.Core.HtmlContainerInt, int)"/>
/// (which exercises the same production per-page paint entry point,
/// <c>HtmlContainerInt.PerformPaint(RGraphics, FragmentainerFragment)</c>, that <c>PdfGenerator</c> uses)
/// rather than because the exact defect was expected to reproduce.
/// </para>
/// <para>
/// PeachPDF's sibling <c>PageMarginPixelsPerPointIntegrationTests</c> class (3 tests, same source file) is
/// dropped entirely rather than ported: it tests an <c>@page {{ margin: ... }}</c> rule round-tripping into
/// <c>HtmlContainer.MarginTop</c>/<c>PixelsPerPoint</c> scaling. HTML-Renderer has no such cascade at all -
/// confirmed by grepping the whole <c>Core/</c> tree for <c>PixelsPerPoint</c> (no matches) and for any
/// <c>@page</c>-margin consumer feeding <c>HtmlContainerInt.MarginTop</c> (none found;
/// <c>MarginTop</c>/<c>MarginBottom</c>/<c>MarginLeft</c>/<c>MarginRight</c> are set only by the hosting
/// application, e.g. <c>PdfGenerator</c>/<c>PdfSharpAdapter</c> callers, never derived from parsed CSS) - this
/// matches the precedent already established for other <c>@page</c>-adjacent features in this port (e.g.
/// <c>@page size</c> is a confirmed parse-only stub per the porting plan's own exclusion list).
/// </para>
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class GhostTextOnPreviousPageIntegrationTests
{
    private const double PageHeight = 400.0;

    [TestMethod]
    public void ForcedBreak_RelocatedHeading_DoesNotPaintOnPreviousPage()
    {
        // .filler pushes the flow close to the page-1 boundary; the forced break on #second lands it flush
        // at exactly PageHeight (zero margins keep the landing position an exact, deterministic multiple of
        // PageHeight - the precise scenario that triggered PeachPDF's "merely touching the clip edge" bug).
        var html = PaintHarness.Wrap(
            "<div style='height:350px;background:rgb(240,240,240)'></div>"
            + "<h2 id='second' style='page-break-before:always;margin:0'>RelocatedHeadingMarker</h2>");

        var (root, container) = PaintHarness.LayoutPaginated(html, pageHeight: PageHeight, margin: 0);

        var heading = PaintHarness.FindById(root, "second");
        Assert.IsNotNull(heading);

        // Confirm the test is actually exercising the boundary-touching case: the heading must land exactly
        // at the page-1 top, not merely somewhere on page 1.
        Assert.AreEqual(PageHeight, heading!.Location.Y, 0.01);

        var page0 = PaintHarness.PaintPage(container, 0);
        var page1 = PaintHarness.PaintPage(container, 1);

        Assert.IsFalse(page0.DrawStringCalls.Any(c => c.Text.Contains("RelocatedHeadingMarker")));
        Assert.IsTrue(page1.DrawStringCalls.Any(c => c.Text.Contains("RelocatedHeadingMarker")));
    }

    [TestMethod]
    public void BreakInsideAvoid_RelocatedBox_DoesNotPaintOnPreviousPage()
    {
        // .filler leaves only 20px of page 0 remaining (400 - 380) - not enough room for even one of
        // #avoid's three 12px lines, so break-inside:avoid has to relocate the whole box rather than let it
        // start there. .filler itself stays short of PageHeight so it still fits on page 0, which is what
        // makes the relocated box land flush at exactly PageHeight (zero margins keep that an exact,
        // deterministic multiple - the precise scenario that triggers the "merely touching the clip edge"
        // bug PeachPDF documents).
        var html = PaintHarness.Wrap(
            "<div style='height:380px;background:rgb(240,240,240)'></div>"
            + "<div id='avoid' style='break-inside:avoid;page-break-inside:avoid;margin:0'>"
            + "<p style='margin:0;font-size:10px;line-height:12px'>AvoidedParagraphMarker</p>"
            + "<p style='margin:0;font-size:10px;line-height:12px'>Second line</p>"
            + "<p style='margin:0;font-size:10px;line-height:12px'>Third line</p>"
            + "</div>");

        var (root, container) = PaintHarness.LayoutPaginated(html, pageHeight: PageHeight, margin: 0);

        var avoidBox = PaintHarness.FindById(root, "avoid");
        Assert.IsNotNull(avoidBox);
        Assert.AreEqual(PageHeight, avoidBox!.Location.Y, 0.01);

        var page0 = PaintHarness.PaintPage(container, 0);
        var page1 = PaintHarness.PaintPage(container, 1);

        Assert.IsFalse(page0.DrawStringCalls.Any(c => c.Text.Contains("AvoidedParagraphMarker")));
        Assert.IsTrue(page1.DrawStringCalls.Any(c => c.Text.Contains("AvoidedParagraphMarker")));
    }
}
