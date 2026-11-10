using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Text;

[DoNotParallelize]
[TestClass]
public sealed class TextSpacingAndIndentLayoutIntegrationTests
{
    private const string WrappingText = "This text is long enough that it wraps onto a second line for sure.";

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#spacing-props")]
    [Spec("css-text-3", "#word-spacing-property")]
    [Spec("css-text-4", "#word-spacing-property")]
    public void WordSpacing_IncreasesGapBetweenAdjacentWords()
    {
        var spacedGap = InterWordGap("<p id='p' style='word-spacing:10px'>AA BB</p>");
        var plainGap = InterWordGap("<p id='p'>AA BB</p>");

        Assert.AreEqual(plainGap + 10, spacedGap, 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#spacing-props")]
    [Spec("css-text-3", "#word-spacing-property")]
    [Spec("css-text-4", "#word-spacing-property")]
    public void WordSpacing_OnAncestor_AffectsDescendantGap()
    {
        var spacedGap = InterWordGap("<div style='word-spacing:10px'><p id='p'>AA BB</p></div>");
        var plainGap = InterWordGap("<p id='p'>AA BB</p>");

        Assert.AreEqual(plainGap + 10, spacedGap, 1);
    }

    [TestMethod]
    [Spec("css21", "text.html#spacing-props")]
    [Spec("css-text-3", "#word-spacing-property")]
    [Spec("css-text-4", "#word-spacing-property")]
    public void WordSpacingNormal_LeavesGapUnchanged()
    {
        var normalGap = InterWordGap("<p id='p' style='word-spacing:normal'>AA BB</p>");
        var plainGap = InterWordGap("<p id='p'>AA BB</p>");

        Assert.AreEqual(plainGap, normalGap, 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#spacing-props")]
    [Spec("css-text-3", "#word-spacing-property")]
    [Spec("css-text-4", "#word-spacing-property")]
    public void WordSpacingNegative_ReducesGap()
    {
        var negativeGap = InterWordGap("<p id='p' style='word-spacing:-5px'>AA BB</p>");
        var plainGap = InterWordGap("<p id='p'>AA BB</p>");

        Assert.AreEqual(plainGap - 5, negativeGap, 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#spacing-props")]
    [Spec("css-text-3", "#letter-spacing-property")]
    [Spec("css-text-4", "#letter-spacing-property")]
    public void LetterSpacing_WidensTheWordByTheSpacingPerCharacter()
    {
        var spaced = FirstWordWidth("<p id='p' style='letter-spacing:10px'>AAAA</p>");
        var plain = FirstWordWidth("<p id='p'>AAAA</p>");

        Assert.IsTrue(spaced >= plain + 30, $"expected at least 3 x 10px more than {plain}, got {spaced}");
    }

    [TestMethod]
    [Spec("css21", "text.html#indentation-prop")]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void TextIndent_IndentsOnlyTheFirstLine()
    {
        var html = LayoutHarness.Wrap(
            "<p id='p' style='width:120px; text-indent:40px; margin:0;'>" + WrappingText + "</p>");
        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;

        var wordsByLine = AllWords(p).GroupBy(w => w.Top).OrderBy(g => g.Key).ToList();
        Assert.IsTrue(wordsByLine.Count >= 2, "fixture text should wrap onto at least two lines");

        var firstLineLeft = wordsByLine[0].Min(w => w.Left);
        var secondLineLeft = wordsByLine[1].Min(w => w.Left);

        Assert.AreEqual(40, firstLineLeft, 1);
        Assert.AreEqual(0, secondLineLeft, 1);
    }

    [TestMethod]
    [Spec("css21", "text.html#indentation-prop")]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void TextIndentZero_NoIndentOnAnyLine()
    {
        var html = LayoutHarness.Wrap("<p id='p' style='margin:0;'>Hello world</p>");
        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;

        var firstWord = AllWords(p).OrderBy(w => w.Left).First();
        Assert.AreEqual(0, firstWord.Left, 1);
    }

    [TestMethod]
    [Spec("css21", "text.html#indentation-prop")]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void NegativeTextIndent_PullsTheFirstLineOutOfTheBlock()
    {
        Assert.AreEqual(30, FirstWordLeft(
            "<div style='width:400px;padding-left:50px'><p id='p' style='margin:0;text-indent:-20px'>hi</p></div>"), 1);
    }

    [TestMethod]
    [Spec("css21", "text.html#indentation-prop")]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void TextIndent_IsInheritedByABlockChild()
    {
        Assert.AreEqual(40, FirstWordLeft(
            "<div style='text-indent:40px'><p id='p' style='margin:0'>hi</p></div>"), 1);
    }

    [TestMethod]
    [Spec("css21", "text.html#indentation-prop")]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void TextIndent_DoesntApplyAfterALineBreak()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='p' style='text-indent:40px'>aa<br>bb</div>"));
        var words = AllWords(LayoutHarness.FindById(root, "p")!).Where(w => !w.IsLineBreak).ToList();

        Assert.AreEqual(40, words[0].Left, 1);
        Assert.AreEqual(0, words[1].Left, 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#indentation-prop")]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void PercentageTextIndent_ResolvesAgainstTheContainingBlockWidth()
    {
        Assert.AreEqual(40, FirstWordLeft(
            "<div style='width:400px'><p id='p' style='width:200px;margin:0;text-indent:10%'>hi</p></div>"), 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#indentation-prop")]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void TextIndent_DoesntApplyToAnAnonymousBlockThatIsntTheFirstChild()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='p' style='text-indent:40px'><div>bb</div>cc</div>"));
        var cc = AllWords(LayoutHarness.FindById(root, "p")!).Single(w => w.Text == "cc");

        Assert.AreEqual(0, cc.Left, 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#indentation-prop")]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    [Spec("css-text-3", "#bidi-linebox")]
    [Spec("css-text-4", "#bidi-linebox")]
    public void TextIndent_InRtl_IndentsFromTheRightEdge()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<p id='p' dir='rtl' style='margin:0;text-indent:40px;width:200px;text-align:right'>hi</p>"));
        var word = AllWords(LayoutHarness.FindById(root, "p")!).Single();

        Assert.AreEqual(160, word.Right, 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#indentation-prop")]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void TextIndent_IndentsTheFirstLineOfAnInlineBlock()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div><span id='p' style='display:inline-block;width:200px;text-indent:40px'>bb</span></div>"));
        var p = LayoutHarness.FindById(root, "p")!;
        var word = AllWords(p).Single();

        Assert.AreEqual(p.Location.X + 40, word.Left, 1);
    }

    private static double FirstWordLeft(string bodyHtml)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(bodyHtml));
        return AllWords(LayoutHarness.FindById(root, "p")!).First().Left;
    }

    private static double FirstWordWidth(string bodyHtml)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(bodyHtml));
        return AllWords(LayoutHarness.FindById(root, "p")!).First().Width;
    }

    private static double InterWordGap(string bodyHtml)
    {
        var html = LayoutHarness.Wrap(bodyHtml);
        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;
        var words = AllWords(p).OrderBy(w => w.Left).ToList();

        Assert.AreEqual(2, words.Count);
        return words[1].Left - (words[0].Left + words[0].Width);
    }

    private static System.Collections.Generic.IEnumerable<TheArtOfDev.HtmlRenderer.Core.Dom.CssRect> AllWords(
        TheArtOfDev.HtmlRenderer.Core.Dom.CssBox box)
    {
        foreach (var word in box.Words) yield return word;
        foreach (var child in box.Boxes)
        {
            foreach (var word in AllWords(child)) yield return word;
        }
    }
}
