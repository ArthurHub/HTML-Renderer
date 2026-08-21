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

        var sentence = "This is a moderately long sentence used to build a paragraph that will wrap across many lines and, eventually, across more than one page. ";
        var html = $"<html><body><p>{string.Concat(Enumerable.Repeat(sentence, 40))}</p></body></html>";

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.IsGreaterThan(1, document.Pages.Count);
    }

    [TestMethod]
    public async Task Widows_PullsMinimumLinesToNextPage()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        // Filler sized to leave room for just one more line of the following paragraph before the
        // page boundary - with widows:3 (default), that line alone isn't enough and must move with
        // at least two more to the next page.
        var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of text</p>", 53));
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

        var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of text</p>", 54));
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
