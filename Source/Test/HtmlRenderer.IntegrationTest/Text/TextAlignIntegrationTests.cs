using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Text;

[DoNotParallelize]
[TestClass]
public sealed class TextAlignIntegrationTests
{
    private const double Delta = 1.0;

    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    [Spec("css-text-3", "#text-align-property")]
    [Spec("css-text-4", "#text-align-property")]
    public void Left_PacksTextAgainstTheLeftEdge()
    {
        var word = Words("<p id='p' style='margin:0;text-align:left;width:200px'>hi</p>").Single();

        Assert.AreEqual(0, word.Left, Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    [Spec("css-text-3", "#white-space-phase-2")]
    [Spec("css-text-4", "#white-space-phase-2")]
    public void Right_PacksTextAgainstTheRightEdge_IgnoringTheTrailingSpace()
    {
        var word = Words("<p id='p' style='margin:0;text-align:right;width:200px'>hi </p>").Single();

        Assert.AreEqual(200, word.Right, Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    [Spec("css-text-3", "#text-align-property")]
    [Spec("css-text-4", "#text-align-property")]
    public void Center_CentersTheTextInTheLineBox()
    {
        var word = Words("<p id='p' style='margin:0;text-align:center;width:200px'>hi</p>").Single();

        Assert.AreEqual(200 - word.Right, word.Left, Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    [Spec("css-text-3", "#text-align-property")]
    [Spec("css-text-4", "#text-align-property")]
    public void Right_AppliesToEveryLine()
    {
        var words = Words("<p id='p' style='margin:0;width:60px;text-align:right'>AAA BBB CCC DDD</p>");

        foreach (var line in words.GroupBy(w => w.Top))
            Assert.AreEqual(60, line.Max(w => w.Right), Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    [Spec("css-text-3", "#text-align-property")]
    [Spec("css-text-4", "#text-align-property")]
    public void TextAlign_IsInherited()
    {
        var word = Words("<div style='text-align:right;width:200px'><p id='p' style='margin:0'>hi</p></div>").Single();

        Assert.AreEqual(200, word.Right, Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    [Spec("css-text-3", "#text-align-property")]
    [Spec("css-text-4", "#text-align-property")]
    [Spec("css-text-3", "#expanding-text")]
    [Spec("css-text-4", "#expanding-text")]
    public void Justify_MakesAllButTheLastLineFlushWithBothSides()
    {
        var lines = Words("<p id='p' style='margin:0;text-align:justify;width:100px'>aa bb cc dd ee ff gg hh ii jj</p>")
            .GroupBy(w => w.Top).OrderBy(g => g.Key).ToList();

        Assert.AreEqual(2, lines.Count);
        Assert.AreEqual(0, lines[0].Min(w => w.Left), Delta);
        Assert.AreEqual(100, lines[0].Max(w => w.Right), Delta);
        Assert.AreEqual(0, lines[1].Min(w => w.Left), Delta);
        Assert.IsTrue(lines[1].Max(w => w.Right) < 90, "the last line keeps its natural spacing");
    }

    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    [Spec("css21", "text.html#indentation-prop")]
    [Spec("css-text-3", "#expanding-text")]
    public void Justify_WithTextIndent_StartsTheFirstLineAtTheIndent()
    {
        var lines = Words("<p id='p' style='margin:0;text-align:justify;text-indent:30px;width:200px'>"
                          + "aaa bbb ccc ddd eee fff ggg hhh iii jjj kkk lll mmm nnn ooo ppp qqq rrr</p>")
            .GroupBy(w => w.Top).OrderBy(g => g.Key).Select(g => g.OrderBy(w => w.Left).ToList()).ToList();

        Assert.IsGreaterThan(2, lines.Count);
        Assert.AreEqual(30, lines[0][0].Left, Delta);
        Assert.AreEqual(0, lines[1][0].Left, Delta);

        foreach (var line in lines.Take(lines.Count - 1))
        {
            Assert.AreEqual(200, line[^1].Right, Delta);
            for (var i = 1; i < line.Count; i++)
                Assert.IsGreaterThanOrEqualTo(line[i - 1].Right, line[i].Left);
        }
    }

    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    [Spec("css-text-3", "#text-align-property")]
    public void Center_InABlockWithEndPaddingAndBorder_CentersInTheContentBox()
    {
        var words = Words("<p id='p' style='margin:0;text-align:center;width:200px;padding-right:16px;border-right:4px solid'>aaa bbb</p>");

        Assert.AreEqual(200 - words.Max(w => w.Right), words.Min(w => w.Left), Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    [Spec("css-text-3", "#text-align-property")]
    public void Center_ImageWithEndPaddingAndBorder_CentersItsBorderBox()
    {
        var root = TestImages.Layout(
            $"<p style='margin:0;text-align:center;width:200px'><img id='i' style='padding-right:16px;border-right:4px solid' src='{TestImages.Png(10, 10)}'></p>");
        var image = TestImages.ImageRect(root, "i");

        Assert.AreEqual(30, image.Width, Delta);
        Assert.AreEqual(200 - image.Right, image.Left, Delta);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-align-property")]
    [Spec("css-text-4", "#text-align-property")]
    public void Center_WordWiderThanTheLine_IsStartAligned()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<p id='p' style='margin:0;text-align:center;width:40px'>aaaaaaaaaaaaaaaaaaaa</p>"));
        var word = LayoutHarness.Descendants(root).SelectMany(b => b.Words).Single();

        Assert.AreEqual(0, word.Left, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    [Spec("css-text-3", "#text-align-property")]
    public void Right_InlineBoxEndPaddingAndBorder_StayInsideTheContentBox()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<p style='margin:0;text-align:right;width:200px'>aaa <span id='s' style='padding-right:15px;border-right:5px solid'>bbb</span></p>"));

        Assert.AreEqual(200, LayoutHarness.FindById(root, "s")!.Rectangles.Values.Single().Right, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    [Spec("css-text-3", "#text-align-property")]
    public void Center_InlineBoxEndPaddingAndBorder_AreCenteredWithTheLine()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<p id='p' style='margin:0;text-align:center;width:200px'>aaa <span id='s' style='padding-right:15px;border-right:5px solid'>bbb</span></p>"));

        var left = LayoutHarness.FindById(root, "p")!.Boxes[0].Words[0].Left;
        var right = LayoutHarness.FindById(root, "s")!.Rectangles.Values.Single().Right;
        Assert.AreEqual(left, 200 - right, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    [Spec("css-text-3", "#text-align-property")]
    [Spec("css-text-4", "#text-align-property")]
    public void InitialValue_InRtl_ActsAsRight()
    {
        var word = Words("<p id='p' dir='rtl' style='margin:0;width:200px'>hi</p>").Single();

        Assert.AreEqual(200, word.Right, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    [Spec("css-text-3", "#expanding-text")]
    [Spec("css-text-4", "#expanding-text")]
    public void Justify_DoesntAlterPreservedWhiteSpace()
    {
        var words = Words("<p id='p' style='margin:0;text-align:justify;white-space:pre;width:300px'>a b\nc d</p>");
        var a = words.First(w => w.Text == "a");
        var space = words.First(w => w.Text == " ");

        Assert.AreEqual(a.Right, space.Left, Delta);
    }

    private static List<CssRect> Words(string bodyHtml)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(bodyHtml));
        return LayoutHarness.Descendants(LayoutHarness.FindById(root, "p")!)
            .SelectMany(b => b.Words)
            .Where(w => !w.IsLineBreak)
            .ToList();
    }
}
