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
/// Later corrections such as orphans reason about the page a forced break actually landed on.
/// </summary>
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

    // orphans:99 can't be satisfied, and moving a box that already starts a page wouldn't help.
    [TestMethod]
    [Spec("css21", "page.html#forced")]
    public async Task AForcedBreak_LandsOnThePageItNames_EvenWhenTheBoxCannotMeetItsOrphansMinimum()
    {
        var html = "<div id='a' style='height:50px'>A</div>"
            + $"<div id='b' style='break-before:page;orphans:99;font:10px Arial'>{LongText()}</div>";

        var (root, container) = await BuildAsync(html);
        var b = FindById(root, "b");
        Assert.IsNotNull(b);

        Assert.AreEqual(container.PageTopOf(1), b.EffectiveTop, 1.0);

        // No blank page in between.
        var slots = container.FragmentTree!.Fragmentainers.Select(f => f.SlotIndex).ToArray();
        Assert.IsTrue(Enumerable.Range(0, slots.Length).SequenceEqual(slots));
    }

    // Here content precedes the orphans box on its page, so moving it to the next page does help.
    [TestMethod]
    [Spec("css21", "page.html#forced")]
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

    // A box taller than the page still starts on the page the break named.
    [TestMethod]
    [Spec("css21", "page.html#forced")]
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
