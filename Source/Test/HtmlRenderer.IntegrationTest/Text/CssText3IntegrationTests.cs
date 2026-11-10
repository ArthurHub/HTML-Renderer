using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Text;

/// <summary>CSS Text Level 3 white space processing, line breaking and alignment.</summary>
[DoNotParallelize]
[TestClass]
public sealed class CssText3IntegrationTests
{
    private const double Delta = 1.0;

    private const string LongWord = "abcdefghijklmnopqrstuvwxyz";

    private const string Cjk = "中文中文中文中文中文中文中文中文";

    [Ignore("Gap, see Spec/css-text-3.yml")]
    [TestMethod]
    [Spec("css-text-3", "#white-space-property")]
    [Spec("css-text-3", "#white-space-phase-1")]
    public void BreakSpaces_PreservesSpaces()
    {
        var p = Paragraph("<p id='p' style='white-space:break-spaces'>A     B</p>");

        Assert.IsTrue(p.LineBoxes[0].Words.Any(w => w.Text == "     "));
    }

    [Ignore("Gap, see Spec/css-text-3.yml")]
    [TestMethod]
    [Spec("css-text-3", "#word-break-property")]
    public void KeepAll_DoesntBreakBetweenCjkCharacters()
    {
        var p = Paragraph($"<p id='p' style='width:50px;word-break:keep-all'>{Cjk}</p>");

        Assert.AreEqual(1, LinesWithText(p));
    }

    [TestMethod]
    [Spec("css-text-3", "#word-break-property")]
    [Spec("css-text-3", "#line-break-details")]
    public void Normal_BreaksBetweenCjkCharacters()
    {
        var p = Paragraph($"<p id='p' style='width:50px'>{Cjk}</p>");

        Assert.IsGreaterThan(1, LinesWithText(p));
    }

    [Ignore("Gap, see Spec/css-text-3.yml")]
    [TestMethod]
    [Spec("css-text-3", "#word-break-property")]
    public void BreakWord_BreaksAWordThatOverflows()
    {
        var p = Paragraph($"<p id='p' style='width:50px;word-break:break-word'>{LongWord}</p>");

        Assert.IsGreaterThan(1, LinesWithText(p));
    }

    [Ignore("Gap, see Spec/css-text-3.yml")]
    [TestMethod]
    [Spec("css-text-3", "#overflow-wrap-property")]
    [DataRow("overflow-wrap:break-word")]
    [DataRow("overflow-wrap:anywhere")]
    [DataRow("word-wrap:break-word")]
    public void OverflowWrap_BreaksAWordThatOverflows(string declaration)
    {
        var p = Paragraph($"<p id='p' style='width:50px;{declaration}'>{LongWord}</p>");

        Assert.IsGreaterThan(1, LinesWithText(p));
    }

    [Ignore("Gap, see Spec/css-text-3.yml")]
    [TestMethod]
    [Spec("css-text-3", "#hyphenation")]
    public void SoftHyphen_IsABreakOpportunity()
    {
        var p = Paragraph("<p id='p' style='width:60px'>abcdefghijkl­mnopqrstuvwx</p>");

        Assert.AreEqual(2, LinesWithText(p));
    }

    [TestMethod]
    [Spec("css-text-3", "#line-break-details")]
    public void HyphenMinus_IsABreakOpportunity()
    {
        var p = Paragraph("<p id='p' style='width:60px'>abcdefghijkl-mnopqrstuvwx</p>");

        Assert.AreEqual(2, LinesWithText(p));
    }

    [TestMethod]
    [Spec("css-text-3", "#justify-algos")]
    public void Justify_ExpandsBetweenCjkCharacters()
    {
        var p = Paragraph($"<p id='p' style='width:100px;text-align:justify'>{Cjk}</p>");
        var firstLine = p.LineBoxes.First(l => l.Words.Count > 1);

        Assert.AreEqual(p.ClientLeft, firstLine.Words[0].Left, Delta);
        Assert.AreEqual(p.ClientRight, firstLine.Words[^1].Right, Delta);
    }

    [Ignore("Gap, see Spec/css-text-3.yml")]
    [TestMethod]
    [Spec("css-text-3", "#text-align-last-property")]
    public void TextAlignLast_AlignsTheLastLine()
    {
        var p = Paragraph("<p id='p' style='width:200px;text-align:justify;text-align-last:right'>hi</p>");

        Assert.AreEqual(p.ClientRight, FirstWord(p).Right, Delta);
    }

    [Ignore("Gap, see Spec/css-text-3.yml")]
    [TestMethod]
    [Spec("css-text-3", "#text-align-property")]
    public void JustifyAll_JustifiesTheLastLine()
    {
        var p = Paragraph("<p id='p' style='width:200px;text-align:justify-all'>hi there</p>");
        var words = p.LineBoxes[^1].Words.Where(w => !w.IsSpaces).ToList();

        Assert.AreEqual(p.ClientRight, words[^1].Right, Delta);
    }

    private static CssBox Paragraph(string body)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(body));
        return LayoutHarness.FindById(root, "p")!;
    }

    private static CssRect FirstWord(CssBox box) =>
        LayoutHarness.Descendants(box).SelectMany(b => b.Words).First(w => !w.IsSpaces);

    private static int LinesWithText(CssBox box) =>
        box.LineBoxes.Count(lb => lb.Words.Any(w => !string.IsNullOrEmpty(w.Text)));
}
