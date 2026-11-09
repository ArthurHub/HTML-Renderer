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
    public void Left_PacksTextAgainstTheLeftEdge()
    {
        var word = Words("<p id='p' style='margin:0;text-align:left;width:200px'>hi</p>").Single();

        Assert.AreEqual(0, word.Left, Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    public void Right_PacksTextAgainstTheRightEdge_IgnoringTheTrailingSpace()
    {
        var word = Words("<p id='p' style='margin:0;text-align:right;width:200px'>hi </p>").Single();

        Assert.AreEqual(200, word.Right, Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    public void Center_CentersTheTextInTheLineBox()
    {
        var word = Words("<p id='p' style='margin:0;text-align:center;width:200px'>hi</p>").Single();

        Assert.AreEqual(200 - word.Right, word.Left, Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    public void Right_AppliesToEveryLine()
    {
        var words = Words("<p id='p' style='margin:0;width:60px;text-align:right'>AAA BBB CCC DDD</p>");

        foreach (var line in words.GroupBy(w => w.Top))
            Assert.AreEqual(60, line.Max(w => w.Right), Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    public void TextAlign_IsInherited()
    {
        var word = Words("<div style='text-align:right;width:200px'><p id='p' style='margin:0'>hi</p></div>").Single();

        Assert.AreEqual(200, word.Right, Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
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

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    public void InitialValue_InRtl_ActsAsRight()
    {
        var word = Words("<p id='p' dir='rtl' style='margin:0;width:200px'>hi</p>").Single();

        Assert.AreEqual(200, word.Right, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
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
