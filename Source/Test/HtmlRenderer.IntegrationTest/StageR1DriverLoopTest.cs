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
/// Several forced breaks in one document, each resuming the layout pass loop.
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
    [Spec("css21", "page.html#forced")]
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

        for (var slot = 0; slot < 3; slot++)
        {
            var fragmentainer = tree.Fragmentainers[slot];
            Assert.AreEqual(slot, fragmentainer.SlotIndex);
        }

        // No sibling is skipped or laid out twice when the second break resumes the child loop.
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
    [Spec("css21", "page.html#forced")]
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

        // The first div's break isn't taken, nothing precedes it. A runaway pass loop fails by timeout.
        var tree = container.FragmentTree;
        Assert.IsNotNull(tree);
        Assert.AreEqual(50, tree.Fragmentainers.Count);
    }
}
