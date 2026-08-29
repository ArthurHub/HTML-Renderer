using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/RepeatingTableRelayoutTests.cs: a container holding a
/// repeating-header table takes the same relocation any other <c>break-inside:avoid</c> box does when it
/// straddles a page boundary - real relayout at its destination, not a translate.
/// </summary>
/// <remarks>
/// PeachPDF's own excuse for excluding this ("laying the table out a second time did not reproduce the
/// first result - the repeating group was detached and replaced by per-page proxies nothing removed, so a
/// second run threw") does not apply here by construction: <c>CssLayoutEngineTable.LayoutCells</c> resets
/// <c>_tableBox.RepeatedHeaderRows = null</c> at the very start of every call (confirmed by direct source
/// read, line ~652), and <see cref="TableHeaderRepeat"/>'s clones are freshly built, fully detached
/// <see cref="CssBox"/> instances - never mutating or reusing state from a previous pass. A relaid-out
/// table's repeated-header list is simply rebuilt from scratch, matching that field's own doc comment
/// ("rebuilt from scratch on every layout pass"). These two tests are accordingly regression pins of that
/// idempotency, not reproductions of a PeachPDF-specific bug - confirmed by actually running them.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class RepeatingTableRelayoutTests
{
    private const double PageHeight = 400;
    private const double Margin = 20;

    private static string CardWithTable(double fillerHeight) => LayoutHarness.Wrap(
        $"<div style='height:{fillerHeight}px'>filler</div>"
        + "<div id='card' style='break-inside:avoid;border:1px solid black'>"
        + "<table style='width:100px;border-collapse:collapse'>"
        + "<thead style='break-inside:avoid'><tr><th>H</th></tr></thead>"
        + "<tbody><tr><td>One</td></tr><tr><td>Two</td></tr><tr><td>Three</td></tr></tbody>"
        + "</table></div>");

    private static CssBox Card(CssBox root) => LayoutHarness.FindById(root, "card")!;

    // A relaid-out box's height is the height of its own content wherever it lands - not the settled
    // height PLUS whatever gap it would carry if it had merely been translated down to its new top.
    [TestMethod]
    [DataRow(340.0)]
    [DataRow(360.0)]
    [DataRow(380.0)]
    public void ACardHoldingARepeatingHeaderTable_IsRelocatedWithoutCarryingAGap(double fillerHeight)
    {
        var (settledRoot, _) = LayoutHarness.Layout(CardWithTable(0), maxWidth: 300, maxHeight: PageHeight, margin: Margin);
        var settledCard = Card(settledRoot);
        var settledHeight = settledCard.ActualBottom - settledCard.Location.Y;

        var (root, container) = LayoutHarness.Layout(CardWithTable(fillerHeight), maxWidth: 300, maxHeight: PageHeight, margin: Margin);
        var card = Card(root);

        // Test setup expects the filler to actually push the card across a page boundary - otherwise
        // RelocateIfNeeded never fires and this asserts nothing.
        Assert.AreNotEqual(
            container.PageIndexOf(card.Location.Y),
            container.PageIndexOf(card.Location.Y - 1),
            "sanity check only - see the real assertion below");
        Assert.AreEqual(0, container.PageIndexOf(card.Location.Y - fillerHeight + 1),
            "test setup expects the card to have started, pre-relocation, back on page 0");
        Assert.IsTrue(container.PageIndexOf(card.Location.Y) >= 1,
            $"test setup expects the card to be relocated to a later page, but it is at Y={card.Location.Y:F1}");

        Assert.AreEqual(settledHeight, card.ActualBottom - card.Location.Y, 1,
            "a relocated card must be the height of its own content, not the settled height plus a carried-over gap");
    }

    [TestMethod]
    public void TheTableInsideARelocatedCard_StillRepeatsItsHeaderExactlyOnce()
    {
        var (root, container) = LayoutHarness.Layout(CardWithTable(360), maxWidth: 300, maxHeight: PageHeight, margin: Margin);

        var card = Card(root);
        Assert.IsTrue(container.PageIndexOf(card.Location.Y) >= 1,
            $"test setup expects the card to be relocated, but it is at Y={card.Location.Y:F1}");

        var table = LayoutHarness.Descendants(card).First(b => b.Display == "table");

        // The table's 3 short rows comfortably fit alongside its header on a single page even after
        // relocation, so RepeatedHeaderRows should be null (no continuation page to repeat onto) - not a
        // stale, non-null list left over from whatever layout pass ran before the relocation's own relayout.
        Assert.IsNull(table.RepeatedHeaderRows,
            "a table that fits on one page after relocation should have nothing to repeat, stale or otherwise");

        // The header itself (in flow) still appears exactly once - no duplicate left behind by an earlier,
        // abandoned layout pass at the card's pre-relocation position.
        var headerCells = LayoutHarness.Descendants(table).Count(b => b.HtmlTag?.Name == "th");
        Assert.AreEqual(1, headerCells);
    }
}
