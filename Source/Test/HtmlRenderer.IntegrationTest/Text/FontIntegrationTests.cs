using System.Drawing;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.WinForms.Adapters;

namespace HtmlRenderer.IntegrationTest.Text;

[DoNotParallelize]
[TestClass]
public sealed class FontIntegrationTests
{
    private const double Delta = 0.01;

    // 16px in points, the font size of the elements in most tests.
    private const double SixteenPx = 12;

    [TestMethod]
    [Spec("css21", "fonts.html#algorithm")]
    [Spec("css-fonts-4", "#font-matching-algorithm")]
    public void FirstInstalledFamily_IsUsed()
    {
        var font = FontOf("<span id='s' style='font-family:NoSuchFontAtAll, Georgia, serif'>x</span>");

        Assert.AreEqual("Georgia", font.Name);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#algorithm")]
    [Spec("css-fonts-4", "#font-matching-algorithm")]
    public void NoInstalledFamily_FallsBackToTheDefaultFont()
    {
        var font = FontOf("<span id='s' style='font-family:NoSuchFontAtAll, AnotherMissingFont'>x</span>");

        Assert.AreEqual("Segoe UI", font.Name);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#algorithm")]
    [Spec("css-fonts-4", "#font-matching-algorithm")]
    public void FontSize_IsMatchedExactly()
    {
        var font = FontOf("<span id='s' style='font-family:Arial;font-size:17px'>x</span>");

        Assert.AreEqual(17 * 72d / 96d, font.Size, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "fonts.html#algorithm")]
    [Spec("css-fonts-4", "#font-family-prop")]
    [Spec("css-fonts-4", "#font-matching-algorithm")]
    [Spec("css-fonts-4", "#cluster-matching")]
    public void FamilyWithoutTheGlyph_FallsBackToTheNextFamily()
    {
        var font = FontOf("<span id='s' style=\"font-family:Arial, 'Segoe UI Emoji'\">\U0001F600</span>");

        Assert.AreEqual("Segoe UI Emoji", font.Name);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    [Spec("css-fonts-4", "#font-family-name-syntax")]
    public void QuotedFamilyName_IsMatched()
    {
        var font = FontOf("<span id='s' style=\"font-family:'Times New Roman', Arial\">x</span>");

        Assert.AreEqual("Times New Roman", font.Name);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    [Spec("css-fonts-4", "#font-family-name-syntax")]
    public void UnquotedIdentifierSequence_IsJoinedIntoOneName()
    {
        var font = FontOf("<span id='s' style='font-family:Times New Roman, Arial'>x</span>");

        Assert.AreEqual("Times New Roman", font.Name);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    [Spec("css-fonts-4", "#font-family-prop")]
    public void FontFamily_IsInherited()
    {
        var font = FontOf("<div style='font-family:Georgia'><span id='s'>x</span></div>");

        Assert.AreEqual("Georgia", font.Name);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    [Spec("css-fonts-4", "#font-family-name-syntax")]
    public void InvalidFamilyList_IsIgnored()
    {
        var font = FontOf("<div style='font-family:Georgia'><span id='s' style='font-family:Red/Black, Arial'>x</span></div>");

        Assert.AreEqual("Georgia", font.Name);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    [Spec("css-fonts-4", "#font-family-name-syntax")]
    public void QuotedGenericName_IsAFamilyNameNotTheKeyword()
    {
        var font = FontOf("<span id='s' style='font-family:\"monospace\"'>x</span>");

        Assert.AreNotEqual("Courier New", font.Name);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#sans-serif-def")]
    [Spec("css-fonts-4", "#generic-font-families")]
    public void SansSerif_MapsToASansSerifFont()
    {
        var font = FontOf("<span id='s' style='font-family:sans-serif'>x</span>");

        CollectionAssert.Contains(new[] { "Segoe UI", "Arial", "Tahoma", "Verdana" }, font.Name);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#monospace-def")]
    [Spec("css-fonts-4", "#generic-font-families")]
    [Spec("css-fonts-4", "#generic-family-name-syntax")]
    public void Monospace_MapsToAMonospaceFont()
    {
        var font = FontOf("<span id='s' style='font-family:monospace'>x</span>");

        Assert.AreEqual("Courier New", font.Name);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "fonts.html#serif-def")]
    [Spec("css-fonts-4", "#generic-font-families")]
    public void Serif_MapsToASerifFont()
    {
        var font = FontOf("<span id='s' style='font-family:serif'>x</span>");

        CollectionAssert.Contains(new[] { "Times New Roman", "Georgia", "Cambria" }, font.Name);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "fonts.html#cursive-def")]
    [Spec("css-fonts-4", "#generic-font-families")]
    public void Cursive_MapsToACursiveFont()
    {
        var font = FontOf("<span id='s' style='font-family:cursive'>x</span>");

        CollectionAssert.Contains(new[] { "Comic Sans MS", "Segoe Script", "Segoe Print", "Gabriola" }, font.Name);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "fonts.html#fantasy-def")]
    [Spec("css-fonts-4", "#generic-font-families")]
    public void Fantasy_MapsToAFantasyFont()
    {
        var font = FontOf("<span id='s' style='font-family:fantasy'>x</span>");

        CollectionAssert.Contains(new[] { "Impact", "Jokerman", "Papyrus", "Gabriola" }, font.Name);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#generic-font-families")]
    [Spec("css-fonts-4", "#generic-font-families")]
    public void GenericFamily_IsUsedAsTheLastAlternative()
    {
        var font = FontOf("<span id='s' style='font-family:NoSuchFontAtAll, monospace'>x</span>");

        Assert.AreEqual("Courier New", font.Name);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-styling")]
    [Spec("css-fonts-4", "#font-style-prop")]
    [DataRow("italic")]
    [DataRow("oblique")]
    public void ItalicAndOblique_SelectASlantedFace(string style)
    {
        var font = FontOf($"<span id='s' style='font-family:Arial;font-style:{style}'>x</span>");

        Assert.IsTrue(font.Italic);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-styling")]
    [Spec("css-fonts-4", "#font-style-prop")]
    public void Normal_InsideItalic_SelectsTheUprightFace()
    {
        var font = FontOf("<div style='font-family:Arial;font-style:italic'><span id='s' style='font-style:normal'>x</span></div>");

        Assert.IsFalse(font.Italic);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-styling")]
    [Spec("css-fonts-4", "#font-style-prop")]
    public void FontStyle_IsInherited()
    {
        var font = FontOf("<div style='font-family:Arial;font-style:italic'><span id='s'>x</span></div>");

        Assert.IsTrue(font.Italic);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "fonts.html#small-caps")]
    [Spec("css-fonts-4", "#font-variant-prop")]
    public void SmallCaps_RendersLowercaseLettersAsCapitals()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<span id='s' style='font-variant:small-caps'>hello</span>"));

        var word = FirstWord(LayoutHarness.FindById(root, "s")!)!;

        Assert.IsFalse(word.Text.Any(char.IsLower), word.Text);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-boldness")]
    [Spec("css-fonts-4", "#missing-weights")]
    [DataRow("100", false)]
    [DataRow("300", false)]
    [DataRow("normal", false)]
    [DataRow("400", false)]
    [DataRow("500", false)]
    [DataRow("600", true)]
    [DataRow("bold", true)]
    [DataRow("700", true)]
    [DataRow("900", true)]
    public void Weight_SelectsTheNearestFaceOfARegularAndBoldFamily(string weight, bool bold)
    {
        var font = FontOf($"<span id='s' style='font-family:Arial;font-weight:{weight}'>x</span>");

        Assert.AreEqual(bold, font.Bold);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-boldness")]
    [Spec("css-fonts-4", "#relative-weights")]
    public void Bolder_OnANormalParent_IsBold()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='font-family:Arial'><span id='s' style='font-weight:bolder'>x</span></div>"));
        var s = LayoutHarness.FindById(root, "s")!;

        Assert.AreEqual(700, s.ActualFontWeight);
        Assert.IsTrue(Font(s).Bold);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-boldness")]
    [Spec("css-fonts-4", "#relative-weights")]
    public void Lighter_OnABoldParent_IsNormal()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='font-family:Arial;font-weight:bold'><span id='s' style='font-weight:lighter'>x</span></div>"));
        var s = LayoutHarness.FindById(root, "s")!;

        Assert.AreEqual(400, s.ActualFontWeight);
        Assert.IsFalse(Font(s).Bold);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "fonts.html#font-boldness")]
    [Spec("css-fonts-4", "#relative-weights")]
    [DataRow(100, 400, 100)]
    [DataRow(200, 400, 100)]
    [DataRow(300, 400, 100)]
    [DataRow(500, 700, 100)]
    [DataRow(600, 900, 400)]
    [DataRow(700, 900, 400)]
    [DataRow(800, 900, 700)]
    [DataRow(900, 900, 700)]
    public void BolderAndLighter_FollowTheTable(int inherited, int bolder, int lighter)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            $"<div style='font-weight:{inherited}'><span id='b' style='font-weight:bolder'>x</span><span id='l' style='font-weight:lighter'>x</span></div>"));

        Assert.AreEqual(bolder, LayoutHarness.FindById(root, "b")!.ActualFontWeight);
        Assert.AreEqual(lighter, LayoutHarness.FindById(root, "l")!.ActualFontWeight);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "fonts.html#font-boldness")]
    [Spec("css-fonts-4", "#relative-weights")]
    public void Bolder_IsInheritedAsTheComputedWeight()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='font-weight:bolder'><span id='s'>x</span></div>"));

        Assert.AreEqual(700, LayoutHarness.FindById(root, "s")!.ActualFontWeight);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-boldness")]
    [Spec("css-fonts-4", "#font-weight-prop")]
    public void FontWeight_IsInherited()
    {
        var font = FontOf("<div style='font-family:Arial;font-weight:bold'><span id='s'>x</span></div>");

        Assert.IsTrue(font.Bold);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#absolute-size-mapping")]
    public void AbsoluteSizeKeywords_GrowFromXxSmallToXxLarge()
    {
        string[] keywords = ["xx-small", "x-small", "small", "medium", "large", "x-large", "xx-large"];
        var body = string.Concat(keywords.Select(k => $"<div id='{k}' style='font-size:{k}'>x</div>"));
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(body));

        var sizes = keywords.Select(k => LayoutHarness.FindById(root, k)!.ActualFont.Size).ToArray();

        for (var i = 1; i < sizes.Length; i++)
            Assert.IsGreaterThan(sizes[i - 1], sizes[i], keywords[i]);
        Assert.IsGreaterThanOrEqualTo(9 * 72d / 96d, sizes[0]);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void Medium_IsTheInitialSize()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='i'>x</div><div id='m' style='font-size:medium'>x</div>"));

        Assert.AreEqual(LayoutHarness.FindById(root, "i")!.ActualFont.Size, LayoutHarness.FindById(root, "m")!.ActualFont.Size, Delta);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void LargerAndSmaller_OnAMediumParent_StepOneKeyword()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='font-size:medium'><span id='larger' style='font-size:larger'>x</span><span id='smaller' style='font-size:smaller'>x</span></div>" +
            "<div id='large' style='font-size:large'>x</div><div id='small' style='font-size:small'>x</div>"));

        Assert.AreEqual(LayoutHarness.FindById(root, "large")!.ActualFont.Size, LayoutHarness.FindById(root, "larger")!.ActualFont.Size, Delta);
        Assert.AreEqual(LayoutHarness.FindById(root, "small")!.ActualFont.Size, LayoutHarness.FindById(root, "smaller")!.ActualFont.Size, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void Larger_IsInheritedAsTheComputedSize()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='p' style='font-size:larger'><span id='s'>x</span></div>"));

        Assert.AreEqual(LayoutHarness.FindById(root, "p")!.ActualFont.Size, LayoutHarness.FindById(root, "s")!.ActualFont.Size, Delta);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void Percentage_RefersToTheParentSize()
    {
        var font = FontOf("<div style='font-size:20px'><span id='s' style='font-size:150%'>x</span></div>");

        Assert.AreEqual(30 * 72d / 96d, font.Size, Delta);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void Em_RefersToTheParentSize()
    {
        var font = FontOf("<div style='font-size:20px'><span id='s' style='font-size:1.5em'>x</span></div>");

        Assert.AreEqual(30 * 72d / 96d, font.Size, Delta);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void Length_IsUsedAsIs()
    {
        var font = FontOf("<span id='s' style='font-size:12pt'>x</span>");

        Assert.AreEqual(12, font.Size, Delta);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void FontSize_IsInherited()
    {
        var font = FontOf("<div style='font-size:16px'><span id='s'>x</span></div>");

        Assert.AreEqual(SixteenPx, font.Size, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void NegativeSize_IsIgnored()
    {
        var font = FontOf("<div style='font-size:16px'><span id='s' style='font-size:-10px'>x</span></div>");

        Assert.AreEqual(SixteenPx, font.Size, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void TinySize_IsKept()
    {
        var font = FontOf("<span id='s' style='font-size:1px'>x</span>");

        Assert.AreEqual(0.75, font.Size, Delta);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    [Spec("css-fonts-4", "#font-prop")]
    public void Shorthand_SetsStyleWeightSizeAndFamily()
    {
        var font = FontOf("<span id='s' style='font:bold italic 16px Georgia, serif'>x</span>");

        Assert.AreEqual("Georgia", font.Name);
        Assert.IsTrue(font.Bold);
        Assert.IsTrue(font.Italic);
        Assert.AreEqual(SixteenPx, font.Size, Delta);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    [Spec("css-fonts-4", "#font-prop")]
    public void Shorthand_SetsTheLineHeight()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='shorthand' style='font:16px/40px Arial'>x</div><div id='longhand' style='font-family:Arial;font-size:16px;line-height:40px'>x</div>"));

        Assert.AreEqual("40px", LayoutHarness.FindById(root, "shorthand")!.LineHeight);
        Assert.AreEqual(LayoutHarness.FindById(root, "longhand")!.Size.Height, LayoutHarness.FindById(root, "shorthand")!.Size.Height, Delta);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    [Spec("css-fonts-4", "#font-prop")]
    public void Shorthand_ResetsOmittedPropertiesToTheirInitialValues()
    {
        var font = FontOf("<div style='font-weight:bold;font-style:italic'><span id='s' style='font:16px Arial'>x</span></div>");

        Assert.IsFalse(font.Bold);
        Assert.IsFalse(font.Italic);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    [Spec("css-fonts-4", "#font-prop")]
    public void Shorthand_PercentageSizeRefersToTheParentSize()
    {
        var font = FontOf("<div style='font-size:20px'><span id='s' style='font:150% Arial'>x</span></div>");

        Assert.AreEqual(30 * 72d / 96d, font.Size, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    [Spec("css-fonts-4", "#font-prop")]
    public void SystemFont_SetsTheFontOfTheSystem()
    {
        var font = FontOf("<div style='font:32px Georgia'><span id='s' style='font:menu'>x</span></div>");
        var menu = SystemFonts.MenuFont!;

        Assert.AreEqual(menu.Name, font.Name);
        Assert.AreEqual(menu.SizeInPoints, font.Size, 0.5);
    }

    private static Font FontOf(string body)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(body));
        return Font(LayoutHarness.FindById(root, "s")!);
    }

    private static Font Font(CssBox box) => ((FontAdapter)box.ActualFont).Font;

    private static CssRect? FirstWord(CssBox box) =>
        box.Words.Count > 0 ? box.Words[0] : box.Boxes.Select(FirstWord).FirstOrDefault(w => w != null);
}
