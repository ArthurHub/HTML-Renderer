using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/BreakValueCascadeTests.cs: the survival of
/// <c>break-before</c>/<c>break-after</c>/<c>break-inside</c> across a STRUCTURAL CLONE of a box -
/// <see cref="TheArtOfDev.HtmlRenderer.Core.Dom.CssBoxProperties.InheritStyle"/>'s <c>everything: true</c>
/// branch. These properties are not ordinarily inherited (an unrelated child must not pick them up from
/// its parent), but a structural duplicate is a fragment of the same element, and css-break-3 §3 attaches
/// these values to the element rather than to one of its boxes - so both directions have to hold.
/// </summary>
/// <remarks>
/// A real, confirmed production bug was found and fixed while porting this file, not merely documented:
/// <c>CssBoxProperties.InheritStyle</c>'s <c>everything: true</c> branch copied every other originating-
/// element property (background, border, position, size, ...) but never <c>_pageBreakInside</c>/
/// <c>_breakBefore</c>/<c>_breakAfter</c> - confirmed by grep, none of the three appeared anywhere in that
/// method. Both of this method's real <c>everything: true</c> callers are exactly the two structural-clone
/// sites this file is about: <c>TableHeaderRepeat.CloneSubtree</c> (a repeated <c>&lt;thead&gt;</c> row) and
/// <c>DomParser.CorrectBlockSplitBadBox</c> (the block-in-inline split, <c>leftbox</c>/<c>rightBox</c>) - so
/// every clone of either kind silently read <c>auto</c>/<c>auto</c>/<c>auto</c> regardless of what the
/// source element declared, before this fix. Fixed at its source in <c>CssBoxProperties.cs</c> - see that
/// method's own remarks for the full mechanism. These tests assert the fix's storage; what the stored value
/// then does (or, mostly, does not do) to pagination is <see cref="StructuralCloneBreakValueBehaviourTests"/>.
/// <para>
/// Adapted, not renamed, for <see cref="RepeatedTableHeaderProxy_CarriesTheSourceBreakValues"/>: PeachPDF's
/// UA stylesheet gives <c>thead</c> an avoiding <c>break-inside</c> unconditionally (its print-scoped rule
/// applies to its own harness's media type), so its fixture can vary <c>break-inside</c> itself as one of
/// the four cases under test and still get a proxy to inspect. This fork's UA default lives under
/// <c>@media print</c>, which <see cref="LayoutHarness"/>'s <c>WinFormsAdapter</c> (media type
/// <c>"screen"</c>) never matches - so <c>break-inside:avoid</c> is held FIXED across all three cases below
/// (needed just to get a repeated header to inspect at all, per this repo's own established convention),
/// varying <c>break-before</c>/<c>break-after</c> instead of also varying <c>break-inside</c> itself, which
/// TableRepeatedGroupConditionsTests already covers from the opposite direction (does an EXPLICIT
/// <c>break-inside:auto</c> suppress the repeat at all).
/// </para>
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class BreakValueCascadeTests
{
    // ── the two structural-clone call sites ────────────────────────────────

    // TableHeaderRepeat.CloneSubtree clones the <thead>'s own ROW (CssLayoutEngineTable's _allRows entry),
    // not the <thead> box itself - so the values under test have to be declared on the <tr>, whose own
    // clone is what CloneAndPosition actually returns into RepeatedHeaderRows. break-inside:avoid is held
    // fixed on the <thead> ITSELF throughout (needed only for repeatsHeader's own eligibility gate, and
    // confirmed by BreakInsideOnAThead_DoesNotReachItsRowsOrCells - TableRepeatedGroupConditionsTests - to
    // never cascade down onto the row being varied here).
    [TestMethod]
    [DataRow("break-inside:avoid", "avoid", "auto", "auto")]
    [DataRow("break-inside:avoid;break-after:avoid", "avoid", "auto", "avoid")]
    [DataRow("break-inside:avoid;break-before:page", "avoid", "page", "auto")]
    public void RepeatedTableHeaderProxy_CarriesTheSourceBreakValues(
        string css, string expectedInside, string expectedBefore, string expectedAfter)
    {
        var rows = string.Concat(Enumerable.Range(1, 20)
            .Select(i => $"<tr><td>Row {i}, Cell 1</td><td>Row {i}, Cell 2</td></tr>"));

        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse'>"
            + $"<thead style='break-inside:avoid'><tr style='{css}'><th>Header 1</th><th>Header 2</th></tr></thead>"
            + $"<tbody>{rows}</tbody></table>");

        var (root, _) = LayoutHarness.Layout(html, 400, 300, margin: 20);
        var table = LayoutHarness.Descendants(root).First(b => b.Display == "table");

        Assert.IsNotNull(table.RepeatedHeaderRows);
        Assert.IsTrue(table.RepeatedHeaderRows!.Count > 0);

        Assert.IsTrue(table.RepeatedHeaderRows.All(clone => clone.BreakInside == expectedInside));
        Assert.IsTrue(table.RepeatedHeaderRows.All(clone => clone.BreakBefore == expectedBefore));
        Assert.IsTrue(table.RepeatedHeaderRows.All(clone => clone.BreakAfter == expectedAfter));
    }

    // DomParser's block-in-inline correction splits one element's box into several (leftbox/rightBox in
    // CorrectBlockSplitBadBox). Every resulting box represents the same <span>, so each has to carry the
    // span's own resolved values.
    [TestMethod]
    [DataRow("break-inside:avoid", "avoid", "auto", "auto")]
    [DataRow("break-after:avoid", "auto", "auto", "avoid")]
    [DataRow("break-before:page", "auto", "page", "auto")]
    public void BlockInsideInlineSplit_EveryFragmentCarriesTheBreakValues(
        string css, string expectedInside, string expectedBefore, string expectedAfter)
    {
        var html = LayoutHarness.Wrap($"<span style='{css}'>before<div>block</div>after</span>");

        var (root, _) = LayoutHarness.Layout(html);
        var spanBoxes = LayoutHarness.Descendants(root).Where(b => b.HtmlTag?.Name == "span").ToList();

        Assert.IsTrue(spanBoxes.Count > 1, $"expected the span to be split, found {spanBoxes.Count} box(es)");

        Assert.IsTrue(spanBoxes.All(b => b.BreakInside == expectedInside));
        Assert.IsTrue(spanBoxes.All(b => b.BreakBefore == expectedBefore));
        Assert.IsTrue(spanBoxes.All(b => b.BreakAfter == expectedAfter));
    }

    // ── the other direction: not inherited ─────────────────────────────────

    // An ordinary child is not a fragment of its parent, so it must not pick the values up. Without this,
    // moving the three fields into InheritStyle's "always" (non-"everything") section would pass every
    // test above just as well.
    [TestMethod]
    public void OrdinaryChild_DoesNotInheritItsParentsBreakValues()
    {
        var html = LayoutHarness.Wrap(
            "<div id='parent' style='break-inside:avoid;break-before:page;break-after:avoid'>"
            + "<div id='child'>text</div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var child = LayoutHarness.FindById(root, "child");
        Assert.IsNotNull(child);

        Assert.AreEqual(CssConstants.Auto, child!.BreakInside);
        Assert.AreEqual(CssConstants.Auto, child.BreakBefore);
        Assert.AreEqual(CssConstants.Auto, child.BreakAfter);
    }

    // A generated-content (::before) box is a real child of the element, not a duplicate of it, and is
    // created through the OTHER, non-"everything" InheritStyle overload (CssData.cs's
    // "beforePseudoBox.InheritStyle(box)" - the single-arg, default-everything:false call). Same guard as
    // above, at the other call site that could plausibly leak these values.
    [TestMethod]
    public void GeneratedContentBox_DoesNotPickUpItsOriginatingElementsBreakValues()
    {
        var html = """
            <!DOCTYPE html><html><head><style>
              #target { break-inside: avoid; break-before: page; break-after: avoid }
              #target::before { content: "x" }
            </style></head><body style='margin:0'><div id="target">text</div></body></html>
            """;

        var (root, _) = LayoutHarness.Layout(html);
        var target = LayoutHarness.FindById(root, "target");
        Assert.IsNotNull(target);

        var before = LayoutHarness.Descendants(target!).FirstOrDefault(b => b.IsBeforePseudoElement);
        Assert.IsNotNull(before);

        Assert.AreEqual(CssConstants.Auto, before!.BreakInside);
        Assert.AreEqual(CssConstants.Auto, before.BreakBefore);
        Assert.AreEqual(CssConstants.Auto, before.BreakAfter);
    }

    // And the element itself really does hold the values the two negative tests above are checking are not
    // propagated - so they are not passing merely because the cascade never stored anything at all.
    [TestMethod]
    public void TheElementItself_HoldsTheValuesTheClonesAreCheckedAgainst()
    {
        var html = LayoutHarness.Wrap(
            "<div id='target' style='break-inside:avoid;break-before:page;break-after:avoid'>text</div>");

        var (root, _) = LayoutHarness.Layout(html);
        var target = LayoutHarness.FindById(root, "target");
        Assert.IsNotNull(target);

        Assert.AreEqual(CssConstants.Avoid, target!.BreakInside);
        Assert.AreEqual(CssConstants.Page, target.BreakBefore);
        Assert.AreEqual(CssConstants.Avoid, target.BreakAfter);
    }
}
