using System.Drawing;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Fragments;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest;

// This assembly parallelizes at the method level (MSTestSettings.cs); HtmlContainerInt's underlying
// adapter singletons (font/brush caches, etc.) aren't safe against that for tests that drive full
// layout passes directly - HtmlRenderingRegressionTests already opts out for the same reason.
[TestClass]
[DoNotParallelize]
public sealed class StageD2FragmentBucketingSmokeTest
{
    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    [TestMethod]
    public async Task MultiPageDocument_ProducesOneFragmentainerPerPage_WithSplitBoxFragments()
    {
        using var wrapper = new HtmlContainer();
        var paragraphs = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of text for pagination</p>", 80));
        await wrapper.SetHtml($"<html><body>{paragraphs}</body></html>");

        var container = GetInternal(wrapper);
        container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(500, 700);
        container.MarginTop = 0;
        wrapper.MaxSize = new SizeF(500, 0);

        using var bitmap = new Bitmap(500, 5000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        var tree = container.FragmentTree;
        Assert.IsNotNull(tree);
        Assert.IsTrue(tree.Fragmentainers.Count > 1, $"expected multiple fragmentainers, got {tree.Fragmentainers.Count}");

        // Slot indices are ascending and each fragmentainer's band matches its slot.
        for (var i = 0; i < tree.Fragmentainers.Count; i++)
        {
            var f = tree.Fragmentainers[i];
            Assert.AreEqual(f.SlotIndex, i, "no blank slots expected in this dense document");
        }

        // The document root CssBox (which spans the whole document) must produce a distinct
        // BoxFragment per fragmentainer - the same underlying box, multiple fragments.
        Assert.AreEqual(tree.Fragmentainers.Count, tree.Fragmentainers.Select(f => f.Root).Distinct().Count());

        // Every fragmentainer's root should trace back to the same document root CssBox.
        foreach (var f in tree.Fragmentainers)
        {
            Assert.AreSame(container.Root, f.Root.Box);
        }
    }

    [TestMethod]
    public async Task HugeMargin_SkipsBlankFragmentainers()
    {
        using var wrapper = new HtmlContainer();
        await wrapper.SetHtml("<html><body><div style='margin-top:3000px;'>content</div></body></html>");

        var container = GetInternal(wrapper);
        container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(500, 700);
        container.MarginTop = 0;
        wrapper.MaxSize = new SizeF(500, 0);

        using var bitmap = new Bitmap(500, 5000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        var tree = container.FragmentTree;
        Assert.IsNotNull(tree);

        // The margin is truncated (D2), so content should land on an early page, not one 3000px down -
        // this also implicitly confirms no run of ~4 blank fragmentainers was materialized for the gap.
        Assert.IsTrue(tree.Fragmentainers.Count <= 2, $"expected at most 2 fragmentainers, got {tree.Fragmentainers.Count}");
    }
}
