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

    // The UA's h1-h6 { break-after: avoid } is under @media print, WinForms renders as screen.
    private const string HeadingStyle = "margin:0; break-after: avoid;";

    /// <summary>
    /// A filler count where the heading fits alone on page 0, but not together with its paragraph.
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
                return count;
        }

        Assert.Fail("could not find a filler count where the heading fits alone but not with its paragraph");
        return -1;
    }

    private static bool StringContains(string haystack, string needle) => haystack.Contains(needle);

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task HeadingAndParagraph_LandOnTheSamePage_NotStranded()
    {
        var count = await FindBoundaryFillerCountAsync();

        var tree = await LayoutAsync(
            $"{Filler(count)}<h4 style='{HeadingStyle}'>Section heading</h4><p style='margin:0;'>Paragraph right after the heading.</p>");

        var pageZeroText = AllText(tree.Fragmentainers[0].Root);
        StringAssert.DoesNotMatch(pageZeroText, new System.Text.RegularExpressions.Regex("Section heading"));

        var withHeading = tree.Fragmentainers.Select(f => AllText(f.Root)).FirstOrDefault(t => t.Contains("Section heading"));
        Assert.IsNotNull(withHeading, "heading should appear on some page");
        StringAssert.Contains(withHeading, "Paragraph right after the heading.");
    }
}
