using System.Drawing;
using System.Drawing.Imaging;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.WinForms.Adapters;

namespace HtmlRenderer.IntegrationTest.Content;

[DoNotParallelize]
[TestClass]
public sealed class RasterImageSizeIntegrationTests
{
    private const double Delta = 0.5;

    [TestMethod]
    [Spec("css21", "conform.html#defs")]
    public void RasterImage_IsOneCssPixelPerImagePixel()
    {
        var container = new HtmlContainerInt(WinFormsAdapter.Instance)
        {
            MaxSize = new RSize(1000, 1000),
            AvoidAsyncImagesLoading = true
        };
        container.SetHtml(LayoutHarness.Wrap($"<img id='i' src='{PngDataUri(7, 5)}'>")).GetAwaiter().GetResult();

        using var bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bitmap);
        using var graphics = new GraphicsAdapter(g, false);
        container.PerformLayout(graphics);

        var image = LayoutHarness.FindById(container.Root!, "i")!;

        Assert.AreEqual(7, image.Words[0].Width, Delta);
        Assert.AreEqual(5, image.Words[0].Height, Delta);
    }

    private static string PngDataUri(int width, int height)
    {
        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return "data:image/png;base64," + Convert.ToBase64String(stream.ToArray());
    }
}
