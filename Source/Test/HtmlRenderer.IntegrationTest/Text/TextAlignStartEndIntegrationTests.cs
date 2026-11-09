using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Text;

[DoNotParallelize]
[TestClass]
public sealed class TextAlignStartEndIntegrationTests
{
    private const double Delta = 1.0;

    private static CssRect FirstWord(CssBox box) =>
        LayoutHarness.Descendants(box).SelectMany(b => b.Words).First(w => !w.IsSpaces);

    [TestMethod]
    public void Start_InLtrBlock_PacksTextAgainstTheLeftEdge()
    {
        var html = LayoutHarness.Wrap("<p id='p' style='text-align: start; width: 200px'>hi</p>");

        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;
        var word = FirstWord(p);

        Assert.AreEqual(p.ClientLeft, word.Left, Delta);
    }

    [Ignore("text-align: start is treated as left, regardless of direction")]
    [TestMethod]
    public void Start_InRtlBlock_PacksTextAgainstTheRightEdge()
    {
        var html = LayoutHarness.Wrap("<p id='p' dir='rtl' style='text-align: start; width: 200px'>hi</p>");

        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;
        var word = FirstWord(p);

        Assert.AreEqual(p.ClientRight, word.Right, Delta);
    }

    [Ignore("text-align: end is treated as left, regardless of direction")]
    [TestMethod]
    public void End_InLtrBlock_PacksTextAgainstTheRightEdge()
    {
        var html = LayoutHarness.Wrap("<p id='p' style='text-align: end; width: 200px'>hi</p>");

        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;
        var word = FirstWord(p);

        Assert.AreEqual(p.ClientRight, word.Right, Delta);
    }

    [TestMethod]
    public void End_InRtlBlock_PacksTextAgainstTheLeftEdge()
    {
        var html = LayoutHarness.Wrap("<p id='p' dir='rtl' style='text-align: end; width: 200px'>hi</p>");

        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;
        var word = FirstWord(p);

        Assert.AreEqual(p.ClientLeft, word.Left, Delta);
    }

    [TestMethod]
    [Spec("css21", "text.html#alignment-prop")]
    public void DefaultsToStart_UnsetTextAlign_BehavesLikeLeftInLtr()
    {
        var html = LayoutHarness.Wrap("<p id='p' style='width: 200px'>hi</p>");

        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;
        var word = FirstWord(p);

        Assert.AreEqual(p.ClientLeft, word.Left, Delta);
    }
}
