using System;
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
/// Ported from PeachPDF.Tests/Integration/StraddlingListMarkerTests.cs: an outside <c>::marker</c> belongs to
/// the fragmentainer its list item BEGINS in, settled the moment the item is placed.
/// </summary>
/// <remarks>
/// <para>
/// HTML-Renderer's marker is architecturally different from PeachPDF's independently-positioned
/// <c>::marker</c> box: it is <see cref="CssBox.ListItemBox"/>, a single field built once by
/// <c>CssBox.CreateListItemBox</c> at the very end of the item's own <c>PerformLayoutImp</c> and
/// repositioned - every time that method runs - directly against the item's own, current
/// <c>Location</c>/<c>ActualPaddingTop</c> (never against a per-pass "epilogue" separate from the item's own
/// placement). PeachPDF's bug (#444) was staleness between when the marker was positioned and which pass
/// completed the item; that specific failure mode does not exist here, since there is only ever one marker-
/// positioning statement and it always runs against the item's own truth. These tests are still ported: they
/// pin the same observable invariant (a marker belongs to the fragmentainer its item begins in, exactly once)
/// as a regression check on this port's own, structurally different mechanism.
/// </para>
/// <para>
/// 3 of PeachPDF's 8 tests are dropped - <c>AnItemCrossingAColumnBoundary_KeepsItsMarkerInTheColumnItBeginsIn</c>,
/// <c>AListWhoseItemsCrossColumnBoundaries_ClaimsEveryWordExactlyOnce</c> and
/// <c>AnItemAColumnPlacedButKeptNothingOf_StillClaimsItsMarkerExactlyOnce</c> all require a real multi-column
/// engine (<c>column-count</c>/<c>column-fill:balance</c> producing one fragmentainer per column). HTML-Renderer
/// has no such engine - out of scope for this whole porting effort, per the plan's general exclusion list.
/// </para>
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class StraddlingListMarkerTests
{
    private const string ItemStyle = "margin:0;font-size:10px;line-height:20px;orphans:1;widows:1";

    /// <summary>
    /// #374's claimed-exactly-once invariant, over the whole document. A marker is a thing that can be
    /// claimed <i>zero</i> times, which is the direction a duplicate-only check would miss.
    /// </summary>
    [TestMethod]
    public void AListItemStraddlingAPageBoundary_ClaimsEveryWordExactlyOnce()
    {
        var (root, container) = Layout();

        AssertSomeItemStraddles(root, container);

        var authored = AllWords(root);
        var claims = ClaimsByWord(container);

        Assert.IsTrue(authored.Count > 0);
        foreach (var w in authored)
        {
            Assert.IsTrue(claims.TryGetValue(w, out var slots) && slots.Count == 1,
                $"'{w.Text}' is claimed by [{(claims.TryGetValue(w, out var s) ? string.Join(",", s) : "")}]");
        }
        Assert.AreEqual(authored.Count, claims.Count);
    }

    /// <summary>
    /// The same statement narrowed to the markers, which is where PeachPDF's bug failed it: every item's
    /// marker is claimed, and by the fragmentainer the item's own first fragment is in.
    /// </summary>
    [TestMethod]
    public void AStraddlingItemsMarker_IsClaimedByTheFragmentainerItsItemBeginsIn()
    {
        var (root, container) = Layout();

        var straddler = AssertSomeItemStraddles(root, container);
        var claims = ClaimsByWord(container);

        foreach (var item in ListItems(root))
        {
            var marker = item.ListItemBox;
            Assert.IsNotNull(marker, $"'{Id(item)}' has no marker box");
            var word = marker!.Words.Single();

            Assert.IsTrue(claims.TryGetValue(word, out var slots),
                $"the marker of '{Id(item)}' is claimed by no fragment at all");
            CollectionAssert.AreEqual(new[] { SlotsOf(container, item).First() }, slots);
        }

        Assert.IsTrue(SlotsOf(container, straddler).Count > 1);
    }

    /// <summary>
    /// The visible symptom, asked of the paint calls themselves: a lost marker is not a mispositioned
    /// bullet, it is a bullet that is never drawn on any page. Numbered so each marker is identifiable in the
    /// log by its own text.
    /// </summary>
    [TestMethod]
    public void EveryMarker_IsDrawnOnExactlyOnePage()
    {
        var (root, container) = Layout(listStyleType: "decimal");

        AssertSomeItemStraddles(root, container);

        var drawn = new List<string>();
        for (var page = 0; page < container.FragmentTree!.Fragmentainers.Count; page++)
        {
            var g = PaintHarness.PaintPage(container, page);
            drawn.AddRange(g.DrawStringCalls.Select(c => c.Text));
        }

        foreach (var item in ListItems(root))
        {
            var label = item.ListItemBox!.Words.Single().Text;
            Assert.AreEqual(1, drawn.Count(t => t == label));
        }
    }

    /// <summary>
    /// The fix's shape, restated positively: the marker still sits against the item's own border box
    /// (CSS 2.1 §12.5.1), for an item that breaks exactly as for one that does not.
    /// </summary>
    [TestMethod]
    public void AMarkerSitsAgainstItsItemsBorderBox_WhetherOrNotTheItemBreaks()
    {
        var (root, container) = Layout();

        var straddler = AssertSomeItemStraddles(root, container);
        var offsets = new List<double>();

        foreach (var item in ListItems(root))
        {
            var marker = item.ListItemBox!;
            var word = marker.Words.Single();

            Assert.IsTrue(word.Top >= item.Location.Y && word.Top <= item.Location.Y + item.ActualLineHeight,
                $"marker of '{Id(item)}' is not beside its item's first line");
            Assert.IsTrue(word.Right <= item.ClientLeft + 0.001,
                $"the marker of '{Id(item)}' overlaps its item's content edge");

            offsets.Add(word.Top - item.Location.Y);
        }

        // The straddling item's marker is offset from its own item exactly as every other item's is - the
        // statement that it was not positioned against something else.
        Assert.AreEqual(1, offsets.Select(o => Math.Round(o, 3)).Distinct().Count());
        Assert.IsTrue(ListItems(root).Contains(straddler));
    }

    /// <summary>
    /// A pass that <i>declines</i> to place the item - css-break-3 §5.2's margin truncation concluding the
    /// break falls before it - has written no position for the marker to sit against until the item is
    /// actually placed on its real page; the claim still stands exactly once there.
    /// </summary>
    [TestMethod]
    public void AnItemWhoseFirstPassDeclinedToPlaceIt_StillClaimsItsMarkerExactlyOnce()
    {
        var html = PaintHarness.Wrap(
            "<ul style='margin:0;padding-left:40px'>"
            + $"<li id='first' style='{ItemStyle}'>first item</li>"
            + $"<li id='pushed' style='{ItemStyle};margin-top:900px'>pushed by its own margin</li></ul>");

        var (root, container) = PaintHarness.LayoutPaginated(html, pageHeight: 850, margin: 10);

        var pushed = PaintHarness.FindById(root, "pushed")!;
        var claims = ClaimsByWord(container);

        Assert.IsTrue(container.FragmentTree!.Fragmentainers.Count > 1,
            "the fixture must span more than one page");
        Assert.IsTrue(SlotsOf(container, pushed).First() > 0,
            "the pushed item must land on a later page than the one it was declined on");

        var word = pushed.ListItemBox!.Words.Single();

        Assert.IsTrue(claims.TryGetValue(word, out var slots),
            "the pushed item's marker is claimed by no fragment at all");
        CollectionAssert.AreEqual(new[] { SlotsOf(container, pushed).First() }, slots);
    }

    // ── Fixtures/helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Three items, the middle one long enough to run over several pages, so exactly one of them straddles
    /// - guaranteed by word count rather than hoped for from platform font metrics (this harness's
    /// deterministic <c>MockAdapter</c> metrics make it so regardless).
    /// </summary>
    private static (CssBox Root, HtmlContainerInt Container) Layout(string listStyleType = "disc")
    {
        var items = string.Join("", new[] { 12, 1200, 12 }.Select((words, i) =>
            $"<li id='li{i}' style='{ItemStyle}'>"
            + string.Join(" ", Enumerable.Range(0, words).Select(w => $"i{i}w{w}"))
            + "</li>"));

        var html = PaintHarness.Wrap(
            $"<ul style='margin:0;padding-left:40px;list-style-type:{listStyleType}'>{items}</ul>");

        return PaintHarness.LayoutPaginated(html, pageHeight: 850, margin: 10);
    }

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

    /// <summary>Every word the document authored, including list-item markers (<see cref="CssBox.ListItemBox"/>
    /// - a field kept separate from <see cref="CssBox.Boxes"/>, so a plain <see cref="Walk"/> alone misses it).</summary>
    private static List<CssRect> AllWords(CssBox root) =>
        Walk(root).SelectMany(b => b.Words)
            .Concat(ListItems(root).Where(li => li.ListItemBox != null).SelectMany(li => li.ListItemBox.Words))
            .ToList();

    private static string? Id(CssBox box) => box.HtmlTag?.TryGetAttribute("id");

    /// <summary>The pagination slots <paramref name="box"/> produced a fragment in, in order.</summary>
    private static List<int> SlotsOf(HtmlContainerInt container, CssBox box) =>
        container.FragmentTree!.Fragmentainers
            .Where(f => Flatten(f.Root).Any(x => ReferenceEquals(x.Box, box)))
            .Select(f => f.SlotIndex)
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

    /// <summary>The fixture's precondition, returned so a test can name the item it is really about.</summary>
    private static CssBox AssertSomeItemStraddles(CssBox root, HtmlContainerInt container)
    {
        Assert.IsTrue(container.FragmentTree!.Fragmentainers.Count > 1,
            "the fixture must span more than one page");

        var straddler = ListItems(root).FirstOrDefault(item => SlotsOf(container, item).Count > 1);
        Assert.IsNotNull(straddler);
        return straddler!;
    }
}
