using System.Drawing;
using System.Drawing.Imaging;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.TestSupport;

/// <summary>Images for replaced element tests, loaded during layout so it sees their intrinsic size.</summary>
internal static class TestImages
{
    /// <summary>A transparent PNG of the given size as a data URI.</summary>
    internal static string Png(int width, int height)
    {
        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return "data:image/png;base64," + Convert.ToBase64String(stream.ToArray());
    }

    /// <summary>Lays out a body fragment, loading its images synchronously.</summary>
    internal static CssBox Layout(string body, double maxWidth = 1000) =>
        LayoutHarness.Layout(LayoutHarness.Wrap(body), maxWidth, prepare: root =>
        {
            root.HtmlContainer!.AvoidAsyncImagesLoading = true;
            root.HtmlContainer.AvoidImagesLateLoading = true;
        }).Root;

    /// <summary>The border box of the image with the given id.</summary>
    internal static RRect ImageRect(CssBox root, string id) =>
        LayoutHarness.FindById(root, id)!.Rectangles.Values.Single();
}
