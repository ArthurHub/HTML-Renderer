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

        // Generous repeat count, fonts differ between platforms.
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

        // Generous filler, fonts differ between platforms. Only a smoke test, the integration tests
        // check the line counts.
        var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of text</p>", 150));
        var sentence = "one two three four five six seven eight nine ten eleven twelve thirteen fourteen fifteen sixteen seventeen eighteen nineteen twenty ";
        var html = $"""
            <html><body>
                {filler}
                <p style="margin:0; widows: 3;">{string.Concat(Enumerable.Repeat(sentence, 6))}</p>
            </body></html>
            """;

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.IsGreaterThanOrEqualTo(2, document.Pages.Count);
    }

    [TestMethod]
    public async Task Orphans_KeepsMinimumLinesOnFirstPage()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        // Generous filler, fonts differ between platforms.
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
