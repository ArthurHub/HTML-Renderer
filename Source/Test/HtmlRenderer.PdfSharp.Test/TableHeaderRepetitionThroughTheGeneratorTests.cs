using System.Text;
using System.Text.RegularExpressions;
using PdfSharp;
using PdfSharp.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

/// <summary>
/// A repeating <c>&lt;thead&gt;</c> above a single row that continues across several pages, laid out through
/// the real generator rather than <c>LayoutHarness</c>.
/// </summary>
/// <remarks>
/// The header repeat is only checked at the start of a row, so with one tall row the header's fill is only on
/// the row's first page. This pins that behavior.
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
