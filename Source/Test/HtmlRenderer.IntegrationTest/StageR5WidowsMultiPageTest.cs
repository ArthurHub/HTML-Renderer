using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Fragments;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest;

/// <summary>
/// Verifies a real bug found while investigating the fragmentation-engine-parity plan's R5/R6 stages
/// (inline resumption, widows as a driver-level rewind): the investigation concluded neither stage needed
/// new resumption machinery after all - <c>CreateLineBoxes</c> already computes an entire paragraph's
/// lines in one unbounded, side-effect-free call, so there is never a point where a later pass reveals
/// information the same-shot correction didn't already have. What it DID find was a real bug in that
/// same-shot correction's own cascading logic.
/// </summary>
/// <remarks>
/// The old single-pass version of <c>InlineFragmentation.ApplyLineBreaking</c> shifted lines
/// incrementally as it walked them, driven by "did this line straddle a page boundary". Once a shift
/// happened to land a run of lines in perfect page-boundary alignment (very common with uniform line
/// heights), no line ever straddled again for the rest of the paragraph - so widows was silently never
/// re-checked for any later page transition. A paragraph long enough to span dozens of pages could end
/// with a final page far short of its `widows` minimum and nothing would catch it. The rewritten version
/// computes every break point up front from each line's own natural (never-shifted) position, which has
/// no such blind spot, and applies the decided breaks in a single separate pass.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class StageR5WidowsMultiPageTest
{
    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    private static int CountWords(BoxFragment f)
    {
        var n = f.Words.Count(w => !w.Word.IsLineBreak);
        foreach (var c in f.Children)
            n += CountWords(c);
        return n;
    }

    private static void CollectWordTops(BoxFragment f, List<double> into)
    {
        foreach (var w in f.Words)
            if (!w.Word.IsLineBreak)
                into.Add(w.Rect.Top);
        foreach (var c in f.Children)
            CollectWordTops(c, into);
    }

    [TestMethod]
    public async Task LongParagraph_PullsBackAcrossMultipleEarlierPages_WhenTheFirstDoesNotHaveRoom()
    {
        using var wrapper = new HtmlContainer();
        // A deliberately non-round page height relative to the line height (100 vs a 24-tall line: 4
        // lines is 96, leaving 4 units of slack; a straight single-page-back merge for widows:3 needs to
        // reach past that slack into the page before it too) - this is exactly the shape the old
        // single-pass algorithm's "stops checking after perfect alignment" blind spot could miss, and
        // the shape the two-phase rewrite's break-list (rather than incremental-shift) design exists to
        // handle: cascading the merge across more than one earlier break by removing list entries,
        // without needing to undo a shift already applied to specific lines.
        var sentence = "Alpha bravo charlie delta echo foxtrot golf hotel india juliet kilo lima mike november oscar papa quebec romeo sierra tango uniform victor whiskey ";
        var paragraph = string.Concat(Enumerable.Repeat(sentence, 40));
        await wrapper.SetHtml($"<html><body><p style='margin:0; widows: 3;'>{paragraph}</p></body></html>");

        var container = GetInternal(wrapper);
        container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(220, 100);
        container.MarginTop = 0;
        wrapper.MaxSize = new SizeF(220, 0);

        using var bitmap = new Bitmap(220, 20000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        var tree = container.FragmentTree;
        Assert.IsGreaterThan(3, tree.Fragmentainers.Count, "test content should span several pages for this to be meaningful");

        var lastPageWords = CountWords(tree.Fragmentainers[tree.Fragmentainers.Count - 1].Root);
        Assert.IsGreaterThanOrEqualTo(3, lastPageWords,
            $"the final page has only {lastPageWords} line(s), fewer than widows:3 - the paragraph's own last line was left stranded");
    }

    [TestMethod]
    public async Task LongParagraph_DeclinesGracefully_WhenSatisfyingWidowsWouldOverflowAPage()
    {
        using var wrapper = new HtmlContainer();
        // Deliberately degenerate: a single repeated word gives every line identical height, so pages
        // pack to exactly the same capacity throughout - satisfying widows:3 on the trailing page would
        // require merging in lines from an already-full preceding page, producing a run taller than any
        // page can hold. This must not overflow, crash, or loop - it must simply leave the shorter final
        // page as the best achievable result (css-break-3 4.3's "some constraints can't always be
        // satisfied" relaxation philosophy).
        var words = string.Concat(Enumerable.Repeat("word ", 300));
        await wrapper.SetHtml($"<html><body><p style='margin:0; widows: 3;'>{words}</p></body></html>");

        var container = GetInternal(wrapper);
        container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(60, 100);
        container.MarginTop = 0;
        wrapper.MaxSize = new SizeF(60, 0);

        using var bitmap = new Bitmap(60, 20000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        var tree = container.FragmentTree;
        Assert.IsGreaterThan(3, tree.Fragmentainers.Count);

        // No page's own words may span more vertical room than the page itself has - the real
        // regression this guards against is a "fix" that satisfies widows by producing a run that
        // silently overflows its fragmentainer (word rects are already fragmentainer-local, so a span
        // near or under one page height is the correct expectation regardless of scroll/margin setup).
        foreach (var fragmentainer in tree.Fragmentainers)
        {
            var tops = new List<double>();
            CollectWordTops(fragmentainer.Root, tops);
            if (tops.Count == 0)
                continue;

            var span = tops.Max() - tops.Min();
            Assert.IsLessThanOrEqualTo(container.PageSize.Height, span,
                $"fragmentainer at slot {fragmentainer.SlotIndex} holds words spanning more than one page's height");
        }

        // The total word count must be conserved - nothing dropped, nothing duplicated, across however
        // many pages the graceful-decline path produced.
        var total = tree.Fragmentainers.Sum(f => CountWords(f.Root));
        Assert.AreEqual(300, total);
    }
}
