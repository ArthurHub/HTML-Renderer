using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class CssImages4Tests
{
    // The adapter decodes every image as 40x30
    private const string Image = "url(data:image/png;base64,iVBORw0KGgo=)";

    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);
    private static readonly RColor Blue = RColor.FromArgb(0, 0, 255);

    [TestMethod]
    [DataRow("png")]
    [DataRow("gif")]
    [DataRow("jpeg")]
    [Spec("css-images-4", "#image-file-formats")]
    public async Task RasterImage_IsDecoded(string format)
    {
        string uri;
        using (var red = new System.Drawing.Bitmap(20, 10))
        using (var stream = new MemoryStream())
        {
            using (var graphics = System.Drawing.Graphics.FromImage(red))
                graphics.Clear(System.Drawing.Color.Red);
            red.Save(stream, format switch { "png" => System.Drawing.Imaging.ImageFormat.Png, "gif" => System.Drawing.Imaging.ImageFormat.Gif, _ => System.Drawing.Imaging.ImageFormat.Jpeg });
            uri = $"data:image/{format};base64,{Convert.ToBase64String(stream.ToArray())}";
        }

        using var page = (System.Drawing.Bitmap)await TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.RenderToImageAsync(
            $"<!DOCTYPE html><html><body style='margin:0'><img src='{uri}'></body></html>",
            new System.Drawing.Size(40, 20),
            System.Drawing.Color.White);

        var inside = page.GetPixel(15, 5);
        Assert.IsTrue(inside.R > 200 && inside.G < 50 && inside.B < 50, $"inside the image: {inside}");
        Assert.AreEqual(System.Drawing.Color.White.ToArgb(), page.GetPixel(30, 5).ToArgb());
    }

    [Ignore("Gap, see Spec/css-images-4.yml")]
    [TestMethod]
    [Spec("css-images-4", "#image-file-formats")]
    public void SvgImage_IsDecoded()
    {
        const string svg = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0naHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmcnIHdpZHRoPSc0MCcgaGVpZ2h0PSczMCcvPg==";
        var root = TestImages.Layout($"<img id='i' src='{svg}'>");

        var rect = TestImages.ImageRect(root, "i");

        Assert.AreEqual(40, rect.Width, 0.5);
        Assert.AreEqual(30, rect.Height, 0.5);
    }

    [Ignore("Gap, see Spec/css-images-4.yml")]
    [TestMethod]
    [DataRow($"image-set({Image} 1x, {Image} 2x)")]
    [DataRow($"cross-fade({Image})")]
    [Spec("css-images-4", "#image-set-notation")]
    [Spec("css-images-4", "#cross-fade-painting")]
    public void ImageFunction_PaintsTheImage(string image)
    {
        var (g, _) = Paint($"<div id='b' style='width:100px;height:100px;background-image:{image};background-repeat:no-repeat'></div>");

        Assert.AreEqual(1, g.DrawImageCalls.Count);
    }

    [Ignore("Gap, see Spec/css-images-4.yml")]
    [TestMethod]
    [Spec("css-images-4", "#element-notation")]
    public void Element_PaintsTheReferencedElement()
    {
        var (g, _) = Paint("<div id='src' style='width:10px;height:10px;background-color:red'></div>"
            + "<div id='b' style='width:100px;height:50px;background-image:element(#src)'></div>");

        Assert.IsTrue(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(), "nothing is painted for the element() image");
    }

    [Ignore("Gap, see Spec/css-images-4.yml")]
    [TestMethod]
    [DataRow("conic-gradient(red, blue)")]
    [DataRow("repeating-conic-gradient(red 0deg 30deg, blue 30deg 60deg)")]
    [Spec("css-images-4", "#conic-gradients")]
    [Spec("css-images-4", "#repeating-gradients")]
    public void ConicGradient_IsPainted(string image)
    {
        var (g, _) = Paint($"<div id='b' style='width:100px;height:50px;background-image:{image}'></div>");

        Assert.IsTrue(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any());
    }

    [TestMethod]
    [Spec("css-images-4", "#linear-gradients")]
    public void LinearGradient_InterpolationMethodAfterTheDirectionKeepsTheDirection()
    {
        var (_, gradient) = Paint("<div id='b' style='width:100px;height:50px;background-image:linear-gradient(to right in srgb, red, blue)'></div>");

        Assert.AreEqual(0, gradient!.P1.X, 0.5);
        Assert.AreEqual(100, gradient.P2.X, 0.5);
        Assert.AreEqual(Red, gradient.Stops.First().Color);
        Assert.AreEqual(Blue, gradient.Stops.Last().Color);
    }

    [TestMethod]
    [DataRow("linear-gradient(in srgb, red, blue)")]
    [DataRow("linear-gradient(in srgb to right, red, blue)")]
    [Spec("css-images-4", "#linear-gradients")]
    public void LinearGradient_LeadingInterpolationMethodIsNotAColorStop(string image)
    {
        var (_, gradient) = Paint($"<div id='b' style='width:100px;height:50px;background-image:{image}'></div>");

        Assert.AreEqual(Red, gradient!.Stops.First().Color);
        Assert.AreEqual(Blue, gradient.Stops.Last().Color);
        Assert.IsTrue(gradient.Stops.All(s => s.Color == Red || s.Color == Blue || s.Position > 0 && s.Position < 1));
    }

    [Ignore("Gap, see Spec/css-images-4.yml")]
    [TestMethod]
    [Spec("css-images-4", "#coloring-gradient-line")]
    public void Gradient_InterpolatesInOklabByDefault()
    {
        var (_, gradient) = Paint("<div id='b' style='width:100px;height:50px;background-image:linear-gradient(to right, red, blue)'></div>");

        // Halfway between red and blue Oklab gives about rgb(140, 83, 162), sRGB gives rgb(128, 0, 128)
        Assert.IsTrue(ColorAt(gradient!, 0.5).G > 50, $"midpoint {ColorAt(gradient!, 0.5)}");
    }

    [Ignore("Gap, see Spec/css-images-4.yml")]
    [TestMethod]
    [Spec("css-images-4", "#color-stop-syntax")]
    public void Gradient_ASingleColorStopPaintsASolidColor()
    {
        var (g, _) = Paint("<div id='b' style='width:100px;height:50px;background-image:linear-gradient(red)'></div>");

        Assert.IsTrue(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == Red));
    }

    private static RColor ColorAt(CssImages3Tests.GradientAdapter.Gradient gradient, double position)
    {
        var stops = gradient.Stops.OrderBy(s => s.Position).ToList();
        var after = stops.FindIndex(s => s.Position >= position);
        if (after <= 0) return stops[Math.Max(after, 0)].Color;

        var (c0, p0) = stops[after - 1];
        var (c1, p1) = stops[after];
        var t = p1 > p0 ? (position - p0) / (p1 - p0) : 0;
        return RColor.FromArgb(
            (int)Math.Round(c0.A + (c1.A - c0.A) * t),
            (int)Math.Round(c0.R + (c1.R - c0.R) * t),
            (int)Math.Round(c0.G + (c1.G - c0.G) * t),
            (int)Math.Round(c0.B + (c1.B - c0.B) * t));
    }

    /// <summary>Lays out and paints the element with id "b", returns the draw calls and the last gradient brush.</summary>
    private static (RecordingGraphics Graphics, CssImages3Tests.GradientAdapter.Gradient? Gradient) Paint(string body)
    {
        var adapter = new CssImages3Tests.GradientAdapter();
        var container = new HtmlContainerInt(adapter)
        {
            MaxSize = new RSize(1000, 4000),
            Location = RPoint.Empty,
            PageSize = new RSize(1000, 4000),
            AvoidAsyncImagesLoading = true,
            AvoidImagesLateLoading = true
        };
        container.SetHtml(PaintHarness.Wrap(body)).GetAwaiter().GetResult();

        using (var layoutGraphics = new RecordingGraphics(adapter))
            container.PerformLayout(layoutGraphics);

        var g = new RecordingGraphics(adapter);
        PaintHarness.PaintBox(container, PaintHarness.FindById(container.Root!, "b")!, g);
        return (g, adapter.Gradients.LastOrDefault());
    }
}
