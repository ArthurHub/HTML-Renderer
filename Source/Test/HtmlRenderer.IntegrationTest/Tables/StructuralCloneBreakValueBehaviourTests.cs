using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/StructuralCloneBreakValueBehaviourTests.cs: what a structurally
/// cloned box's <c>break-*</c> values actually DO to pagination, now that <see cref="BreakValueCascadeTests"/>
/// confirms (and this batch's fix to <c>CssBoxProperties.InheritStyle</c> ensures) the clone carries them.
/// </summary>
/// <remarks>
/// Characterization, not desired output: carrying the values is a box-model correctness fix, and on its own
/// changes no observable output today, for two independent reasons pinned below.
/// <list type="number">
/// <item><b>Repeated table header.</b> A <see cref="CssBox.RepeatedHeaderRows"/> clone is never inserted
/// into any box's <see cref="CssBox.Boxes"/> - it exists purely for the fragment tree/paint to find
/// (<c>TableHeaderRepeat.CloneAndPosition</c>'s own doc comment: "fully detached... so re-running table
/// layout can never mistake it for real content"). <c>BlockFragmentation</c>'s forced-break/keep-with-next
/// machinery walks the LIVE box tree (<c>DomUtils.GetPreviousSibling</c>, a child loop over
/// <c>Boxes</c>) - a clone that is never a member of any <c>Boxes</c> collection is invisible to all of it,
/// so its break values are stored and read by nothing.</item>
/// <item><b>Block-in-inline split.</b> Only inline boxes are split by <c>DomParser.CorrectBlockSplitBadBox</c>,
/// and css-break-3 §3.1/§3.2 apply break properties to block-level boxes (not inlines) - so the values are
/// inert on a split fragment by specification, independent of this fork's own architecture. Separately,
/// <c>CorrectBlockSplitBadBox</c> wraps each fragment behind an anonymous block
/// (<c>CssBox.CreateBox(leftBlock/parentBox, badBox.HtmlTag)</c> creates <c>leftbox</c>/<c>rightBox</c> as
/// new, separate boxes - the split fragments become their CHILDREN, not their equals in the sibling chain),
/// so a sibling walk from a following box sees the wrapper, never the span fragment itself.</item>
/// </list>
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class StructuralCloneBreakValueBehaviourTests
{
    private const double PageHeight = 300;
    private const double Margin = 20;

    // A repeating header must never be the last thing on a page - it is always followed, on the same page,
    // by at least one real row. This holds structurally today (the table engine never places a header
    // repeat without following it with the row that opened that page), independently of the header's own
    // break values - which is exactly what makes it the right invariant to pin before anything starts
    // reading them.
    [TestMethod]
    [DataRow("")]
    [DataRow("break-after:avoid")]
    [DataRow("break-inside:avoid")]
    public void RepeatedHeader_IsNeverStrandedWithoutARowBeneathIt(string rowCss)
    {
        var (_, container) = LayoutHarness.Layout(TableDocument(rowCss), 400, PageHeight, margin: Margin);

        var tree = container.FragmentTree!;
        Assert.IsTrue(tree.Fragmentainers.Count > 1, "fixture must paginate");

        foreach (var fragmentainer in tree.Fragmentainers)
        {
            var boxes = Flatten(fragmentainer.Root).ToList();

            // A repeated header's own words are drawn via its clone rows, which are never part of the live
            // tree - detected here as any word starting with "Header".
            var headerFragment = boxes.FirstOrDefault(f => f.Words.Any(w => w.Word.Text?.StartsWith("Header") == true));
            if (headerFragment is null) continue;

            var rowBelow = boxes.Any(f => f.Words.Any(w => w.Word.Text?.StartsWith("Row") == true)
                && f.Rect.Top >= headerFragment.Rect.Top);

            Assert.IsTrue(rowBelow,
                $"page {fragmentainer.SlotIndex} repeats the header with no body row beneath it");
        }
    }

    // The shape to worry about now that a repeated row's clone carries its own edge values: a forced
    // break-after taken once per repetition would paginate without bound. Confirmed inert in both
    // directions - it adds no page, including the single one css-break-3 3.1 would otherwise have the
    // element's own trailing edge produce, because the clone is never reached by BlockFragmentation at all
    // (see class remarks).
    [TestMethod]
    public void RepeatedHeaderWithForcedBreakAfter_IsCurrentlyInert()
    {
        var (_, plain) = LayoutHarness.Layout(TableDocument(""), 400, PageHeight, margin: Margin);
        var (_, forced) = LayoutHarness.Layout(TableDocument("break-after:page"), 400, PageHeight, margin: Margin);

        var plainPages = plain.FragmentTree!.Fragmentainers.Count;
        var forcedPages = forced.FragmentTree!.Fragmentainers.Count;

        Assert.IsTrue(plainPages > 1, "fixture must paginate");
        Assert.AreEqual(plainPages, forcedPages);
    }

    // Both halves of the split-fragment story at once: every fragment now carries the span's own
    // break-after (BreakValueCascadeTests' own subject, the storage fix) - but the anonymous wrapper
    // CorrectBlockSplitBadBox creates still separates each fragment from the sibling that would otherwise
    // read it, so a box genuinely chained by break-after:avoid to the span never actually gets pulled
    // across a page boundary the way it would if chained to an ordinary, unsplit break-after:avoid box.
    // Adapted from PeachPDF's own direct call into DomUtils.GetPrecedingKeepWithNextRun - this fork's
    // equivalent (BlockFragmentation.CollectPrecedingKeepWithNextRun) is private, so this is stated as the
    // observable layout outcome instead: the same fixture, once with an ordinary break-after:avoid box
    // immediately preceding 'kept' (which DOES get pulled - PageBreakIntegrationTests already covers this
    // as a general invariant) and once with the split span in its place (which does not).
    [TestMethod]
    public void SplitFragmentsCarryBreakAfter_ButAnAnonymousWrapperSeparatesThemFromTheirSibling()
    {
        var splitHtml = LayoutHarness.Wrap(
            "<div style='height:120px'>filler</div>"
            + "<span style='break-after:avoid'>lead<div>split</div>tail</span>"
            + "<div id='kept' style='height:100px;break-inside:avoid'>kept</div>");

        var (splitRoot, splitContainer) = LayoutHarness.Layout(splitHtml, 400, 200, margin: Margin);

        var spans = LayoutHarness.Descendants(splitRoot).Where(b => b.HtmlTag?.Name == "span").ToList();
        Assert.IsTrue(spans.Count > 1, $"expected the span to be split, found {spans.Count} box(es)");
        Assert.IsTrue(spans.All(s => s.BreakAfter == CssConstants.Avoid), "the storage fix should still hold here");

        var splitKept = LayoutHarness.FindById(splitRoot, "kept")!;

        var predecessor = DomUtils.GetPreviousSibling(splitKept);
        Assert.IsNotNull(predecessor);
        Assert.IsNull(predecessor!.HtmlTag);
        Assert.AreEqual(CssConstants.Auto, predecessor.BreakAfter,
            "the anonymous wrapper CorrectBlockSplitBadBox creates carries none of the span's own values");

        // The negative control: an ORDINARY (unsplit) break-after:avoid box immediately preceding 'kept'
        // DOES get pulled across the same boundary - proving the split shape's own outcome above is really
        // caused by the anonymous wrapper, not by some other reason 'kept' just never moves.
        var ordinaryHtml = LayoutHarness.Wrap(
            "<div style='height:120px'>filler</div>"
            + "<div id='lead' style='break-after:avoid'>lead</div>"
            + "<div id='kept' style='height:100px;break-inside:avoid'>kept</div>");

        var (ordinaryRoot, ordinaryContainer) = LayoutHarness.Layout(ordinaryHtml, 400, 200, margin: Margin);
        var ordinaryLead = LayoutHarness.FindById(ordinaryRoot, "lead")!;
        var ordinaryKept = LayoutHarness.FindById(ordinaryRoot, "kept")!;

        Assert.AreEqual(
            ordinaryContainer.PageIndexOf(ordinaryKept.Location.Y),
            ordinaryContainer.PageIndexOf(ordinaryLead.Location.Y),
            "sanity check: an ordinary break-after:avoid box IS pulled onto its next sibling's page");

        Assert.AreNotEqual(
            splitContainer.PageIndexOf(splitKept.Location.Y),
            splitContainer.PageIndexOf(predecessor.EffectiveTop),
            "the split span's own break-after is inert - its anonymous wrapper is left behind while 'kept' moves on alone");
    }

    // ── helpers ───────────────────────────────────────────────────────────

    private static string TableDocument(string headerRowCss)
    {
        var rows = string.Concat(Enumerable.Range(1, 30)
            .Select(i => $"<tr><td>Row {i} Cell 1</td><td>Row {i} Cell 2</td></tr>"));

        return LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse'>"
            + $"<thead style='break-inside:avoid'><tr style='{headerRowCss}'><th>Header 1</th><th>Header 2</th></tr></thead>"
            + $"<tbody>{rows}</tbody></table>");
    }

    private static System.Collections.Generic.IEnumerable<TheArtOfDev.HtmlRenderer.Core.Fragments.BoxFragment> Flatten(
        TheArtOfDev.HtmlRenderer.Core.Fragments.BoxFragment fragment)
    {
        yield return fragment;
        foreach (var child in fragment.Children)
            foreach (var descendant in Flatten(child))
                yield return descendant;
    }
}
