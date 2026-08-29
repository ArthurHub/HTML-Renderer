using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// Ported from PeachPDF.Tests' Acid2 table regression cases (CSS 2.1 §17.5.3 row-stretch, §17.6.1
/// border-spacing).
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class Acid2TableIntegrationTests
{
    private const double Delta = 0.5;

    [TestMethod]
    public void TableCell_ExplicitHeightShorterThanRow_StretchesToRowHeight()
    {
        var html = LayoutHarness.Wrap(
            "<table><tr>"
            + "<td id='tall' style='height:40px;'></td>"
            + "<td id='short' style='height:5px;'></td>"
            + "</tr></table>");
        var (root, _) = LayoutHarness.Layout(html);

        var tall = LayoutHarness.FindById(root, "tall")!;
        var shortCell = LayoutHarness.FindById(root, "short")!;

        Assert.AreEqual(tall.ActualBottom, shortCell.ActualBottom, Delta);
    }

    [TestMethod]
    public void BorderSpacingZero_AdjacentCells_HaveNoGapBetweenThem()
    {
        var html = LayoutHarness.Wrap(
            "<table style='border-spacing:0;'><tr>"
            + "<td id='c1' style='width:20px;'>a</td>"
            + "<td id='c2' style='width:20px;'>b</td>"
            + "</tr></table>");
        var (root, _) = LayoutHarness.Layout(html);

        var c1 = LayoutHarness.FindById(root, "c1")!;
        var c2 = LayoutHarness.FindById(root, "c2")!;

        Assert.AreEqual(c1.ActualRight, c2.Location.X, Delta);
    }
}
