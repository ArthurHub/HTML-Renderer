using System.Collections.Generic;
using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragments;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest.Fragmentation;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/BlockContentListMarkerTests.cs. PeachPDF's root defect
/// (<c>CssBox.LayoutOutsideMarker</c> re-parenting a block-content item's marker into the anonymous block its
/// inline run needs, then scanning only <i>direct</i> children to find it again) does not exist by
/// construction here: HTML-Renderer's marker is <see cref="CssBox.ListItemBox"/>, a field entirely separate
/// from <see cref="CssBox.Boxes"/> - it is never wrapped in an anonymous block regardless of whether the
/// item's content is inline or block-level, so every PeachPDF test asserting that structural fact
/// (<c>AnOutsideMarker_IsNotWrappedInTheItemsAnonymousBlock</c>), the resulting mispositioned content
/// (<c>AnItemWhoseContentIsBlockLevel_LaysThatContentOutBelowTheItemsTop</c>), or the marker's own screen
/// position (<c>AnItemWhoseContentIsBlockLevel_PositionsItsMarkerLikeAnInlineOne</c>,
/// <c>AnItemMixingInlineAndBlockContent_KeepsItsAnonymousBlockAndItsMarker</c>) is general list-layout
/// correctness unrelated to pagination, not fragment-claiming - out of scope for this porting batch per the
/// plan's own instruction to port only the claiming-relevant half of this file.
/// <c>AnInsideMarker_IsStillWrappedWithTheItemsInlineRun</c> is dropped outright:
/// <c>list-style-position</c> is parsed and stored (<c>CssBoxProperties.ListStylePosition</c>) but never
/// consulted by <c>CssBox.CreateListItemBox</c> - confirmed by reading it in full - so this port has no
/// "inside" marker rendering mode at all, matching the parse-only-stub precedent already established for
/// other CSS properties this porting effort has found (e.g. the <c>page</c> property in Batch 2).
/// <c>AnItemWhoseKeptContentCarriesNoWords_KeepsItsMarkerWhereItBegins</c> is dropped: it requires a real
/// multi-column engine (<c>column-count</c>), out of scope for this whole porting effort.
/// <para>
/// The 2 tests that remain are genuinely about fragment-claiming: whether a block-content item's marker is
/// claimed by a fragment at all (single-page - the base case PeachPDF's bug broke completely, drawing the
/// marker on <i>no</i> page), and whether the item travels whole with its marker across a real forced page
/// break (multi-page - the pagination-relevant half of PeachPDF's own file). The second is ported but
/// <c>[Ignore]</c>d: it hits a different, confirmed gap (forced-break ancestor propagation, not the marker
/// mechanism) - see its own remarks below.
/// </para>
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class BlockContentListMarkerTests
{
    private const string BlockItemList =
        "<ol style='margin:0;padding-left:40px'>"
        + "<li id='block'><p style='margin:0'>block content here</p></li>"
        + "<li id='inline'>inline content</li></ol>";

    /// <summary>
    /// The fragment-tree statement of PeachPDF's own symptom: a marker no fragment claims is the state paint
    /// reads when it draws nothing. Single page - this is the base case, not a pagination scenario.
    /// </summary>
    [TestMethod]
    public void AnItemWhoseContentIsBlockLevel_HasItsMarkerClaimedExactlyOnce()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(BlockItemList));

        var claims = ClaimsByWord(container);

        foreach (var item in ListItems(root))
        {
            var marker = item.ListItemBox;
            Assert.IsNotNull(marker, $"'{Id(item)}' has no marker box");
            var word = marker!.Words.Single();

            Assert.IsTrue(claims.TryGetValue(word, out var slots),
                $"the marker of '{Id(item)}' is claimed by no fragment at all");
            Assert.AreEqual(1, slots!.Count);
        }
    }

    /// <summary>
    /// An item whose only content asks to start on the next page has nothing to keep on the page it was
    /// declined on, so css-break-3 §3.1 moves the whole item - "a break before a container's own first
    /// in-flow child is the break point before the container".
    /// </summary>
    /// <remarks>
    /// Confirmed NOT to reproduce here, for a reason unrelated to the marker mechanism this file is
    /// otherwise about: <c>BlockFragmentation.TryGetForcedBreakTarget</c>'s own doc comment documents a
    /// deliberate scope limit - "suppressed when there's no previous sibling", since full css-break-3 §3.1
    /// ancestor propagation (a forced break with no previous sibling really belongs to the nearest ancestor
    /// that HAS one) is out of scope for this port. Here <c>p</c> (the <c>break-before:page</c> box) is
    /// <c>li</c>'s only child, so <c>prevSibling == null</c> for <c>p</c> itself and the forced break is
    /// suppressed outright - it never even reaches the point where <c>li</c> (which DOES have a previous
    /// sibling, the earlier <c>&lt;p&gt;before&lt;/p&gt;</c>) could inherit it. Confirmed by running this
    /// test unignored: the whole document stays on one page, never spanning more than one fragmentainer at
    /// all. This is the same confirmed gap as this port's own <c>PageBreakIntegrationTests</c> remarks call
    /// out for margin-truncation propagation - here it blocks a forced break instead.
    /// </remarks>
    [TestMethod]
    [Ignore("Confirmed gap: BlockFragmentation.TryGetForcedBreakTarget suppresses a forced break-before " +
        "entirely when the box has no previous sibling (full css-break-3 §3.1 ancestor propagation - " +
        "letting a nested first-in-flow-child's break become its own parentless ancestor's - is out of " +
        "scope for this port, per that method's own doc comment). Here <p style='break-before:page'> is " +
        "<li>'s only child, so the break is suppressed before it could ever reach <li> (which does have a " +
        "previous sibling). Confirmed by running this test unignored: the document never spans more than " +
        "one fragmentainer at all.")]
    public void AnItemDeferredBeforeItsContentWasEverFlowed_TravelsWholeWithItsMarker()
    {
        var html = PaintHarness.Wrap(
            "<p style='margin:0'>before</p>"
            + "<ol style='margin:0;padding-left:40px'>"
            + "<li id='deferred'><p style='margin:0;break-before:page'>content here</p></li></ol>");

        var (root, container) = PaintHarness.LayoutPaginated(html, pageHeight: 850, margin: 10);

        var item = PaintHarness.FindById(root, "deferred")!;
        var word = item.ListItemBox!.Words.Single();
        var fragments = FragmentsOf(container, item);

        Assert.IsTrue(container.FragmentTree!.Fragmentainers.Count > 1,
            "the fixture must span more than one page");

        // One fragment, on the page the item's content is on - no stub left behind on the page it was
        // declined on, and the marker with it.
        Assert.AreEqual(1, fragments.Count);
        var fragment = fragments[0];

        Assert.IsTrue(fragment.FragmentainerIndex > 0);
        Assert.IsNotNull(fragment.MarkerFragment);
        var claims = ClaimsByWord(container);
        Assert.AreEqual(1, claims[word].Count);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static IEnumerable<CssBox> Walk(CssBox box)
    {
        yield return box;
        foreach (var b in box.Boxes)
            foreach (var d in Walk(b))
                yield return d;
    }

    private static IEnumerable<BoxFragment> Flatten(BoxFragment fragment)
    {
        yield return fragment;
        foreach (var child in fragment.Children)
            foreach (var d in Flatten(child))
                yield return d;
        if (fragment.MarkerFragment != null)
            foreach (var d in Flatten(fragment.MarkerFragment))
                yield return d;
    }

    private static List<CssBox> ListItems(CssBox root) =>
        Walk(root).Where(b => b.Display == CssConstants.ListItem).ToList();

    private static string? Id(CssBox box) => box.HtmlTag?.TryGetAttribute("id");

    private static List<BoxFragment> FragmentsOf(HtmlContainerInt container, CssBox box) =>
        container.FragmentTree!.Fragmentainers
            .SelectMany(f => Flatten(f.Root))
            .Where(f => ReferenceEquals(f.Box, box))
            .ToList();

    private static Dictionary<CssRect, List<int>> ClaimsByWord(HtmlContainerInt container)
    {
        var claims = new Dictionary<CssRect, List<int>>(ReferenceEqualityComparer.Instance);
        foreach (var fragmentainer in container.FragmentTree!.Fragmentainers)
        {
            foreach (var word in Flatten(fragmentainer.Root).SelectMany(f => f.Words))
            {
                if (!claims.TryGetValue(word.Word, out var slots))
                    claims[word.Word] = slots = new List<int>();
                slots.Add(fragmentainer.SlotIndex);
            }
        }
        return claims;
    }
}
