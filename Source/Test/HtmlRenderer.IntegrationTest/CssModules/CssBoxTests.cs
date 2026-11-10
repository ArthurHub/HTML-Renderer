using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.CssModules;

[DoNotParallelize]
[TestClass]
public sealed class CssBoxTests
{
    private const double Delta = 0.5;

    [TestMethod]
    [Spec("css-box-3", "#margin-physical")]
    public void MarginLonghands_SetTheirOwnSideOnly()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:200px'><div id='b' style='margin-top:5px;margin-right:7px;margin-bottom:11px;margin-left:13px;height:10px'></div><div id='n' style='height:10px'></div></div>"));
        var b = LayoutHarness.FindById(root, "b")!;
        var n = LayoutHarness.FindById(root, "n")!;

        Assert.AreEqual(13, b.Location.X, Delta);
        Assert.AreEqual(5, b.Location.Y, Delta);
        Assert.AreEqual(200 - 13 - 7, b.Size.Width, Delta);
        Assert.AreEqual(b.ActualBottom + 11, n.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css-box-3", "#margin-physical")]
    public void Margin_DoesNotApplyToTableCells()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr><td id='a' style='padding:0'>a</td><td id='b' style='margin-left:50px;padding:0'>b</td></tr></table>"));
        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(a.ActualRight, b.Location.X, Delta);
    }

    [TestMethod]
    [Spec("css-box-3", "#padding-physical")]
    public void PaddingLonghands_SetTheirOwnSideOnly()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='b' style='width:100px;padding-top:5px;padding-right:7px;padding-bottom:11px;padding-left:13px'><div id='c' style='height:10px'></div></div>"));
        var b = LayoutHarness.FindById(root, "b")!;
        var c = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(13, c.Location.X, Delta);
        Assert.AreEqual(5, c.Location.Y, Delta);
        Assert.AreEqual(100 + 13 + 7, b.Size.Width, Delta);
        Assert.AreEqual(10 + 5 + 11, b.Size.Height, Delta);
    }

    [TestMethod]
    [Spec("css-box-3", "#padding-physical")]
    public void Padding_AppliesToTableCells()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr><td id='a' style='padding:0 0 0 30px'>a</td></tr></table>"));
        var a = LayoutHarness.FindById(root, "a")!;

        Assert.AreEqual(30, LayoutHarness.Descendants(a).SelectMany(d => d.Words).First().Left - a.Location.X, Delta);
    }
}
