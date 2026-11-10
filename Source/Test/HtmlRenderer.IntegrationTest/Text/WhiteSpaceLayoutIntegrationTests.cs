using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Text;

[DoNotParallelize]
[TestClass]
public sealed class WhiteSpaceLayoutIntegrationTests
{
    private const double Delta = 0.5;

    [TestMethod]
    [Spec("css21", "text.html#white-space-prop")]
    [Spec("css-text-3", "#white-space-property")]
    [Spec("css-text-4", "#white-space-property")]
    public void Pre_PreservesMultipleConsecutiveSpacesAsLiteralWord()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p' style='white-space:pre'>A     B</p>"));
        var p = LayoutHarness.FindById(root, "p")!;
        var words = p.LineBoxes[0].Words;

        Assert.IsTrue(words.Any(w => w.Text == "     "));
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-prop")]
    [Spec("css-text-3", "#white-space-property")]
    [Spec("css-text-4", "#white-space-property")]
    public void Normal_CollapsesConsecutiveSpaces_NoLiteralSpaceWord()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>A     B</p>"));
        var p = LayoutHarness.FindById(root, "p")!;
        var words = p.LineBoxes[0].Words;

        Assert.IsFalse(words.Any(w => w.Text != null && w.Text.Length > 0 && w.Text.All(char.IsWhiteSpace)));
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-prop")]
    [Spec("css-text-3", "#white-space-property")]
    [Spec("css-text-4", "#white-space-property")]
    public void Pre_TreatsExplicitNewlineAsForcedLineBreak()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p' style='white-space:pre'>A\nB</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual(2, p.LineBoxes.Count);
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-prop")]
    [Spec("css-text-3", "#white-space-property")]
    [Spec("css-text-4", "#white-space-property")]
    public void Pre_TreatsCrLfAsASingleNewline()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p' style='white-space:pre'>A\r\nB</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual(2, p.LineBoxes.Count);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#white-space-prop")]
    public void Pre_TreatsALoneCarriageReturnAsANewline()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p' style='white-space:pre'>A\rB</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual(2, p.LineBoxes.Count);
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-prop")]
    [Spec("css-text-3", "#white-space-property")]
    [Spec("css-text-4", "#white-space-property")]
    public void Pre_DoesntWrapAtTheLineBoxEdge()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p' style='margin:0;white-space:pre;width:30px'>AAAA BBBB</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual(1, p.LineBoxes.Count);
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-prop")]
    [Spec("css-text-3", "#white-space-property")]
    [Spec("css-text-4", "#white-space-property")]
    public void Normal_IgnoresEmbeddedNewline_NoForcedBreak()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>A\nB</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual(1, p.LineBoxes.Count);
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-prop")]
    [Spec("css-text-3", "#white-space-property")]
    [Spec("css-text-4", "#white-space-property")]
    [Spec("css-text-4", "#text-wrapping")]
    public void NoWrap_PreventsWrapping_EvenWhenNarrowerThanContent()
    {
        var html = LayoutHarness.Wrap("<p id='p' style='white-space:nowrap; width:50px'>a long run of unwrapped text here</p>");
        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual(1, p.LineBoxes.Count);
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-prop")]
    [Spec("css-text-3", "#white-space-property")]
    [Spec("css-text-4", "#white-space-property")]
    [Spec("css-text-4", "#text-wrapping")]
    public void Normal_WrapsAtNarrowWidth_ForContrastWithNoWrap()
    {
        var html = LayoutHarness.Wrap("<p id='p' style='width:50px'>a long run of unwrapped text here</p>");
        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(p.LineBoxes.Count > 1);
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-prop")]
    [Spec("css-text-3", "#white-space-property")]
    [Spec("css-text-4", "#white-space-property")]
    public void PreWrap_PreservesSpacesAndWraps()
    {
        var words = Words("<p id='p' style='margin:0;white-space:pre-wrap;width:40px'>AAAA   BBBB CCCC</p>");

        Assert.IsTrue(words.Any(w => w.Text == "   "));
        Assert.AreEqual(3, words.Where(w => !w.IsSpaces).Select(w => w.Top).Distinct().Count());
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-prop")]
    [Spec("css-text-3", "#white-space-property")]
    [Spec("css-text-4", "#white-space-property")]
    public void PreLine_CollapsesSpacesButKeepsNewlines()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p' style='margin:0;white-space:pre-line'>A    B\nC</p>"));
        var p = LayoutHarness.FindById(root, "p")!;
        var words = AllWords(p).ToList();

        Assert.AreEqual(2, p.LineBoxes.Count);
        Assert.IsFalse(words.Any(w => w.IsSpaces && !w.IsLineBreak));
        Assert.AreEqual(GapBetween("<p id='p' style='margin:0'>A B</p>", "A", "B"), Gap(words, "A", "B"), Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-prop")]
    [Spec("css-text-3", "#white-space-property")]
    [Spec("css-text-4", "#white-space-property")]
    public void PreLine_WrapsAtTheLineBoxEdge()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p' style='margin:0;white-space:pre-line;width:40px'>AAAA BBBB CCCC</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual(3, p.LineBoxes.Count);
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-prop")]
    [Spec("css-text-3", "#white-space-property")]
    [Spec("css-text-4", "#white-space-property")]
    public void WhiteSpace_IsInherited()
    {
        var words = Words("<div style='white-space:pre'><p id='p' style='margin:0'>A   B</p></div>");

        Assert.IsTrue(words.Any(w => w.Text == "   "));
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-model")]
    [Spec("css-text-3", "#line-break-transform")]
    [Spec("css-text-4", "#line-break-transform")]
    public void Normal_TransformsALinefeedAndTheSpacesAroundItIntoOneSpace()
    {
        var expected = GapBetween("<p id='p' style='margin:0'>AA BB</p>", "AA", "BB");

        Assert.AreEqual(expected, GapBetween("<p id='p' style='margin:0'>AA   \n   BB</p>", "AA", "BB"), Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-model")]
    [Spec("css-text-3", "#white-space-phase-1")]
    [Spec("css-text-4", "#white-space-phase-1")]
    public void Normal_ConvertsATabToASpace()
    {
        var expected = GapBetween("<p id='p' style='margin:0'>AA BB</p>", "AA", "BB");

        Assert.AreEqual(expected, GapBetween("<p id='p' style='margin:0'>AA\tBB</p>", "AA", "BB"), Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-model")]
    [Spec("css-text-3", "#white-space-phase-2")]
    [Spec("css-text-4", "#white-space-phase-2")]
    public void Normal_RemovesASpaceAtTheBeginningOfALine()
    {
        var word = Words("<p id='p' style='margin:0'>   AA</p>").Single();

        Assert.AreEqual(0, word.Left, Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-model")]
    [Spec("css-text-3", "#white-space-phase-1")]
    [Spec("css-text-4", "#white-space-phase-1")]
    public void Normal_CollapsesASpaceAtTheStartOfAnInlineWithOneBeforeIt_WhenTheFirstHasNoSpaceAfter()
    {
        var expected = GapBetween("<p id='p' style='margin:0'>AA BB</p>", "AA", "BB");

        Assert.AreEqual(expected, GapBetween("<p id='p' style='margin:0'>AA<span> BB</span></p>", "AA", "BB"), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#white-space-model")]
    [Spec("css-text-3", "#white-space-phase-1")]
    [Spec("css-text-4", "#white-space-phase-1")]
    public void Normal_RemovesASpaceFollowingAnotherSpaceAcrossAnElementBoundary()
    {
        var expected = GapBetween("<p id='p' style='margin:0'>AA BB</p>", "AA", "BB");

        Assert.AreEqual(expected, GapBetween("<p id='p' style='margin:0'>AA <span> BB</span></p>", "AA", "BB"), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#white-space-model")]
    [Spec("css-text-3", "#white-space-phase-2")]
    [Spec("css-text-4", "#white-space-phase-2")]
    public void Pre_RendersATabUpToTheNextTabStop()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<p id='p' style='margin:0;white-space:pre'>A\tB</p><p id='s' style='margin:0;white-space:pre'>        </p>"));
        var b = AllWords(LayoutHarness.FindById(root, "p")!).Single(w => w.Text == "B");
        var eightSpaces = AllWords(LayoutHarness.FindById(root, "s")!).Single();

        Assert.AreEqual(eightSpaces.Width, b.Left, 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#white-space-model")]
    [Spec("css-text-3", "#white-space-phase-1")]
    [Spec("css-text-4", "#white-space-phase-1")]
    public void NoBreakSpaces_ArentCollapsed()
    {
        var expected = GapBetween("<p id='p' style='margin:0'>A B</p>", "A", "B");

        Assert.IsTrue(GapBetween("<p id='p' style='margin:0'>A  B</p>", "A", "B") > expected + 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#white-space-model")]
    [Spec("css-text-3", "#white-space-phase-1")]
    [Spec("css-text-4", "#white-space-phase-1")]
    public void Nbsp_OnlyContent_ProducesNonZeroHeight_MatchingRealText()
    {
        var (nbspRoot, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='b'>&nbsp;</div>"));
        var (textRoot, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='b'>A</div>"));
        var nbspBox = LayoutHarness.FindById(nbspRoot, "b")!;
        var textBox = LayoutHarness.FindById(textRoot, "b")!;

        var nbspHeight = nbspBox.ActualBottom - nbspBox.Location.Y;
        var textHeight = textBox.ActualBottom - textBox.Location.Y;

        Assert.IsTrue(nbspHeight > 0, $"Expected non-zero height for nbsp-only content, got {nbspHeight}");
        Assert.IsTrue(nbspHeight >= textHeight - 1 && nbspHeight <= textHeight + 1);
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-model")]
    [Spec("css-text-3", "#white-space-phase-2")]
    [Spec("css-text-4", "#white-space-phase-2")]
    public void OrdinaryWhitespaceOnlyContent_StillProducesZeroHeight_NoRegression()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='b'>   </div>"));
        var box = LayoutHarness.FindById(root, "b")!;

        var height = box.ActualBottom - box.Location.Y;
        Assert.IsTrue(height >= 0 && height <= 0.5);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#white-space-model")]
    [Spec("css-text-3", "#white-space-phase-1")]
    [Spec("css-text-4", "#white-space-phase-1")]
    public void Nbsp_BetweenTokens_PreventsLineWrap_ContrastOrdinarySpace()
    {
        // Narrow enough that an ordinary space between "10" and "km" wraps.
        var (nbspRoot, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p' style='width:35px'>10&nbsp;km</p>"));
        var pNbsp = LayoutHarness.FindById(nbspRoot, "p")!;

        var (spaceRoot, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p' style='width:15px'>10 km</p>"));
        var pSpace = LayoutHarness.FindById(spaceRoot, "p")!;

        Assert.AreEqual(1, pNbsp.LineBoxes.Count);
        Assert.IsTrue(pSpace.LineBoxes.Count > 1,
            "expected ordinary space to still allow wrapping, for contrast with nbsp");
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#white-space-model")]
    [Spec("css-text-3", "#line-break-details")]
    [Spec("css-text-4", "#line-break-details")]
    public void ElementBoundaryWithoutWhiteSpace_IsntALineBreakingOpportunity()
    {
        var words = Words("<p id='p' style='margin:0;width:60px'>AAAA<b>BBBB</b></p>");

        Assert.AreEqual(1, words.Select(w => w.Top).Distinct().Count());
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#white-space-model")]
    [Spec("css-text-3", "#line-break-details")]
    [Spec("css-text-4", "#line-break-details")]
    public void Float_DoesntIntroduceALineBreakingOpportunity()
    {
        var words = Words("<p id='p' style='margin:0;width:60px'>AAAA<span style='float:right'>x</span>BBBB</p>");

        Assert.AreEqual(
            words.Single(w => w.Text == "AAAA").Top,
            words.Single(w => w.Text == "BBBB").Top, Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#ctrlchars")]
    [Spec("css-text-3", "#characters")]
    [Spec("css-text-4", "#characters")]
    public void CombiningCharacter_StaysWithItsBaseCharacter()
    {
        var word = Words("<p id='p' style='margin:0'>öx</p>").Single();

        Assert.AreEqual("öx", word.Text);
    }

    [Ignore("Gap, see Spec/css-text-3.yml")]
    [TestMethod]
    [Spec("css-text-3", "#characters")]
    [Spec("css-text-4", "#characters")]
    public void BreakAll_DoesntSeparateACombiningMarkFromItsBase()
    {
        var words = Words("<p id='p' style='margin:0;width:1px;word-break:break-all'>öx</p>");

        Assert.IsTrue(words.Any(w => w.Text == "ö"));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#ctrlchars")]
    [Spec("css-text-3", "#white-space-processing")]
    [Spec("css-text-4", "#white-space-processing")]
    [DataRow("\u000B")]
    [DataRow("\u0085")]
    public void ControlCharacter_IsRenderedLikeANormalCharacter(string control)
    {
        var words = Words($"<p id='p' style='margin:0'>A{control}B</p>");

        Assert.AreEqual(1, words.Count);
    }

    [TestMethod]
    [Spec("css-text-3", "#word-break-property")]
    [Spec("css-text-4", "#word-break-property")]
    public void BreakAll_ForcesMidWordBreak_ContrastNormal()
    {
        // No space anywhere: "normal" has no break opportunity and overflows, "break-all" wraps.
        const string longWord = "abcdefghijklmnopqrstuvwxyz";

        var (normalRoot, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<p id='p' style='width:50px'>{longWord}</p>"));
        var pNormal = LayoutHarness.FindById(normalRoot, "p")!;

        var (breakAllRoot, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap($"<p id='p' style='width:50px; word-break:break-all'>{longWord}</p>"));
        var pBreakAll = LayoutHarness.FindById(breakAllRoot, "p")!;

        // An overflowing word pushes an empty line box ahead of it, count only lines with text.
        Assert.AreEqual(1, LinesWithWordContent(pNormal));
        Assert.IsTrue(LinesWithWordContent(pBreakAll) > 1, "expected break-all to force a mid-word break");
    }

    private static int LinesWithWordContent(CssBox box) =>
        box.LineBoxes.Count(lb => lb.Words.Any(w => !string.IsNullOrEmpty(w.Text)));

    private static List<CssRect> Words(string bodyHtml)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(bodyHtml));
        return AllWords(LayoutHarness.FindById(root, "p")!).Where(w => !w.IsLineBreak).ToList();
    }

    private static double GapBetween(string bodyHtml, string first, string second) => Gap(Words(bodyHtml), first, second);

    private static double Gap(IEnumerable<CssRect> words, string first, string second)
    {
        var list = words.ToList();
        return list.Single(w => w.Text == second).Left - list.Single(w => w.Text == first).Right;
    }

    private static IEnumerable<CssRect> AllWords(CssBox box) => LayoutHarness.Descendants(box).SelectMany(b => b.Words);
}
