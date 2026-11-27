using System.Text;
using System.Text.RegularExpressions;
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.SkiaSharp;

namespace HtmlRenderer.SkiaSharp.Test;

// Parallel full layout passes race on the adapter singletons.
[TestClass]
[DoNotParallelize]
public sealed class PdfGeneratorTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task GeneratePdf_FromHtml_WritesOnePage()
    {
        const string html = """
            <html>
                <head><link rel="Stylesheet" href="StyleSheet" /></head>
                <body>
                    <h1>SkiaSharp</h1>
                    <p>This is html rendered text</p>
                </body>
            </html>
            """;

        var pdf = await PdfGenerator.GeneratePdfAsync(html, stylesheetLoad: DemoUtils.OnStylesheetLoad);
        SaveArtifact(pdf);

        Assert.AreEqual("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
        Assert.AreEqual(1, CountPages(pdf));
    }

    [TestMethod]
    public async Task GeneratePdf_LongContent_BreaksIntoPages()
    {
        var html = "<body>" + string.Concat(Enumerable.Range(1, 200).Select(i => $"<p>Paragraph {i}</p>")) + "</body>";

        using var stream = new MemoryStream();
        var pageCount = await PdfGenerator.GeneratePdfAsync(html, stream);
        var pdf = stream.ToArray();
        SaveArtifact(pdf);

        Assert.IsGreaterThan(1, pageCount);
        Assert.AreEqual(pageCount, CountPages(pdf));
    }

    [TestMethod]
    public async Task GeneratePdf_EmbedsPngAndJpegImages()
    {
        var png = CreateImageDataUri(SKEncodedImageFormat.Png, "image/png");
        var jpeg = CreateImageDataUri(SKEncodedImageFormat.Jpeg, "image/jpeg");
        var html = $"<img src='{png}' width='20' height='20'><img src='{jpeg}' width='20' height='20'>" +
                   $"<div style='width: 40px; height: 40px; background-image: url({png})'></div>";

        var pdf = await PdfGenerator.GeneratePdfAsync(html);
        SaveArtifact(pdf);

        // A JPEG is embedded as is, not decoded to pixels
        var content = Encoding.ASCII.GetString(pdf);
        Assert.AreEqual(1, CountPages(pdf));
        Assert.Contains("/Subtype /Image", content);
        Assert.Contains("/DCTDecode", content);
    }

    [TestMethod]
    public async Task GeneratePdf_KeepsPngImagesLosslessWithNewMetadata()
    {
        var png = CreateImageDataUri(SKEncodedImageFormat.Png, "image/png");
        var config = new PdfGenerateConfig { Metadata = new SKDocumentPdfMetadata { Title = "Lossless" } };

        var pdf = await PdfGenerator.GeneratePdfAsync($"<img src='{png}' width='20' height='20'>", config);

        var content = Encoding.ASCII.GetString(pdf);
        Assert.Contains("/Subtype /Image", content);
        Assert.DoesNotContain("/DCTDecode", content);
    }

    [TestMethod]
    public async Task GeneratePdf_UsesThePrintMediaType()
    {
        const string html = "<style>@media screen { div { height: 5000px } }</style><div>Short on paper</div>";

        var pdf = await PdfGenerator.GeneratePdfAsync(html);

        Assert.AreEqual(1, CountPages(pdf));
    }

    [TestMethod]
    public async Task GeneratePdf_PageSize_SetsTheMediaBox()
    {
        var config = new PdfGenerateConfig { PageSize = PdfPageSizes.Landscape(PdfPageSizes.A4) };

        var pdf = await PdfGenerator.GeneratePdfAsync("<p>Landscape</p>", config);

        StringAssert.Matches(Encoding.ASCII.GetString(pdf), new Regex(@"/MediaBox \[0 0 842 595\]"));
    }

    [TestMethod]
    public async Task GeneratePdf_Links_BecomeAnnotations()
    {
        const string html = """
            <a href="https://github.com/ArthurHub/HTML-Renderer">Web link</a>
            <a href="#target">Anchor link</a>
            <div style="height: 2000px"></div>
            <h2 id="target">Target</h2>
            """;

        var pdf = await PdfGenerator.GeneratePdfAsync(html);
        SaveArtifact(pdf);
        var text = Encoding.ASCII.GetString(pdf);

        StringAssert.Contains(text, "https://github.com/ArthurHub/HTML-Renderer");
        StringAssert.Contains(text, "target");
        Assert.IsGreaterThan(1, CountPages(pdf));
    }

    [TestMethod]
    public async Task GeneratePdf_EmptyHtml_WritesABlankPage()
    {
        var pdf = await PdfGenerator.GeneratePdfAsync(string.Empty);

        Assert.AreEqual(1, CountPages(pdf));
    }

    private static string CreateImageDataUri(SKEncodedImageFormat format, string mediaType)
    {
        using var surface = SKSurface.Create(new SKImageInfo(4, 4));
        surface.Canvas.Clear(SKColors.Lime);
        using var image = surface.Snapshot();
        using var data = image.Encode(format, 100);
        return $"data:{mediaType};base64," + Convert.ToBase64String(data.ToArray());
    }

    private static int CountPages(byte[] pdf)
    {
        return Regex.Matches(Encoding.ASCII.GetString(pdf), @"/Type /Page\b").Count;
    }

    private void SaveArtifact(byte[] pdf)
    {
        var outputDirectory = Environment.GetEnvironmentVariable("PDF_OUTPUT_DIRECTORY");
        if (string.IsNullOrWhiteSpace(outputDirectory))
            return;

        var directory = Path.Combine(outputDirectory, "SkiaSharp");
        Directory.CreateDirectory(directory);
        File.WriteAllBytes(Path.Combine(directory, TestContext.TestName + ".pdf"), pdf);
    }
}
