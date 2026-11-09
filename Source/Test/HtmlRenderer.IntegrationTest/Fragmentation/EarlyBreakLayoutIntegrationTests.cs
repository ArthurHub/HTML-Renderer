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
/// A box moved to the next page is laid out again there instead of being translated, on a 200px page with
/// 20px margins (band <c>[20, 220)</c>).
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class EarlyBreakLayoutIntegrationTests
{
    private const double PageHeight = 200;
    private const int Margin = 20;
    private const double LineHeight = 20;

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

    /// <summary>The gap a translation carries shows up as one line sitting further below its predecessor
    /// than the line height accounts for.</summary>
    private static void AssertLinesAreEvenlySpaced(CssBox card)
    {
        var tops = card.LineBoxes
            .SelectMany(l => l.Words)
            .Select(w => System.Math.Round(w.Top, 3))
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        Assert.IsTrue(tops.Count > 1, "fixture must produce more than one line for spacing to mean anything");

        for (var i = 1; i < tops.Count; i++)
        {
            Assert.IsTrue(tops[i] - tops[i - 1] <= LineHeight + 0.5,
                $"line {i} sits {tops[i] - tops[i - 1]:F1}px below its predecessor, more than the {LineHeight}px "
                + "line height - a fragmentainer gap carried inside the box");
        }
    }

    private static string GapDocument(double fillerHeight, string cardCss) =>
        $"<div style='height:{fillerHeight}px'>filler</div>"
        + $"<div id='card' style='{cardCss};orphans:1;widows:1;line-height:{LineHeight}px;font-size:10px;width:60px'>Aaa Bbb Ccc Ddd Eee Fff Ggg Hhh</div>";

    // Sweeps filler heights, where the card starts straddling depends on font metrics.
    [TestMethod]
    public async Task RelocatedBox_HasNoInteriorGap()
    {
        var checkedAny = false;
        for (var filler = 100.0; filler < 200; filler += 5)
        {
            var (root, container) = await BuildAsync(GapDocument(filler, "break-inside:avoid"));
            var card = FindById(root, "card");
            if (card == null) continue;

            if (container.PageIndexOf(card.EffectiveTop) != container.PageIndexOf(card.ActualBottom - 0.01))
                continue; // relocation failed to land it on a single page - not the case under test

            checkedAny = true;
            AssertLinesAreEvenlySpaced(card);
        }

        Assert.IsTrue(checkedAny, "no filler height in range produced a relocatable card - test is not meaningful as written");
    }

    [TestMethod]
    public async Task RelocatedMonolithicBox_HasNoInteriorGap()
    {
        var checkedAny = false;
        for (var filler = 100.0; filler < 200; filler += 5)
        {
            var (root, _) = await BuildAsync(GapDocument(filler, "overflow:hidden"));
            var card = FindById(root, "card");
            if (card == null) continue;

            checkedAny = true;
            AssertLinesAreEvenlySpaced(card);
        }

        Assert.IsTrue(checkedAny, "test is not meaningful as written");
    }

    // A translated box keeps the gap as height it does not use, so its height depends on where it happened
    // to straddle. Laid out again, it is the height of its own content wherever it lands.
    [TestMethod]
    public async Task RelocatedBox_IsNoTallerThanTheSameBoxThatNeverMoved()
    {
        var (undisturbed, _) = await BuildAsync(GapDocument(0, "break-inside:avoid"));
        var settled = HeightOfCard(undisturbed);

        for (var filler = 100.0; filler < 200; filler += 5)
        {
            var (root, _) = await BuildAsync(GapDocument(filler, "break-inside:avoid"));
            Assert.AreEqual(settled, HeightOfCard(root), 1.0);
        }
    }

    private static double HeightOfCard(CssBox root)
    {
        var card = FindById(root, "card");
        return System.Math.Round(card.ActualBottom - card.EffectiveTop, 3);
    }

    // An avoid that can't be satisfied is dropped instead of moving the box down page by page.
    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task BoxTallerThanTheBand_MovesAtMostOnce()
    {
        var lines = string.Concat(Enumerable.Range(0, 14).Select(i => $"Line {i}<br>"));
        var html = "<div style='height:130px'>filler</div>"
            + $"<div id='card' style='break-inside:avoid;orphans:1;widows:1;line-height:{LineHeight}px;font-size:10px'>{lines}</div>";

        var (root, container) = await BuildAsync(html);
        var card = FindById(root, "card");
        Assert.IsNotNull(card);

        var tops = Walk(card).SelectMany(b => b.Words).Select(w => w.Top).Distinct().ToList();
        Assert.IsTrue(tops.Count > 0, "fixture must produce words for this to test anything");
        var span = tops.Max() - tops.Min();
        Assert.IsTrue(span > container.PageSize.Height,
            $"fixture must be taller than one band for this to test relaxation, was {span:F1}");

        Assert.IsTrue(container.PageIndexOf(tops.Min()) <= 1,
            $"a box that fits nowhere must not walk down the document, but its first line landed at y={tops.Min():F1}");
    }

    // orphans/widows reaches the same mechanism, so it gets the same guarantee.
    [TestMethod]
    public async Task OrphansPushedParagraph_HasNoInteriorGap()
    {
        var html = "<div style='height:145px'>filler</div>"
            + $"<div id='card' style='orphans:3;widows:3;line-height:{LineHeight}px;font-size:10px;width:60px'>Aaa Bbb Ccc Ddd Eee Fff Ggg Hhh</div>";

        var (root, _) = await BuildAsync(html);
        AssertLinesAreEvenlySpaced(FindById(root, "card"));
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task PulledRun_MovesTogetherToTheDestinationBandTop()
    {
        var checkedAny = false;
        for (var filler = 80.0; filler < 160; filler += 5)
        {
            var (heading, card, container) = await PulledRunAsync(filler);
            if (heading == null || card == null) continue;

            var headingPage = container.PageIndexOf(heading.EffectiveTop);
            if (container.PageIndexOf(card.EffectiveTop) != headingPage) continue;
            if (headingPage == 0) continue; // not actually pulled anywhere - nothing to check here

            checkedAny = true;
            Assert.IsTrue(heading.ActualBottom <= card.EffectiveTop + 1.0,
                "the heading must still sit above the box it is chained to");
            Assert.AreEqual(container.PageTopOf(headingPage), heading.EffectiveTop, 1.0);
        }

        Assert.IsTrue(checkedAny, "test is not meaningful as written");
    }

    // Where the run is still part of the pass being laid out, it is re-run rather than moved, so the box
    // that pulled it re-flows at its new position like any other relocated box.
    [TestMethod]
    public async Task PulledRun_IsLaidOutAgain_SoTheBoxHasNoInteriorGap()
    {
        var checkedAny = false;
        for (var filler = 80.0; filler < 160; filler += 5)
        {
            var (heading, card, container) = await PulledRunAsync(filler);
            if (heading == null || card == null) continue;
            if (container.PageIndexOf(heading.EffectiveTop) == 0) continue;

            checkedAny = true;
            AssertLinesAreEvenlySpaced(card);
        }

        Assert.IsTrue(checkedAny, "test is not meaningful as written");
    }

    // Every word is claimed by exactly one fragment, no ghost and no lost content.
    [TestMethod]
    public async Task PulledRun_ClaimsEveryWordExactlyOnce()
    {
        for (var filler = 80.0; filler < 160; filler += 20)
        {
            var (_, _, container) = await PulledRunAsync(filler);

            var claimed = container.FragmentTree!.Fragmentainers
                .SelectMany(f => Flatten(f.Root))
                .SelectMany(f => f.Words)
                .Select(w => w.Word)
                .ToList();

            Assert.IsTrue(claimed.Count > 0);
            Assert.AreEqual(claimed.Count, claimed.Distinct().Count());
        }
    }

    private static async Task<(CssBox Heading, CssBox Card, HtmlContainerInt Container)> PulledRunAsync(double fillerHeight)
    {
        var html = $"<div style='height:{fillerHeight}px'>filler</div>"
            + "<h2 id='heading' style='margin:0;break-after:avoid;font-size:10px;line-height:20px'>Heading</h2>"
            + $"<div id='card' style='break-inside:avoid;orphans:1;widows:1;line-height:{LineHeight}px;font-size:10px;width:60px'>Aaa Bbb Ccc Ddd Eee Fff Ggg Hhh</div>";

        var (root, container) = await BuildAsync(html);
        return (FindById(root, "heading"), FindById(root, "card"), container);
    }

    // ── §3.1 propagation (the container travels too) ──────────────────────────────────────────────

    // css-break-3 §3.1: the break before a first child is the break before its container.
    [TestMethod]
    [DataRow("break-inside:avoid")]
    [DataRow("overflow:hidden")]
    public async Task RelocatedFirstChild_TakesItsContainerWithIt(string cardCss)
    {
        var html = "<div style='height:170px'>filler</div>"
            + "<div id='wrapper' style='background:#eee;border:1px solid #000'>"
            + $"<div id='card' style='{cardCss};orphans:1;widows:1;line-height:{LineHeight}px;font-size:10px;width:60px'>Aaa Bbb Ccc Ddd Eee Fff Ggg Hhh</div></div>";

        var (root, container) = await BuildAsync(html);
        var wrapper = FindById(root, "wrapper");
        var card = FindById(root, "card");
        Assert.IsNotNull(wrapper);
        Assert.IsNotNull(card);

        var nextBandTop = container.PageTopOf(1);
        Assert.AreEqual(nextBandTop, wrapper.Location.Y, 1.0);
        Assert.IsTrue(card.EffectiveTop >= wrapper.Location.Y - 0.001,
            $"the card must sit inside its wrapper, but is at {card.EffectiveTop:F1} against {wrapper.Location.Y:F1}");

        Assert.IsTrue(wrapper.Location.Y > container.PageTopOf(0) + 1.0);
    }

    // A container that doesn't fit the destination stays, the box moves alone.
    [TestMethod]
    public async Task RelocatedFirstChild_LeavesAContainerThatDoesNotFitTheDestination()
    {
        // The wrapper's extent from its top to the card's bottom is 180px against a 160px band.
        var html = "<div style='height:20px'>filler</div>"
            + "<div id='wrapper' style='padding-top:170px;background:#eee;border:1px solid #000'>"
            + $"<div id='card' style='break-inside:avoid;orphans:1;widows:1;line-height:{LineHeight}px;font-size:10px;width:60px'>Aaa Bbb Ccc Ddd Eee Fff Ggg Hhh</div></div>";

        var (root, container) = await BuildAsync(html);
        var wrapper = FindById(root, "wrapper");
        var card = FindById(root, "card");
        Assert.IsNotNull(wrapper);
        Assert.IsNotNull(card);

        Assert.AreEqual(0, container.PageIndexOf(wrapper.Location.Y));
        Assert.AreEqual(1, container.PageIndexOf(card.EffectiveTop));
    }

    [TestMethod]
    [Spec("css21", "page.html#page-break-props")]
    public async Task BoxContainingARepeatingTable_IsStillRelocated()
    {
        var rows = string.Concat(Enumerable.Range(0, 4).Select(i => $"<tr><td>Row {i}</td></tr>"));
        var html = "<div style='height:170px'>filler</div>"
            + "<div id='card' style='break-inside:avoid;orphans:1;widows:1;font-size:10px;line-height:20px'>"
            + $"<table style='border-collapse:collapse'><thead><tr><th>Heading</th></tr></thead><tbody>{rows}</tbody></table></div>";

        var (root, container) = await BuildAsync(html);
        var card = FindById(root, "card");
        Assert.IsNotNull(card);

        Assert.AreEqual(1, container.PageIndexOf(card.EffectiveTop));
        Assert.AreEqual(container.PageTopOf(1), card.EffectiveTop, 1.0);
    }
}
