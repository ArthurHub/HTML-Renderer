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
/// The marker of a list item with block-level content is claimed by exactly one fragment.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class BlockContentListMarkerTests
{
    private const string BlockItemList =
        "<ol style='margin:0;padding-left:40px'>"
        + "<li id='block'><p style='margin:0'>block content here</p></li>"
        + "<li id='inline'>inline content</li></ol>";

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

    // The forced break on the item's only child is the break before the item.
    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "page.html#forced")]
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
