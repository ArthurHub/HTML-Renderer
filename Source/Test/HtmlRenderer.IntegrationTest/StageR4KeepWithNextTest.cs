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
/// Verifies the R4 stage of the fragmentation-engine-parity plan: keep-with-next
/// (<c>BlockFragmentation.EnforceKeepWithNext</c>) now fires for the ordinary case, not just as a side
/// effect of the following box also being <c>break-inside:avoid</c>/monolithic.
/// </summary>
/// <remarks>
/// The pre-existing <c>KeepWithNext_HeadingStaysWithFollowingParagraph</c> PDF test (still passing, still
/// kept) only ever asserted a page COUNT of 2 - which is also exactly what you get if the heading is left
/// stranded alone at the bottom of page 1 while the paragraph moves to page 2 by itself (2 pages either
/// way). It never actually proved the heading and paragraph land on the SAME page. This test does, using
/// the fragment tree directly: filler content is calibrated so the heading provably fits alone on page 0
/// in isolation (confirmed by a companion assertion with no trailing paragraph), then, with the paragraph
/// present, both must appear in the SAME fragmentainer.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class StageR4KeepWithNextTest
{
    private const int FillerCount = 39;

    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    private static async Task<FragmentTree> LayoutAsync(string bodyHtml)
    {
        using var wrapper = new HtmlContainer();
        await wrapper.SetHtml($"<html><body>{bodyHtml}</body></html>");

        var container = GetInternal(wrapper);
        container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(595, 800);
        container.MarginTop = 20;
        wrapper.MaxSize = new SizeF(595, 0);

        using var bitmap = new Bitmap(595, 20000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        return container.FragmentTree;
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

    private static string Filler() =>
        string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of text</p>", FillerCount));

    // WinForms reports media type "screen", not "print" - the UA stylesheet's h1-h6 { break-after: avoid }
    // rule lives under @media print (see PdfSharpAdapter vs RAdapter.DefaultMediaType) and never applies
    // to this IntegrationTest project's WinForms-based HtmlContainer. Set it explicitly rather than
    // relying on the UA default.
    private const string HeadingStyle = "margin:0; break-after: avoid;";

    [TestMethod]
    public async Task Precondition_HeadingAloneFitsOnPageZero()
    {
        // Establishes the calibration this stage's real test depends on: with FillerCount fillers and no
        // trailing paragraph, the heading fits on the same page as the filler (a stray trailing blank
        // fragmentainer past it is an unrelated pre-existing quirk, not what this checks).
        var tree = await LayoutAsync($"{Filler()}<h4 style='{HeadingStyle}'>Section heading</h4>");
        StringAssert.Contains(AllText(tree.Fragmentainers[0].Root), "Section heading");
    }

    [TestMethod]
    public async Task HeadingAndParagraph_LandOnTheSamePage_NotStranded()
    {
        var tree = await LayoutAsync(
            $"{Filler()}<h4 style='{HeadingStyle}'>Section heading</h4><p style='margin:0;'>Paragraph right after the heading.</p>");

        // Page 0's own text must NOT contain the heading - it should have been pulled forward to join
        // the paragraph, not left stranded where the precondition test shows it would otherwise fit.
        var pageZeroText = AllText(tree.Fragmentainers[0].Root);
        StringAssert.DoesNotMatch(pageZeroText, new System.Text.RegularExpressions.Regex("Section heading"));

        var withHeading = tree.Fragmentainers.Select(f => AllText(f.Root)).FirstOrDefault(t => t.Contains("Section heading"));
        Assert.IsNotNull(withHeading, "heading should appear on some page");
        StringAssert.Contains(withHeading, "Paragraph right after the heading.");
    }
}
