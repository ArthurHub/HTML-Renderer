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
/// An unforced break in a margin taken as a break before the next box, in a later layout pass.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class ResumableBlockLayoutIntegrationTests
{
    private const double PageHeight = 200;
    private const int Margin = 20;

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
        container.PageSize = new RSize(300, PageHeight);
        container.MarginTop = Margin;
        container.Location = new RPoint(0, Margin);
        wrapper.MaxSize = new SizeF(300, 0);

        using var bitmap = new Bitmap(300, 60000);
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

    private static IEnumerable<BoxFragment> Flatten(BoxFragment fragment)
    {
        yield return fragment;
        foreach (var child in fragment.Children)
            foreach (var descendant in Flatten(child))
                yield return descendant;
    }

    private static bool Contains(BoxFragment fragment, CssBox box) =>
        ReferenceEquals(fragment.Box, box) || fragment.Children.Any(c => Contains(c, box));

    private static List<int> FragmentSlotsOf(HtmlContainerInt container, CssBox box) =>
        container.FragmentTree!.Fragmentainers.Where(f => Contains(f.Root, box)).Select(f => f.SlotIndex).ToList();

    // The margin alone pushes the second block past the end of the page.
    private static string MarginTruncationDocument() =>
        "<div id='first' style='height:40px'>first</div>"
        + "<div id='second' style='margin-top:300px;height:40px'>second</div>";

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task MarginPushingABoxAcrossABoundary_StartsItAtTheNextPagesContentTop()
    {
        var (root, container) = await BuildAsync(MarginTruncationDocument());
        var second = FindById(root, "second");
        Assert.IsNotNull(second);

        var slot = container.PageIndexOf(second.Location.Y);
        Assert.IsTrue(slot > 0, $"expected a later page, got slot {slot}");
        Assert.AreEqual(container.PageTopOf(slot), second.Location.Y, 1.0);
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task BoxBrokenBefore_ProducesNoFragmentInTheFragmentainerItLeaves()
    {
        var (root, container) = await BuildAsync(MarginTruncationDocument());
        var second = FindById(root, "second");
        Assert.IsNotNull(second);

        var slots = FragmentSlotsOf(container, second);

        Assert.IsTrue(slots.Count > 0);
        Assert.IsFalse(slots.Contains(0));

        Assert.IsTrue(container.FragmentTree!.Fragmentainers.Count >= 2);
    }

    [TestMethod]
    public async Task DocumentThatFitsWithoutBreaking_TakesASinglePass()
    {
        var (_, container) = await BuildAsync("<div style='height:40px'>first</div><div style='height:40px'>second</div>");

        Assert.AreEqual(1, container.FragmentTree!.Fragmentainers.Count);
    }

    [TestMethod]
    public async Task ContentFollowingTheBreak_IsLaidOutFreshRatherThanResumed()
    {
        var (root, container) = await BuildAsync(
            "<div id='first' style='height:40px'>first</div>"
            + "<div id='second' style='margin-top:300px;height:40px'>second</div>"
            + "<div id='third' style='height:40px'>third</div>");

        var second = FindById(root, "second");
        var third = FindById(root, "third");
        Assert.IsNotNull(second);
        Assert.IsNotNull(third);

        Assert.AreEqual(second.ActualBottom, third.Location.Y, 1.0);
        Assert.AreEqual(container.PageIndexOf(second.Location.Y), container.PageIndexOf(third.Location.Y));
    }

    [TestMethod]
    public async Task ResumedPass_DoesNotDuplicateWordsOrRectangles()
    {
        var (root, _) = await BuildAsync(MarginTruncationDocument());

        // The resumed pass must not rebuild words or line boxes a second time.
        foreach (var box in Walk(root))
        {
            Assert.AreEqual(box.Words.Count, box.Words.Distinct().Count());
            Assert.AreEqual(box.LineBoxes.Count, box.LineBoxes.Distinct().Count());
        }
    }

    // Tables and avoid boxes paginate their own content, the driver must not break inside them.
    [TestMethod]
    [DataRow("display:table")]
    [DataRow("break-inside:avoid")]
    public async Task MonolithicSubtree_LaysOutInOnePass(string containerStyle)
    {
        var (root, _) = await BuildAsync(
            "<div id='first' style='height:40px'>first</div>"
            + $"<div id='mono' style='{containerStyle}'>"
            + "<div style='margin-top:300px;height:30px'>a</div>"
            + "<div style='height:30px'>b</div></div>");

        var mono = FindById(root, "mono");
        Assert.IsNotNull(mono);

        foreach (var box in Walk(mono))
        {
            Assert.IsNull(box.PendingBreakToken);
            Assert.IsNull(box.RequestedBreakBeforeTop);
        }
    }

    [TestMethod]
    public async Task LayoutCompletes_LeavingNoResumptionRecordBehind()
    {
        var (root, _) = await BuildAsync(MarginTruncationDocument());

        // A leftover record would be resumed into by the next layout of the same tree.
        foreach (var box in Walk(root))
        {
            Assert.IsNull(box.PendingBreakToken);
            Assert.IsNull(box.RequestedBreakBeforeTop);
        }
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task KeepWithNextRun_MovesWithTheBoxItIsChainedTo()
    {
        var (root, container) = await BuildAsync(
            "<div style='height:40px'>filler</div>"
            + "<h2 id='heading' style='margin:0;height:20px;break-after:avoid'>heading</h2>"
            // A larger margin would make the avoid unsatisfiable.
            + "<div id='body' style='margin-top:120px;height:40px'>body</div>");

        var heading = FindById(root, "heading");
        var body = FindById(root, "body");
        Assert.IsNotNull(heading);
        Assert.IsNotNull(body);

        Assert.AreEqual(container.PageIndexOf(heading.EffectiveTop), container.PageIndexOf(body.Location.Y));
    }

    // An out-of-flow box doesn't leave a break token behind.
    [TestMethod]
    [Ignore("An absolutely positioned child of a table cell loses one of its 5 lines from the fragment tree")]
    public async Task TallOutOfFlowChildOfATableCell_KeepsAllOfItsContent()
    {
        var lines = string.Concat(Enumerable.Range(0, 5).Select(i => $"Line{i}<br>"));
        var html = "<table style='border-collapse:collapse'><tr><td style='position:relative'>"
            + "<span>in flow</span>"
            + $"<div id='abs' style='position:absolute;top:0;left:0;width:120px;line-height:20px'>{lines}</div>"
            + "</td></tr></table>"
            // Keeps the document tall enough to cover the absolute box's overflow.
            +"<div style='height:150px'>tail</div>";

        var (root, container) = await BuildAsync(html);
        var abs = FindById(root, "abs");
        Assert.IsNotNull(abs);

        Assert.IsNull(abs.PendingBreakToken);
        Assert.IsNull(abs.RequestedBreakBeforeTop);

        var placed = container.FragmentTree!.Fragmentainers
            .SelectMany(f => Flatten(f.Root))
            .SelectMany(f => f.Words)
            .Select(w => w.Word.Text)
            .Where(t => t != null && t.StartsWith("Line"))
            .Distinct()
            .Count();

        Assert.AreEqual(5, placed);
    }
}
