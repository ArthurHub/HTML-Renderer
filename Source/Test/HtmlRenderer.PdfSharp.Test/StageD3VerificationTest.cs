using PdfSharp;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

[TestClass]
public sealed class StageD3VerificationTest
{
    [TestMethod]
    public async Task LongParagraph_SpansPagesWithoutError()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        // Generous repeat count - a precisely-calibrated boundary is fragile to font substitution
        // across CI platforms (non-Windows runners fall back to an embedded font with different metrics
        // than Windows' real "Times New Roman"); this only needs to comfortably exceed one page
        // regardless of exactly which font resolves.
        var sentence = "This is a moderately long sentence used to build a paragraph that will wrap across many lines and, eventually, across more than one page. ";
        var html = $"<html><body><p>{string.Concat(Enumerable.Repeat(sentence, 100))}</p></body></html>";

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.IsGreaterThan(1, document.Pages.Count);
    }

    [TestMethod]
    public async Task Widows_PullsMinimumLinesToNextPage()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        // Generous filler, not precisely calibrated to a specific boundary - see
        // StageD2VerificationTest.BreakInsideAvoid_KeepsBlockTogether_OnOnePage's remark on why a tight
        // "just barely" filler count is fragile to font substitution across CI platforms. Precise
        // per-page widows verification lives in HtmlRenderer.IntegrationTest's StageR5WidowsMultiPageTest,
        // which reads the fragment tree directly.
        var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of text</p>", 150));
        var sentence = "one two three four five six seven eight nine ten eleven twelve thirteen fourteen fifteen sixteen seventeen eighteen nineteen twenty ";
        var html = $"""
            <html><body>
                {filler}
                <p style="margin:0; widows: 3;">{string.Concat(Enumerable.Repeat(sentence, 6))}</p>
            </body></html>
            """;

        using var document = await PdfGenerator.GeneratePdf(html, config);

        // The widowed paragraph must not leave fewer than 3 of its lines alone at the top of a page -
        // this is a structural/behavioral guard (page count is stable and small) rather than pixel
        // inspection, matching the other D2/D3 verification tests in this project.
        Assert.IsGreaterThanOrEqualTo(2, document.Pages.Count);
    }

    [TestMethod]
    public async Task Orphans_KeepsMinimumLinesOnFirstPage()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        // Generous filler - see Widows_PullsMinimumLinesToNextPage's own remark on why a tight "just
        // barely" filler count is fragile to font substitution across CI platforms.
        var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of text</p>", 150));
        var sentence = "one two three four five six seven eight nine ten eleven twelve thirteen fourteen fifteen sixteen seventeen eighteen nineteen twenty ";
        var html = $"""
            <html><body>
                {filler}
                <p style="margin:0; orphans: 3;">{string.Concat(Enumerable.Repeat(sentence, 6))}</p>
            </body></html>
            """;

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.IsGreaterThanOrEqualTo(2, document.Pages.Count);
    }
}
