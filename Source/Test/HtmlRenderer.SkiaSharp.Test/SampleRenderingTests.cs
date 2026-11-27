using System.Globalization;
using System.Reflection;
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.SkiaSharp;

namespace HtmlRenderer.SkiaSharp.Test;

/// <summary>
/// Renders every regression sample and compares it pixel by pixel against <c>Baselines/&lt;OS&gt;/&lt;sample&gt;.png</c>.
/// </summary>
/// <remarks>
/// The baselines are per OS, the installed fonts and Skia's font backends differ between them.
/// The environment variables are the same as for the WinForms image regression suite:
/// <c>HTML_RENDERER_APPROVE_BASELINES=1</c> writes the current rendering as baseline, <c>HTML_RENDERER_PIXEL_TOLERANCE</c>
/// sets the per-channel tolerance (default 2), <c>HTML_RENDERER_MAX_DIFFERENT_PIXELS</c> how many pixels may differ beyond
/// that (default 64), <c>HTML_RENDERER_REGRESSION_OUTPUT_DIRECTORY</c> is where the actual and
/// diff images of a failure go, and <c>HTML_RENDERER_SAVE_ALL_RENDERED=1</c> also saves the passing ones.
/// A missing baseline only makes the test inconclusive, so a new OS can get its baselines from the CI artifacts.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class SampleRenderingTests
{
    private const int MaxWidth = 1200;
    private const int MaxHeight = 6400;

    private static readonly object SamplesInitLock = new();
    private static bool _samplesLoaded;

    [TestMethod]
    [DynamicData(nameof(GetSamples), DynamicDataDisplayName = nameof(GetSampleDisplayName))]
    public async Task Render_DemoTestSample_MatchesBaselineImage(string sampleName, string sampleHtml)
    {
        var baselineDirectory = Path.Combine(GetBaselineRoot(), GetPlatformName());
        var outputDirectory = Path.Combine(GetOutputRoot(), "SkiaSharp");
        var approveBaselines = GetBooleanEnvironmentVariable("HTML_RENDERER_APPROVE_BASELINES");
        var tolerance = GetIntEnvironmentVariable("HTML_RENDERER_PIXEL_TOLERANCE", 2);
        // Skia's anti-aliasing differs by a few scattered pixels between CPUs, a layout change moves hundreds
        var maxDifferentPixels = GetIntEnvironmentVariable("HTML_RENDERER_MAX_DIFFERENT_PIXELS", 64);
        var saveAllRendered = GetBooleanEnvironmentVariable("HTML_RENDERER_SAVE_ALL_RENDERED");

        var fileName = SanitizeFileName(sampleName);
        var baselinePath = Path.Combine(baselineDirectory, fileName + ".png");
        var actualPath = Path.Combine(outputDirectory, fileName + ".actual.png");
        var diffPath = Path.Combine(outputDirectory, fileName + ".diff.png");

        using var image = await HtmlRender.RenderToImageAsync(
            sampleHtml,
            SKSizeI.Empty,
            new SKSizeI(MaxWidth, MaxHeight),
            SKColors.White,
            stylesheetLoad: DemoUtils.OnStylesheetLoad,
            imageLoad: OnImageLoad);
        using var rendered = SKBitmap.FromImage(image);

        if (!File.Exists(baselinePath))
        {
            if (approveBaselines)
            {
                Save(rendered, baselinePath);
                return;
            }

            Save(rendered, actualPath);
            Assert.Inconclusive($"[{sampleName}] No {GetPlatformName()} baseline yet, the rendering is at {actualPath}");
        }

        using var baseline = SKBitmap.Decode(baselinePath);
        var differentPixels = CountDifferentPixels(baseline, rendered, tolerance);
        if (differentPixels <= maxDifferentPixels)
        {
            if (saveAllRendered)
                Save(rendered, actualPath);
            return;
        }

        if (approveBaselines)
        {
            Save(rendered, baselinePath);
            return;
        }

        Save(rendered, actualPath);
        using (var diff = CreateDiffImage(baseline, rendered, tolerance))
            Save(diff, diffPath);

        var totalPixels = Math.Max(baseline.Width, rendered.Width) * Math.Max(baseline.Height, rendered.Height);
        Assert.Fail(
            $"[{sampleName}] Difference: {((double)differentPixels * 100 / totalPixels).ToString("F3", CultureInfo.InvariantCulture)}% ({differentPixels}/{totalPixels} pixels){Environment.NewLine}" +
            $"  Baseline: {baselinePath}{Environment.NewLine}" +
            $"  Actual:   {actualPath}{Environment.NewLine}" +
            $"  Diff:     {diffPath}");
    }

    public static IEnumerable<object[]> GetSamples()
    {
        EnsureSamplesLoaded();
        foreach (var sample in SamplesLoader.TestSamples)
        {
            yield return [sample.Name, sample.Html];
        }
    }

    public static string GetSampleDisplayName(MethodInfo methodInfo, object[] data)
    {
        return data.Length > 0 && data[0] is string sampleName
            ? $"{methodInfo.Name}_{sampleName}"
            : methodInfo.Name;
    }

    private static void EnsureSamplesLoaded()
    {
        lock (SamplesInitLock)
        {
            if (_samplesLoaded)
                return;

            SamplesLoader.Init("Regression", typeof(HtmlRender).Assembly.GetName().Version!.ToString());
            _samplesLoaded = true;
        }
    }

    private static void OnImageLoad(object? sender, HtmlImageLoadEventArgs e)
    {
        using var stream = DemoUtils.GetImageStream(e.Src);
        if (stream == null)
            return;

        var image = SKImage.FromEncodedData(SKData.Create(stream));
        if (image != null)
            e.Callback(image);
    }

    /// <summary>
    /// Counts the pixels that differ by more than the tolerance in any channel, outside of either image counts as white.
    /// </summary>
    private static int CountDifferentPixels(SKBitmap baseline, SKBitmap actual, int tolerance)
    {
        var expectedPixels = baseline.Pixels;
        var actualPixels = actual.Pixels;
        var width = Math.Max(baseline.Width, actual.Width);
        var height = Math.Max(baseline.Height, actual.Height);

        var differentPixels = 0;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (Differs(GetPixelOrWhite(expectedPixels, baseline, x, y), GetPixelOrWhite(actualPixels, actual, x, y), tolerance))
                    differentPixels++;
            }
        }
        return differentPixels;
    }

    /// <summary>
    /// The baseline darkened, with the different pixels in red.
    /// </summary>
    private static SKBitmap CreateDiffImage(SKBitmap baseline, SKBitmap actual, int tolerance)
    {
        var expectedPixels = baseline.Pixels;
        var actualPixels = actual.Pixels;
        var width = Math.Max(baseline.Width, actual.Width);
        var height = Math.Max(baseline.Height, actual.Height);

        var diffPixels = new SKColor[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var expected = GetPixelOrWhite(expectedPixels, baseline, x, y);
                diffPixels[y * width + x] = Differs(expected, GetPixelOrWhite(actualPixels, actual, x, y), tolerance)
                    ? SKColors.Red
                    : new SKColor((byte)(expected.Red / 3), (byte)(expected.Green / 3), (byte)(expected.Blue / 3));
            }
        }

        return new SKBitmap(width, height) { Pixels = diffPixels };
    }

    private static bool Differs(SKColor expected, SKColor actual, int tolerance)
    {
        return Math.Abs(expected.Alpha - actual.Alpha) > tolerance
               || Math.Abs(expected.Red - actual.Red) > tolerance
               || Math.Abs(expected.Green - actual.Green) > tolerance
               || Math.Abs(expected.Blue - actual.Blue) > tolerance;
    }

    private static SKColor GetPixelOrWhite(SKColor[] pixels, SKBitmap image, int x, int y)
    {
        return x < image.Width && y < image.Height ? pixels[y * image.Width + x] : SKColors.White;
    }

    private static void Save(SKBitmap bitmap, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(path, data.ToArray());
    }

    private static string GetPlatformName()
    {
        if (OperatingSystem.IsWindows())
            return "Windows";
        if (OperatingSystem.IsMacOS())
            return "macOS";
        return "Linux";
    }

    private static string GetBaselineRoot()
    {
        var configured = Environment.GetEnvironmentVariable("HTML_RENDERER_SKIA_BASELINE_DIRECTORY");
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        // The baselines stay in the project folder, the test runs from its bin folder below it
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "HtmlRenderer.SkiaSharp.Test.csproj")))
                return Path.Combine(directory.FullName, "Baselines");
        }

        return Path.Combine(AppContext.BaseDirectory, "Baselines");
    }

    private static string GetOutputRoot()
    {
        var configured = Environment.GetEnvironmentVariable("HTML_RENDERER_REGRESSION_OUTPUT_DIRECTORY");
        return string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(Path.GetTempPath(), "html-renderer-image-regression")
            : configured;
    }

    private static bool GetBooleanEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    private static int GetIntEnvironmentVariable(string name, int defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}
