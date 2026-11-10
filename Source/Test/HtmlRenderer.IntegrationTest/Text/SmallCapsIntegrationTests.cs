using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Text;

// The regex cache in RegexParserUtils isn't thread-safe, parallel SetHtml calls can corrupt it.
[DoNotParallelize]
[TestClass]
public sealed class SmallCapsIntegrationTests
{
    /// <summary>Text gets an anonymous child box, the element's own box has no words.</summary>
    private static CssBox FindWordsBox(CssBox root, string id)
    {
        var element = LayoutHarness.FindById(root, id)!;
        if (element.Words.Count > 0) return element;

        var wordsChild = element.Boxes.FirstOrDefault(b => b.Words.Count > 0);
        Assert.IsNotNull(wordsChild, $"no descendant of #{id} owns any words");
        return wordsChild!;
    }

    [TestMethod]
    [Spec("css21", "fonts.html#small-caps")]
    [Spec("css-fonts-4", "#font-variant-prop")]
    public void FontVariant_SmallCaps_StandaloneProperty_ParsesAndStores()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<b id='w' style='font-variant:small-caps'>Hello</b>"));
        var w = LayoutHarness.FindById(root, "w")!;

        Assert.AreEqual("small-caps", w.FontVariant);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    [Spec("css-fonts-4", "#font-prop")]
    public void FontVariant_SmallCaps_ViaFontShorthand_ParsesAndStores()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<b id='w' style='font:italic small-caps bold 12px Arial'>Hello</b>"));
        var w = LayoutHarness.FindById(root, "w")!;

        Assert.AreEqual("small-caps", w.FontVariant);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "fonts.html#small-caps")]
    [Spec("css-fonts-4", "#font-variant-prop")]
    public void SmallCaps_SplitsWordIntoCaseRuns()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<b id='w' style='font-variant:small-caps'>Hello</b>"));
        var box = FindWordsBox(root, "w");

        // "ELLO" is the synthesized small-caps run
        Assert.AreEqual(2, box.Words.Count);
        Assert.AreEqual("H", box.Words[0].Text);
        Assert.AreEqual("ELLO", box.Words[1].Text);
    }

    [TestMethod]
    public void NoSmallCaps_WordIsNotSplit_Regression()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<b id='w'>Hello</b>"));
        var box = FindWordsBox(root, "w");

        Assert.AreEqual(1, box.Words.Count);
        Assert.AreEqual("Hello", box.Words[0].Text);
    }

    [TestMethod]
    public void SmallCaps_WordWithNoLowercaseLetters_IsNotSplit()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<b id='w' style='font-variant:small-caps'>ABC</b>"));
        var box = FindWordsBox(root, "w");

        Assert.AreEqual(1, box.Words.Count);
        Assert.AreEqual("ABC", box.Words[0].Text);
    }

    // font-variant-caps (CSS Fonts 3) isn't supported, it's ignored.

    [TestMethod]
    public void AllSmallCaps_WordWithNoLowercaseLetters_WordStaysIntact()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<b id='w' style='font-variant-caps:all-small-caps'>ABC</b>"));
        var box = FindWordsBox(root, "w");

        Assert.AreEqual(1, box.Words.Count);
        Assert.AreEqual("ABC", box.Words[0].Text);
    }

    [TestMethod]
    public void AllSmallCaps_WordWithNoLetters_IsNotSplit()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<b id='w' style='font-variant-caps:all-small-caps'>123</b>"));
        var box = FindWordsBox(root, "w");

        Assert.AreEqual(1, box.Words.Count);
        Assert.AreEqual("123", box.Words[0].Text);
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#font-variant-caps-prop")]
    public void AllSmallCaps_MixedCaseWord_WouldSplitIntoThreeRuns()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<b id='w' style='font-variant-caps:all-small-caps'>AbC</b>"));
        var box = FindWordsBox(root, "w");

        Assert.AreEqual(3, box.Words.Count);
        Assert.AreEqual("A", box.Words[0].Text);
        Assert.AreEqual("B", box.Words[1].Text);
        Assert.AreEqual("C", box.Words[2].Text);
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#font-variant-caps-prop")]
    public void AllSmallCaps_DigitRunBetweenLowercaseRuns_WouldSplitIntoThreeRuns()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<b id='w' style='font-variant-caps:all-small-caps'>a1b</b>"));
        var box = FindWordsBox(root, "w");

        Assert.AreEqual(3, box.Words.Count);
        Assert.AreEqual("A", box.Words[0].Text);
        Assert.AreEqual("1", box.Words[1].Text);
        Assert.AreEqual("B", box.Words[2].Text);
    }
}
