using System.Text;
using PdfSharp;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

/// <summary>
/// Every PDF page draws its own text, not only the first one.
/// </summary>
// Parallel full layout passes race on the adapter singletons.
[TestClass]
[DoNotParallelize]
public sealed class MultiPageTextVisibilityTest
{
    [TestMethod]
    public async Task EveryPage_HasRealTextDrawingOperators()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

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
