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
    [Spec("css21", "page.html#break-inside")]
    public async Task LongParagraph_PullsBackAcrossMultipleEarlierPages_WhenTheFirstDoesNotHaveRoom()
    {
        using var wrapper = new HtmlContainer();
        // Page height 100 against a 24px line leaves 4px of slack per page, every page break lines up.
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

        var lastPageTops = new List<double>();
        CollectWordTops(tree.Fragmentainers[tree.Fragmentainers.Count - 1].Root, lastPageTops);
        var lastPageLines = lastPageTops.Distinct().Count();
        Assert.IsGreaterThanOrEqualTo(3, lastPageLines,
            $"the final page has only {lastPageLines} line(s), fewer than widows:3");
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task LongParagraph_DeclinesGracefully_WhenSatisfyingWidowsWouldOverflowAPage()
    {
        using var wrapper = new HtmlContainer();
        // Every page is full, so widows:3 on the last page can only be met by overflowing the one before.
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

        // Word rects are fragmentainer-local.
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

        var total = tree.Fragmentainers.Sum(f => CountWords(f.Root));
        Assert.AreEqual(300, total);
    }
}
