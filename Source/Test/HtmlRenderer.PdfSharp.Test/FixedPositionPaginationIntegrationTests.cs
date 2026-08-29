using System.Text;
using System.Text.RegularExpressions;
using PdfSharp;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

/// <summary>
/// Ported from PeachPDF.Tests' <c>FixedPositionPaginationIntegrationTests</c>, which confirms
/// <c>position:fixed</c> content repeats identically across real, multi-page <see cref="PdfGenerator"/>
/// output by scanning generated PDF content streams for a fixed box's own fill operator appearing on
/// every one of 3 real pages, produced there via <c>page-break-before:always</c>.
/// </summary>
/// <remarks>
/// <para>
/// This fork has no <c>page-break-before</c>/<c>page-break-after</c> support at all - already confirmed
/// and documented by <c>HtmlRenderer.IntegrationTest.Positioning.FixedPositionPaginationIntegrationTests</c>'s
/// own <c>PageBreakBefore_PushesFollowingContentToTheNextSimulatedPage</c> test, which is <c>[Ignore]</c>d
/// for exactly that reason (no <c>PageBreakBefore</c>/<c>PageBreakAfter</c> property anywhere in
/// <c>CssBoxProperties</c>, and the default stylesheet's own rules for it are inert). Real multiple pages
/// are forced here the same way <c>MultiPageTextVisibilityTest</c>/<c>FixedPositionRepeatsPerPdfPageTest</c>
/// already do it: enough filler paragraph content to genuinely overflow several A4 pages.
/// </para>
/// <para>
/// This is deliberately NOT a duplicate of the two fixed-position tests already in this branch's
/// history:
/// </para>
/// <list type="bullet">
/// <item><c>HtmlRenderer.IntegrationTest.FixedPositionRepeatsPerPageTest</c> asserts against the
/// fragment tree directly (<c>FragmentainerFragment.Root</c> word positions) and never generates a real
/// PDF at all.</item>
/// <item><c>HtmlRenderer.PdfSharp.Test.FixedPositionRepeatsPerPdfPageTest</c> does go through the real
/// generator, but only ever checks a relative <c>Tj</c>-operator COUNT for fixed TEXT content - it never
/// confirms a fixed box's own painted geometry (a <c>background-color</c> fill, drawn through
/// <c>GraphicsAdapter.DrawRectangle(RBrush,...)</c>, a different code path than <c>DrawString</c>), and
/// never confirms the fixed content repaints at the SAME page-local position on every page rather than
/// merely "some extra content exists somewhere".</item>
/// </list>
/// <para>
/// This test fills that specific, previously-uncovered gap. The content-stream shape of a filled rect -
/// a "&lt;r&gt; &lt;g&gt; &lt;b&gt; rg" color-set operator followed (not necessarily immediately, since
/// PdfSharp's writer elides a redundant "gs"/state push in between) by an "x y w h re" path and an "f"
/// fill operator - was confirmed empirically by dumping a real generated content stream for this exact
/// fixture during porting, not guessed.
/// </para>
/// </remarks>
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
