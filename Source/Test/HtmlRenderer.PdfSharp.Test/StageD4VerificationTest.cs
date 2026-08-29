using PdfSharp;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

[TestClass]
public sealed class StageD4VerificationTest
{
    [TestMethod]
    public async Task LargeTableWithHeader_SpansMultiplePagesWithoutError()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        var rows = string.Concat(Enumerable.Range(0, 60)
            .Select(i => $"<tr><td>row {i} a</td><td>row {i} b</td></tr>"));
        var html = $"""
            <html><body>
                <table border="1" style="width:100%;">
                    <thead><tr><th>Column A</th><th>Column B</th></tr></thead>
                    <tbody>{rows}</tbody>
                </table>
            </body></html>
            """;

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.IsGreaterThan(1, document.Pages.Count);
    }
}
