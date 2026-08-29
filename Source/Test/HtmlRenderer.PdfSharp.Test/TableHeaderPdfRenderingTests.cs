using System.Text;
using System.Text.RegularExpressions;
using PdfSharp;
using PdfSharp.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

/// <summary>
/// Ported from PeachPDF.Tests' <c>TableHeaderPdfRenderingTests</c>: real end-to-end PDF rendering of a
/// repeating <c>&lt;thead&gt;</c> across multiple real generated pages.
/// </summary>
/// <remarks>
/// <para>
/// PeachPDF's own file verified only page count and "the page has some content" (its own doc-comment
/// admits as much: "Extracting and decoding PDF text content is complex... For full text verification,
/// manual inspection... would be needed"). This branch's own history has since specifically shown that
/// page-count-only checks are NOT sufficient to catch a real header-repeat bug (see
/// <c>MultiPageTextVisibilityTest</c>'s own doc-comment, and Batch 4's phantom-double-header-paint fix) -
/// so every test here additionally confirms the header's own background fill repeats on every real
/// generated page via the raw content stream, using the same fill-operator convention confirmed
/// empirically while porting (see <see cref="HeaderFillPattern"/>).
/// </para>
/// <para>
/// PeachPDF's file name says "header/footer", but only 1 of its 5 tests is pure footer-repetition
/// (<c>TableFooter_MultiPageTable_GeneratesWithFooter</c>) and 1 mixes header+footer in a single-page,
/// paint-order-recording test (<c>TableHeaderAndFooter_SinglePageTable_PaintsEachCellTextExactlyOnce</c>,
/// which used PeachPDF's own <c>FragmentPaintHarness</c>/<c>DrawStringRecordingGraphics</c> test-only
/// mock, not a real generated PDF). This fork implements no <c>&lt;tfoot&gt;</c> repeat at all -
/// confirmed across this whole branch by <c>TableHeaderRepeat.cs</c> being thead-only - so the pure-footer
/// test is dropped outright, and the mixed test is rewritten below as a header-only, real-PDF,
/// content-stream-level equivalent (<see cref="TableHeader_SinglePageTable_PaintsHeaderBackgroundExactlyOnce"/>)
/// rather than ported with PeachPDF's own mock-graphics harness, which this project has no equivalent of
/// and which would test paint-call sequencing rather than the real generated PDF this project's own
/// established pattern (<c>MultiPageTextVisibilityTest</c>, <c>FixedPositionRepeatsPerPdfPageTest</c>)
/// insists on.
/// </para>
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class TableHeaderPdfRenderingTests
{
    // A distinctively-colored header fill (rgb(2,4,6), well away from default black text/gray borders),
    // matched the same way FixedPositionPaginationIntegrationTests/HandleLinksPaginationTests's sibling
    // files match a filled rect: a "r g b rg" color-set operator followed, not necessarily immediately
    // (PdfSharp's writer can interleave a "/GSn gs" state push), by an "re" path and an "f" fill.
    // Confirmed empirically against a real generated content stream during porting, not guessed.
    private static readonly Regex HeaderFillPattern = new(@"0\.00\d* 0\.01\d* 0\.02\d* rg[\s\S]{0,80}?re\s*\r?\nf");

    private const string HeaderBackground = "background-color: rgb(2,4,6);";

    private static void AssertHeaderRepeatsOnEveryPage(PdfDocument document)
    {
        for (var i = 0; i < document.Pages.Count; i++)
        {
            var content = document.Pages[i].Contents.Elements.GetDictionary(0);
            var text = Encoding.Latin1.GetString(content!.Stream.Value);
            var matches = HeaderFillPattern.Matches(text);
            Assert.IsGreaterThanOrEqualTo(1, matches.Count, $"page {i} should draw the repeated header's own background fill.");
        }
    }

    [TestMethod]
    public async Task TableHeader_MultiPageTable_RepeatsHeaderOnEveryRealGeneratedPage()
    {
        var rows = string.Concat(Enumerable.Range(1, 100)
            .Select(i => $"<tr><td>Row {i} Col 1</td><td>Row {i} Col 2</td><td>Row {i} Col 3</td></tr>"));
        var html = $"""
            <html><body>
                <table style="width:100%; border-collapse:collapse;" border="1">
                    <thead><tr>
                        <th style="{HeaderBackground} padding:10px;">Header Column 1</th>
                        <th style="{HeaderBackground} padding:10px;">Header Column 2</th>
                        <th style="{HeaderBackground} padding:10px;">Header Column 3</th>
                    </tr></thead>
                    <tbody>{rows}</tbody>
                </table>
            </body></html>
            """;

        using var document = await PdfGenerator.GeneratePdf(html, PageSize.A4, margin: 20);

        Assert.IsGreaterThanOrEqualTo(2, document.Pages.Count, $"PDF should have at least 2 pages but has {document.Pages.Count}");
        AssertHeaderRepeatsOnEveryPage(document);
    }

    [TestMethod]
    public async Task TableHeader_ThreePageTable_RepeatsHeaderOnEveryRealGeneratedPage()
    {
        var rows = string.Concat(Enumerable.Range(1, 150)
            .Select(i => $"<tr><td>{i}</td><td>Employee {i}</td><td>Dept {i % 10}</td></tr>"));
        var html = $"""
            <html><body>
                <table style="width:100%; border-collapse:collapse;" border="1">
                    <thead><tr>
                        <th style="{HeaderBackground} padding:8px;">ID</th>
                        <th style="{HeaderBackground} padding:8px;">Name</th>
                        <th style="{HeaderBackground} padding:8px;">Department</th>
                    </tr></thead>
                    <tbody>{rows}</tbody>
                </table>
            </body></html>
            """;

        using var document = await PdfGenerator.GeneratePdf(html, PageSize.A4, margin: 20);

        Assert.IsGreaterThanOrEqualTo(3, document.Pages.Count, $"PDF should have at least 3 pages but has {document.Pages.Count}");
        AssertHeaderRepeatsOnEveryPage(document);
    }

    [TestMethod]
    public async Task TableHeader_ComplexHeaderWithColspan_RepeatsAcrossPages()
    {
        var rows = string.Concat(Enumerable.Range(1, 80)
            .Select(i => $"<tr><td>First{i}</td><td>Last{i}</td><td>email{i}@example.com</td><td>555-{i:D4}</td></tr>"));
        var html = $"""
            <html><body>
                <table style="width:100%; border-collapse:collapse;" border="1">
                    <thead>
                        <tr>
                            <th style="{HeaderBackground} padding:8px;" colspan="2">Personal Information</th>
                            <th style="{HeaderBackground} padding:8px;" colspan="2">Contact Details</th>
                        </tr>
                        <tr>
                            <th style="{HeaderBackground} padding:8px;">First Name</th>
                            <th style="{HeaderBackground} padding:8px;">Last Name</th>
                            <th style="{HeaderBackground} padding:8px;">Email</th>
                            <th style="{HeaderBackground} padding:8px;">Phone</th>
                        </tr>
                    </thead>
                    <tbody>{rows}</tbody>
                </table>
            </body></html>
            """;

        using var document = await PdfGenerator.GeneratePdf(html, PageSize.A4, margin: 20);

        Assert.IsGreaterThanOrEqualTo(2, document.Pages.Count, "PDF should have at least 2 pages for complex header test");
        AssertHeaderRepeatsOnEveryPage(document);
    }

    /// <summary>
    /// Rewrite of PeachPDF's <c>TableHeaderAndFooter_SinglePageTable_PaintsEachCellTextExactlyOnce</c> -
    /// a regression test for two real bugs PeachPDF found together there: a header/footer row never
    /// getting its own <c>Bounds</c> set (so paint-time visibility culling silently dropped it), and its
    /// proxy row being both self-registering AND explicitly re-added by its caller, painting every
    /// header/footer cell twice at identical coordinates. This fork's <c>TableHeaderRepeat</c> mechanism
    /// is architecturally different (an independent, already-positioned <c>CssBox</c> clone per repeat,
    /// not a shared-subtree proxy - see <c>RepeatedTableHeaderClipIntegrationTests</c>'s own doc-comment
    /// from Batch 4), so this exact bug shape doesn't apply here; ported instead as a real-PDF regression
    /// PIN, through the real generator, that a single-page table's header paints its background exactly
    /// once - not zero (dropped by culling) and not two (duplicated by a proxy re-add), the same class of
    /// defect PeachPDF's test was guarding against, verified the way this project's own established
    /// pattern requires (a real content stream, not a mock paint-recording harness).
    /// </summary>
    [TestMethod]
    public async Task TableHeader_SinglePageTable_PaintsHeaderBackgroundExactlyOnce()
    {
        var html = $"""
            <html><body><table>
                <thead><tr><th style="{HeaderBackground}">Header</th></tr></thead>
                <tbody><tr><td>Body</td></tr></tbody>
            </table></body></html>
            """;

        using var document = await PdfGenerator.GeneratePdf(html, PageSize.A4, margin: 20);

        Assert.AreEqual(1, document.Pages.Count, "this fixture is deliberately small enough to fit on one page");

        var content = document.Pages[0].Contents.Elements.GetDictionary(0);
        var text = Encoding.Latin1.GetString(content!.Stream.Value);
        var matches = HeaderFillPattern.Matches(text);
        Assert.AreEqual(1, matches.Count, "the header's own background should paint exactly once - not dropped, not duplicated");
    }
}
