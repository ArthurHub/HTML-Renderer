using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragments;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest.Fragmentation;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/StraddlingLineClaimTests.cs: content taller than the whole page
/// band has nowhere to go (css-break-3 §2's "content too large for any fragment" case), so layout leaves it
/// exactly where it is - and every band it geometrically covers must still claim it. Maps to
/// <c>FragmentEmitter.Overlaps</c> (a strict rect/band overlap test, applied independently per band with no
/// special case for oversized content, so an oversized word is claimed by construction rather than through
/// any dedicated "straddling" logic).
/// </summary>
/// <remarks>
/// Only 1 of PeachPDF's 3 tests ports:
/// <list type="bullet">
/// <item><c>ContentAfterAWordTallerThanTheBand_SeesATruthfulCursor</c> is dropped: it asserts
/// <c>HtmlContainerInt.CursorSpills</c> stays zero, a counter belonging to PeachPDF's own per-pass document
/// cursor. Confirmed by reading <c>Core/Fragmentation/InlineFragmentation.cs</c> and
/// <c>Core/Fragmentation/BlockFragmentation.cs</c> in full plus <c>HtmlContainerInt.cs</c>: this port has no
/// such cursor at all (layout runs the whole document's flow in one pass; there is nothing for a "stale
/// cursor after an oversized word" bug to corrupt).</item>
/// <item><c>ARowOrLineTheEngineCouldNotFit_ContinuesOnTheNextPageInstead</c> is dropped: all 3
/// <c>[InlineData]</c> rows use <c>display:grid</c>/<c>display:flex</c>, neither of which exists in
/// HTML-Renderer - <c>MonolithicContent.RunsAnEngineOfItsOwn</c>'s own doc comment confirms this port's
/// <c>PaginatesItsOwnContent</c> narrows to table/inline-table only, matching the general flex/grid
/// exclusion already established for this porting effort.</item>
/// </list>
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class StraddlingLineClaimTests
{
    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    private static async Task<(CssBox Root, HtmlContainerInt Container)> BuildAsync(
        string bodyHtml, double pageHeight = 842, double pageWidth = 600, int marginTop = 10)
    {
        var wrapper = new HtmlContainer();
        await wrapper.SetHtml($"<html><body style='margin:0'>{bodyHtml}</body></html>");

        var container = GetInternal(wrapper);
        container.PageSize = new RSize(pageWidth, pageHeight);
        container.MarginTop = marginTop;
        container.Location = new RPoint(0, marginTop);
        wrapper.MaxSize = new SizeF((float)pageWidth, 0);

        using var bitmap = new Bitmap((int)pageWidth, 60000);
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
    }

    private static List<int> SlotsClaiming(HtmlContainerInt container, CssRect word) =>
        container.FragmentTree!.Fragmentainers
            .Where(f => Flatten(f.Root).SelectMany(b => b.Words).Any(w => ReferenceEquals(w.Word, word)))
            .Select(f => f.SlotIndex)
            .ToList();

    /// <summary>
    /// A word taller than the whole band - the one production mechanism left that leaves a word straddling
    /// by more than a hairline: content with nowhere to fit must not be treated as breakable (moving it only
    /// repeats the problem forever), so layout leaves it exactly where it naturally lands, covering more than
    /// one band by construction. The word's own rectangle straddles here because its height comes from the
    /// font rather than from <c>line-height</c> - hence an enormous <c>font-size</c> rather than an enormous
    /// leading, which would grow the line box while leaving the word itself small enough to fit.
    /// </summary>
    [TestMethod]
    public async Task AWordTallerThanTheBand_IsClaimedByEveryBandItCovers()
    {
        var (root, container) = await BuildAsync("<p style='font-size:1800px;line-height:1;margin:0'>T</p>");

        var word = Walk(root).SelectMany(b => b.Words).Single(w => w.Text == "T");
        var band = container.PageIndexOf(word.Top);

        Assert.IsTrue(word.Height > container.PageBottomOf(band) - container.PageTopOf(band),
            $"the fixture must produce a word taller than the band, not {word.Height}");

        // Every band the word covers, from the grid's own materialized fragmentainers - "claimed by band +
        // 1" alone would still pass if a taller word silently lost the bands below its second.
        var covered = container.FragmentTree!.Fragmentainers
            .Select(f => f.SlotIndex)
            .Where(slot => word.Bottom > container.PageTopOf(slot) && word.Top < container.PageBottomOf(slot))
            .ToList();

        Assert.IsTrue(covered.Count > 2, $"the fixture must span more than two bands, not {covered.Count}");
        CollectionAssert.AreEqual(covered, SlotsClaiming(container, word));
    }
}
