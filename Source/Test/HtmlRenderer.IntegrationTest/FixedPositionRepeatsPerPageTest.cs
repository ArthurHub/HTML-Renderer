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
/// Verifies a real, spec-confirmed missing feature found while auditing this port's fragmentation engine
/// against PeachPDF a third time, then checking the actual W3C text directly
/// (<see href="https://www.w3.org/TR/css-position-3/">css-position-3</see>): "in paged media, the page
/// area of each page; fixed positioned boxes are thus replicated on every page", and user agents "must
/// not paginate the content of fixed-positioned boxes". A <c>position:fixed</c> element (a print
/// header/watermark - <c>bottom</c>/<c>right</c> anchoring is a separate, pre-existing gap: neither
/// property is parsed for absolute/fixed positioning at all, so a bottom-anchored footer, the more common
/// real print pattern, is out of scope here) previously rendered on exactly one page - wherever its
/// <c>top</c>/<c>left</c> offset happened to be interpreted as an absolute document coordinate - instead
/// of being replicated identically on every page.
/// </summary>
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
    public async Task FixedElement_StillRendersOnce_WithoutARealPageGrid()
    {
        // WinForms/WPF's continuous-scroll convention (no PageSize set - HasRealPageGrid=false): the
        // repeat-per-page mechanism must not apply here at all, since "stays put" for that viewport is a
        // paint-time scroll-offset suppression (CssBox.IsFixed), not a per-page repeat concern - confirms
        // the new exclusion in FragmentEmitter is correctly gated on HasRealPageGrid.
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
