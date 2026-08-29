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
/// Verifies a real, previously-undocumented gap found while auditing this port's fragmentation engine
/// against PeachPDF a second time (the first audit produced the R0-R10 plan; this is a later, separate
/// pass over what remained): <c>InlineFragmentation.ApplyLineBreaking</c>'s orphans merge-back correction
/// only ever ran once at least one earlier break already existed (<c>breaks.Count > 1</c>), which can
/// never be true while still deciding a paragraph's very FIRST run - so a paragraph starting close enough
/// to a page's bottom that fewer than <c>orphans</c> lines fit there was left with a too-small stranded
/// first fragment, uncorrected. Confirmed by temporarily reverting the fix and re-running this exact test:
/// it reliably reproduced a 1-line first page against <c>orphans:2</c> at several filler counts (13, 28,
/// 43, 58 - the same ~15-count period the page-height/line-height ratio produces).
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class OrphansOnFirstRunTest
{
    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    private static IEnumerable<(string Text, double Top)> AllTargetWords(BoxFragment f)
    {
        foreach (var w in f.Words)
            if (!w.Word.IsLineBreak && w.Word.Text.StartsWith("TargetLine"))
                yield return (w.Word.Text, w.Rect.Top);
        foreach (var c in f.Children)
            foreach (var x in AllTargetWords(c))
                yield return x;
    }

    [TestMethod]
    public async Task ParagraphStartingNearPageBottom_NeverStrandsFewerThanOrphansLines()
    {
        // Sweep filler counts rather than hardcoding one - this is a "just barely fits" calibration
        // (see this session's own established testing lesson), and the exact boundary depends on
        // font-metric arithmetic other changes are expected to keep touching.
        for (var fillerCount = 1; fillerCount < 60; fillerCount++)
        {
            using var wrapper = new HtmlContainer();
            var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line</p>", fillerCount));
            await wrapper.SetHtml(
                $"""
                <html><body>
                    {filler}
                    <p style="margin:0; orphans:2;">TargetLineOne TargetLineTwo TargetLineThree TargetLineFour TargetLineFive TargetLineSix TargetLineSeven TargetLineEight TargetLineNine TargetLineTen</p>
                </body></html>
                """);

            var container = GetInternal(wrapper);
            container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(200, 300);
            container.MarginTop = 0;
            wrapper.MaxSize = new SizeF(200, 0);

            using var bitmap = new Bitmap(200, 20000);
            using var g = Graphics.FromImage(bitmap);
            wrapper.PerformLayout(g);

            var tree = container.FragmentTree;
            Assert.IsNotNull(tree);

            var wordInfo = new List<(string Text, int Page, double Top)>();
            for (var pi = 0; pi < tree.Fragmentainers.Count; pi++)
                foreach (var w in AllTargetWords(tree.Fragmentainers[pi].Root))
                    wordInfo.Add((w.Text, pi, System.Math.Round(w.Top, 1)));

            if (wordInfo.Count == 0)
                continue; // paragraph didn't appear in this bitmap height at this filler count - try the next

            var firstPage = wordInfo[0].Page;
            var linesOnFirstPage = wordInfo.Where(w => w.Page == firstPage).Select(w => w.Top).Distinct().Count();

            Assert.IsGreaterThanOrEqualTo(2, linesOnFirstPage,
                $"at fillerCount={fillerCount}, the paragraph's first page-fragment kept only {linesOnFirstPage} line(s), fewer than orphans:2 requires");
        }
    }
}
