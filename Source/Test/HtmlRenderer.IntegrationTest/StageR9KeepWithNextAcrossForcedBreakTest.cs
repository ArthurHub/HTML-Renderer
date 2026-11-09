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
/// Keep-with-next still applies in a layout pass that resumed after a forced break.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class StageR9KeepWithNextAcrossForcedBreakTest
{
    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    private static string AllText(BoxFragment f)
    {
        var words = new List<string>();
        void Collect(BoxFragment x)
        {
            foreach (var w in x.Words)
                if (!w.Word.IsLineBreak)
                    words.Add(w.Word.Text);
            foreach (var c in x.Children)
                Collect(c);
        }
        Collect(f);
        return string.Join(" ", words);
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task KeepWithNextPairRightAfterAForcedBreak_StaysTogether_OnTheResumedPage()
    {
        using var wrapper = new HtmlContainer();
        var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of text</p>", 39));
        await wrapper.SetHtml(
            $"""
            <html><body>
                <div style="margin:0; break-before: page;">ForcedBreakMarker</div>
                {filler}
                <h4 style="margin:0; break-after: avoid;">Section heading</h4>
                <p style="margin:0;">Paragraph right after the heading.</p>
            </body></html>
            """);

        var container = GetInternal(wrapper);
        container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(595, 800);
        container.MarginTop = 20;
        wrapper.MaxSize = new SizeF(595, 0);

        using var bitmap = new Bitmap(595, 20000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        var tree = container.FragmentTree;
        Assert.IsNotNull(tree);
        Assert.IsGreaterThanOrEqualTo(2, tree.Fragmentainers.Count, "the forced break must actually introduce a real pass boundary for this test to be meaningful");

        var pageOfHeading = -1;
        var pageOfParagraph = -1;
        for (var i = 0; i < tree.Fragmentainers.Count; i++)
        {
            var text = AllText(tree.Fragmentainers[i].Root);
            if (text.Contains("Section heading")) pageOfHeading = i;
            if (text.Contains("Paragraph right after the heading.")) pageOfParagraph = i;
        }

        Assert.AreNotEqual(-1, pageOfHeading, "heading must not be lost");
        Assert.AreNotEqual(-1, pageOfParagraph, "paragraph must not be lost");
        Assert.AreEqual(pageOfHeading, pageOfParagraph, "break-after:avoid must keep the heading with its paragraph even immediately after resuming from an unrelated forced break");
    }
}
