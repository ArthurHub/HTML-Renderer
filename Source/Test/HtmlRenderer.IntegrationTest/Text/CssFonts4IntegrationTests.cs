using System.Collections.Concurrent;
using System.Drawing;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.WinForms.Adapters;
using DemoResources = TheArtOfDev.HtmlRenderer.Demo.Common.Resources;

namespace HtmlRenderer.IntegrationTest.Text;

/// <summary>
/// CSS Fonts Level 4 font selection, with <c>@font-face</c> faces loaded from the Liberation fonts of the demo.
/// The faces are registered on the shared <see cref="WinFormsAdapter"/>, so every test uses its own family names.
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class CssFonts4IntegrationTests
{
    private const string Sans = "LiberationSans-Regular.ttf";
    private const string SansItalic = "LiberationSans-Italic.ttf";
    private const string Mono = "LiberationMono-Regular.ttf";
    private const string MonoBold = "LiberationMono-Bold.ttf";
    private const string MonoItalic = "LiberationMono-Italic.ttf";

    private static readonly ConcurrentDictionary<string, string> FontUris = new();

    [TestMethod]
    [Spec("css-fonts-4", "#font-face-rule")]
    [Spec("css-fonts-4", "#font-family-desc")]
    [Spec("css-fonts-4", "#font-face-src-loading")]
    public void FontFace_DefinesAFamilyElementsCanUse()
    {
        var font = FontOf(
            $"@font-face {{ font-family: F4Basic; src: url({FontUri(Sans)}) }}",
            "<span id='s' style='font-family:F4Basic, Georgia'>x</span>");

        Assert.AreEqual("Liberation Sans", font.Name);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#local-font-fallback")]
    public void FontFace_LocalSource_UsesTheInstalledFont()
    {
        var font = FontOf(
            "@font-face { font-family: F4Local; src: local(NoSuchFontAtAll), local(Georgia) }",
            "<span id='s' style='font-family:F4Local, Arial'>x</span>");

        Assert.AreEqual("Georgia", font.Name);
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#local-font-fallback")]
    public void FontFace_LocalSource_MatchesTheFullFontName()
    {
        var font = FontOf(
            "@font-face { font-family: F4LocalFull; src: local('Arial Bold') }",
            "<span id='s' style='font-family:F4LocalFull, Georgia'>x</span>");

        Assert.AreEqual("Arial", font.Name);
        Assert.IsTrue(font.Bold);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#src-desc")]
    public void FontFace_SourceThatFailsToLoad_FallsBackToTheNextOne()
    {
        var font = FontOf(
            $"@font-face {{ font-family: F4Broken; src: url(data:font/ttf;base64,AAAAAAAA), url({FontUri(Sans)}) }}",
            "<span id='s' style='font-family:F4Broken, Georgia'>x</span>");

        Assert.AreEqual("Liberation Sans", font.Name);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#font-face-src-formats")]
    public void FontFace_SourceInASupportedFormat_IsLoaded()
    {
        var font = FontOf(
            $"@font-face {{ font-family: F4Truetype; src: url({FontUri(Sans)}) format('truetype') }}",
            "<span id='s' style='font-family:F4Truetype, Georgia'>x</span>");

        Assert.AreEqual("Liberation Sans", font.Name);
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#font-face-src-formats")]
    [Spec("css-fonts-5", "#font-face-src-parsing")]
    public void FontFace_SourceInAnUnsupportedFormat_IsSkipped()
    {
        var font = FontOf(
            $"@font-face {{ font-family: F4Format; src: url({FontUri(Mono)}) format('embedded-opentype'), url({FontUri(Sans)}) format('truetype') }}",
            "<span id='s' style='font-family:F4Format'>x</span>");

        Assert.AreEqual("Liberation Sans", font.Name);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#font-prop-desc")]
    [Spec("css-fonts-4", "#font-style-matching")]
    [DataRow("400", "Liberation Sans")]
    [DataRow("300", "Liberation Sans")]
    [DataRow("700", "Liberation Mono")]
    [DataRow("900", "Liberation Mono")]
    public void FontFace_WeightDescriptor_SelectsTheNearestFace(string weight, string expected)
    {
        var font = FontOf(
            $"@font-face {{ font-family: F4Weight; src: url({FontUri(Sans)}); font-weight: 400 }}" +
            $"@font-face {{ font-family: F4Weight; src: url({FontUri(MonoBold)}); font-weight: 700 }}",
            $"<span id='s' style='font-family:F4Weight;font-weight:{weight}'>x</span>");

        Assert.AreEqual(expected, font.Name);
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#font-prop-desc")]
    public void FontFace_WeightRange_CoversEveryWeightInIt()
    {
        var font = FontOf(
            $"@font-face {{ font-family: F4WeightRange; src: url({FontUri(Sans)}); font-weight: 100 900 }}" +
            $"@font-face {{ font-family: F4WeightRange; src: url({FontUri(Mono)}); font-weight: 200 }}",
            "<span id='s' style='font-family:F4WeightRange;font-weight:300'>x</span>");

        Assert.AreEqual("Liberation Sans", font.Name);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#font-stretch-prop")]
    [Spec("css-fonts-4", "#font-stretch-desc")]
    [Spec("css-fonts-4", "#font-style-matching")]
    [DataRow("normal", "Liberation Sans")]
    [DataRow("expanded", "Liberation Sans")]
    [DataRow("condensed", "Liberation Mono")]
    [DataRow("semi-condensed", "Liberation Mono")]
    public void FontFace_StretchDescriptor_SelectsTheNearestFace(string stretch, string expected)
    {
        var font = FontOf(StretchFaces("F4Stretch" + stretch), $"<span id='s' style='font-family:F4Stretch{stretch};font-stretch:{stretch}'>x</span>");

        Assert.AreEqual(expected, font.Name);
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#font-width-prop")]
    public void FontWidth_SelectsTheNearestFace()
    {
        var font = FontOf(StretchFaces("F4Width"), "<span id='s' style='font-family:F4Width;font-width:condensed'>x</span>");

        Assert.AreEqual("Liberation Mono", font.Name);
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#font-width-prop")]
    public void FontStretch_Percentage_SelectsTheNearestFace()
    {
        var font = FontOf(StretchFaces("F4StretchPercent"), "<span id='s' style='font-family:F4StretchPercent;font-stretch:75%'>x</span>");

        Assert.AreEqual("Liberation Mono", font.Name);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#font-prop-desc")]
    [Spec("css-fonts-4", "#font-style-matching")]
    [DataRow("normal", "Liberation Sans")]
    [DataRow("italic", "Liberation Mono")]
    [DataRow("oblique", "Liberation Mono")]
    public void FontFace_StyleDescriptor_SelectsTheFace(string style, string expected)
    {
        var font = FontOf(
            $"@font-face {{ font-family: F4Style; src: url({FontUri(Sans)}) }}" +
            $"@font-face {{ font-family: F4Style; src: url({FontUri(MonoItalic)}); font-style: italic }}",
            $"<span id='s' style='font-family:F4Style;font-style:{style}'>x</span>");

        Assert.AreEqual(expected, font.Name);
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#font-style-matching")]
    public void FontFace_WidthIsMatchedBeforeStyle()
    {
        var font = FontOf(
            $"@font-face {{ font-family: F4Order; src: url({FontUri(SansItalic)}); font-style: italic }}" +
            $"@font-face {{ font-family: F4Order; src: url({FontUri(MonoItalic)}); font-stretch: condensed }}",
            "<span id='s' style='font-family:F4Order;font-style:italic;font-stretch:condensed'>x</span>");

        Assert.AreEqual("Liberation Mono", font.Name);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#unicode-range-desc")]
    public void FontFace_UnicodeRange_SelectsTheFaceThatCoversTheText()
    {
        var (root, _) = Layout(
            UnicodeRangeFaces("F4Range"),
            "<span id='digits' style='font-family:F4Range'>123</span><span id='letters' style='font-family:F4Range'>abc</span>");

        Assert.AreEqual("Liberation Mono", Font(TextBox(root, "digits")).Name);
        Assert.AreEqual("Liberation Sans", Font(TextBox(root, "letters")).Name);
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#unicode-range-desc")]
    public void FontFace_UnicodeRange_IsAppliedPerCharacter()
    {
        var (root, _) = Layout(
            UnicodeRangeFaces("F4RangeMixed"),
            "<span id='mixed' style='font-family:F4RangeMixed'>1aaaa</span> <span id='digit' style='font-family:F4RangeMixed'>1</span> <span id='letters' style='font-family:F4RangeMixed'>aaaa</span>");

        var expected = WordWidth(root, "digit") + WordWidth(root, "letters");

        Assert.AreEqual(expected, WordWidth(root, "mixed"), 1);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#font-prop")]
    public void Shorthand_SetsTheFontWidth()
    {
        var font = FontOf(StretchFaces("F4ShorthandWidth"), "<span id='s' style='font:condensed 16px F4ShorthandWidth'>x</span>");

        Assert.AreEqual("Liberation Mono", font.Name);
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#font-weight-prop")]
    [DataRow("650", 650)]
    [DataRow("1", 1)]
    [DataRow("1000", 1000)]
    [DataRow("450.5", 450.5)]
    public void FontWeight_AcceptsAnyNumberFrom1To1000(string value, double expected)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<span id='s' style='font-weight:{value}'>x</span>"));

        Assert.AreEqual(expected, LayoutHarness.FindById(root, "s")!.ActualFontWeight, 0.01);
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#font-style-prop")]
    [DataRow("oblique 10deg")]
    [DataRow("oblique -10deg")]
    public void ObliqueWithAngle_SelectsASlantedFace(string style)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<span id='s' style='font-family:Arial;font-style:{style}'>x</span>"));

        Assert.IsTrue(Font(LayoutHarness.FindById(root, "s")!).Italic);
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#generic-font-families")]
    public void Math_MapsToAMathFont()
    {
        var font = FontOf("", "<span id='s' style='font-family:NoSuchFontAtAll, math'>x</span>");

        Assert.AreEqual("Cambria Math", font.Name);
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#generic-family-name-syntax")]
    public void UnavailableGenericFunction_IsSkipped()
    {
        var font = FontOf("", "<div style='font-family:Georgia'><span id='s' style='font-family:generic(kai), Arial'>x</span></div>");

        Assert.AreEqual("Arial", font.Name);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#absolute-size-mapping")]
    [DataRow("xx-small", 3d / 5)]
    [DataRow("x-small", 3d / 4)]
    [DataRow("large", 6d / 5)]
    public void AbsoluteSizeKeywords_FollowTheScalingFactors(string keyword, double factor)
    {
        AssertScaledFromMedium(keyword, factor);
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#absolute-size-mapping")]
    [DataRow("small", 8d / 9)]
    [DataRow("x-large", 3d / 2)]
    [DataRow("xx-large", 2d)]
    [DataRow("xxx-large", 3d)]
    public void SmallAndXLargeAndAbove_FollowTheScalingFactors(string keyword, double factor)
    {
        AssertScaledFromMedium(keyword, factor);
    }

    private static void AssertScaledFromMedium(string keyword, double factor)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            $"<div id='m' style='font-size:medium'>x</div><div id='k' style='font-size:{keyword}'>x</div>"));

        var medium = LayoutHarness.FindById(root, "m")!.ActualFont.Size;

        Assert.AreEqual(medium * factor, LayoutHarness.FindById(root, "k")!.ActualFont.Size, 0.5);
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void XxxLarge_IsLargerThanXxLarge()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='xx' style='font-size:xx-large'>x</div><div id='xxx' style='font-size:xxx-large'>x</div>"));

        Assert.IsGreaterThan(LayoutHarness.FindById(root, "xx")!.ActualFont.Size, LayoutHarness.FindById(root, "xxx")!.ActualFont.Size);
    }

    private static string StretchFaces(string family) =>
        $"@font-face {{ font-family: {family}; src: url({FontUri(Sans)}); font-stretch: normal }}" +
        $"@font-face {{ font-family: {family}; src: url({FontUri(Mono)}); font-stretch: condensed }}";

    private static string UnicodeRangeFaces(string family) =>
        $"@font-face {{ font-family: {family}; src: url({FontUri(Sans)}); unicode-range: U+0-2F, U+3A-10FFFF }}" +
        $"@font-face {{ font-family: {family}; src: url({FontUri(Mono)}); unicode-range: U+30-39 }}";

    private static string FontUri(string file) =>
        FontUris.GetOrAdd(file, f => "data:font/ttf;base64," + Convert.ToBase64String(DemoResources.GetFontBytes(f)));

    private static (CssBox Root, TheArtOfDev.HtmlRenderer.Core.HtmlContainerInt Container) Layout(string css, string body) =>
        LayoutHarness.Layout($"<html><head><style>{css}</style></head><body style='margin:0'>{body}</body></html>");

    private static Font FontOf(string css, string body)
    {
        var (root, _) = Layout(css, body);
        return Font(LayoutHarness.FindById(root, "s")!);
    }

    private static Font Font(CssBox box) => ((FontAdapter)box.ActualFont).Font;

    private static CssBox TextBox(CssBox root, string id) =>
        LayoutHarness.Descendants(LayoutHarness.FindById(root, id)!).First(b => b.Words.Count > 0);

    private static double WordWidth(CssBox root, string id) => TextBox(root, id).Words.Sum(w => w.Width);
}
