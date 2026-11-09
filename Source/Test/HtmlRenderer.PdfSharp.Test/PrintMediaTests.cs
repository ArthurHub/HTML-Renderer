using PdfSharp;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

// Parallel full layout passes race on the adapter singletons.
[TestClass]
[DoNotParallelize]
public sealed class PrintMediaTests
{
    [TestMethod]
    [Spec("html", "rendering.html#print-media")]
    public async Task Pdf_IsRenderedForPrintMedia()
    {
        const string html = "<html><head><style>div { height: 10pt } @media print { div { height: 1000pt } }</style></head>"
            + "<body style='margin:0'><div></div></body></html>";

        using var document = await PdfGenerator.GeneratePdf(html, new PdfGenerateConfig { PageSize = PageSize.A4 });

        Assert.AreEqual(2, document.Pages.Count);
    }
}
