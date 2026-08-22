using System.Text;
using PdfSharp;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

/// <summary>
/// End-to-end confirmation, through the real <see cref="PdfGenerator"/> path, of the fragment-tree-level
/// fix verified in <c>FixedPositionRepeatsPerPageTest</c> (HtmlRenderer.IntegrationTest): a
/// <c>position:fixed</c> element (css-position-3, paged media - "fixed positioned boxes are thus
/// replicated on every page") must show up in every generated PDF page, not just the page its
/// <c>top</c>/<c>left</c> offset happened to land on when misinterpreted as an absolute document
/// coordinate.
/// </summary>
/// <remarks>
/// Verified by a RELATIVE Tj-operator-count comparison (with the fixed header vs. without, same filler
/// content otherwise), not a literal-text search: PdfSharp draws through a Type0/CID font here, so a
/// page's content stream holds hex glyph-index strings (<c>&lt;0037004B...&gt; Tj</c>), never the source
/// text itself - the same reality <c>MultiPageTextVisibilityTest</c> works around by checking only for a
/// <c>Tj</c> operator's presence, not its content. A page with genuinely one extra line of fixed content
/// drawn on it gets exactly one extra <c>Tj</c> versus the same page without that content.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class FixedPositionRepeatsPerPdfPageTest
{
    private static int CountTj(byte[] streamBytes) =>
        Encoding.Latin1.GetString(streamBytes).Split("Tj").Length - 1;

    [TestMethod]
    public async Task FixedHeaderMarker_AddsOneExtraTextOperatorToEveryGeneratedPage()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        var sentence = "This is a moderately long sentence used to build a paragraph that will wrap across many lines and, eventually, across more than one page. ";
        var body = $"<p>{string.Concat(Enumerable.Repeat(sentence, 200))}</p>";

        using var withFixed = await PdfGenerator.GeneratePdf(
            $"""<html><body><div style="position:fixed; top:5px; left:5px;">FixedHeaderMarkerText</div>{body}</body></html>""",
            config);
        using var withoutFixed = await PdfGenerator.GeneratePdf($"<html><body>{body}</body></html>", config);

        Assert.IsGreaterThanOrEqualTo(3, withoutFixed.Pages.Count, "test content should span at least 3 pages for this to be meaningful");
        Assert.AreEqual(withoutFixed.Pages.Count, withFixed.Pages.Count, "adding a fixed header should not itself change how many pages the body content needs");

        for (var i = 0; i < withFixed.Pages.Count; i++)
        {
            var withCount = CountTj(withFixed.Pages[i].Contents.Elements.GetDictionary(0)!.Stream.Value);
            var withoutCount = CountTj(withoutFixed.Pages[i].Contents.Elements.GetDictionary(0)!.Stream.Value);
            Assert.AreEqual(withoutCount + 1, withCount,
                $"page {i} should have exactly one extra text-drawing operator for the repeated fixed header (with={withCount}, without={withoutCount})");
        }
    }
}
