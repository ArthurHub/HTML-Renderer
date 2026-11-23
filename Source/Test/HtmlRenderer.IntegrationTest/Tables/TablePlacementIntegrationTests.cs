using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Tables;

[TestClass]
public sealed class TablePlacementIntegrationTests
{
    private const double Delta = 1.0;

    [TestMethod]
    public void Table_InAContainerAboveAndLeftOfTheOrigin_StaysInPlace()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<table id='t' style='border-spacing:0'><tr><td id='c' style='padding:0'>x</td></tr></table><p id='p' style='margin:0'>after</p>"),
            prepare: r => r.HtmlContainer!.Location = new RPoint(-30, -50));
        var table = LayoutHarness.FindById(root, "t")!;
        var cell = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(-30, table.Location.X, Delta);
        Assert.AreEqual(-50, table.Location.Y, Delta);
        Assert.AreEqual(table.Location.X, cell.Location.X, Delta);
        Assert.AreEqual(table.Location.Y, cell.Location.Y, Delta);
        Assert.AreEqual(table.ActualBottom, LayoutHarness.FindById(root, "p")!.Location.Y, Delta);
    }
}
