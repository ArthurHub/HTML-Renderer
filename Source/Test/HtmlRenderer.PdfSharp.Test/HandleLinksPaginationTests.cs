using PdfSharp;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

/// <summary>
/// A link annotation lands on the PDF page its link is painted on, also when blank page slots are skipped.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class HandleLinksPaginationTests
{
    [TestMethod]
    public async Task Link_PastGenuineMultiPageFillerContent_LandsOnExactlyOneCorrectlyMappedPage()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of body text</p>", 150));
        var html = $"""
            <html><body>
                {filler}
                <p><a href="https://example.com/">a link well past the first page's worth of filler</a></p>
            </body></html>
            """;

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.IsGreaterThan(1, document.Pages.Count, "filler content should span multiple pages for this to be meaningful");
        Assert.AreEqual(0, document.Pages[0].Annotations.Count, "the link sits well past the first page's worth of filler");

        var annotatedPages = Enumerable.Range(0, document.Pages.Count)
            .Where(i => document.Pages[i].Annotations.Count > 0)
            .ToList();
        Assert.AreEqual(1, annotatedPages.Count, "the single link should be attributed to exactly one page, never split or duplicated across pages by HandleLinks' per-fragmentainer band matching");
        Assert.AreEqual(1, document.Pages[annotatedPages[0]].Annotations.Count);
    }

    [TestMethod]
    public async Task Link_AfterAContentEmptySpacer_LandsOnTheCorrectMaterializedPage()
    {
        // The spacer has no text/background/border, so every page-slot it alone spans is content-empty
        // and is never materialized as a fragmentainer at all (FragmentEmitter.HasContentInBand /
        // PdfGenerator.AddPdfPages' blank-page-skipping loop) - the linked paragraph sits several grid
        // slots into the document but on an early materialized PDF page. HandleLinks must map the link's
        // fragmentainer band to the PDF page actually generated for it via slotToPage, not to its raw
        // (non-contiguous) SlotIndex.
        const string html = """
            <html><body>
                <p style='margin:0;'>page one content</p>
                <div style='height:2500pt;'></div>
                <p><a href="https://example.com/">a link after the gap</a></p>
            </body></html>
            """;

        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.IsGreaterThanOrEqualTo(2, document.Pages.Count);
        Assert.AreEqual(0, document.Pages[0].Annotations.Count, "the link is not on the first page");

        var annotatedPages = Enumerable.Range(0, document.Pages.Count)
            .Where(i => document.Pages[i].Annotations.Count > 0)
            .ToList();
        Assert.AreEqual(1, annotatedPages.Count, "the single link should be attributed to exactly one materialized page");
        Assert.AreEqual(1, document.Pages[annotatedPages[0]].Annotations.Count);

        // Blank-page skipping (css-break-3 5.2's margin truncation, plus the content-empty-slot skip)
        // keeps the document short despite the 2500pt gap - if the link were misattributed by a raw,
        // non-contiguous slot index instead of the materialized-page index, this would either throw
        // (indexing document.Pages out of range) or silently land on the wrong page.
        Assert.IsLessThanOrEqualTo(6, document.Pages.Count, "the 2500pt content-empty gap should be skipped, not paginated through as blank pages");
    }
}
