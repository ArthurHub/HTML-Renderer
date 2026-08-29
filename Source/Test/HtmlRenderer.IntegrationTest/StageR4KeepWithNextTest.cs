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

    private static string Filler(int count) =>
        string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of text</p>", count));

    // WinForms reports media type "screen", not "print" - the UA stylesheet's h1-h6 { break-after: avoid }
    // rule lives under @media print (see PdfSharpAdapter vs RAdapter.DefaultMediaType) and never applies
    // to this IntegrationTest project's WinForms-based HtmlContainer. Set it explicitly rather than
    // relying on the UA default.
    private const string HeadingStyle = "margin:0; break-after: avoid;";

    /// <summary>
    /// Finds, by direct search rather than a hardcoded magic number, a filler count where the heading
    /// fits alone on page 0 but heading+paragraph together do not - the exact boundary this stage's real
    /// test needs. Hardcoding the count made this test fragile to unrelated, still-correct changes
    /// elsewhere in the pagination arithmetic (this happened once already, when InlineFragmentation's
    /// algorithm was rewritten for an unrelated widows bug and shifted the boundary by one filler).
    /// </summary>
    private static async Task<int> FindBoundaryFillerCountAsync()
    {
        for (var count = 20; count < 80; count++)
        {
            var headingAlone = await LayoutAsync($"{Filler(count)}<h4 style='{HeadingStyle}'>Section heading</h4>");
            var headingFitsAlone = StringContains(AllText(headingAlone.Fragmentainers[0].Root), "Section heading");
            if (!headingFitsAlone)
                continue;

            var withParagraph = await LayoutAsync(
                $"{Filler(count)}<h4 style='{HeadingStyle}'>Section heading</h4><p style='margin:0;'>Paragraph right after the heading.</p>");
            var bothFitOnPageZero = withParagraph.Fragmentainers.Count >= 1
                && StringContains(AllText(withParagraph.Fragmentainers[0].Root), "Paragraph right after the heading.");
            if (!bothFitOnPageZero)
                return count; // heading alone fits; heading+paragraph together doesn't - the boundary.
        }

        Assert.Fail("could not find a filler count where the heading fits alone but not with its paragraph");
        return -1;
    }

    private static bool StringContains(string haystack, string needle) => haystack.Contains(needle);

    [TestMethod]
    public async Task HeadingAndParagraph_LandOnTheSamePage_NotStranded()
    {
        var count = await FindBoundaryFillerCountAsync();

        var tree = await LayoutAsync(
            $"{Filler(count)}<h4 style='{HeadingStyle}'>Section heading</h4><p style='margin:0;'>Paragraph right after the heading.</p>");

        // Page 0's own text must NOT contain the heading - it should have been pulled forward to join
        // the paragraph, not left stranded where the boundary search shows it would otherwise fit alone.
        var pageZeroText = AllText(tree.Fragmentainers[0].Root);
        StringAssert.DoesNotMatch(pageZeroText, new System.Text.RegularExpressions.Regex("Section heading"));

        var withHeading = tree.Fragmentainers.Select(f => AllText(f.Root)).FirstOrDefault(t => t.Contains("Section heading"));
        Assert.IsNotNull(withHeading, "heading should appear on some page");
        StringAssert.Contains(withHeading, "Paragraph right after the heading.");
    }
}
