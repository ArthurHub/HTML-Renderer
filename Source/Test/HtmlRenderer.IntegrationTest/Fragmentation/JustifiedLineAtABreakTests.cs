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
/// Ported from PeachPDF.Tests/Integration/JustifiedLineAtABreakTests.cs: CSS Text §7.3 exempts the last
/// line of a block from <c>text-align: justify</c> - a line that ends at a fragmentation break is not that
/// line (the block continues in the next fragmentainer), so it must still be justified like any other.
/// </summary>
/// <remarks>
/// Verified NOT to reproduce in this port, and ported anyway as a locked-in non-regression check (per the
/// port plan's guidance for a scenario that doesn't reproduce but is still meaningful to pin). PeachPDF's
/// bug depended on its own resumable-pass architecture: a pass that stops mid-block leaves
/// <c>LineBoxes</c> looking complete when it is not, so "is this line the last one" (read off
/// <c>LineBoxes.Count - 1</c>) gave a false positive for whatever line a pass happened to stop on.
/// HTML-Renderer's <c>CssLayoutEngine.CreateLineBoxes</c> computes an entire paragraph's lines in one
/// unbounded, side-effect-free call (confirmed by <c>InlineFragmentation.ApplyLineBreaking</c>'s own doc
/// comment: the "run of already-laid-out lines... is monolithic and never straddles" - fragmentation only
/// ever shifts already-finished lines' Y coordinates afterward, never touches <c>LineBoxes</c> membership),
/// so by the time <c>CssLayoutEngine.ApplyJustifyAlignment</c> reads <c>LineBoxes[LineBoxes.Count - 1]</c>
/// (its own exact check), that index always names the block's true last line, page break or not. All three
/// tests below pass unmodified from PeachPDF's own assertions - none needed adaptation or <c>[Ignore]</c>.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class JustifiedLineAtABreakTests
{
    private const string Style = "text-align:justify;font-size:10px;line-height:18px;orphans:1;widows:1;margin:0";

    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    private static async Task<(CssBox Root, HtmlContainerInt Container)> BuildAsync(string bodyHtml, double pageWidth = 200)
    {
        var wrapper = new HtmlContainer();
        await wrapper.SetHtml($"<html><body style='margin:0'>{bodyHtml}</body></html>");

        var container = GetInternal(wrapper);
        container.PageSize = new RSize(pageWidth, 300);
        container.MarginTop = 10;
        container.Location = new RPoint(0, 10);
        wrapper.MaxSize = new SizeF((float)pageWidth, 0);

        using var bitmap = new Bitmap((int)pageWidth, 20000);
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

    private static string Words(int count) => string.Join(" ", Enumerable.Range(0, count).Select(i => $"w{i}"));

    private static string Document(int wordCount) => $"<p id='p' style='{Style}'>{Words(wordCount)}</p>";

    /// <summary>The line a page break falls after is justified: its last word ends at the block's right
    /// edge, as every other justified line's does.</summary>
    [TestMethod]
    public async Task TheLineAPageBreakFallsAfter_IsJustified()
    {
        var (root, container) = await BuildAsync(Document(244));

        Assert.IsTrue(container.FragmentTree!.Fragmentainers.Count > 1, "fixture does not paginate, so it asserts nothing");

        var block = FindById(root, "p");
        Assert.IsNotNull(block);

        // The last line the first page kept - the one the break falls after.
        var lastOnFirstPage = block.LineBoxes.Last(line => container.PageIndexOf(line.Words[0].Top) == 0);

        Assert.AreEqual(block.ClientRight, lastOnFirstPage.Words[lastOnFirstPage.Words.Count - 1].Right, 1.0);
    }

    /// <summary>The control, and the half that must not regress: the block's real last line is still
    /// exempt.</summary>
    [TestMethod]
    public async Task TheBlocksOwnLastLine_IsNotJustified()
    {
        var (root, container) = await BuildAsync(Document(244));

        var block = FindById(root, "p");
        Assert.IsNotNull(block);
        var lastLine = block.LineBoxes[block.LineBoxes.Count - 1];

        Assert.IsTrue(container.FragmentTree!.Fragmentainers.Count > 1);
        Assert.IsTrue(lastLine.Words[lastLine.Words.Count - 1].Right < block.ClientRight - 1,
            $"the block's last line was justified: ends at {lastLine.Words[lastLine.Words.Count - 1].Right:F1} against a right edge of {block.ClientRight:F1}");
    }

    /// <summary>A block short enough not to break has exactly one exempt line and no break to confuse it
    /// with, which is what keeps the two tests above from both passing on a fixture that never
    /// justifies.</summary>
    [TestMethod]
    public async Task ABlockThatDoesNotBreak_JustifiesEveryLineButItsLast()
    {
        var (root, _) = await BuildAsync(Document(40), pageWidth: 200);

        var block = FindById(root, "p");
        Assert.IsNotNull(block);

        Assert.IsTrue(block.LineBoxes.Count > 2, "fixture must wrap onto several lines");
        foreach (var line in block.LineBoxes.Take(block.LineBoxes.Count - 1))
            Assert.AreEqual(block.ClientRight, line.Words[line.Words.Count - 1].Right, 1.0);

        var last = block.LineBoxes[block.LineBoxes.Count - 1];
        Assert.IsTrue(last.Words[last.Words.Count - 1].Right < block.ClientRight - 1);
    }
}
