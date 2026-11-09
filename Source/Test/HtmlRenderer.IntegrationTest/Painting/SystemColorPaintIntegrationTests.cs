using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class SystemColorPaintIntegrationTests
{
    [TestMethod]
    [DataRow("ActiveBorder")]
    [DataRow("ActiveCaption")]
    [DataRow("AppWorkspace")]
    [DataRow("Background")]
    [DataRow("ButtonFace")]
    [DataRow("ButtonHighlight")]
    [DataRow("ButtonShadow")]
    [DataRow("ButtonText")]
    [DataRow("CaptionText")]
    [DataRow("GrayText")]
    [DataRow("Highlight")]
    [DataRow("HighlightText")]
    [DataRow("InactiveBorder")]
    [DataRow("InactiveCaption")]
    [DataRow("InactiveCaptionText")]
    [DataRow("InfoBackground")]
    [DataRow("InfoText")]
    [DataRow("Menu")]
    [DataRow("MenuText")]
    [DataRow("Scrollbar")]
    [DataRow("ThreeDDarkShadow")]
    [DataRow("ThreeDFace")]
    [DataRow("ThreeDHighlight")]
    [DataRow("ThreeDLightShadow")]
    [DataRow("ThreeDShadow")]
    [DataRow("Window")]
    [DataRow("WindowFrame")]
    [DataRow("WindowText")]
    [Spec("css21", "ui.html#system-colors")]
    public void SystemColor_IsAValidBackgroundColor(string name)
    {
        var placeholder = RColor.FromArgb(1, 2, 3);
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            $"<div id='b' style='width:100px;height:50px;background-color:rgb(1,2,3);background-color:{name}'></div>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "b")!);

        var fill = g.Log.OfType<RecordingGraphics.DrawRectCall>().Single();
        Assert.AreNotEqual(placeholder, fill.Color);
        Assert.AreEqual(255, fill.Color.A);
    }

    [TestMethod]
    [DataRow("windowtext")]
    [DataRow("WINDOWTEXT")]
    [Spec("css21", "ui.html#system-colors")]
    public void SystemColor_IsCaseInsensitive(string name)
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            $"<div id='b' style='color:rgb(1,2,3);color:{name}'>x</div>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "b")!);

        Assert.AreNotEqual(RColor.FromArgb(1, 2, 3), g.DrawStringCalls.Single().Color);
    }
}
