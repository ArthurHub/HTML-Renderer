using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class ColorFunctionPaintIntegrationTests
{
    [TestMethod]
    [DataRow("lab(50 40 59.5)", 255, 191, 87, 0)]
    [DataRow("oklch(62.8% 0.258 29.2 / 0.5)", 128, 255, 0, 0)]
    [DataRow("hwb(120 0% 0%)", 255, 0, 255, 0)]
    [Spec("css-color-4", "#the-color-property")]
    [Spec("css-color-4", "#lab-colors")]
    public void ColorFunction_PaintsTextAndBackground(string color, int a, int r, int g, int b)
    {
        AssertPainted(color, RColor.FromArgb(a, r, g, b));
    }

    [TestMethod]
    [DataRow("color-mix(in srgb, red, blue)", 255, 128, 0, 128)]
    [DataRow("color-mix(in srgb, red 20%, blue 20%)", 102, 128, 0, 128)]
    [Spec("css-color-5", "#color-mix")]
    public void ColorMix_PaintsTextAndBackground(string color, int a, int r, int g, int b)
    {
        AssertPainted(color, RColor.FromArgb(a, r, g, b));
    }

    private static void AssertPainted(string color, RColor expected)
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            $"<div id='b' style='width:100px;height:50px;color:{color};background-color:{color}'>x</div>"));

        var graphics = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "b")!);

        Assert.AreEqual(expected, graphics.DrawStringCalls.Single().Color);
        Assert.AreEqual(expected, graphics.Log.OfType<RecordingGraphics.DrawRectCall>().Single().Color);
    }
}
