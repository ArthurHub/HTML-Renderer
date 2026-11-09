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
/// Content taller than a page stays where it is, and every page it covers claims it.
/// </summary>
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

    // A word taller than the page fits nowhere, so it stays and every page it covers claims it. Its height
    // comes from font-size, a tall line-height would only grow the line box.
    [TestMethod]
    public async Task AWordTallerThanTheBand_IsClaimedByEveryBandItCovers()
    {
        var (root, container) = await BuildAsync("<p style='font-size:1800px;line-height:1;margin:0'>T</p>");

        var word = Walk(root).SelectMany(b => b.Words).Single(w => w.Text == "T");
        var band = container.PageIndexOf(word.Top);

        Assert.IsTrue(word.Height > container.PageBottomOf(band) - container.PageTopOf(band),
            $"the fixture must produce a word taller than the band, not {word.Height}");

        var covered = container.FragmentTree!.Fragmentainers
            .Select(f => f.SlotIndex)
            .Where(slot => word.Bottom > container.PageTopOf(slot) && word.Top < container.PageBottomOf(slot))
            .ToList();

        Assert.IsTrue(covered.Count > 2, $"the fixture must span more than two bands, not {covered.Count}");
        CollectionAssert.AreEqual(covered, SlotsClaiming(container, word));
    }
}
