using System.Text;
using PdfSharp;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

/// <summary>
/// Regression coverage for a real bug found while giving <c>FragmentPainter</c> a page-origin translate
/// (so <c>HtmlContainerInt.PerformPaint(RGraphics)</c>'s multi-fragmentainer fallback could stop
/// depending on <c>CssBox.Paint</c>): <c>FragmentPainter.PaintFragmentContent</c> painted line
/// backgrounds/borders from the fragment tree's already page-local rects, but painted the actual text via
/// <c>CssBox.PaintWords</c>, which reads <c>CssRect.Rectangle</c> straight off the live box tree - still
/// absolute document-Y - offset only by <c>ScrollOffset</c> (always zero for PDF generation). Every page
/// after the first got a content stream with zero text-draw operators, since a fresh per-page
/// <c>XGraphics</c>'s origin is that page's own band top, not the document's. Existing tests only ever
/// asserted page *count*, never that a page's content stream actually contains text - this would have
/// stayed silently broken indefinitely otherwise.
/// </summary>
// Concurrent full-layout-pass tests race on shared adapter singleton state (same MSTest ClassLevel
// parallelism issue documented for HtmlRenderer.IntegrationTest) - reproduced here: this test passes
// reliably alone but intermittently reports a missing Tj on page 0 when run alongside the rest of the
// suite.
[TestClass]
[DoNotParallelize]
public sealed class MultiPageTextVisibilityTest
{
    [TestMethod]
    public async Task EveryPage_HasRealTextDrawingOperators()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        // Sized well past what fits on one A4 page, so every page has genuine paragraph content, not
        // just a trailing sliver.
        var sentence = "This is a moderately long sentence used to build a paragraph that will wrap across many lines and, eventually, across more than one page. ";
        var html = $"<html><body><p>{string.Concat(Enumerable.Repeat(sentence, 200))}</p></body></html>";

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.IsGreaterThanOrEqualTo(3, document.Pages.Count, "Test content should span at least 3 pages for this to be a meaningful check.");

        for (var i = 0; i < document.Pages.Count; i++)
        {
            var content = document.Pages[i].Contents.Elements.GetDictionary(0);
            var text = Encoding.Latin1.GetString(content!.Stream.Value);
            StringAssert.Contains(text, "Tj", $"Page {i} has no text-drawing operators - its content is invisible.");
        }
    }
}
