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
public sealed class FixedPositionRepeatsPerPageTest
{
    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    private static IEnumerable<(string Text, double Top)> AllWords(BoxFragment f)
    {
        foreach (var w in f.Words)
            if (!w.Word.IsLineBreak)
                yield return (w.Word.Text, w.Rect.Top);
        foreach (var c in f.Children)
            foreach (var x in AllWords(c))
                yield return x;
    }

    [TestMethod]
    [Spec("css21", "visuren.html#fixed-positioning")]
    public async Task TopLeftFixedElement_RepeatsIdenticallyOnEveryPage()
    {
        using var wrapper = new HtmlContainer();
        var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of body text</p>", 60));
        await wrapper.SetHtml(
            $"""
            <html><body>
                <div style="position:fixed; top:5px; left:5px;">PageHeaderMarker</div>
                {filler}
            </body></html>
            """);

        var container = GetInternal(wrapper);
        container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(300, 300);
        container.MarginTop = 0;
        wrapper.MaxSize = new SizeF(300, 0);

        using var bitmap = new Bitmap(300, 20000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        var tree = container.FragmentTree;
        Assert.IsGreaterThan(1, tree.Fragmentainers.Count, "the filler content must genuinely span multiple pages for this test to be meaningful");

        for (var i = 0; i < tree.Fragmentainers.Count; i++)
        {
            var markerHits = AllWords(tree.Fragmentainers[i].Root).Where(w => w.Text == "PageHeaderMarker").ToList();
            Assert.AreEqual(1, markerHits.Count, $"page {i} should show the fixed marker exactly once - not zero (missing) and not more than one (duplicated by both the repeat mechanism and the normal walk)");
            Assert.AreEqual(5.0, markerHits[0].Top, 0.5, $"page {i}'s marker must be at the same page-relative offset (top:5px) as every other page");
        }
    }

    [TestMethod]
    [Spec("css21", "visuren.html#fixed-positioning")]
    public async Task FixedElement_StillRendersOnce_WithoutARealPageGrid()
    {
        // Continuous media, no PageSize, so no page grid to repeat on
        using var wrapper = new HtmlContainer();
        var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of body text</p>", 60));
        await wrapper.SetHtml(
            $"""
            <html><body>
                <div style="position:fixed; top:5px; left:5px;">PageHeaderMarker</div>
                {filler}
            </body></html>
            """);

        wrapper.MaxSize = new SizeF(300, 0);
        using var bitmap = new Bitmap(300, 20000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        var container = GetInternal(wrapper);
        Assert.IsFalse(container.HasRealPageGrid);
        var tree = container.FragmentTree;
        Assert.AreEqual(1, tree.Fragmentainers.Count);

        var markerHits = AllWords(tree.Fragmentainers[0].Root).Where(w => w.Text == "PageHeaderMarker").ToList();
        Assert.AreEqual(1, markerHits.Count, "the fixed element must still render exactly once via the normal walk when there's no real page grid");
    }
}
