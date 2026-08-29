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
/// Ported from PeachPDF.Tests/Integration/FragmentainerCursorIntegrationTests.cs: a forced break steps a
/// layout pass over slots without ending it, so the band a later correction (orphans) should reason about
/// is the one the break actually landed on, not the one the pass nominally started in.
/// </summary>
/// <remarks>
/// Adapted per the port plan: PeachPDF's <c>FragmentainerContext</c> (an explicit per-pass "which
/// fragmentainer is being filled" cursor object) has no counterpart here. This port has no separate cursor
/// at all - <c>InlineFragmentation.ApplyLineBreaking</c> always computes <c>firstPageIndex</c> directly from
/// <c>lines[0].LineTop</c>, the line's own real, already-placed position (which already reflects wherever a
/// forced break/margin-truncation/relocation put the box), so there is no stale-cursor state that could
/// disagree with it. These 3 tests (of PeachPDF's 5 - 2 more dropped, both using the directional
/// <c>break-before: right</c> value, unsupported per the port plan's exclusion list) are ported as
/// regression checks against the equivalent real-geometry reasoning.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class FragmentainerCursorIntegrationTests
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

    private static string LongText() => string.Join(" ", Enumerable.Range(0, 600).Select(i => "word" + i));

    // orphans: 99 is a minimum no band here can satisfy, which is §5.4 read through §4.3's relaxation
    // ladder: the constraint is given up rather than acted on pointlessly. A box placed by a forced break
    // is already at the top of a fresh page - moving it again (as if there were a whole page of content
    // above it) would blank the page the forced break named.
    [TestMethod]
    public async Task AForcedBreak_LandsOnThePageItNames_EvenWhenTheBoxCannotMeetItsOrphansMinimum()
    {
        var html = "<div id='a' style='height:50px'>A</div>"
            + $"<div id='b' style='break-before:page;orphans:99;font:10px Arial'>{LongText()}</div>";

        var (root, container) = await BuildAsync(html);
        var b = FindById(root, "b");
        Assert.IsNotNull(b);

        Assert.AreEqual(container.PageTopOf(1), b.EffectiveTop, 1.0);

        // And no blank page in the middle: every fragmentainer from the first to the last carries content.
        var slots = container.FragmentTree!.Fragmentainers.Select(f => f.SlotIndex).ToArray();
        Assert.IsTrue(Enumerable.Range(0, slots.Length).SequenceEqual(slots));
    }

    // The other side of the same reasoning: here the box with the unsatisfiable orphans minimum is not the
    // one the forced break placed - it follows it on the same page - so there genuinely is something above
    // it, and the orphans mover is entitled to fire and start it on the next page.
    [TestMethod]
    public async Task AboveTheForcedBreaksBox_IsStillRoomAbove_ForWhatFollowsItOnThatPage()
    {
        var html = "<div id='a' style='height:50px'>A</div>"
            + "<div id='b' style='break-before:page;font:10px Arial'>B</div>"
            + $"<div id='c' style='orphans:99;font:10px Arial'>{LongText()}</div>";

        var (root, container) = await BuildAsync(html);
        var b = FindById(root, "b");
        var c = FindById(root, "c");
        Assert.IsNotNull(b);
        Assert.IsNotNull(c);

        Assert.AreEqual(container.PageTopOf(1), b.EffectiveTop, 1.0);
        Assert.AreEqual(container.PageTopOf(2), c.EffectiveTop, 1.0);
    }

    // The shape most likely to have relied on a wrong cursor: content that overflows the fragmentainer the
    // break stepped to. A box taller than the band still starts on the page the break named, and the box
    // after it picks up at its real bottom, not at the bottom of some other band.
    [TestMethod]
    public async Task AfterAForcedBreak_ABoxTallerThanTheBand_StillStartsOnThePageTheBreakNamed()
    {
        var html = "<div id='x' style='height:50px'>X</div>"
            + "<div id='tall' style='break-before:page;height:900px'>TALL</div>"
            + "<div id='after' style='font:10px Arial'>after</div>";

        var (root, container) = await BuildAsync(html);
        var tall = FindById(root, "tall");
        var after = FindById(root, "after");
        Assert.IsNotNull(tall);
        Assert.IsNotNull(after);

        Assert.AreEqual(container.PageTopOf(1), tall.Location.Y, 1.0);
        Assert.AreEqual(tall.ActualBottom, after.Location.Y, 1.0);
    }
}
