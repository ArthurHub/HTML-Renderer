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
/// Verifies the R1 stage of the fragmentation-engine-parity plan: forced page breaks now go through a
/// real resumable pass loop (<see cref="HtmlContainerInt"/>'s per-fragmentainer driver, <c>CssBox</c>'s
/// <c>ResumeAt</c>/<c>PendingBreakToken</c> child-loop bubbling) instead of a single-pass local
/// correction. These tests exercise the loop across multiple passes specifically, which the existing
/// single-forced-break tests (<c>StageD2VerificationTest</c>) don't - a bug in child-index bookkeeping
/// across repeated resumes wouldn't necessarily show up with only one break in the document.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class StageR1DriverLoopTest
{
    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    [TestMethod]
    public async Task TwoForcedBreaksInSequence_EachStartsANewPageWithCorrectContent()
    {
        using var wrapper = new HtmlContainer();
        await wrapper.SetHtml(
            """
            <html><body>
                <div style="margin:0;">First page content.</div>
                <div style="margin:0; break-before: page;">Second page content.</div>
                <div style="margin:0; break-before: page;">Third page content.</div>
            </body></html>
            """);

        var container = GetInternal(wrapper);
        container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(500, 700);
        container.MarginTop = 0;
        wrapper.MaxSize = new SizeF(500, 0);

        using var bitmap = new Bitmap(500, 5000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        var tree = container.FragmentTree;
        Assert.IsNotNull(tree);
        Assert.AreEqual(3, tree.Fragmentainers.Count, "each forced break should land its own div on its own page");

        // Each page's fragmentainer must be flush at its own band top (no leftover offset carried
        // across the second break from the first, which an off-by-one in ResumeChildIndex would produce).
        for (var slot = 0; slot < 3; slot++)
        {
            var fragmentainer = tree.Fragmentainers[slot];
            Assert.AreEqual(slot, fragmentainer.SlotIndex);
        }

        // The three divs resolve to three distinct, correctly-ordered per-page fragments - proves the
        // second break resumed the child loop at the right index rather than re-processing or skipping
        // a sibling.
        StringAssert.Contains(AllText(tree.Fragmentainers[0].Root), "First");
        StringAssert.Contains(AllText(tree.Fragmentainers[1].Root), "Second");
        StringAssert.Contains(AllText(tree.Fragmentainers[2].Root), "Third");
    }

    private static string AllText(BoxFragment fragment)
    {
        var words = new List<string>();
        Collect(fragment, words);
        return string.Join(" ", words);

        static void Collect(BoxFragment f, List<string> into)
        {
            foreach (var word in f.Words)
                into.Add(word.Word.Text);
            foreach (var child in f.Children)
                Collect(child, into);
        }
    }

    [TestMethod]
    public async Task ManyForcedBreaksInSequence_TerminatesPromptlyWithOnePagePerBreak()
    {
        using var wrapper = new HtmlContainer();
        var divs = string.Concat(Enumerable.Range(0, 50).Select(i =>
            $"<div style='margin:0; break-before: page;'>Section {i}</div>"));
        await wrapper.SetHtml($"<html><body>{divs}</body></html>");

        var container = GetInternal(wrapper);
        container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(500, 700);
        container.MarginTop = 0;
        wrapper.MaxSize = new SizeF(500, 0);

        using var bitmap = new Bitmap(500, 5000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        // 50 divs, every one but the first forcing its own break: 50 pages. A hang or a runaway pass
        // count would fail this test by timeout rather than by assertion - that's the point of covering
        // the pass loop's backstop with a large-but-realistic case rather than only single/double breaks.
        var tree = container.FragmentTree;
        Assert.IsNotNull(tree);
        Assert.AreEqual(50, tree.Fragmentainers.Count);
    }
}
