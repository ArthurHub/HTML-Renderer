using PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

/// <summary>
/// Tests for <see cref="PageSizeConverter"/> from the PDFsharp NuGet package, which
/// <see cref="TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.AddPdfPagesAsync(PdfSharp.Pdf.PdfDocument, string, PdfGenerateConfig, TheArtOfDev.HtmlRenderer.Core.Entities.CssData, System.EventHandler{TheArtOfDev.HtmlRenderer.Core.Entities.HtmlStylesheetLoadEventArgs}, System.EventHandler{TheArtOfDev.HtmlRenderer.Core.Entities.HtmlImageLoadEventArgs})"/>
/// calls directly to resolve the page size in points for each configured <see cref="PageSize"/>.
/// </summary>
[TestClass]
public sealed class PageSizeConverterTests
{
    [TestMethod]
    [DataRow(PageSize.A4, 595d, 842d)]
    [DataRow(PageSize.Letter, 612d, 792d)]
    [DataRow(PageSize.Legal, 612d, 1008d)]
    [DataRow(PageSize.A0, 2384d, 3370d)]
    [DataRow(PageSize.Tabloid, 792d, 1224d)]
    public void ToSize_KnownPageSize_ReturnsExpectedPointDimensions(PageSize pageSize, double expectedWidth, double expectedHeight)
    {
        var size = PageSizeConverter.ToSize(pageSize);

        Assert.AreEqual(expectedWidth, size.Width);
        Assert.AreEqual(expectedHeight, size.Height);
    }

    [TestMethod]
    public void ToSize_Undefined_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => PageSizeConverter.ToSize(PageSize.Undefined));
    }
}
