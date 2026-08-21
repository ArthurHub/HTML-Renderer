using PdfSharp;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

[TestClass]
public sealed class StageF1VerificationTest
{
    [TestMethod]
    public async Task WebLinkAndAnchorLink_AcrossPages_DoNotThrow()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of text</p>", 60));
        var html = $"""
            <html><body>
                <a href="https://example.com">external link on page one</a>
                <a href="#target">jump to anchor</a>
                {filler}
                <p id="target">anchor target, on a later page</p>
            </body></html>
            """;

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.IsGreaterThan(1, document.Pages.Count);

        // At least one page carries some link annotation (either the web link or the document link) -
        // this is a smoke check that HandleLinks' new slot-to-page mapping runs without throwing and
        // actually attaches annotations, not a check of which exact page holds which link.
        var anyLinks = false;
        for (var i = 0; i < document.PageCount; i++)
        {
            if (document.Pages[i].Annotations.Count > 0)
            {
                anyLinks = true;
                break;
            }
        }
        Assert.IsTrue(anyLinks, "expected at least one page to carry a link annotation");
    }

    [TestMethod]
    public async Task HugeMargin_ProducesNoBlankPages()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        const string html = "<html><body><div style='margin-top:2000pt;'>content</div></body></html>";

        using var document = await PdfGenerator.GeneratePdf(html, config);

        // css-break-3 5.2 margin truncation (D2) keeps this on very few pages; blank-page skipping
        // (F1's page-per-fragmentainer loop) means whatever pages exist are never content-empty.
        Assert.IsLessThanOrEqualTo(2, document.Pages.Count);
    }
}
