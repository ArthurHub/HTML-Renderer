using System;
using System.Collections.Generic;
using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragments;

namespace HtmlRenderer.IntegrationTest.Painting;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/FragmentPaintIntegrationTests.cs: paint driven by the fragment
/// tree - each page paints its own fragmentainer and nothing else, and every drawn rectangle is the one the
/// fragment carries. Maps directly to <c>Core/Paint/FragmentPainter.cs</c>, and (for the multi-page tests)
/// its production per-page entry point <c>HtmlContainerInt.PerformPaint(RGraphics, FragmentainerFragment)</c>,
/// exercised here through <see cref="PaintHarness.PaintPage(TheArtOfDev.HtmlRenderer.Core.HtmlContainerInt, int)"/>.
/// </summary>
/// <remarks>
/// All 8 of PeachPDF's tests port. Fixtures use CSS <c>px</c> directly (this port's <see cref="RSize"/>-based
/// <c>PageSize</c> matches 1:1, per the convention <c>PageBreakIntegrationTests</c> already established)
/// rather than PeachPDF's <c>pt</c>.
/// <para>
/// One confirmed gap surfaced while porting: <c>StackingOrder_IsPreservedWhenPaintingFromFragments</c> is
/// <c>[Ignore]</c>d - z-index is a parse-only stub, never consulted by paint order (see that test's own
/// remarks). A second, more consequential real bug was found and fixed as part of this batch, not merely
/// documented: <c>HtmlContainerInt.PerformPaint(RGraphics, FragmentainerFragment)</c> - the per-page paint
/// entry point <c>PdfGenerator</c>'s own page loop calls - pushed a paint clip starting at Y=<c>MarginTop</c>
/// rather than Y=0, silently clipping away the first <c>MarginTop</c>-tall strip of every single page's own
/// content (fragment-tree geometry is already band-local, where a band's own top is local Y=0, not
/// <c>MarginTop</c> - see <c>FragmentEmitter</c>'s own doc comment). Found while adapting
/// <c>StraddlingListMarkerTests.EveryMarker_IsDrawnOnExactlyOnePage</c> (a list item landing entirely within
/// the clipped strip and never appearing in any page's paint log, with no exception raised), fixed at its
/// source in <c>HtmlContainerInt.cs</c> - see that method's own updated remarks for the full mechanism.
/// </para>
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class FragmentPaintIntegrationTests
{
    [TestMethod]
    public void EachPage_PaintsOnlyItsOwnFragmentainersContent()
    {
        var (_, container) = PaintHarness.LayoutPaginated(PaintHarness.Wrap(
            "<p style='margin:0;height:150px'>PageOneMarker</p>"
            + "<p style='margin:0;height:150px;page-break-before:always'>PageTwoMarker</p>"),
            pageHeight: 200, margin: 0);

        Assert.AreEqual(2, container.FragmentTree!.Fragmentainers.Count);

        var page0 = PaintHarness.PaintPage(container, 0);
        var page1 = PaintHarness.PaintPage(container, 1);

        Assert.IsTrue(page0.DrawStringCalls.Any(c => c.Text.Contains("PageOneMarker")));
        Assert.IsFalse(page0.DrawStringCalls.Any(c => c.Text.Contains("PageTwoMarker")));

        Assert.IsTrue(page1.DrawStringCalls.Any(c => c.Text.Contains("PageTwoMarker")));
        Assert.IsFalse(page1.DrawStringCalls.Any(c => c.Text.Contains("PageOneMarker")));
    }

    [TestMethod]
    public void PaintedText_LandsAtItsOwnFragmentsCoordinates()
    {
        var (_, container) = PaintHarness.LayoutPaginated(PaintHarness.Wrap(
            "<p style='margin:0;height:150px'>PageOneMarker</p>"
            + "<p style='margin:0;height:150px;page-break-before:always'>PageTwoMarker</p>"),
            pageHeight: 200, margin: 0);

        for (var page = 0; page < 2; page++)
        {
            var recording = PaintHarness.PaintPage(container, page);
            var fragmentainer = container.FragmentTree!.Fragmentainers[page];
            var wordRects = WordRects(fragmentainer.Root).ToList();

            Assert.IsTrue(recording.DrawStringCalls.Count > 0);

            // Every drawn glyph run sits exactly where its own fragment says, in page-local coordinates -
            // no page offset is applied at paint time any more.
            foreach (var call in recording.DrawStringCalls)
            {
                Assert.IsTrue(wordRects.Any(r => Math.Abs(r.X - call.Point.X) < 0.001));
            }

            // Page 1's content is 200px down the document but paints near its own page top.
            Assert.IsTrue(recording.DrawStringCalls.All(c => c.Point.Y >= 0 && c.Point.Y <= 200));
        }
    }

    [TestMethod]
    public void BoxSpanningTwoPages_PaintsItsBackgroundOnBoth()
    {
        var (_, container) = PaintHarness.LayoutPaginated(PaintHarness.Wrap(
            "<div id='tall' style='height:300px;background:rgb(10,20,30)'>x</div>"),
            pageHeight: 200, margin: 0);

        Assert.AreEqual(2, container.FragmentTree!.Fragmentainers.Count);

        for (var page = 0; page < 2; page++)
        {
            var recording = PaintHarness.PaintPage(container, page);

            // Sliced, not cloned: each fragment paints the whole box's background rectangle and the page
            // clip does the cutting (box-decoration-break: slice, the initial value).
            Assert.IsTrue(recording.Log.OfType<RecordingGraphics.DrawRectCall>()
                .Any(r => r.Color == RColorOf(10, 20, 30)));
        }
    }

    [TestMethod]
    public void FixedBox_PaintsAtTheSameCoordinatesOnEveryPage()
    {
        var (_, container) = PaintHarness.LayoutPaginated(PaintHarness.Wrap(
            "<div style='position:fixed;top:10px;left:10px;width:40px;height:20px;background:rgb(1,2,3)'></div>"
            + "<p style='margin:0;height:150px'>One</p>"
            + "<p style='margin:0;height:150px;page-break-before:always'>Two</p>"),
            pageHeight: 200, margin: 0);

        Assert.AreEqual(2, container.FragmentTree!.Fragmentainers.Count);

        var painted = new List<RecordingGraphics.DrawRectCall>();

        for (var page = 0; page < 2; page++)
        {
            var recording = PaintHarness.PaintPage(container, page);
            var matches = recording.Log.OfType<RecordingGraphics.DrawRectCall>()
                .Where(r => r.Color == RColorOf(1, 2, 3)).ToList();

            Assert.AreEqual(1, matches.Count);
            painted.Add(matches[0]);
        }

        Assert.AreEqual(painted[0].X, painted[1].X, 0.001);
        Assert.AreEqual(painted[0].Y, painted[1].Y, 0.001);
    }

    // CSS 2.1 Appendix E within one stacking context: the lower z-index sibling should paint first.
    [Ignore("Confirmed gap, unrelated to fragment-tree painting: z-index is parsed and stored " +
        "(CssEngine/StyleProperties/Flow/ZIndexProperty.cs) but never consulted anywhere in " +
        "Core/Paint/ - confirmed by grepping the whole paint tree for it (no matches). " +
        "FragmentPainter.PaintFragmentContent paints absolutely-positioned children in fragment.Children " +
        "(document/DOM) order, with no z-index sort at all - so this fixture paints blue (DOM-first, " +
        "higher z-index) before red (DOM-second, lower z-index), the opposite of what css-break-3 - " +
        "unrelated, this is a pre-existing paint-order gap - requires. Confirmed by running this test " +
        "unignored.")]
    [TestMethod]
    public void StackingOrder_IsPreservedWhenPaintingFromFragments()
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div style='position:relative'>"
            + "<div style='position:absolute;z-index:2;width:10px;height:10px;background:rgb(0,0,255)'></div>"
            + "<div style='position:absolute;z-index:1;width:10px;height:10px;background:rgb(255,0,0)'></div>"
            + "</div>"));

        var recording = PaintHarness.PaintPage(container, 0);

        var rects = recording.Log.OfType<RecordingGraphics.DrawRectCall>().ToList();
        var red = rects.FindIndex(r => r.Color == RColorOf(255, 0, 0));
        var blue = rects.FindIndex(r => r.Color == RColorOf(0, 0, 255));

        Assert.IsTrue(red >= 0 && blue >= 0, "both positioned boxes must paint");
        Assert.IsTrue(red < blue, "the lower z-index sibling must paint first");
    }

    [TestMethod]
    public void RowspanCell_ShowsThroughEveryRowItSpans()
    {
        // A rowspan placeholder has no content of its own; the spanned cell reaches it as a fragment
        // child, so the ordinary paint walk draws it once per row it spans.
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table><tr><td rowspan='2'>SpannedCell</td><td>A</td></tr><tr><td>B</td></tr></table>"));

        var recording = PaintHarness.PaintPage(container, 0);

        Assert.IsTrue(recording.DrawStringCalls.Any(c => c.Text.Contains("SpannedCell")));
    }

    [TestMethod]
    public void VisibilityHidden_ReservesLayoutSpace_ButPaintsNothing_VisibleSiblingStillPaints()
    {
        // Unlike display:none (which removes the box from layout entirely), visibility:hidden must still
        // reserve its own space - the visible sibling starts right after it, not overlapping.
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='hidden' style='visibility:hidden;height:50px;background:rgb(10,20,30)'>Hidden</div>"
            + "<div id='visible' style='height:50px;background:rgb(40,50,60)'>Visible</div>"));

        var hidden = PaintHarness.FindById(root, "hidden")!;
        var visible = PaintHarness.FindById(root, "visible")!;

        Assert.AreEqual(hidden.ActualBottom, visible.Location.Y, 1);

        var recording = PaintHarness.PaintPage(container, 0);

        Assert.IsFalse(recording.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColorOf(10, 20, 30)));
        Assert.IsFalse(recording.DrawStringCalls.Any(c => c.Text.Contains("Hidden")));

        Assert.IsTrue(recording.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColorOf(40, 50, 60)));
        Assert.IsTrue(recording.DrawStringCalls.Any(c => c.Text.Contains("Visible")));
    }

    [TestMethod]
    public void VisibilityCollapse_ReservesLayoutSpace_ButPaintsNothing_VisibleSiblingStillPaints()
    {
        // HTML-Renderer doesn't implement table row/column collapse layout either - FragmentPainter's own
        // paint gate (Core/Paint/FragmentPainter.cs's PaintFragment) checks only "!= CssConstants.Visible",
        // not the specific value, so visibility:collapse renders identically to visibility:hidden here too.
        // Confirmed by direct source read.
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='collapsed' style='visibility:collapse;height:50px;background:rgb(10,20,30)'>Collapsed</div>"
            + "<div id='visible' style='height:50px;background:rgb(40,50,60)'>Visible</div>"));

        var collapsed = PaintHarness.FindById(root, "collapsed")!;
        var visible = PaintHarness.FindById(root, "visible")!;

        Assert.AreEqual(collapsed.ActualBottom, visible.Location.Y, 1);

        var recording = PaintHarness.PaintPage(container, 0);

        Assert.IsFalse(recording.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColorOf(10, 20, 30)));
        Assert.IsFalse(recording.DrawStringCalls.Any(c => c.Text.Contains("Collapsed")));

        Assert.IsTrue(recording.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColorOf(40, 50, 60)));
        Assert.IsTrue(recording.DrawStringCalls.Any(c => c.Text.Contains("Visible")));
    }

    private static RColor RColorOf(int r, int g, int b) => RColor.FromArgb(r, g, b);

    private static IEnumerable<RRect> WordRects(BoxFragment fragment)
    {
        foreach (var word in fragment.Words)
            yield return word.Rect;

        foreach (var child in fragment.Children)
            foreach (var rect in WordRects(child))
                yield return rect;

        if (fragment.MarkerFragment != null)
            foreach (var rect in WordRects(fragment.MarkerFragment))
                yield return rect;
    }
}
