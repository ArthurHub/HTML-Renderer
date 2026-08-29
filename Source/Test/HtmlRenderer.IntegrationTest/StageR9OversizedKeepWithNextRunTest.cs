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
/// Verifies a real bug found while investigating the fragmentation-engine-parity plan's R9 stage: an
/// earlier version of <see cref="TheArtOfDev.HtmlRenderer.Core.Fragmentation.BlockFragmentation.EnforceKeepWithNext"/>
/// always pulled the WHOLE preceding <c>break-after:avoid</c>-chained run to a child's page without
/// checking whether the run then fit there. For a long chain (taller than one page combined), this did
/// not just mis-place content - it corrupted layout outright: each subsequent chained sibling's own
/// keep-with-next check re-fired against the now artificially-stretched-out run, compounding
/// <c>CssBox.OffsetTop</c> shifts on the same earlier boxes without bound (observed reaching a box
/// position of roughly 8.6e11 for a 60-member chain on a short page, before the fix). The fix implements
/// css-break-3 §4.3's actual staged relaxation - trim the run from its front until what remains fits, or
/// drop it entirely rather than pulling something that can't fit.
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

    [TestMethod]
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

        // The real bug produced an ActualSize.Height in the hundreds of billions and zero fragmentainers
        // (FragmentEmitter could not bucket geometry that far out of range) - a sane document is nowhere
        // close to that regardless of exact page count, which depends on font metrics.
        Assert.IsLessThan(100_000.0, wrapper.ActualSize.Height, "document height must stay sane - not blow up from compounding OffsetTop shifts");
        Assert.IsGreaterThan(0, tree.Fragmentainers.Count);

        var allWords = tree.Fragmentainers.SelectMany(f => AllWords(f.Root)).ToList();
        var expected = Enumerable.Range(0, 60).Select(i => $"RunMember{i}").Append("FinalParagraph").Append("TopMarker");
        foreach (var e in expected)
        {
            Assert.AreEqual(1, allWords.Count(w => w == e), $"'{e}' must appear exactly once - not lost or duplicated");
        }
    }
}
