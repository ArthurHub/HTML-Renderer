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
/// A <c>break-after:avoid</c> chain taller than a page can't be kept together, the avoids get dropped.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class StageR9OversizedKeepWithNextRunTest
{
    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    private static IEnumerable<string> AllWords(BoxFragment f)
    {
        foreach (var w in f.Words)
            if (!w.Word.IsLineBreak)
                yield return w.Word.Text;
        foreach (var c in f.Children)
            foreach (var w in AllWords(c))
                yield return w;
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
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task LongAvoidChainTallerThanOnePage_NeverCorruptsGeometry_AndLosesNothing()
    {
        using var wrapper = new HtmlContainer();
        var runMembers = string.Concat(Enumerable.Range(0, 60).Select(i =>
            $"<p style='margin:0; break-after: avoid;'>RunMember{i} filler filler filler filler filler</p>"));
        await wrapper.SetHtml(
            $"""
            <html><body>
                <div style="margin:0;">TopMarker</div>
                {runMembers}
                <p style="margin:0;">FinalParagraph</p>
            </body></html>
            """);

        var container = GetInternal(wrapper);
        container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(595, 400);
        container.MarginTop = 20;
        wrapper.MaxSize = new SizeF(595, 0);

        using var bitmap = new Bitmap(595, 20000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        var tree = container.FragmentTree;
        Assert.IsNotNull(tree);

        // The exact page count depends on font metrics.
        Assert.IsLessThan(100_000.0, wrapper.ActualSize.Height, "document height must stay sane - not blow up from compounding OffsetTop shifts");
        Assert.IsGreaterThan(0, tree.Fragmentainers.Count);

        // CSS 2.1 §13.3.3: the avoids are dropped rather than letting the run overflow a page.
        foreach (var fragmentainer in tree.Fragmentainers)
        {
            var tops = new List<double>();
            CollectWordTops(fragmentainer.Root, tops);
            if (tops.Count > 0)
                Assert.IsLessThanOrEqualTo(container.PageSize.Height, tops.Max() - tops.Min(),
                    $"fragmentainer at slot {fragmentainer.SlotIndex} holds words spanning more than one page's height");
        }

        var allWords = tree.Fragmentainers.SelectMany(f => AllWords(f.Root)).ToList();
        var expected = Enumerable.Range(0, 60).Select(i => $"RunMember{i}").Append("FinalParagraph").Append("TopMarker");
        foreach (var e in expected)
        {
            Assert.AreEqual(1, allWords.Count(w => w == e), $"'{e}' must appear exactly once - not lost or duplicated");
        }
    }
}
