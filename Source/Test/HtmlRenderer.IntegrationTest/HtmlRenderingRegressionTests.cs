using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace HtmlRenderer.IntegrationTest;

[TestClass]
public sealed class HtmlRenderingRegressionTests
{
    private const int MaxWidth = 1200;
    private const int MaxHeight = 6400;

    private static readonly object SamplesInitLock = new();
    private static readonly object ImageCacheLock = new();
    private static readonly Dictionary<string, Image> ImageCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Lazy<Image> PlaceholderImage = new(CreatePlaceholderImage);

    private static bool _samplesLoaded;

    [DoNotParallelize]
    [TestMethod]
    [DynamicData(nameof(GetSamples), DynamicDataDisplayName = nameof(GetSampleDisplayName))]
    public void Render_DemoTestSample_MatchesBaselineImage(string sampleName, string sampleHtml)
    {
        EnsureSamplesLoaded();

        var baselineDirectory = GetBaselineDirectory();
        var outputDirectory = GetOutputDirectory();
        var approveBaselines = GetBooleanEnvironmentVariable("HTML_RENDERER_APPROVE_BASELINES");
        var tolerance = GetIntEnvironmentVariable("HTML_RENDERER_PIXEL_TOLERANCE", 2);
        var saveAllRendered = GetBooleanEnvironmentVariable("HTML_RENDERER_SAVE_ALL_RENDERED");

        Directory.CreateDirectory(baselineDirectory);
        Directory.CreateDirectory(outputDirectory);

        var sampleFileName = SanitizeFileName(sampleName);
        var baselinePath = Path.Combine(baselineDirectory, sampleFileName + ".png");
        var actualPath = Path.Combine(outputDirectory, sampleFileName + ".actual.png");
        var diffPath = Path.Combine(outputDirectory, sampleFileName + ".diff.png");

        using var rendered = RenderSample(sampleHtml);
        if (!File.Exists(baselinePath))
        {
            if (approveBaselines)
            {
                rendered.Save(baselinePath);
                return;
            }

            rendered.Save(actualPath);
            Assert.Fail($"[{sampleName}] Missing baseline image: {baselinePath}");
        }

        using var baseline = new Bitmap(new MemoryStream(File.ReadAllBytes(baselinePath)));
        CompareImages(baseline, rendered, tolerance, out var differentPixels, out var totalPixels);

        var differencePercentage = totalPixels == 0
            ? 0
            : (double)differentPixels * 100.0 / totalPixels;

        if (differentPixels == 0)
        {
            if (saveAllRendered)
            {
                rendered.Save(actualPath);
            }
            return;
        }

        if (approveBaselines)
        {
            rendered.Save(baselinePath);
            return;
        }

        rendered.Save(actualPath);
        using var diff = CreateDiffImage(baseline, rendered, tolerance);
        diff.Save(diffPath);

        Assert.Fail(
            $"[{sampleName}] Difference: {differencePercentage.ToString("F3", CultureInfo.InvariantCulture)}% ({differentPixels}/{totalPixels} pixels){Environment.NewLine}" +
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
        if (_samplesLoaded)
        {
            return;
        }

        lock (SamplesInitLock)
        {
            if (_samplesLoaded)
            {
                return;
            }

            SamplesLoader.Init("Regression", typeof(HtmlRender).Assembly.GetName().Version.ToString());
            _samplesLoaded = true;
        }
    }

    private static Bitmap RenderSample(string html)
    {
        return (Bitmap)HtmlRender.RenderToImage(
            html,
            minSize: Size.Empty,
            maxSize: new Size(MaxWidth, MaxHeight),
            backgroundColor: Color.White,
            stylesheetLoad: DemoUtils.OnStylesheetLoad,
            imageLoad: OnImageLoad);
    }

    private static void OnImageLoad(object? sender, HtmlImageLoadEventArgs e)
    {
        var image = TryLoadResourceImage(e.Src);
        if (image == null && RequiresPlaceholderImage(e.Src))
        {
            image = PlaceholderImage.Value;
        }

        if (e.Attributes != null && e.Attributes.TryGetValue("byrect", out var byRectValue))
        {
            var split = byRectValue.Split(',');
            if (split.Length == 4
                && double.TryParse(split[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var x)
                && double.TryParse(split[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y)
                && double.TryParse(split[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var width)
                && double.TryParse(split[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var height))
            {
                e.Callback(image ?? PlaceholderImage.Value, x, y, width, height);
                return;
            }
        }

        if (e.Attributes != null && e.Attributes.ContainsKey("byevent"))
        {
            e.Callback(image ?? PlaceholderImage.Value);
            return;
        }

        if (image != null)
        {
            e.Callback(image);
        }
    }

    private static Image? TryLoadResourceImage(string? src)
    {
        if (string.IsNullOrWhiteSpace(src))
        {
            return null;
        }

        lock (ImageCacheLock)
        {
            if (ImageCache.TryGetValue(src, out var cached))
            {
                return cached;
            }

            using var stream = DemoUtils.GetImageStream(src);
            if (stream == null)
            {
                return null;
            }

            using var image = Image.FromStream(stream);
            var cachedImage = new Bitmap(image);
            ImageCache[src] = cachedImage;
            return cachedImage;
        }
    }

    private static bool RequiresPlaceholderImage(string? src)
    {
        if (string.IsNullOrWhiteSpace(src))
        {
            return true;
        }

        if (src.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || src.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (src.StartsWith('#'))
        {
            return true;
        }

        if (Uri.TryCreate(src, UriKind.Absolute, out var uri))
        {
            return uri.Scheme != Uri.UriSchemeFile;
        }

        return false;
    }

    private static void CompareImages(Bitmap baseline, Bitmap actual, int tolerance, out int differentPixels, out int totalPixels)
    {
        var width = Math.Max(baseline.Width, actual.Width);
        var height = Math.Max(baseline.Height, actual.Height);

        differentPixels = 0;
        totalPixels = width * height;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var expected = GetPixelOrDefault(baseline, x, y);
                var rendered = GetPixelOrDefault(actual, x, y);

                var pixelDiffers = Math.Abs(expected.A - rendered.A) > tolerance
                                   || Math.Abs(expected.R - rendered.R) > tolerance
                                   || Math.Abs(expected.G - rendered.G) > tolerance
                                   || Math.Abs(expected.B - rendered.B) > tolerance;

                if (pixelDiffers)
                {
                    differentPixels++;
                }
            }
        }
    }

    private static Bitmap CreateDiffImage(Bitmap baseline, Bitmap actual, int tolerance)
    {
        var width = Math.Max(baseline.Width, actual.Width);
        var height = Math.Max(baseline.Height, actual.Height);
        var diff = new Bitmap(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var expected = GetPixelOrDefault(baseline, x, y);
                var rendered = GetPixelOrDefault(actual, x, y);

                var pixelDiffers = Math.Abs(expected.A - rendered.A) > tolerance
                    || Math.Abs(expected.R - rendered.R) > tolerance
                    || Math.Abs(expected.G - rendered.G) > tolerance
                    || Math.Abs(expected.B - rendered.B) > tolerance;

                if (pixelDiffers)
                {
                    diff.SetPixel(x, y, Color.Red);
                    continue;
                }

                diff.SetPixel(
                    x,
                    y,
                    Color.FromArgb(
                        255,
                        expected.R / 3,
                        expected.G / 3,
                        expected.B / 3));
            }
        }

        return diff;
    }

    private static Color GetPixelOrDefault(Bitmap image, int x, int y)
    {
        if (x < image.Width && y < image.Height)
        {
            return image.GetPixel(x, y);
        }

        return Color.White;
    }

    private static string GetBaselineDirectory()
    {
        var configuredDirectory = Environment.GetEnvironmentVariable("HTML_RENDERER_BASELINE_DIRECTORY");
        if (!string.IsNullOrWhiteSpace(configuredDirectory))
        {
            return configuredDirectory;
        }

        var baseDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        while (baseDirectory != null)
        {
            var projectBaselineDirectory = Path.Combine(baseDirectory.FullName, "Baselines");
            if (Directory.Exists(projectBaselineDirectory))
            {
                return projectBaselineDirectory;
            }

            var sourceBaselineDirectory = Path.Combine(baseDirectory.FullName, "Test", "HtmlRenderer.IntegrationTest", "Baselines");
            if (Directory.Exists(sourceBaselineDirectory))
            {
                return sourceBaselineDirectory;
            }

            var repositoryBaselineDirectory = Path.Combine(baseDirectory.FullName, "Source", "Test", "HtmlRenderer.IntegrationTest", "Baselines");
            if (Directory.Exists(repositoryBaselineDirectory))
            {
                return repositoryBaselineDirectory;
            }

            baseDirectory = baseDirectory.Parent;
        }

        throw new InvalidOperationException(
            "Unable to locate baseline directory. Set HTML_RENDERER_BASELINE_DIRECTORY to an existing Baselines folder.");
    }

    private static string GetOutputDirectory()
    {
        var configuredDirectory = Environment.GetEnvironmentVariable("HTML_RENDERER_REGRESSION_OUTPUT_DIRECTORY");
        if (!string.IsNullOrWhiteSpace(configuredDirectory))
        {
            return configuredDirectory;
        }

        return Path.Combine(Path.GetTempPath(), "html-renderer-image-regression");
    }

    private static bool GetBooleanEnvironmentVariable(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
               || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
               || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
    }

    private static int GetIntEnvironmentVariable(string key, int fallback)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return fallback;
    }

    private static string SanitizeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(name.Length);
        foreach (var c in name)
        {
            if (invalidChars.Contains(c))
            {
                builder.Append('_');
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }

    private static Image CreatePlaceholderImage()
    {
        var bitmap = new Bitmap(120, 80);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.WhiteSmoke);
        using var borderPen = new Pen(Color.DimGray, 2);
        graphics.DrawRectangle(borderPen, 1, 1, bitmap.Width - 3, bitmap.Height - 3);
        using var crossPen = new Pen(Color.IndianRed, 2);
        graphics.DrawLine(crossPen, 0, 0, bitmap.Width - 1, bitmap.Height - 1);
        graphics.DrawLine(crossPen, bitmap.Width - 1, 0, 0, bitmap.Height - 1);
        return bitmap;
    }
}
