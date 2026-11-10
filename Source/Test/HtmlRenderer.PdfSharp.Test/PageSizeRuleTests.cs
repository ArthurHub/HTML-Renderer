using PdfSharp;
using PdfSharp.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

// Parallel full layout passes race on the adapter singletons.
[TestClass]
[DoNotParallelize]
public sealed class PageSizeRuleTests
{
    private static async Task<PdfDocument> Generate(string css, string body = "<p>x</p>") =>
        await PdfGenerator.GeneratePdf(
            $"<html><head><style>{css}</style></head><body>{body}</body></html>",
            new PdfGenerateConfig { PageSize = PageSize.A4 });

    [TestMethod]
    [Spec("css-page-3", "#page-model")]
    public async Task PageSize_ComesFromTheConfiguration()
    {
        using var document = await Generate("");

        Assert.AreEqual(595, document.Pages[0].Width.Point, 1);
        Assert.AreEqual(842, document.Pages[0].Height.Point, 1);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-page-3.yml")]
    [Spec("css-page-3", "#page-model")]
    [Spec("css-page-3", "#page-size-prop")]
    [DataRow("@page { size: A5 }", 420d, 595d)]
    [DataRow("@page { size: 200pt 300pt }", 200d, 300d)]
    [DataRow("@page { size: landscape }", 842d, 595d)]
    public async Task SizeProperty_SetsThePageSize(string css, double width, double height)
    {
        using var document = await Generate(css);

        Assert.AreEqual(width, document.Pages[0].Width.Point, 1);
        Assert.AreEqual(height, document.Pages[0].Height.Point, 1);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-page-3.yml")]
    [Spec("css-page-3", "#using-named-pages")]
    public async Task NamedPage_UsesItsPageRule()
    {
        using var document = await Generate(
            "@page wide { size: 842pt 595pt } div { page: wide }",
            "<p>x</p><div>wide</div>");

        Assert.AreEqual(2, document.Pages.Count);
        Assert.AreEqual(595, document.Pages[0].Width.Point, 1);
        Assert.AreEqual(842, document.Pages[1].Width.Point, 1);
    }
}
