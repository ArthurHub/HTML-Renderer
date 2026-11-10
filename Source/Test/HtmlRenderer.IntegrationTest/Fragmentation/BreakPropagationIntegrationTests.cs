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
/// Whether a forced break or a keep stated on a first or last child applies to the break point of its ancestors.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class BreakPropagationIntegrationTests
{
    private const double PageHeight = 300;

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
        container.MarginTop = 0;
        container.Location = new RPoint(0, 0);
        wrapper.MaxSize = new SizeF(400, 0);

        using var bitmap = new Bitmap(400, 40000);
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

    private static CssBox FindById(CssBox root, string id)
    {
        foreach (var box in Walk(root))
            if (box.HtmlTag?.TryGetAttribute("id") == id)
                return box;
        return null!;
    }

    private static int SlotOf(HtmlContainerInt container, CssBox box) => container.PageIndexOf(box.Location.Y);

    #region Propagation moves the container, not only the child

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "page.html#forced")]
    [Spec("css-break-3", "#break-propagation")]
    public async Task ForcedBreakBeforeAFirstChild_MovesTheContainer()
    {
        var (root, container) = await BuildAsync(
            "<div id='lead' style='height:60pt'>lead</div>"
            + "<div id='wrap' style='border:1pt solid black'>"
            + "<div id='first' style='break-before:page;height:40pt'>first</div>"
            + "</div>");

        var wrap = FindById(root, "wrap");
        var first = FindById(root, "first");
        Assert.IsNotNull(wrap);
        Assert.IsNotNull(first);

        Assert.AreEqual(container.PageTopOf(1), wrap.Location.Y, 3);
        Assert.AreEqual(wrap.ClientTop, first.Location.Y, 3);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "page.html#forced")]
    [Spec("css-break-3", "#break-propagation")]
    public async Task ForcedBreakBeforeANestedFirstChild_MovesTheOutermostContainerItBegins()
    {
        var (root, container) = await BuildAsync(
            "<div id='lead' style='height:60pt'>lead</div>"
            + "<div id='outer'><div id='inner'>"
            + "<div id='deep' style='break-before:page;height:40pt'>deep</div>"
            + "</div></div>");

        var outer = FindById(root, "outer");
        Assert.IsNotNull(outer);

        Assert.AreEqual(container.PageTopOf(1), outer.Location.Y, 3);
    }

    [TestMethod]
    [Spec("css21", "page.html#forced")]
    [Spec("css-break-3", "#forced-breaks")]
    public async Task ForcedBreakBeforeALaterChild_LeavesTheContainerWhereItIs()
    {
        var (root, container) = await BuildAsync(
            "<div id='wrap'>"
            + "<div id='first' style='height:60pt'>first</div>"
            + "<div id='second' style='break-before:page;height:40pt'>second</div>"
            + "</div>");

        var wrap = FindById(root, "wrap");
        var second = FindById(root, "second");
        Assert.IsNotNull(wrap);
        Assert.IsNotNull(second);

        Assert.AreEqual(0, SlotOf(container, wrap));
        Assert.AreEqual(1, SlotOf(container, second));
    }

    [TestMethod]
    [Spec("css-break-3", "#break-propagation")]
    public async Task ForcedBreakBeforeTheFirstBoxInTheFlow_ManufacturesNoBlankPage()
    {
        var (root, container) = await BuildAsync(
            "<div id='wrap'><div id='only' style='break-before:page;height:40pt'>only</div></div>");

        var wrap = FindById(root, "wrap");
        Assert.IsNotNull(wrap);

        Assert.AreEqual(0, SlotOf(container, wrap));
        Assert.AreEqual(1, container.FragmentTree!.Fragmentainers.Count);
    }

    #endregion

    #region Combination and precedence (§3.1)

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "page.html#forced")]
    [Spec("css-break-3", "#break-propagation")]
    public async Task BreakAfterOnALastChild_ForcesTheBreakBeforeTheFollowingSibling()
    {
        var (root, container) = await BuildAsync(
            "<div id='wrap'><div id='tail' style='break-after:page;height:40pt'>tail</div></div>"
            + "<div id='next' style='height:40pt'>next</div>");

        var next = FindById(root, "next");
        Assert.IsNotNull(next);

        Assert.AreEqual(container.PageTopOf(1), next.Location.Y, 3);
    }

    #endregion

    #region Keep-with-next across a container

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    [Spec("css-break-3", "#unforced-breaks")]
    public async Task BreakInsideAvoidOnAFirstChild_PullsTheRunAcrossTheContainer()
    {
        var (root, container) = await BuildAsync(
            "<div id='lead' style='height:200pt'>lead</div>"
            + "<h2 id='head' style='margin:0;break-after:avoid'>Head</h2>"
            + "<div id='wrap'>"
            + "<div id='body' style='break-inside:avoid;height:150pt'>body</div>"
            + "</div>");

        var head = FindById(root, "head");
        var wrap = FindById(root, "wrap");
        var body = FindById(root, "body");
        Assert.IsNotNull(head);
        Assert.IsNotNull(wrap);
        Assert.IsNotNull(body);

        Assert.AreEqual(1, SlotOf(container, body));
        Assert.AreEqual(1, SlotOf(container, wrap));
        Assert.AreEqual(1, SlotOf(container, head));
    }

    #endregion
}
