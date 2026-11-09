using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest.Fragmentation;

/// <summary>
/// <c>orphans</c> and <c>widows</c>. Break tests sweep filler heights and count the lines on either side of
/// the break, line positions depend on font metrics.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class OrphansWidowsIntegrationTests
{
    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    private static async Task<(CssBox Root, HtmlContainerInt Container)> BuildAsync(
        string bodyHtml, double pageHeight = 100, double pageWidth = 400, int marginTop = 0)
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

    private static CssBox FindById(CssBox root, string id) =>
        Walk(root).FirstOrDefault(b => b.HtmlTag?.TryGetAttribute("id") == id)!;

    // ─── CSS parsing/inheritance ────────────────────────────────────────────

    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    public async Task Orphans_DefaultsToTwo()
    {
        var (root, _) = await BuildAsync("<p id='p'>text</p>");
        Assert.AreEqual("2", FindById(root, "p").Orphans);
    }

    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    public async Task Widows_DefaultsToTwo()
    {
        var (root, _) = await BuildAsync("<p id='p'>text</p>");
        Assert.AreEqual("2", FindById(root, "p").Widows);
    }

    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    public async Task Orphans_ParsesExplicitValue()
    {
        var (root, _) = await BuildAsync("<p id='p' style='orphans:3'>text</p>");
        Assert.AreEqual("3", FindById(root, "p").Orphans);
    }

    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    public async Task Widows_ParsesExplicitValue()
    {
        var (root, _) = await BuildAsync("<p id='p' style='widows:1'>text</p>");
        Assert.AreEqual("1", FindById(root, "p").Widows);
    }

    // The declared "0" is kept, layout uses 2 instead.
    [TestMethod]
    public async Task Orphans_ZeroResolvesToDefault_ThoughTheDeclaredStringStaysZero()
    {
        var (root, _) = await BuildAsync("<p id='p' style='orphans:0'>text</p>");
        var p = FindById(root, "p");
        Assert.AreEqual("0", p.Orphans);
        Assert.AreEqual(2, p.ActualOrphans);
    }

    [TestMethod]
    public async Task Widows_ZeroResolvesToDefault_ThoughTheDeclaredStringStaysZero()
    {
        var (root, _) = await BuildAsync("<p id='p' style='widows:0'>text</p>");
        var p = FindById(root, "p");
        Assert.AreEqual("0", p.Widows);
        Assert.AreEqual(2, p.ActualWidows);
    }

    [TestMethod]
    public async Task Widows_NegativeResolvesToDefault()
    {
        var (root, _) = await BuildAsync("<p id='p' style='widows:-1'>text</p>");
        Assert.AreEqual(2, FindById(root, "p").ActualWidows);
    }

    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    public async Task Widows_IsInherited()
    {
        var (root, _) = await BuildAsync("<div style='widows:4'><p id='p'>text</p></div>");
        Assert.AreEqual("4", FindById(root, "p").Widows);
    }

    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    public async Task Orphans_IsInherited()
    {
        var (root, _) = await BuildAsync("<div style='orphans:5'><p id='p'>text</p></div>");
        Assert.AreEqual("5", FindById(root, "p").Orphans);
    }

    // ─── Break-avoidance behavior ────────────────────────────────────────────

    private const double LineHeight = 20;

    // One word per line through wrapping, not <br>: <br> in the middle of a run splits the paragraph into
    // anonymous blocks of one line each.
    private static string Paragraph(int lineCount, string extraStyle = "") =>
        "<p id='p' style='width:50px;line-height:" + LineHeight + "px;margin:0;padding:0;" + extraStyle + "'>"
        + string.Join(" ", Enumerable.Range(1, lineCount).Select(i => $"Line{i}"))
        + "</p>";

    /// <summary>How many of a paragraph's own lines fall on each side of a page boundary, at the given
    /// filler height. Returns null if the paragraph did not appear at all at this bitmap height.</summary>
    private static async Task<(int Before, int After, int PageIndex)?> SplitAsync(
        double fillerHeight, int lineCount, string extraStyle, double pageHeight = 100)
    {
        var html = $"<div style='height:{fillerHeight}px'></div>" + Paragraph(lineCount, extraStyle);
        var (root, container) = await BuildAsync(html, pageHeight: pageHeight);
        var p = FindById(root, "p");
        if (p == null) return null;

        var tops = Walk(p).SelectMany(b => b.Words).Where(w => !w.IsLineBreak).Select(w => w.Top).Distinct().OrderBy(t => t).ToList();
        if (tops.Count == 0) return null;

        var pageIndex = container.PageIndexOf(tops[0]);
        var boundary = container.PageTopOf(pageIndex + 1);
        var before = tops.Count(t => t < boundary);
        var after = tops.Count(t => t >= boundary);
        return (before, after, pageIndex);
    }

    // css-break-3 §5.4 moves the fewest lines across the break, not the whole box.
    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    public async Task Widows2_OnlyOneLineWouldFollowTheBreak_MovesOneLineRatherThanTheWholeBox()
    {
        var checkedAny = false;
        for (var filler = 0.0; filler < 100; filler += 2)
        {
            var split = await SplitAsync(filler, 4, "widows:2");
            if (split is not { Before: > 0, After: > 0 } s) continue;
            if (s.Before + s.After != 4) continue; // not all 4 lines visible on this bitmap height

            checkedAny = true;
            Assert.IsTrue(s.After >= 2, $"filler={filler}: widows:2 violated, only {s.After} line(s) after the break");
            Assert.IsTrue(s.Before >= 1, $"filler={filler}: nothing at all left before the break");
        }

        Assert.IsTrue(checkedAny, "test is not meaningful as written");
    }

    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    public async Task Widows3_MovesAsManyLinesAsItTakes()
    {
        var checkedAny = false;
        for (var filler = 0.0; filler < 100; filler += 2)
        {
            var split = await SplitAsync(filler, 5, "widows:3");
            if (split is not { Before: > 0, After: > 0 } s) continue;
            if (s.Before + s.After != 5) continue;

            checkedAny = true;
            Assert.IsTrue(s.After >= 3, $"filler={filler}: widows:3 violated, only {s.After} line(s) after the break");
        }

        Assert.IsTrue(checkedAny, "test is not meaningful as written");
    }

    // widows:4 on a 4-line paragraph leaves nothing before the break, so the whole box moves.
    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    public async Task Widows4_CannotBeSatisfiedWithoutBreakingOrphans_PushesTheWholeBox()
    {
        var checkedAny = false;
        for (var filler = 0.0; filler < 100; filler += 2)
        {
            var split = await SplitAsync(filler, 4, "widows:4");
            if (split is not { } s) continue;
            if (s.Before + s.After != 4) continue;
            if (s.Before == 0) continue; // already on a fresh page - nothing to characterize here

            checkedAny = true;
            Assert.IsTrue(s.Before == 0 || s.After == 0,
                $"filler={filler}: expected the whole box pushed together (0 before or 0 after), got {s.Before}/{s.After}");
        }

        Assert.IsTrue(checkedAny, "test is not meaningful as written");
    }

    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    public async Task Orphans2_ParagraphNudgedWhenOnlyOneLineWouldPrecedeTheBreak()
    {
        var checkedAny = false;
        for (var filler = 0.0; filler < 100; filler += 2)
        {
            var split = await SplitAsync(filler, 4, "orphans:2");
            if (split is not { } s) continue;
            if (s.Before + s.After != 4) continue;
            if (s.Before == 0) continue;

            checkedAny = true;
            Assert.IsTrue(s.Before == 0 || s.Before >= 2,
                $"filler={filler}: orphans:2 violated, only {s.Before} line(s) before the break");
        }

        Assert.IsTrue(checkedAny, "test is not meaningful as written");
    }

    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    public async Task OrphansWidows_NoEffect_WhenSplitAlreadySatisfiesBothMinimums_Regression()
    {
        // A taller page leaves slack on both sides of a natural 2-2 split.
        var checkedAny = false;
        for (var filler = 0.0; filler < 400; filler += 4)
        {
            var natural = await SplitAsync(filler, 4, "orphans:1;widows:1", pageHeight: 200);
            if (natural is not { Before: >= 2, After: >= 2 } n) continue;

            checkedAny = true;
            var split = await SplitAsync(filler, 4, "orphans:2;widows:2", pageHeight: 200);
            Assert.AreEqual(n, split, $"filler={filler}");
        }

        Assert.IsTrue(checkedAny, "test is not meaningful as written");
    }

    // An 8-line paragraph is taller than the page, moving it whole wouldn't help.
    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task TallParagraph_ExceedsOnePage_IsNotNudged()
    {
        var html = "<div style='height:38px'></div>" + Paragraph(8);
        var (root, container) = await BuildAsync(html);
        var p = FindById(root, "p");
        Assert.IsNotNull(p);

        var tops = Walk(p).SelectMany(b => b.Words).Where(w => !w.IsLineBreak).Select(w => w.Top).Distinct().OrderBy(t => t).ToList();
        Assert.AreEqual(8, tops.Count, "fixture must produce all 8 of its own lines for this to test anything");

        var pageIndexes = tops.Select(container.PageIndexOf).Distinct().ToList();
        Assert.IsTrue(pageIndexes.Count > 1, "fixture must actually straddle more than one page for this to test anything");
        Assert.AreEqual(38, tops[0], 1.0, "the page break should fall between the paragraph's lines, not before it");
    }

    // A paragraph taller than the page still doesn't leave a single line behind.
    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    public async Task Orphans2_ParagraphTallerThanTheBand_BreaksBeforeItselfRatherThanStrandingOneLine()
    {
        var checkedAny = false;
        for (var filler = 0.0; filler < 100; filler += 2)
        {
            var html = $"<div style='height:{filler}px'></div>" + Paragraph(8, "orphans:2");
            var (root, container) = await BuildAsync(html);
            var p = FindById(root, "p");
            if (p == null) continue;

            var tops = Walk(p).SelectMany(b => b.Words).Where(w => !w.IsLineBreak).Select(w => w.Top).Distinct().OrderBy(t => t).ToList();
            if (tops.Count == 0) continue;

            var pageIndex = container.PageIndexOf(tops[0]);
            var boundary = container.PageTopOf(pageIndex + 1);
            var before = tops.Count(t => t < boundary);
            if (before == 0) continue; // nothing straddling here - not the case under test
            if (tops.Count(t => t >= boundary) == 0) continue; // whole paragraph already on one page

            checkedAny = true;
            Assert.IsTrue(before >= 2, $"filler={filler}: only {before} line(s) stranded before the break, fewer than orphans:2");
        }

        Assert.IsTrue(checkedAny, "test is not meaningful as written");
    }

    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    public async Task Orphans1_ParagraphTallerThanTheBand_KeepsItsSingleLineFragment()
    {
        var foundASingleLineFragment = false;
        for (var filler = 0.0; filler < 100; filler += 2)
        {
            var html = $"<div style='height:{filler}px'></div>" + Paragraph(8, "orphans:1");
            var (root, container) = await BuildAsync(html);
            var p = FindById(root, "p");
            if (p == null) continue;

            var tops = Walk(p).SelectMany(b => b.Words).Where(w => !w.IsLineBreak).Select(w => w.Top).Distinct().OrderBy(t => t).ToList();
            if (tops.Count == 0) continue;

            var pageIndex = container.PageIndexOf(tops[0]);
            var boundary = container.PageTopOf(pageIndex + 1);
            var before = tops.Count(t => t < boundary);
            if (before == 1)
                foundASingleLineFragment = true;
        }

        Assert.IsTrue(foundASingleLineFragment, "expected at least one filler height where orphans:1 keeps exactly one stranded line");
    }

    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    public async Task Orphans2_SatisfiedByTheNaturalBreak_LeavesTheParagraphWhereItIs()
    {
        var checkedAny = false;
        for (var filler = 0.0; filler < 100; filler += 2)
        {
            var natural = await SplitAsync(filler, 8, "orphans:1");
            if (natural is not { Before: >= 2, After: > 0 } n) continue;

            checkedAny = true;
            var split = await SplitAsync(filler, 8, "orphans:2");
            Assert.AreEqual(n, split, $"filler={filler}");
        }

        Assert.IsTrue(checkedAny, "test is not meaningful as written");
    }

    // At the top of a page, moving the box gives it no more room, so orphans is dropped.
    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task Orphans2_NothingAboveItInTheFragmentainer_IsLeftWhereItIs()
    {
        var html = "<div style='height:0'></div>" + Paragraph(3, "orphans:2");
        var (root, container) = await BuildAsync(html, pageHeight: 30);
        var p = FindById(root, "p");
        Assert.IsNotNull(p);

        Assert.AreEqual(0, p.EffectiveTop, 1.0);
    }

    // Must terminate instead of moving the paragraph down page by page.
    [TestMethod]
    [Timeout(10000)]
    public async Task Orphans2_HeadingAndParagraph_AreCorrectedOnceRatherThanWalkingTheDocument()
    {
        var html = "<div style='height:70px'></div>"
            + "<h2 style='margin:0;font-size:10px'>Heading</h2>"
            + Paragraph(8, "orphans:2");

        var (root, _) = await BuildAsync(html, pageHeight: 100);
        Assert.IsNotNull(FindById(root, "p"));
    }

    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    public async Task Widows2_ParagraphStartingOnSecondPage_StillCorrected()
    {
        var checkedAny = false;
        for (var filler = 100.0; filler < 200; filler += 2)
        {
            var split = await SplitAsync(filler, 4, "widows:2");
            if (split is not { PageIndex: > 0, Before: > 0, After: > 0 } s) continue;
            if (s.Before + s.After != 4) continue;

            checkedAny = true;
            Assert.IsTrue(s.After >= 2, $"filler={filler}: widows:2 violated on a paragraph starting past page 0, only {s.After} line(s) after");
        }

        Assert.IsTrue(checkedAny, "test is not meaningful as written - no filler in range started the paragraph past page 0 while still straddling");
    }

    // orphans:1 and widows:1 never require moving a line.
    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    public async Task Orphans1Widows1_MatchesDictionaryCssValues_NoEffect_Regression()
    {
        var oneBefore = false;
        var oneAfter = false;
        for (var filler = 0.0; filler < 100; filler += 2)
        {
            var split = await SplitAsync(filler, 4, "orphans:1;widows:1");
            if (split is not { } s) continue;
            if (s.Before + s.After != 4) continue;

            oneBefore |= s.Before == 1;
            oneAfter |= s.After == 1;
        }

        Assert.IsTrue(oneBefore, "orphans:1 should leave a single line before the break");
        Assert.IsTrue(oneAfter, "widows:1 should leave a single line after the break");
    }
}
