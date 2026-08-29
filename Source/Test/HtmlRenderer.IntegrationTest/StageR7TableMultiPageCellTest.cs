using System.Drawing;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Fragments;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest;

/// <summary>
/// Verifies the fragmentation-engine-parity plan's R7 investigation finding: a table cell whose own
/// content spans several pages by itself (the content routes through the same, already-fixed
/// <c>CssBox.PerformLayoutImp</c>/<c>CreateLineBoxes</c>/<c>ApplyLineBreaking</c> machinery as any other
/// box) is preserved intact and subsequent rows correctly continue after it - no TableBreakToken/
/// TableRowCursor machinery needed for this case, matching the R2/R5/R6 finding that this port's
/// architecture rarely needs what it looks like it needs at first glance.
/// </summary>
/// <remarks>
/// Does NOT cover repeated-header behavior for this shape - a row whose own content spans multiple
/// pages by itself only gets a header repeat inserted for the first page it crosses onto, not further
/// intermediate pages that same row continues to span (see the KNOWN LIMITATION comment beside
/// CssLayoutEngineTable.LayoutCells's repeat-check). Confirmed via direct testing, not fixed - the far
/// more common shape (many ordinary rows, table spans many pages) already repeats correctly per
/// StageD4RepeatedHeaderTest.ThreadRepeatsOnEveryPageTheTableSpans.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class StageR7TableMultiPageCellTest
{
    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    private static string AllText(BoxFragment f)
    {
        var words = new System.Collections.Generic.List<string>();
        void Collect(BoxFragment x)
        {
            foreach (var w in x.Words)
                if (!w.Word.IsLineBreak)
                    words.Add(w.Word.Text);
            foreach (var c in x.Children)
                Collect(c);
        }
        Collect(f);
        return string.Join(" ", words);
    }

    [TestMethod]
    public async Task RowAfterAMultiPageSpanningCell_IsNotLost()
    {
        using var wrapper = new HtmlContainer();
        var sentence = "one two three four five six seven eight nine ten ";
        var longCell = string.Concat(Enumerable.Repeat(sentence, 100));
        await wrapper.SetHtml(
            $"""
            <html><body>
                <table style="border-collapse:collapse;">
                    <tr><td style="border:1px solid black;">{longCell}</td><td style="border:1px solid black;">short</td></tr>
                    <tr><td style="border:1px solid black;">RowTwoCellOne</td><td style="border:1px solid black;">RowTwoCellTwo</td></tr>
                </table>
            </body></html>
            """);

        var container = GetInternal(wrapper);
        container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(400, 700);
        container.MarginTop = 0;
        wrapper.MaxSize = new SizeF(400, 0);

        using var bitmap = new Bitmap(400, 20000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        var tree = container.FragmentTree;
        Assert.IsNotNull(tree);
        Assert.IsGreaterThan(3, tree.Fragmentainers.Count, "the long cell should genuinely span several pages for this to be meaningful");

        var allText = string.Join(" ", tree.Fragmentainers.Select(f => AllText(f.Root)));
        StringAssert.Contains(allText, "short");
        StringAssert.Contains(allText, "RowTwoCellOne");
        StringAssert.Contains(allText, "RowTwoCellTwo");
    }
}
