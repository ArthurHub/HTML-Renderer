using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Tables;

[TestClass]
public sealed class ColspanIntegrationTests
{
    private const double Delta = 1.0;

    [TestMethod]
    [Spec("html", "tables.html#forming-a-table")]
    [Spec("css-tables-3", "#dimensioning-the-row-column-grid--step1")]
    public void RowsWithDifferentCellCounts_ShareTheColumnsTheirColspansAddUpTo()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table><tr><td id='a'>Test</td><td colspan='3'>8 570</td><td id='b'>Test</td></tr>"
            + "<tr><td colspan='4'>1 449</td><td id='c'>1 187</td></tr></table>"));
        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;
        var c = LayoutHarness.FindById(root, "c")!;

        Assert.IsGreaterThan(a.ActualBottom, c.Location.Y);
        Assert.AreEqual(b.Location.X, c.Location.X, Delta);
    }
}
