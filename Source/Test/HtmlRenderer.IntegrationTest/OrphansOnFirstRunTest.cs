using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Fragments;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest;

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
    [Spec("css21", "page.html#break-inside")]
    public async Task ParagraphStartingNearPageBottom_NeverStrandsFewerThanOrphansLines()
    {
        // Where the paragraph straddles depends on font metrics, so sweep instead of calibrating.
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
