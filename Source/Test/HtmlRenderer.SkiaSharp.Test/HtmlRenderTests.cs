using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.SkiaSharp;
using TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters;

namespace HtmlRenderer.SkiaSharp.Test;

// Parallel full layout passes race on the adapter singletons.
[TestClass]
[DoNotParallelize]
public sealed class HtmlRenderTests
{
    [TestMethod]
    public async Task RenderToImage_DrawsTheText()
    {
        using var image = await HtmlRender.RenderToImageAsync("<div style='font-size: 24px'>Hello World</div>");

        Assert.IsGreaterThan(50, image.Width);
        Assert.IsGreaterThan(20, image.Height);
        Assert.IsGreaterThan(0, CountPixels(image, c => c.Red < 128 && c.Green < 128 && c.Blue < 128));
    }

    [TestMethod]
    public async Task Measure_WrapsTheTextAtTheMaxWidth()
    {
        const string html = "<div>The quick brown fox jumps over the lazy dog, again and again and again.</div>";

        var unbounded = await HtmlRender.MeasureAsync(html);
        var bounded = await HtmlRender.MeasureAsync(html, 100);

        Assert.IsGreaterThan(100, unbounded.Width);
        Assert.IsLessThanOrEqualTo(100, bounded.Width);
        Assert.IsGreaterThan(unbounded.Height, bounded.Height);
    }

    [TestMethod]
    public async Task RenderToImage_FillsTheBackground()
    {
        using var image = await Render("<div style='width: 100px'><div style='width: 50px; height: 50px; background-color: #ff0000'></div></div>");

        Assert.AreEqual(new SKColor(255, 0, 0), GetPixel(image, 25, 25));
        Assert.AreEqual(SKColors.White, GetPixel(image, 75, 25));
    }

    [TestMethod]
    public async Task RenderToImage_DrawsTheBorder()
    {
        using var image = await Render("<div style='width: 40px; height: 40px; border: 5px solid #0000ff'></div>");

        Assert.AreEqual(new SKColor(0, 0, 255), GetPixel(image, 2, 25));
        Assert.AreEqual(new SKColor(0, 0, 255), GetPixel(image, 25, 47));
        Assert.AreEqual(SKColors.White, GetPixel(image, 25, 25));
    }

    [TestMethod]
    public async Task RenderToImage_DrawsASubPixelBorderOnePixelWide()
    {
        using var image = await Render("<div style='width: 40px; height: 40px; border: 0.5px solid #0000ff'></div>");

        Assert.AreEqual(new SKColor(0, 0, 255), GetPixel(image, 20, 0));
        Assert.AreEqual(new SKColor(0, 0, 255), GetPixel(image, 0, 20));
        Assert.AreEqual(SKColors.White, GetPixel(image, 20, 20));
    }

    [TestMethod]
    public async Task RenderToImage_RoundsTheCorners()
    {
        using var image = await Render("<div style='width: 60px; height: 60px; border-radius: 30px; background-color: #00ff00'></div>");

        Assert.AreEqual(SKColors.White, GetPixel(image, 1, 1));
        Assert.AreEqual(new SKColor(0, 255, 0), GetPixel(image, 30, 30));
    }

    [TestMethod]
    public async Task RenderToImage_PaintsALinearGradient()
    {
        using var image = await Render("<div style='width: 100px; height: 20px; background-image: linear-gradient(to right, #ff0000, #0000ff)'></div>");

        var left = GetPixel(image, 2, 10);
        var right = GetPixel(image, 97, 10);
        Assert.IsGreaterThan(left.Blue, left.Red);
        Assert.IsGreaterThan(right.Red, right.Blue);
    }

    [TestMethod]
    public async Task RenderToImage_DrawsADataUriImage()
    {
        using var image = await Render($"<img src='{CreatePngDataUri(SKColors.Lime)}' width='20' height='20'>");

        Assert.AreEqual(SKColors.Lime, GetPixel(image, 10, 10));
    }

    [TestMethod]
    public async Task RenderToImage_DrawsAnImageFromTheLoadEvent()
    {
        using var green = CreateImage(SKColors.Lime);

        using var image = await HtmlRender.RenderToImageAsync(
            "<body style='margin: 0'><img src='custom' width='20' height='20'></body>",
            imageLoad: (_, e) => e.Callback(green));

        Assert.AreEqual(SKColors.Lime, GetPixel(image, 10, 10));
    }

    [TestMethod]
    public async Task RenderToImage_KeepsATransparentBackground()
    {
        using var image = await HtmlRender.RenderToImageAsync(
            "<body style='margin: 0'><div style='width: 10px; height: 10px; background-color: #ff0000'></div><div style='width: 20px; height: 10px'></div></body>",
            backgroundColor: SKColors.Transparent);

        Assert.AreEqual(255, GetPixel(image, 5, 5).Alpha);
        Assert.AreEqual(0, GetPixel(image, 15, 15).Alpha);
    }

    [TestMethod]
    public async Task Render_DrawsAtTheLocationOnTheCanvas()
    {
        using var bitmap = new SKBitmap(100, 100);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);
            await HtmlRender.RenderAsync(canvas, "<body style='margin: 0'><div style='width: 10px; height: 10px; background-color: #ff0000'></div></body>", 50, 50);
        }

        Assert.AreEqual(SKColors.White, bitmap.GetPixel(5, 5));
        Assert.AreEqual(new SKColor(255, 0, 0), bitmap.GetPixel(55, 55));
    }

    [TestMethod]
    public async Task FontFace_UsesTheLoadedFont()
    {
        var fontBytes = Resources.GetFontBytes("Pacifico-Regular.ttf");
        var fontUri = "data:font/ttf;base64," + Convert.ToBase64String(fontBytes);
        const string text = "Hello World";

        var size = await HtmlRender.MeasureAsync(
            $"<style>@font-face {{ font-family: 'Test Face'; src: url('{fontUri}'); }}</style>" +
            $"<body style='margin: 0'><span style='font-family: \"Test Face\"; font-size: 30px; white-space: nowrap'>{text}</span></body>");

        // 30px are 22.5pt, the unit of the adapter's font size
        using var typeface = SKTypeface.FromData(SKData.CreateCopy(fontBytes));
        var font = new FontAdapter(typeface, SKFontStyle.Normal, 22.5);
        Assert.AreEqual(font.MeasureText(text), size.Width, 1);
    }

    [TestMethod]
    public async Task FallbackFont_IsUsedForCharactersTheFontLacks()
    {
        const string text = "漢字";
        using var typeface = SKFontManager.Default.MatchCharacter(text[0]);
        if (typeface == null)
            Assert.Inconclusive("No installed font has CJK glyphs.");

        // Pacifico has no CJK glyphs, without a fallback the text is measured as two missing glyph boxes
        var fontBytes = Resources.GetFontBytes("Pacifico-Regular.ttf");
        var fontUri = "data:font/ttf;base64," + Convert.ToBase64String(fontBytes);
        var size = await HtmlRender.MeasureAsync(
            $"<style>@font-face {{ font-family: 'Test Face'; src: url('{fontUri}'); }}</style>" +
            $"<body style='margin: 0'><span style='font-family: \"Test Face\"; font-size: 40px'>{text}</span></body>");

        using var pacifico = SKTypeface.FromData(SKData.CreateCopy(fontBytes));
        using var pacificoFont = new SKFont(pacifico, 40) { Subpixel = true, LinearMetrics = true };
        Assert.IsFalse(pacificoFont.ContainsGlyphs(text));
        Assert.IsGreaterThan(0, size.Width);
        Assert.AreNotEqual(pacificoFont.MeasureText(text), size.Width, 0.5f);
    }

    [TestMethod]
    public async Task Monospace_ResolvesToAnInstalledMonospacedFont()
    {
        var narrow = await MeasureSpan("monospace", "iiiiiiiiii");
        var wide = await MeasureSpan("monospace", "WWWWWWWWWW");

        Assert.AreEqual(wide, narrow, 1);
    }

    [TestMethod]
    public async Task RenderToImage_LimitsTheHeightWithoutAMaxHeight()
    {
        using var image = await HtmlRender.RenderToImageAsync("<body style='margin: 0'><div style='width: 10px; height: 50000px'></div></body>");

        Assert.AreEqual(32768, image.Height);
    }

    [TestMethod]
    public async Task RenderToImage_TilesABackgroundImagePartLargerThanTheImage()
    {
        using var green = CreateImage(SKColors.Lime);

        using var image = await HtmlRender.RenderToImageAsync(
            "<body style='margin: 0'><div style='width: 20px; height: 20px; background-image: url(custom)'></div></body>",
            imageLoad: (_, e) => e.Callback(green, 2, 2, 10, 10));

        Assert.AreEqual(SKColors.Lime, GetPixel(image, 10, 10));
    }

    #region Helpers

    private static async Task<float> MeasureSpan(string fontFamily, string text)
    {
        var size = await HtmlRender.MeasureAsync($"<body style='margin: 0'><span style='font-family: {fontFamily}; font-size: 20px; white-space: nowrap'>{text}</span></body>");
        return size.Width;
    }

    private static Task<SKImage> Render(string body)
    {
        return HtmlRender.RenderToImageAsync($"<body style='margin: 0'>{body}</body>", 200, 200);
    }

    private static SKColor GetPixel(SKImage image, int x, int y)
    {
        using var bitmap = SKBitmap.FromImage(image);
        return bitmap.GetPixel(x, y);
    }

    private static int CountPixels(SKImage image, Func<SKColor, bool> predicate)
    {
        using var bitmap = SKBitmap.FromImage(image);
        var count = 0;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (predicate(bitmap.GetPixel(x, y)))
                    count++;
            }
        }
        return count;
    }

    private static SKImage CreateImage(SKColor color)
    {
        using var surface = SKSurface.Create(new SKImageInfo(4, 4));
        surface.Canvas.Clear(color);
        return surface.Snapshot();
    }

    private static string CreatePngDataUri(SKColor color)
    {
        using var image = CreateImage(color);
        using var png = image.Encode(SKEncodedImageFormat.Png, 100);
        return "data:image/png;base64," + Convert.ToBase64String(png.ToArray());
    }

    #endregion
}
