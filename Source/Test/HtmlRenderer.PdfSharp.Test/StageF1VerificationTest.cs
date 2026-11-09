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

        // Generous filler, fonts differ between platforms.
        var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of text</p>", 150));
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

        // Smoke check only, it doesn't verify which page holds which link.
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
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task HugeMargin_ProducesNoBlankPages()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        const string html = "<html><body><div style='margin-top:2000pt;'>content</div></body></html>";

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.IsLessThanOrEqualTo(2, document.Pages.Count);
    }
}
