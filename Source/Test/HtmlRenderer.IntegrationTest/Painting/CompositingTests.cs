using System.Drawing;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class CompositingTests
{
    [TestMethod]
    [Spec("compositing-1", "#simplealphacompositing")]
    [Spec("compositing-1", "#generalformula")]
    [Spec("compositing-1", "#porterduffcompositingoperators_srcover")]
    [Spec("compositing-1", "#blendingnormal")]
    public async Task TranslucentBoxes_AreDrawnOverEachOtherWithTheirAlpha()
    {
        using var image = (Bitmap)await HtmlRender.RenderToImageAsync(
            "<!DOCTYPE html><html><body style='margin:0'><div style='height:50px;background:rgba(0,0,255,0.5)'><div style='height:20px;background:rgba(255,0,0,0.5)'></div></div></body></html>",
            new Size(100, 60),
            Color.White);

        // Source-over: co = cs + cb (1 - as), half blue over white, then half red over that
        AssertColor(Color.FromArgb(127, 127, 255), image.GetPixel(50, 35));
        AssertColor(Color.FromArgb(191, 64, 127), image.GetPixel(50, 10));
    }

    private static void AssertColor(Color expected, Color actual)
    {
        Assert.AreEqual(expected.R, actual.R, 2, $"R of {actual}");
        Assert.AreEqual(expected.G, actual.G, 2, $"G of {actual}");
        Assert.AreEqual(expected.B, actual.B, 2, $"B of {actual}");
    }
}
