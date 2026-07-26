using System.Runtime.InteropServices;
using PdfSharp;
using PdfSharp.Drawing;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

[TestClass]
public sealed class PdfGeneratorTests
{
    [TestMethod]
    public void GeneratePdf_FromHtml_CreatesPdfDocument()
    {
        // Arrange
        var config = new PdfGenerateConfig
        {
            PageSize = PageSize.A4
        };
        config.SetMargins(20);

        const string html = """
            <html>
                <head>
                    <link rel="Stylesheet" href="StyleSheet" />
                </head>
                <body>
                    <h1>PdfSharp</h1>
                    <p>This is html rendered text <img src="ImageIcon" /></p>
                </body>
            </html>
            """;

        // Act
        using var document = PdfGenerator.GeneratePdf(html, config, null, DemoUtils.OnStylesheetLoad, OnImageLoadPdfSharp);

        // Assert
        Assert.AreEqual(1, document.Pages.Count);

        using var stream = new MemoryStream();
        document.Save(stream, false);

        var pdf = stream.ToArray();
        Assert.IsGreaterThan(4, pdf.Length);
        Assert.AreEqual("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));

        var pdfOutputDirectory = Environment.GetEnvironmentVariable("PDF_OUTPUT_DIRECTORY");
        if (string.IsNullOrWhiteSpace(pdfOutputDirectory))
        {
            pdfOutputDirectory = Path.Combine(Path.GetTempPath(), "html-renderer-pdf-artifacts");
        }

        Directory.CreateDirectory(pdfOutputDirectory);

        var pdfPath = Path.Combine(
            pdfOutputDirectory,
            $"{nameof(GeneratePdf_FromHtml_CreatesPdfDocument)}-{GetPlatformName()}.pdf");
        File.WriteAllBytes(pdfPath, pdf);
    }

    [TestMethod]
    public void GeneratePdf_FromHtml_WithMultipleFonts_CreatesPdfDocument()
    {
        // Arrange
        var config = new PdfGenerateConfig
        {
            PageSize = PageSize.A4
        };
        config.SetMargins(20);

        const string html = """
            <html>
                <head>
                    <link rel="Stylesheet" href="StyleSheet" />
                    <style>
                        .helvetica { font-family: Helvetica; }
                        .mono { font-family: monospace; }
                    </style>
                </head>
                <body>
                    <p class="helvetica">Helvetica text</p>
                    <p class="mono">Monospace text</p>
                </body>
            </html>
            """;

        // Act
        using var document = PdfGenerator.GeneratePdf(html, config, null, DemoUtils.OnStylesheetLoad, null);

        // Assert
        Assert.AreEqual(1, document.Pages.Count);

        using var stream = new MemoryStream();
        document.Save(stream, false);

        var pdf = stream.ToArray();
        Assert.IsGreaterThan(4, pdf.Length);
        Assert.AreEqual("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));

        var pdfOutputDirectory = Environment.GetEnvironmentVariable("PDF_OUTPUT_DIRECTORY");
        if (string.IsNullOrWhiteSpace(pdfOutputDirectory))
        {
            pdfOutputDirectory = Path.Combine(Path.GetTempPath(), "html-renderer-pdf-artifacts");
        }

        Directory.CreateDirectory(pdfOutputDirectory);

        var pdfPath = Path.Combine(
            pdfOutputDirectory,
            $"{nameof(GeneratePdf_FromHtml_WithMultipleFonts_CreatesPdfDocument)}-{GetPlatformName()}.pdf");
        File.WriteAllBytes(pdfPath, pdf);
    }

    private static void OnImageLoadPdfSharp(object? sender, HtmlImageLoadEventArgs e)
    {
        if (!string.Equals(e.Src, "ImageIcon", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        using var imageStream = DemoUtils.GetImageStream(e.Src);
        if (imageStream == null)
        {
            throw new InvalidOperationException("Expected demo image resource was not found.");
        }

        e.Callback(XImage.FromStream(imageStream));
    }

    private static string GetPlatformName()
    {
        if (OperatingSystem.IsWindows())
        {
            return "windows";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "macos";
        }

        if (OperatingSystem.IsLinux())
        {
            return "linux";
        }

        return RuntimeInformation.RuntimeIdentifier;
    }
}