using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest.Fragmentation;

/// <summary>
/// Forced breaks and margin truncation on a 1000px page with a 50px top margin (band <c>[50, 1050)</c>).
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class PageBreakIntegrationTests
{
    private const double PageHeight = 1000;
    private const int MarginTop = 50;

    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    private static async Task<(CssBox Root, HtmlContainerInt Container)> BuildAsync(string bodyHtml)
    {
        var wrapper = new HtmlContainer();
        await wrapper.SetHtml($"<html><body style='margin:0'>{bodyHtml}</body></html>");

        var container = GetInternal(wrapper);
        container.PageSize = new RSize(400, PageHeight);
        container.MarginTop = MarginTop;
        // Content starts at MarginTop like in PdfGenerator, the page grid is anchored there.
        container.Location = new RPoint(0, MarginTop);
        wrapper.MaxSize = new SizeF(400, 0);

        using var bitmap = new Bitmap(400, 60000);
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

    private static CssBox FindByClass(CssBox root, string className)
    {
        foreach (var box in Walk(root))
        {
            var classAttr = box.HtmlTag?.TryGetAttribute("class", "");
            if (!string.IsNullOrEmpty(classAttr) && System.Array.IndexOf(classAttr.Split(' '), className) >= 0)
                return box;
        }
        return null!;
    }

    private static CssBox FindById(CssBox root, string id)
    {
        foreach (var box in Walk(root))
        {
            if (box.HtmlTag?.TryGetAttribute("id") == id)
                return box;
        }
        return null!;
    }

    // An empty page-break-after:always div followed by a page-break-inside:avoid box.
    [TestMethod]
    [Spec("css21", "page.html#forced")]
    public async Task PageBreakAfter_ForcesBreakBeforeNextSection()
    {
        var (root, container) = await BuildAsync(
            "<div class='filler' style='height:200px'>filler</div>"
            + "<div style='height:0;page-break-after:always'></div>"
            + "<div class='bordered' style='border:2px solid black;padding:10px;break-inside:avoid;page-break-inside:avoid;height:100px'>Section B</div>");

        var bordered = FindByClass(root, "bordered");
        Assert.IsNotNull(bordered);

        Assert.AreEqual(1, container.PageIndexOf(bordered.Location.Y),
            $"Bordered box should start on page 2, but starts at y={bordered.Location.Y}");

        Assert.AreEqual(
            container.PageIndexOf(bordered.Location.Y),
            container.PageIndexOf(bordered.ActualBottom - 0.01),
            "Bordered box must not be split across pages");
    }

    [TestMethod]
    [Spec("css21", "page.html#forced")]
    public async Task PageBreakBefore_ForcesBreak()
    {
        var (root, container) = await BuildAsync(
            "<div class='filler' style='height:100px'>filler</div>"
            + "<div class='bordered' style='page-break-before:always;break-inside:avoid;height:80px'>Section B</div>");

        var bordered = FindByClass(root, "bordered");
        Assert.IsNotNull(bordered);
        Assert.AreEqual(1, container.PageIndexOf(bordered.Location.Y),
            $"Bordered box with break-before:page should start on page 2, but starts at y={bordered.Location.Y}");
    }

    // css-break-3 §3.3 maps page-break-before:always onto break-before:page.
    [TestMethod]
    [Spec("css21", "page.html#forced")]
    public async Task BreakBeforePage_ForcesBreak()
    {
        var (root, container) = await BuildAsync(ForcedBreakHtml("break-before:page"));

        var second = FindByClass(root, "second");
        Assert.IsNotNull(second);
        Assert.AreEqual(1, container.PageIndexOf(second.Location.Y),
            $"break-before: page should start the box on page 2, but it starts at y={second.Location.Y}");
    }

    // 'always' is also accepted on the unprefixed break-before (css-break-4).
    [TestMethod]
    public async Task BreakBeforeAlways_IsAcceptedAsAForcedBreak_UnlikePeachPDF()
    {
        var (root, container) = await BuildAsync(ForcedBreakHtml("break-before:always"));

        var second = FindByClass(root, "second");
        Assert.IsNotNull(second);
        Assert.AreEqual("always", second.BreakBefore);
        Assert.AreEqual(1, container.PageIndexOf(second.Location.Y),
            $"break-before: always should force a break, but the box starts at y={second.Location.Y}");
    }

    private static string ForcedBreakHtml(string breakDeclaration) =>
        $"<div class='first' style='height:80px'>First</div>"
        + $"<div class='second' style='height:80px;{breakDeclaration}'>Second</div>";

    [TestMethod]
    [DataRow("break-inside:avoid;page-break-inside:avoid")]
    [DataRow("break-inside:avoid-page")]
    [Spec("css21", "page.html#page-break-props")]
    public async Task BreakInside_AvoidingAPageBreak_PositionsAtTopOfNextPage(string declaration)
    {
        var (root, container) = await BuildAsync(BreakInsideHtml(declaration));

        var avoidBox = FindByClass(root, "avoid");
        Assert.IsNotNull(avoidBox);

        Assert.AreEqual(
            container.PageIndexOf(avoidBox.Location.Y),
            container.PageIndexOf(avoidBox.ActualBottom - 0.01),
            $"Box with '{declaration}' must not be split across pages");

        Assert.AreEqual(1, container.PageIndexOf(avoidBox.Location.Y),
            "Test setup expects the avoid box to be relocated to the next page to validate positioning.");

        Assert.AreEqual(container.PageTopOf(1), avoidBox.Location.Y, 0.5,
            "Relocated box should sit flush at its page's content top");
    }

    // css-break-3 §3.2: avoid-column and avoid-region don't avoid page breaks.
    [TestMethod]
    [DataRow("break-inside:avoid-column")]
    [DataRow("break-inside:avoid-region")]
    public async Task BreakInside_AvoidingAnotherContext_DoesNotSuppressAPageBreak(string declaration)
    {
        var (root, container) = await BuildAsync(BreakInsideHtml(declaration));

        var avoidBox = FindByClass(root, "avoid");
        Assert.IsNotNull(avoidBox);

        Assert.AreNotEqual(
            container.PageIndexOf(avoidBox.Location.Y),
            container.PageIndexOf(avoidBox.ActualBottom - 0.01),
            $"'{declaration}' must not suppress the page break, so the box should still straddle it");
    }

    private static string BreakInsideHtml(string declaration) =>
        "<div class='filler' style='height:900px'>filler</div>"
        + $"<div class='avoid' style='{declaration};margin:0;border:1px solid black;padding:8px;height:200px'>Keep together</div>";

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task Margin_NotCrossingPageBoundary_IsNotTruncated()
    {
        var (root, _) = await BuildAsync(
            "<div class='filler' style='height:200px;margin:0;padding:0;border:0'></div>"
            + "<div class='second' style='margin-top:100px'>Second</div>");

        var filler = FindByClass(root, "filler");
        var second = FindByClass(root, "second");
        Assert.IsNotNull(filler);
        Assert.IsNotNull(second);

        Assert.AreEqual(filler.ActualBottom + 100, second.Location.Y, 0.5,
            "second's margin-top doesn't cross a page boundary, so it must be completely unaffected by truncation");
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task Margin_CrossingOnePageBoundary_TruncatesToZero_LandsAtTopOfNextPage()
    {
        var (root, container) = await BuildAsync(
            "<div class='filler' style='height:900px;margin:0;padding:0;border:0'></div>"
            + "<div class='second' style='margin-top:200px'>Second</div>");

        var filler = FindByClass(root, "filler");
        var second = FindByClass(root, "second");
        Assert.IsNotNull(filler);
        Assert.IsNotNull(second);

        Assert.IsTrue(filler.ActualBottom + 200 > container.PageTopOf(1),
            "test setup should cross a real page boundary");

        Assert.AreEqual(1, container.PageIndexOf(second.Location.Y),
            $"second should land on page 2, but starts at y={second.Location.Y}");
        Assert.AreEqual(container.PageTopOf(1), second.Location.Y, 0.5,
            "Truncated margin should leave second flush at its page's content top");
    }

    // A margin spanning several pages still only moves the box to the next page.
    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task HugeMultiPageMargin_TruncatesToZero_LandsOnVeryNextPage()
    {
        var (root, container) = await BuildAsync(
            "<div class='filler' style='height:50px;margin:0;padding:0;border:0'></div>"
            + "<div class='second' style='margin-top:9000px'>Second</div>");

        var second = FindByClass(root, "second");
        Assert.IsNotNull(second);

        Assert.AreEqual(1, container.PageIndexOf(second.Location.Y),
            "filler ends well within page index 0, so the very next page is page index 1, not one reached by the untruncated margin");
        Assert.AreEqual(container.PageTopOf(1), second.Location.Y, 0.5);
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task ForcedBreak_MarginAfterBreak_IsPreservedNotTruncated()
    {
        var (root, container) = await BuildAsync(
            "<div class='filler' style='height:100px;margin:0;padding:0;border:0'></div>"
            + "<div class='second' style='page-break-before:always;margin-top:50px'>Second</div>");

        var second = FindByClass(root, "second");
        Assert.IsNotNull(second);

        Assert.AreEqual(container.PageTopOf(1) + 50, second.Location.Y, 0.5,
            "second's own margin-top should be added normally on top of the forced-break relocation, not truncated");
    }

    #region Margin truncation before a container's first child (css-break-3 §5.2)

    private static string FirstChildDocument(string outerStyle, string margin = "1200px") =>
        $"<div id='outer' style='{outerStyle}'>"
        + $"<div id='first' style='margin-top:{margin};height:80px'>first</div>"
        + "</div>";

    // A border or padding on the container stops the margin from collapsing through it.
    [TestMethod]
    [DataRow("border-top:1px solid black")]
    [DataRow("padding-top:1px")]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task FirstChildOfACollapseBlockingContainer_HasItsOversizedMarginTruncated(string outerStyle)
    {
        var (root, container) = await BuildAsync(FirstChildDocument(outerStyle));

        var first = FindById(root, "first");
        Assert.IsNotNull(first);

        Assert.AreEqual(container.PageTopOf(1), first.Location.Y, 1,
            "flush at the very next slot's content top - never wherever the untruncated margin reached");
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task TruncatedFirstChildMargin_IsTakenAsABreakBefore()
    {
        var (_, container) = await BuildAsync(FirstChildDocument("border-top:1px solid black"));

        Assert.IsTrue(container.FragmentTree!.Fragmentainers.Count >= 2,
            "the truncated margin must actually open a second fragmentainer");
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task FirstChildMargin_StayingWithinItsOwnSlot_IsNotTruncated()
    {
        var (root, _) = await BuildAsync(FirstChildDocument("border-top:1px solid black", margin: "200px"));

        var outer = FindById(root, "outer");
        var first = FindById(root, "first");
        Assert.IsNotNull(outer);
        Assert.IsNotNull(first);

        Assert.AreEqual(outer.ClientTop + 200, first.Location.Y, 1);
    }

    // The heading stays on the first page while 'body' moves, so 'wrap' spans both pages.
    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task FirstChildRelocation_PullsTheRunAcrossTheContainer()
    {
        var (root, container) = await BuildAsync(
            "<div id='lead' style='height:400px'>lead</div>"
            + "<h2 id='head' style='margin:0;break-after:avoid'>Head</h2>"
            + "<div id='wrap' style='border-top:1px solid black'>"
            + "<div id='body' style='margin-top:800px;height:80px'>body</div>"
            + "</div>");

        var head = FindById(root, "head");
        var wrap = FindById(root, "wrap");
        var body = FindById(root, "body");
        Assert.IsNotNull(head);
        Assert.IsNotNull(wrap);
        Assert.IsNotNull(body);

        Assert.AreEqual(container.PageTopOf(1), head.Location.Y, 1,
            "the run's head lands on the destination band's own content top");
        Assert.AreEqual(container.PageIndexOf(head.Location.Y), container.PageIndexOf(wrap.Location.Y),
            "the container follows the pulled run rather than being left spanning the boundary");
        Assert.AreEqual(container.PageIndexOf(head.Location.Y), container.PageIndexOf(body.Location.Y));
    }

    // Same document with and without the wrapping container should put the heading on the same page.
    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task FirstChildRelocation_MatchesTheEquivalentSiblingShape()
    {
        static string Document(string open, string close) =>
            "<div id='lead' style='height:400px'>lead</div>"
            + "<h2 id='head' style='margin:0;break-after:avoid'>Head</h2>"
            + open
            + "<div id='body' style='margin-top:800px;height:80px'>body</div>"
            + close;

        var (nested, container) = await BuildAsync(Document("<div id='wrap'>", "</div>"));
        var (flat, _) = await BuildAsync(Document("", ""));

        var nestedHead = FindById(nested, "head");
        var flatHead = FindById(flat, "head");
        Assert.IsNotNull(nestedHead);
        Assert.IsNotNull(flatHead);

        Assert.AreEqual(container.PageIndexOf(flatHead.Location.Y), container.PageIndexOf(nestedHead.Location.Y));
    }

    // css-break-3 §5.2 keeps the margin of a box whose forced break isn't taken.
    [TestMethod]
    [Ignore("The crossing margin is truncated even when the box carries a forced break-before that isn't taken")]
    public async Task FirstBoxInTheFlow_CarryingAnUntakenForcedBreak_KeepsItsMargin()
    {
        var (root, _) = await BuildAsync(
            "<div id='wrap' style='border-top:1px solid black'>"
            + "<div id='only' style='break-before:page;margin-top:1200px;height:80px'>only</div>"
            + "</div>");

        var wrap = FindById(root, "wrap");
        var only = FindById(root, "only");
        Assert.IsNotNull(only);

        Assert.AreEqual(wrap.ClientTop + 1200, only.Location.Y, 1);
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task FirstChildMarginCollapsingToTheRoot_IsStillTruncated()
    {
        var (root, container) = await BuildAsync(FirstChildDocument(""));

        var first = FindById(root, "first");
        Assert.IsNotNull(first);

        Assert.AreEqual(container.PageTopOf(1), first.Location.Y, 1,
            "the margin is truncated even when it collapses through to the root");
    }

    #endregion
}
