using System.Text;
using System.Text.RegularExpressions;
using PdfSharp;
using PdfSharp.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

/// <summary>
/// Ported from PeachPDF.Tests' <c>TableHeaderRepetitionThroughTheGeneratorTests</c>: a repeating
/// <c>&lt;thead&gt;</c> above a single row that continues mid-cell across several pages, laid out the way
/// the real generator lays a document out - not PeachPDF's isolated <c>LayoutHarness</c>, which parses
/// once and computes its own content band. PeachPDF's own remarks are explicit that this distinction is
/// the file's whole point: two green unit-level tests over the same behavior weren't enough to know the
/// header was wrong, because nothing else in that suite laid a document out through the real generator's
/// own path with a repeating header.
/// </summary>
/// <remarks>
/// <para>
/// Three of PeachPDF's adaptation notes don't apply here at all: this fork implements no
/// <c>&lt;tfoot&gt;</c> repeat (confirmed across the whole branch by <c>TableHeaderRepeat.cs</c> being
/// thead-only), its <c>@page</c> at-rule is confirmed parse-only with no consumer anywhere (so the
/// original fixture's <c>@page { size: a6; margin: 12mm }</c> is dropped in favor of driving page size
/// through <see cref="PdfGenerateConfig"/> directly, the same as every other real-generator test in this
/// project), and this project has no <c>InternalsVisibleTo</c> access to <c>HtmlContainerInt</c>/
/// <c>FragmentTree</c> (only <c>HtmlRenderer.Test</c>/<c>HtmlRenderer.IntegrationTest</c> do - confirmed
/// by reading <c>Source/HtmlRenderer/HtmlRenderer.csproj</c>'s <c>InternalsVisibleTo</c> list), so every
/// assertion here goes through the real generated PDF's content stream instead of the fragment tree
/// PeachPDF's own version asserted on.
/// </para>
/// <para>
/// Porting this fixture surfaced a real, already-documented architectural gap rather than a new bug:
/// <c>HtmlRenderer.IntegrationTest.Tables.TableSpannedBandRepetitionTests</c> (Batch 4) already pins that
/// <c>CssLayoutEngineTable</c>'s header-repeat loop only re-checks for a "did we cross into a new band"
/// transition at the START of each subsequent row's own iteration - so a table whose only body row is a
/// single cell tall enough to overflow through several bands on its own, with no later row to trigger that
/// check, never gets the header repeated onto any of those bands at all. That is exactly PeachPDF's
/// fixture shape (one row, one very tall cell, no trailing row) - confirmed empirically here, through the
/// full real <see cref="PdfGenerator"/> pipeline rather than <c>LayoutHarness</c>, by dumping a real
/// generated content stream during porting: the header's own background fill appears once on the row's
/// starting page and not at all on any of the pages the row's content continues onto afterward. This is
/// pinned below as the accurate, current, real-pipeline-confirmed behavior (extending Batch 4's coverage
/// of the same gap past the isolated layout harness) rather than silently reproduced as if it were
/// correct, or forced to pass by reshaping the fixture into something PeachPDF never tested.
/// </para>
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class TableHeaderRepetitionThroughTheGeneratorTests
{
    // See TableHeaderPdfRenderingTests.HeaderFillPattern for how this convention was confirmed.
    private static readonly Regex HeaderFillPattern = new(@"0\.00\d* 0\.01\d* 0\.02\d* rg[\s\S]{0,80}?re\s*\r?\nf");

    private const int Clauses = 3000;

    private static async Task<PdfDocument> LayoutFixtureAsync()
    {
        var clauses = string.Join(" ", Enumerable.Range(1, Clauses).Select(i => $"clause{i}"));
        var html = $"""
            <html><body>
                <table style="width:100%; border-collapse:collapse;" border="1">
                    <thead><tr><th style="background-color: rgb(2,4,6);">Header</th></tr></thead>
                    <tbody><tr style="break-inside:auto"><td>{clauses}</td></tr></tbody>
                </table>
            </body></html>
            """;

        return await PdfGenerator.GeneratePdf(html, PageSize.A4, margin: 20);
    }

    private static int HeaderMatchCount(PdfDocument document, int pageIndex)
    {
        var content = document.Pages[pageIndex].Contents.Elements.GetDictionary(0);
        var text = Encoding.Latin1.GetString(content!.Stream.Value);
        return HeaderFillPattern.Matches(text).Count;
    }

    /// <summary>
    /// The documented gap (see class remarks), confirmed to survive all the way through the real
    /// generator: with no trailing row after the one tall, continuing cell, the header-repeat loop's
    /// per-row slot-advance check never fires again after the row's own starting page, so the header is
    /// drawn on that first page only - not on any of the further real PDF pages the cell's content
    /// continues onto.
    /// </summary>
    [TestMethod]
    public async Task SingleContinuingRow_HeaderRepeatsOnlyOnItsOwnStartingPage()
    {
        using var document = await LayoutFixtureAsync();

        Assert.IsGreaterThan(3, document.Pages.Count, "fixture must genuinely continue across several real pages for this to be meaningful");

        Assert.AreEqual(1, HeaderMatchCount(document, 0), "the header is in flow on its own starting page");

        for (var i = 1; i < document.Pages.Count; i++)
        {
            Assert.AreEqual(0, HeaderMatchCount(document, i),
                $"page {i}: TableSpannedBandRepetitionTests' documented gap - a single continuing row with " +
                "no trailing row never re-triggers the header-repeat loop's slot-advance check, so no further " +
                "page gets the header repeated onto it");
        }
    }

    /// <summary>
    /// Regardless of the header-repeat gap above, the row's own text content must still flow onto and
    /// remain visible on every real page it continues across - matching this project's own established
    /// "page count alone is not sufficient" bar (<c>MultiPageTextVisibilityTest</c>) applied to this
    /// specific fixture shape.
    /// </summary>
    [TestMethod]
    public async Task SingleContinuingRow_EveryRealGeneratedPageStillCarriesRealTextContent()
    {
        using var document = await LayoutFixtureAsync();

        Assert.IsGreaterThan(3, document.Pages.Count, "fixture must genuinely continue across several real pages for this to be meaningful");

        for (var i = 0; i < document.Pages.Count; i++)
        {
            var content = document.Pages[i].Contents.Elements.GetDictionary(0);
            var text = Encoding.Latin1.GetString(content!.Stream.Value);
            StringAssert.Contains(text, "Tj", $"page {i} has no text-drawing operators - the continuing row's content is invisible there.");
        }
    }
}
