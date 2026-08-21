using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Fragments;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest;

/// <summary>
/// Verifies the fragmentation-engine-parity plan's R9 investigation finding: PeachPDF's "keep-with-next
/// run-pull rewind across an already-frozen fragmentainer" does not have a counterpart problem in this
/// port's architecture, so no new rewind machinery is needed - the existing same-pass
/// <see cref="TheArtOfDev.HtmlRenderer.Core.Fragmentation.BlockFragmentation.EnforceKeepWithNext"/> (R4)
/// already covers it.
/// </summary>
/// <remarks>
/// PeachPDF needs a real cross-pass rewind because ordinary overflow-driven pagination is itself a real
/// pass boundary there - a keep-with-next violation discovered while laying out page N+1 may need to
/// reach back into page N's content, which was already committed via that pass's own <c>EmitPass</c>.
/// In this port, only a FORCED break (<c>break-before/after: page</c>) ever creates a real pass boundary
/// in <c>HtmlContainerInt.DriveLayoutPasses</c> - ordinary overflow and <c>break-inside:avoid</c> are both
/// same-pass local corrections (R2/R3), and <c>FragmentEmitter</c> runs once, only after every pass has
/// settled, so nothing is ever truly "frozen" mid-layout the way PeachPDF's per-pass emit makes it.
/// A keep-with-next run is therefore always laid out - and checked by <c>EnforceKeepWithNext</c> - within
/// the SAME pass as the sibling it's chained to, even immediately after resuming from an unrelated forced
/// break earlier in the document, as this test confirms directly against the fragment tree. And a run
/// could never need to be pulled across a forced break itself either way: the forced break is the
/// intentional separator keep-with-next exists to avoid accidentally recreating, not an obstacle to undo.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class StageR9KeepWithNextAcrossForcedBreakTest
{
    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    private static string AllText(BoxFragment f)
    {
        var words = new List<string>();
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
    public async Task KeepWithNextPairRightAfterAForcedBreak_StaysTogether_OnTheResumedPage()
    {
        using var wrapper = new HtmlContainer();
        var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of text</p>", 39));
        await wrapper.SetHtml(
            $"""
            <html><body>
                <div style="margin:0; break-before: page;">ForcedBreakMarker</div>
                {filler}
                <h4 style="margin:0; break-after: avoid;">Section heading</h4>
                <p style="margin:0;">Paragraph right after the heading.</p>
            </body></html>
            """);

        var container = GetInternal(wrapper);
        container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(595, 800);
        container.MarginTop = 20;
        wrapper.MaxSize = new SizeF(595, 0);

        using var bitmap = new Bitmap(595, 20000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        var tree = container.FragmentTree;
        Assert.IsNotNull(tree);
        Assert.IsGreaterThanOrEqualTo(2, tree.Fragmentainers.Count, "the forced break must actually introduce a real pass boundary for this test to be meaningful");

        var pageOfHeading = -1;
        var pageOfParagraph = -1;
        for (var i = 0; i < tree.Fragmentainers.Count; i++)
        {
            var text = AllText(tree.Fragmentainers[i].Root);
            if (text.Contains("Section heading")) pageOfHeading = i;
            if (text.Contains("Paragraph right after the heading.")) pageOfParagraph = i;
        }

        Assert.AreNotEqual(-1, pageOfHeading, "heading must not be lost");
        Assert.AreNotEqual(-1, pageOfParagraph, "paragraph must not be lost");
        Assert.AreEqual(pageOfHeading, pageOfParagraph, "break-after:avoid must keep the heading with its paragraph even immediately after resuming from an unrelated forced break");
    }
}
