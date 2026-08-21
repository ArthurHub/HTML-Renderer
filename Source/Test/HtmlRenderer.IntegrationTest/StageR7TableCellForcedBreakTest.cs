using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Fragments;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest;

/// <summary>
/// Verifies a real regression found while investigating the fragmentation-engine-parity plan's R7 stage
/// (table resumption), introduced by R1's forced-break deferral: <c>CssLayoutEngineTable</c>'s row loop
/// calls <c>cell.PerformLayout</c> directly and does not participate in the <c>PendingBreakToken</c>
/// bubbling protocol an ordinary block-child loop does. A forced break nested inside a table cell (e.g. a
/// <c>&lt;div style="break-before:page"&gt;</c> inside a <c>&lt;td&gt;</c>) would request deferral to a
/// later pass exactly like any other box - but nothing ever reads that request or resumes it, since a
/// table row is not itself laid out via the block-child loop. The deferred content's own layout returned
/// before ever calling <c>CreateLineBoxes</c>, yet its words had already been measured (unconditional,
/// at the top of every <c>PerformLayoutImp</c> call) - so it ended up rendered at a stale/default (0,0)
/// position, silently overlapping whatever else was there, rather than being lost outright or correctly
/// paginated. Confirmed by direct fragment-tree inspection before the fix: the word appeared, but at the
/// wrong position, with no new page created for it.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class StageR7TableCellForcedBreakTest
{
    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    /// <summary>
    /// Reconstructs each word's ABSOLUTE document-Y (fragment rects are page-band-local, so comparing
    /// raw <c>Rect.Top</c> values across different fragmentainers is meaningless - a word at local Y=0
    /// on page 2 is not "above" a word at local Y=10 on page 1).
    /// </summary>
    private static void CollectWordsWithAbsoluteY(BoxFragment f, double bandTop, List<(string Text, double AbsoluteTop)> into)
    {
        foreach (var w in f.Words)
            if (!w.Word.IsLineBreak)
                into.Add((w.Word.Text, w.Rect.Top + bandTop));
        foreach (var c in f.Children)
            CollectWordsWithAbsoluteY(c, bandTop, into);
    }

    [TestMethod]
    public async Task ForcedBreakInsideTableCell_DoesNotOverlapOrLoseContent()
    {
        using var wrapper = new HtmlContainer();
        await wrapper.SetHtml(
            """
            <html><body>
                <table><tr><td>
                    <div style="margin:0;">BeforeMarker</div>
                    <div style="margin:0; break-before: page;">AfterMarker</div>
                </td></tr></table>
            </body></html>
            """);

        var container = GetInternal(wrapper);
        container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(500, 700);
        container.MarginTop = 0;
        wrapper.MaxSize = new SizeF(500, 0);

        using var bitmap = new Bitmap(500, 5000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        var tree = container.FragmentTree;
        Assert.IsNotNull(tree);

        var words = new List<(string Text, double AbsoluteTop)>();
        foreach (var f in tree.Fragmentainers)
            CollectWordsWithAbsoluteY(f.Root, f.LocalOriginY, words);

        var before = words.Find(w => w.Text == "BeforeMarker");
        var after = words.Find(w => w.Text == "AfterMarker");

        Assert.IsNotNull(before.Text, "BeforeMarker must still be present");
        Assert.IsNotNull(after.Text, "AfterMarker must still be present - not silently dropped");

        // The real regression: AfterMarker rendered at the SAME position as BeforeMarker (or at a
        // stale/default position near zero) rather than being placed below it in normal document flow.
        Assert.IsGreaterThan(before.AbsoluteTop, after.AbsoluteTop,
            $"AfterMarker (absoluteTop={after.AbsoluteTop}) must render below BeforeMarker (absoluteTop={before.AbsoluteTop}), not overlapping it");
    }
}
