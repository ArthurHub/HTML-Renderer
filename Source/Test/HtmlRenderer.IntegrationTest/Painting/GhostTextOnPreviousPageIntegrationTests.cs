using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HtmlRenderer.IntegrationTest.Painting;

/// <summary>
/// A box moved flush to the next page's top paints nothing on the page it left.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class GhostTextOnPreviousPageIntegrationTests
{
    private const double PageHeight = 400.0;

    [TestMethod]
    [Spec("css21", "page.html#forced")]
    public void ForcedBreak_RelocatedHeading_DoesNotPaintOnPreviousPage()
    {
        var html = PaintHarness.Wrap(
            "<div style='height:350px;background:rgb(240,240,240)'></div>"
            + "<h2 id='second' style='page-break-before:always;margin:0'>RelocatedHeadingMarker</h2>");

        var (root, container) = PaintHarness.LayoutPaginated(html, pageHeight: PageHeight, margin: 0);

        var heading = PaintHarness.FindById(root, "second");
        Assert.IsNotNull(heading);

        // The heading touches the page boundary exactly.
        Assert.AreEqual(PageHeight, heading!.Location.Y, 0.01);

        var page0 = PaintHarness.PaintPage(container, 0);
        var page1 = PaintHarness.PaintPage(container, 1);

        Assert.IsFalse(page0.DrawStringCalls.Any(c => c.Text.Contains("RelocatedHeadingMarker")));
        Assert.IsTrue(page1.DrawStringCalls.Any(c => c.Text.Contains("RelocatedHeadingMarker")));
    }

    [TestMethod]
    [Spec("css21", "page.html#page-break-props")]
    public void BreakInsideAvoid_RelocatedBox_DoesNotPaintOnPreviousPage()
    {
        // 20px are left on page 0, too little for the three 12px lines.
        var html = PaintHarness.Wrap(
            "<div style='height:380px;background:rgb(240,240,240)'></div>"
            + "<div id='avoid' style='break-inside:avoid;page-break-inside:avoid;margin:0'>"
            + "<p style='margin:0;font-size:10px;line-height:12px'>AvoidedParagraphMarker</p>"
            + "<p style='margin:0;font-size:10px;line-height:12px'>Second line</p>"
            + "<p style='margin:0;font-size:10px;line-height:12px'>Third line</p>"
            + "</div>");

        var (root, container) = PaintHarness.LayoutPaginated(html, pageHeight: PageHeight, margin: 0);

        var avoidBox = PaintHarness.FindById(root, "avoid");
        Assert.IsNotNull(avoidBox);
        Assert.AreEqual(PageHeight, avoidBox!.Location.Y, 0.01);

        var page0 = PaintHarness.PaintPage(container, 0);
        var page1 = PaintHarness.PaintPage(container, 1);

        Assert.IsFalse(page0.DrawStringCalls.Any(c => c.Text.Contains("AvoidedParagraphMarker")));
        Assert.IsTrue(page1.DrawStringCalls.Any(c => c.Text.Contains("AvoidedParagraphMarker")));
    }
}
