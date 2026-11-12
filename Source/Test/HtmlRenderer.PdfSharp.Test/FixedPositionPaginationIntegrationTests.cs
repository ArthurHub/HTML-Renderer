using System.Text;
using System.Text.RegularExpressions;
using PdfSharp;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

/// <summary>
/// <c>position: fixed</c> content repaints at the same page position on every generated PDF page, checked
/// through the fill of a fixed box in each page's content stream. Filler content makes the pages.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class FixedPositionPaginationIntegrationTests
{
    [TestMethod]
    public async Task FixedPositionBox_BackgroundRepeatsAtTheSamePosition_OnEveryRealGeneratedPage()
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4 };
        config.SetMargins(20);

        // Generous filler, not a precisely-calibrated boundary - a tight "just barely" filler count is
        // fragile to font substitution across CI platforms (see StageF1VerificationTest's own remark).
        var sentence = "This is a moderately long sentence used to build a paragraph that will wrap across many lines and, eventually, across more than one page. ";
        var body = $"<p>{string.Concat(Enumerable.Repeat(sentence, 200))}</p>";
        var html = $"""
            <html><body>
                <div style="position:fixed; top:10pt; left:10pt; width:30pt; height:30pt; background-color: rgb(12,34,56);"></div>
                {body}
            </body></html>
            """;

        using var document = await PdfGenerator.GeneratePdf(html, config);

        Assert.IsGreaterThanOrEqualTo(3, document.Pages.Count, "test content should span at least 3 pages for this to be a meaningful check.");

        // rgb(12,34,56) as PDF 0..1 fractions (12/255, 34/255, 56/255 -> ~0.047, ~0.133, ~0.220),
        // tolerant of PdfSharp's own variable-precision decimal formatting of each component.
        var fixedRectPattern = new Regex(@"0\.04\d* 0\.13\d* 0\.2\d* rg[\s\S]{0,80}?([\d.]+) ([\d.]+) ([\d.]+) ([\d.]+) re\s*\r?\nf");

        var hasFirst = false;
        var firstX = 0.0;
        var firstY = 0.0;
        for (var i = 0; i < document.Pages.Count; i++)
        {
            var content = document.Pages[i].Contents.Elements.GetDictionary(0);
            var text = Encoding.Latin1.GetString(content!.Stream.Value);
            var matches = fixedRectPattern.Matches(text);

            Assert.AreEqual(1, matches.Count, $"page {i} should draw the fixed box's background exactly once - not zero (missing) and not more than one (duplicated).");

            var x = double.Parse(matches[0].Groups[1].Value);
            var y = double.Parse(matches[0].Groups[2].Value);
            if (!hasFirst)
            {
                firstX = x;
                firstY = y;
                hasFirst = true;
            }
            else
            {
                Assert.AreEqual(firstX, x, 0.5, $"page {i}'s fixed box should paint at the same page-local X as every other page.");
                Assert.AreEqual(firstY, y, 0.5, $"page {i}'s fixed box should paint at the same page-local Y as every other page.");
            }
        }
    }
}
