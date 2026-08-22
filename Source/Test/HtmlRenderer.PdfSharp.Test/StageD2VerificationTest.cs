using PdfSharp;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

[TestClass]
public sealed class StageD2VerificationTest
{
    [TestMethod]
    public async Task ForcedBreakBefore_Page_StartsNewPage()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        const string html = """
            <html><body>
                <p>Page one content.</p>
                <div style="break-before: page;">Page two content.</div>
            </body></html>
            """;

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.AreEqual(2, document.Pages.Count);
    }

    [TestMethod]
    public async Task NoForcedBreak_SmallContent_StaysOnOnePage()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        const string html = "<html><body><h1>Title</h1><p>Body text.</p></body></html>";

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.AreEqual(1, document.Pages.Count);
    }

    [TestMethod]
    public async Task LegacyPageBreakBefore_Always_StartsNewPage()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        const string html = """
            <html><body>
                <p>Page one content.</p>
                <div style="page-break-before: always;">Page two content.</div>
            </body></html>
            """;

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.AreEqual(2, document.Pages.Count);
    }

    [TestMethod]
    public async Task BreakInsideAvoid_KeepsBlockTogether_OnOnePage()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        // Enough filler to span several pages regardless of exactly which font ends up resolving on
        // whatever machine runs this (a small, precisely-calibrated filler count is fragile to font
        // substitution - CI's non-Windows runners fall back to an embedded font with different metrics
        // than Windows' real "Times New Roman", so a boundary tuned for one silently misses the other;
        // see this project's own established testing lesson about hardcoded "just barely" magic
        // numbers). Precise per-page content verification lives in HtmlRenderer.IntegrationTest's
        // ContainerLeftBehindTest/StageR3RelocationTest, which read the fragment tree directly instead
        // of inferring behavior from a PDF's total page count - this is only a regression-style guard
        // that the avoid-block relocation doesn't crash or misbehave outright.
        var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of text</p>", 150));
        var html = $"""
            <html><body>
                {filler}
                <div style="break-inside: avoid; border: 1px solid black;">
                    <p>first</p><p>second</p><p>third</p>
                </div>
            </body></html>
            """;

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.IsGreaterThanOrEqualTo(2, document.Pages.Count);
    }

    [TestMethod]
    public async Task ManyParagraphs_FlowAcrossMultiplePages()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        var paragraphs = string.Concat(Enumerable.Repeat(
            "<p>A reasonably long paragraph of filler text used to force real multi-page pagination in this test.</p>",
            120));
        var html = $"<html><body>{paragraphs}</body></html>";

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.IsGreaterThan(1, document.Pages.Count);
    }

    [TestMethod]
    public async Task HugeMargin_DoesNotProduceRunawayBlankPages()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        // A margin far taller than a single page - margin truncation (css-break-3 5.2) must
        // discard it rather than paginating through blank vertical space.
        const string html = "<html><body><div style='margin-top:2000pt;'>content</div></body></html>";

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.IsLessThanOrEqualTo(2, document.Pages.Count);
    }

    [TestMethod]
    public async Task KeepWithNext_HeadingStaysWithFollowingParagraph()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        // h4 has UA break-after: avoid. Generous filler (see BreakInsideAvoid_KeepsBlockTogether_OnOnePage's
        // own remark on why a precisely-calibrated boundary is fragile to font substitution across CI
        // platforms) - this is a regression-style guard that the pair doesn't blow up across an
        // unreasonable number of pages, not a precise "did they move together" check (that lives at the
        // fragment-tree level, in HtmlRenderer.IntegrationTest's ContainerLeftBehindKeepWithNextTest).
        var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of text</p>", 150));
        var html = $"""
            <html><body>
                {filler}
                <h4>Section heading</h4>
                <p>Paragraph right after the heading.</p>
            </body></html>
            """;

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.IsGreaterThanOrEqualTo(2, document.Pages.Count);
    }
}
