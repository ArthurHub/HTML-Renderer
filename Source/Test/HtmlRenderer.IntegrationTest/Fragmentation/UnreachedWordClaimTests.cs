using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragments;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest.Fragmentation;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/UnreachedWordClaimTests.cs: #374's workhorse invariant - every
/// word the document authored is claimed by exactly one fragment - checked over several shapes that each
/// reach line-building by their own route (an inline box, a float, a list item), plus the specific symptom
/// PeachPDF's own bug (#433) produced, that the first page's fragment claimed words far past what it shows.
/// </summary>
/// <remarks>
/// PeachPDF's bug was that an unpositioned word (still at its zero-initialized rectangle) fell inside the
/// FIRST slot's own band, so pagination claimed it there. That specific failure mode does not exist in this
/// port's architecture - <c>HtmlContainerInt.PerformLayout</c> runs the whole document's flow to completion,
/// positioning every word, before <c>FragmentEmitter.Finish</c> ever walks the tree (see
/// <c>FragmentEmitter</c>'s own doc comment: "layout already positions every box correctly across however
/// many pages the document spans... so unlike PeachPDF's pass-based emitter, this one does not need to
/// collect per-pass output"). These tests are ported anyway as a direct regression pin of the underlying
/// invariant on this port's own (different) mechanism - <c>FragmentEmitter.Overlaps</c>, a strict per-band
/// rectangle overlap with no tolerance (PeachPDF's own <c>BandMembershipToleranceTests</c> is dropped
/// entirely from this port - documented in this batch's commit message - for why "no tolerance" does not
/// also imply a double-claim risk here: <c>InlineFragmentation.ApplyLineBreaking</c>'s break decisions and
/// this same overlap test both derive from the same <c>PageTopOf</c>/<c>PageBottomOf</c> arithmetic via one
/// uniform per-run shift, not two independently-rounded fitting tests, so there is no separate computation
/// left for the emitter to disagree with).
/// <para>
/// One PeachPDF theory row is dropped: <c>column-count:2</c>, since HTML-Renderer has no multi-column engine
/// (out of scope for this whole porting effort, per the plan's general exclusion list).
/// </para>
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class UnreachedWordClaimTests
{
    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    private static async Task<(CssBox Root, HtmlContainerInt Container)> BuildAsync(
        string bodyHtml, double pageHeight = 850, double pageWidth = 600, int marginTop = 10)
    {
        var wrapper = new HtmlContainer();
        await wrapper.SetHtml($"<html><body style='margin:0'>{bodyHtml}</body></html>");

        var container = GetInternal(wrapper);
        container.PageSize = new RSize(pageWidth, pageHeight);
        container.MarginTop = marginTop;
        container.Location = new RPoint(0, marginTop);
        wrapper.MaxSize = new SizeF((float)pageWidth, 0);

        using var bitmap = new Bitmap((int)pageWidth, 200000);
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

    /// <summary>
    /// Every word the document authored, including list-item markers (<see cref="CssBox.ListItemBox"/> - a
    /// field kept separate from <see cref="CssBox.Boxes"/>, so a plain <see cref="Walk"/> alone misses it).
    /// </summary>
    private static List<CssRect> WordsIn(CssBox box) =>
        Walk(box).SelectMany(b => b.Words)
            .Concat(Walk(box).Where(b => b.Display == CssConstants.ListItem && b.ListItemBox != null)
                .SelectMany(b => b.ListItemBox.Words))
            .ToList();

    private static List<CssRect> ClaimedWords(HtmlContainerInt container) =>
        container.FragmentTree!.Fragmentainers
            .SelectMany(f => Flatten(f.Root))
            .SelectMany(f => f.Words)
            .Select(w => w.Word)
            .ToList();

    private static string DescribeDoubleClaims(HtmlContainerInt container)
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

        var doubled = claims.Where(c => c.Value.Count > 1).ToList();
        return $"{doubled.Count} words claimed more than once: " + string.Join("; ", doubled
            .Take(8)
            .Select(c => $"'{c.Key.Text}' by [{string.Join(",", c.Value)}], lives in "
                         + container.PageIndexOf(c.Key.Top)));
    }

    private static string Document(string template, int wordCount) =>
        $"<html><body style='margin:0'>{template.Replace("{F}", string.Join(" ", System.Linq.Enumerable.Range(0, wordCount).Select(i => $"w{i}")))}</body></html>";

    /// <summary>
    /// #374's workhorse invariant, over the whole document: every word the document authored is claimed by
    /// exactly one fragment. It fails one way if a fragment claims a word another one also holds, and the
    /// other way if a word is dropped entirely. Asked of several shapes because what stops is the fill
    /// rather than the paragraph: an inline box, a float and a list item each reach line-building by their
    /// own route.
    /// </summary>
    [TestMethod]
    [DataRow("<p style='orphans:1;widows:1;font-size:10px;line-height:20px'>{F}</p>")]
    [DataRow("<p style='orphans:1;widows:1;font-size:10px;line-height:20px'>{F} <b>bold words carried across the break</b> {F}</p>")]
    [DataRow("<p style='orphans:1;widows:1;font-size:10px;line-height:20px'><span style='color:red'>{F}</span></p>")]
    [DataRow("<div style='orphans:1;widows:1;font-size:10px;line-height:20px'>{F}<span style='float:left;width:40px'>fl oa ted</span>{F}</div>")]
    [DataRow("<ul><li style='orphans:1;widows:1;font-size:10px;line-height:20px'>{F}</li></ul>")]
    public async Task AParagraphSplitAtAPageBoundary_ClaimsEveryWordExactlyOnce(string template)
    {
        var (root, container) = await BuildAsync(Document(template, 2500));

        Assert.IsTrue(container.FragmentTree!.Fragmentainers.Count > 1,
            "the fixture must span more than one page");

        var authored = WordsIn(root);
        var claimed = ClaimedWords(container);

        Assert.IsTrue(authored.Count > 0);
        Assert.AreEqual(
            claimed.Count,
            claimed.Distinct(ReferenceEqualityComparer.Instance).Count(),
            DescribeDoubleClaims(container));
        Assert.AreEqual(authored.Count, claimed.Count);
    }

    /// <summary>
    /// The symptom PeachPDF's #433 stated concretely: the first page's own text layer holds only the words
    /// that page shows.
    /// </summary>
    [TestMethod]
    public async Task TheFirstPage_ClaimsOnlyTheWordsItShows()
    {
        var (root, container) = await BuildAsync(Document(
            "<p style='orphans:1;widows:1;font-size:10px;line-height:20px'>{F}</p>", 3000));

        var fragmentainers = container.FragmentTree!.Fragmentainers;
        Assert.IsTrue(fragmentainers.Count > 1, "the fixture must span more than one page");

        var onFirstPage = Flatten(fragmentainers[0].Root).SelectMany(f => f.Words).ToList();
        var authored = WordsIn(root).Count;

        Assert.IsTrue(onFirstPage.Count > 0);
        Assert.IsTrue(onFirstPage.Count < authored,
            $"the first page claimed {onFirstPage.Count} of the document's {authored} words");

        // Stated from the page grid rather than from any internal flag, so it is an independent statement
        // of the symptom: every word this page claims really does sit in this page's band.
        Assert.IsTrue(onFirstPage.All(w => container.PageIndexOf(w.Word.Top) == 0));
    }

    /// <summary>
    /// A list whose items each fit on one line still needs every marker claimed - an outside marker
    /// (<see cref="CssBox.ListItemBox"/>) is positioned by the item's own layout epilogue
    /// (<c>CssBox.CreateListItemBox</c>), not by the ordinary inline flow, so this asks the claim invariant
    /// of a box type the paragraph-shaped fixtures above never exercise.
    /// </summary>
    [TestMethod]
    public async Task AListWhoseItemsDoNotBreak_StillClaimsEveryMarker()
    {
        var items = string.Join("", System.Linq.Enumerable.Range(0, 200)
            .Select(i => $"<li style='font-size:10px;line-height:20px'>item {i} of the list</li>"));
        var (root, container) = await BuildAsync($"<ul>{items}</ul>");

        Assert.IsTrue(container.FragmentTree!.Fragmentainers.Count > 1,
            "the fixture must span more than one page");

        var markerWords = Walk(root)
            .Where(b => b.Display == CssConstants.ListItem)
            .Select(b => b.ListItemBox)
            .Where(m => m != null)
            .SelectMany(m => m.Words)
            .ToList();

        Assert.IsTrue(markerWords.Count > 0);

        var claimed = new HashSet<CssRect>(ClaimedWords(container), ReferenceEqualityComparer.Instance);

        Assert.IsTrue(markerWords.All(w => claimed.Contains(w)));
    }
}
