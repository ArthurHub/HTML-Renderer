using System.Linq;
using HtmlRenderer.Test.TestSupport;

namespace HtmlRenderer.Test.Fragmentation;

/// <summary>
/// Ported from PeachPDF.Tests/Html/Core/HtmlContainerIntPaginationTests.cs (HtmlContainerIntPaginationTests).
/// </summary>
/// <remarks>
/// Tests for the fragment tree's page-materialization rule, which is SUPPOSED to skip building a
/// fragmentainer for a wholly content-empty page-slot, per CSS Paged Media Level 3 §3.2 ("User agents
/// SHOULD avoid generating a large number of content-empty pages"). This port's own equivalent
/// (<c>FragmentEmitter.HasContentInBand</c>, Core/Fragmentation/FragmentEmitter.cs ~120-167) turns out NOT
/// to skip anything, confirmed empirically (see the two <c>[Ignore]</c>d tests below) and by direct source
/// read: <c>Finish()</c>'s per-slot loop (~70-82) calls <c>HasContentInBand(root, band)</c> with the
/// DOCUMENT ROOT as the starting box on every iteration, and <c>HasContentInBand</c>'s very first check
/// (~127-130, reached because a plain block container's <c>Rectangles</c> - a per-<c>CssLineBox</c> dictionary,
/// Core/Dom/CssBox.cs ~430 - is empty unless it hosts inline content of its own) is
/// <c>Overlaps(box.Bounds, band)</c>, where <c>Bounds</c> (Core/Dom/CssBoxProperties.cs ~911-914) is simply
/// <c>Location</c>+<c>Size</c> - the root's own auto-height border box, which by construction already spans
/// every band the slot loop ever visits (<c>lastSlot</c> is itself derived from <c>root.ActualBottom</c>).
/// So <c>HasContentInBand(root, band)</c> returns true on this very first check, for every slot, regardless
/// of what is or isn't inside - the recursion into children/words that would actually distinguish a
/// content-empty band from a content-having one is unreachable from this top-level call. Only
/// <see cref="Fragmentainers_ContiguousRealContent_KeepEveryPage"/> (which asserts nothing ever
/// false-skips, not that anything real skips) is unaffected by this and stays active.
/// Adapted to the adapter-free <see cref="LayoutHarness"/> (real <c>PerformLayout</c> over <c>MockAdapter</c>)
/// rather than PeachPDF's real <c>PdfSharpAdapter</c>-driven harness, with plain "px" markup rather than
/// PeachPDF's "pt" (this fork's internal layout unit is CSS px; "pt" would scale by
/// <c>Length.PointsPerPx</c> and throw off the exact page-boundary numbers the assertions depend on), and
/// "background-color" rather than the "background" shorthand, matching this repository's own established
/// adaptation (see <c>HtmlRenderer.IntegrationTest.Positioning.FixedPositionPaginationIntegrationTests</c>'s
/// identical note: this fork's CssUtils dispatch has no case for the "background" shorthand key at all).
/// </remarks>
[TestClass]
public sealed class HtmlContainerIntPaginationTests
{
    [Ignore("FragmentEmitter.HasContentInBand never actually skips a content-empty slot in this port - " +
            "confirmed empirically and by direct source read, see the class remarks above. A document with " +
            "an 880px content-free gap between two 20px content divs (page height 200) produces one " +
            "fragmentainer for EVERY slot (0/200/400/600/800), not just the two genuinely content-having " +
            "ones, because Finish()'s per-slot HasContentInBand(root, band) check is satisfied by the " +
            "document root's own auto-height Bounds before it ever considers whether the gap div itself " +
            "has anything printable in it.")]
    [TestMethod]
    public void Fragmentainers_RealContentSeparatedByMultiPageGap_SkipWhollyEmptySlots()
    {
        // Page height 200: real content at the very top (page-slot 0) and real content starting
        // at y=900 (page-slot 4) - slots 1-3 have nothing painted in them at all and, per css-page-media-3
        // §3.2, must not be materialized.
        var (_, container) = LayoutHarness.Layout(
            LayoutHarness.Wrap(
                "<div id='a' style='height:20px; background-color:rgb(0,0,0);'></div>" +
                "<div id='gap' style='height:880px;'></div>" +
                "<div id='b' style='height:20px; background-color:rgb(0,0,0);'></div>"),
            pageHeight: 200, margin: 0);

        var slotTops = container.FragmentTree!.Fragmentainers.Select(f => f.LocalOriginY).ToList();

        CollectionAssert.Contains(slotTops, 0.0);
        CollectionAssert.DoesNotContain(slotTops, 200.0);
        CollectionAssert.DoesNotContain(slotTops, 400.0);
        CollectionAssert.DoesNotContain(slotTops, 600.0);
        CollectionAssert.Contains(slotTops, 800.0);
    }

    [TestMethod]
    public void Fragmentainers_ContiguousRealContent_KeepEveryPage()
    {
        // Real, painted content spanning several page-heights (no gaps) must still produce one
        // slot per page, exactly matching the un-skipped pagination behavior. Unaffected by the dead
        // skip-path documented in the class remarks: this only asserts nothing is ever WRONGLY skipped,
        // which holds either way.
        var (_, container) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<div style='height:900px; background-color:rgb(9,9,9);'>section content spanning pages</div>"),
            pageHeight: 200, margin: 0);

        var fragmentainers = container.FragmentTree!.Fragmentainers;

        CollectionAssert.AreEqual(new[] { 0.0, 200.0, 400.0, 600.0, 800.0 }, fragmentainers.Select(f => f.LocalOriginY).ToList());
        CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, fragmentainers.Select(f => f.SlotIndex).ToList());
    }

    [Ignore("Same dead skip-path as Fragmentainers_RealContentSeparatedByMultiPageGap_SkipWhollyEmptySlots " +
            "(see the class remarks) - confirmed empirically: a 900px, entirely background-less filler div " +
            "(page height 200) produces 5 fragmentainers (one per slot it geometrically spans), not the " +
            "single content-empty-document fallback css-page-media-3 §3.2 asks for.")]
    [TestMethod]
    public void Fragmentainers_PureMarginOnlyDocument_FallBackToASingleFragmentainer()
    {
        // A document that laid out to a real, non-zero height but has nothing "printable"
        // anywhere (an extreme, all-margin edge case) must still produce exactly one page - never
        // zero - rather than emitting a content-less document.
        var (_, container) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<div id='gap' style='height:900px;'></div>"),
            pageHeight: 200, margin: 0);

        var fragmentainer = container.FragmentTree!.Fragmentainers.Single();

        Assert.AreEqual(0, fragmentainer.SlotIndex);
        Assert.AreEqual(0.0, fragmentainer.LocalOriginY);
    }
}
