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
/// Ported from PeachPDF.Tests/Integration/BreakPropagationIntegrationTests.cs: css-break-3 §3.1 forced-
/// break combination and propagation - whether a break point stated before/after a box travels up to an
/// ancestor that begins/ends with it.
/// </summary>
/// <remarks>
/// Confirmed, by reading both the call site (<c>CssBox.PerformLayoutImp</c>'s child loop, which calls
/// <c>BlockFragmentation.TryGetForcedBreakTarget(this, prevSibling, ...)</c> with <c>prevSibling</c> scoped
/// to the box's OWN parent's child list) and <c>TryGetForcedBreakTarget</c>'s own remark: unlike PeachPDF's
/// <c>BreakPropagation.PropagatesBreakBeforeOutward</c>, this port does NOT climb the ancestor chain for a
/// forced <c>break-before</c>/<c>break-after</c> - a first-in-flow child's forced break-before is
/// suppressed outright (never redirected onto its parent), and a last-in-flow child's <c>break-after</c>
/// never bubbles up to make its parent's own <c>BreakAfter</c> "page" either (<c>CssBoxProperties</c> has
/// no such cascade - confirmed by reading the <c>BreakAfter</c>/<c>BreakBefore</c> property getters, plain
/// backing-field reads with no ancestor lookup). This is explicitly called out as "out of scope for this
/// port" in <c>TryGetForcedBreakTarget</c>'s own doc remark. Real ancestor-following DOES exist, but only
/// for the RELOCATION-triggered movers (<c>BlockFragmentation.PropagateContainerRelocation</c>, called from
/// both <c>RelocateIfNeeded</c> and <c>EnforceKeepWithNext</c>) - confirmed working end to end by this
/// repo's own pre-existing <c>ContainerLeftBehindTest.cs</c>/<c>ContainerLeftBehindKeepWithNextTest.cs</c>.
/// Three tests below (<c>ForcedBreakBeforeAFirstChild_MovesTheContainer</c>,
/// <c>ForcedBreakBeforeANestedFirstChild_MovesTheOutermostContainerItBegins</c>,
/// <c>BreakAfterOnALastChild_ForcesTheBreakBeforeTheFollowingSibling</c>) are ported with their PeachPDF
/// assertions intact but <c>[Ignore]</c>d against this confirmed gap; a fourth
/// (<c>AForcedBreakPropagatedOutOfAContainer_BreaksTheKeepWithNextChain</c>) is dropped rather than
/// Ignored, since its premise (a forced break that travelled out of the container) never occurs here at
/// all, so there is nothing left of the scenario to characterize as pending.
/// <para>
/// Also dropped: the 2 directional-break-value tests (<c>recto</c>/<c>verso</c>, unsupported per the port
/// plan's exclusion list) and <c>ForcedBreakBeforeAnEngineItem_DoesNotTravelOutOfTheEngine</c> (flex/grid -
/// no such layout engine exists in this port, and PeachPDF's own <c>BreakPropagation</c> type it asserts
/// against has no counterpart here either).
/// </para>
/// </remarks>
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

    #region Propagation moves the container, not only the child (confirmed gap - forced breaks only)

    [TestMethod]
    [Ignore("Confirmed gap: BlockFragmentation.TryGetForcedBreakTarget suppresses a forced break-before on a "
        + "first-in-flow child (no previous sibling) rather than redirecting it onto the parent - see this "
        + "method's own doc remark ('Full cross-ancestor propagation is out of scope for this port'). Unlike "
        + "PeachPDF, the break is simply dropped: 'wrap' never moves and 'first' lands wherever ordinary flow "
        + "puts it, both on the original page.")]
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
    [Ignore("Same confirmed gap as ForcedBreakBeforeAFirstChild_MovesTheContainer - see this class's own doc "
        + "remark - just nested one level deeper.")]
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

    // A box with an in-flow sibling above it names its own break point directly (prevSibling != null in
    // TryGetForcedBreakTarget), so nothing needs to propagate and the container stays where it is.
    [TestMethod]
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

    // §3.1 propagation would stop before breaking through the fragmentation root, so the chain here reaches
    // the root either way (whether or not ancestor propagation exists) and no break is taken - which is
    // also §4.4's "no empty fragmentainer" falling out rather than being asserted. Passes in this port for
    // the same *suppression* that makes the two Ignored tests above fail their PeachPDF assertions - the
    // observable outcome (nothing moves, single page) happens to coincide here.
    [TestMethod]
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
    [Ignore("Confirmed gap: a container's own BreakAfter is a plain CSS-cascaded backing field "
        + "(CssBoxProperties.BreakAfter) with no bubbling from its last in-flow child's break-after - "
        + "TryGetForcedBreakTarget only ever tests prevSibling.BreakAfter directly, and prevSibling here is "
        + "'wrap' itself (whose own break-after was never set), not 'tail' (whose break-after:page never "
        + "reaches 'wrap'). So 'next' is not pushed to a new page at all.")]
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

    #region Keep-with-next across a container (relocation-triggered - confirmed working)

    // The §4.3 movers (RelocateIfNeeded/EnforceKeepWithNext) reach ancestor-propagation through a real,
    // confirmed mechanism (PropagateContainerRelocation): the run is collected, the anchor (the first
    // in-flow box the relocated box begins) travels, and the child loop that owns the run positions it.
    [TestMethod]
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
