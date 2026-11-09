using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class ForegroundColorPaintIntegrationTests
{
    [TestMethod]
    [DataRow("rgb(10,20,30)")]
    [DataRow("#0a141e")]
    [Spec("css21", "colors.html#colors")]
    public void Color_PaintsTheText(string color)
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            $"<div id='b' style='color:{color}'>x</div>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "b")!);

        Assert.AreEqual(RColor.FromArgb(10, 20, 30), g.DrawStringCalls.Single().Color);
    }

    [TestMethod]
    [Spec("css21", "colors.html#colors")]
    public void Color_IsInherited()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div style='color:rgb(10,20,30)'><p id='b' style='margin:0'><span>x</span></p></div>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "b")!);

        Assert.AreEqual(RColor.FromArgb(10, 20, 30), g.DrawStringCalls.Single().Color);
    }

    [TestMethod]
    [Spec("css21", "colors.html#colors")]
    public void Color_InitialValueIsBlack()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap("<div id='b'>x</div>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "b")!);

        Assert.AreEqual(RColor.Black, g.DrawStringCalls.Single().Color);
    }
}
