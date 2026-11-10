using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Layout;

[DoNotParallelize]
[TestClass]
public sealed class NormalFlowIntegrationTests
{
    private const double Delta = 1.0;

    [TestMethod]
    [Spec("css21", "visuren.html#block-formatting")]
    public void Blocks_AreLaidOutOneAfterTheOtherFromTheTop()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='cb' style='padding-top:5px'><div id='a' style='height:20px'></div><div id='b' style='height:30px'></div></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(5, a.Location.Y, Delta);
        Assert.AreEqual(a.ActualBottom, b.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#block-formatting")]
    public void ABlockNextToAFloat_StillTouchesTheContainingBlocksLeftEdge()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:300px'><div style='float:left;width:100px;height:50px'></div><div id='b' style='height:20px'>text</div></div>"));

        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(0, b.Location.X, Delta);
        Assert.AreEqual(300, b.Size.Width, Delta);
        Assert.IsGreaterThanOrEqualTo(100 - Delta, FindWord(root, "text")!.Left);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#block-formatting")]
    public void FloatsInsideAnInlineBlock_DoNotShortenLinesOutsideIt()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:300px'><div style='display:inline-block;height:10px'><div style='float:left;width:50px;height:80px'></div></div><p style='margin:0'>text</p></div>"));

        Assert.IsLessThan(10, FindWord(root, "text")!.Left);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#block-formatting")]
    public void FloatsInsideAnOverflowHiddenBlock_DoNotShortenLinesOutsideIt()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:300px'><div style='overflow:hidden;height:10px'><div style='float:left;width:50px;height:80px'></div></div><p style='margin:0'>text</p></div>"));

        Assert.IsLessThan(10, FindWord(root, "text")!.Left);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#inline-formatting")]
    [Spec("css-inline-3", "#line-boxes")]
    public void InlineBoxes_AreLaidOutHorizontallyRespectingMarginsBordersAndPadding()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div><span>first</span><span style='margin-left:10px;border-left:5px solid red;padding-left:7px'>second</span></div>"));

        var first = FindWord(root, "first")!;
        var second = FindWord(root, "second")!;

        Assert.AreEqual(first.Top, second.Top, Delta);
        Assert.AreEqual(first.Right + 22, second.Left, 2);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#inline-formatting")]
    [Spec("css-inline-3", "#line-boxes")]
    public void ContentThatDoesNotFit_IsDistributedOverStackedLineBoxes()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='d' style='width:60px'>aaaa bbbb cccc dddd</div>"));

        var lines = LayoutHarness.FindById(root, "d")!.LineBoxes;

        Assert.IsGreaterThan(1, lines.Count);
        for (var i = 1; i < lines.Count; i++)
        {
            Assert.AreEqual(lines[i - 1].LineBottom, lines[i].LineTop, Delta);
        }
    }

    [TestMethod]
    [Spec("css21", "visuren.html#inline-formatting")]
    public void AnUnbreakableWord_OverflowsTheLineBox()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:40px'>aaaaaaaaaaaaaaaaaaaa</div>"));

        var word = FindWord(root, "aaaaaaaaaaaaaaaaaaaa")!;

        Assert.AreEqual(0, word.Left, Delta);
        Assert.IsGreaterThan(40, word.Right);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#inline-formatting")]
    [Spec("css-inline-3", "#line-boxes")]
    public void AnUnbreakableWord_StaysOnTheFirstLine()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='d' style='width:40px'>aaaaaaaaaaaaaaaaaaaa</div>"));

        var d = LayoutHarness.FindById(root, "d")!;

        Assert.AreEqual(d.Location.Y, FindWord(root, "aaaaaaaaaaaaaaaaaaaa")!.Top, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#inline-formatting")]
    [Spec("css-inline-3", "#line-boxes")]
    public void ASplitInlineBox_HasNoPaddingWhereItIsSplit()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='d' style='width:120px'><span style='padding-left:20px;padding-right:20px'>aaaa bbbb cccc dddd eeee ffff</span></div>"));

        var d = LayoutHarness.FindById(root, "d")!;
        Assert.IsGreaterThan(1, d.LineBoxes.Count);

        var secondLineFirstWord = d.LineBoxes[1].Words[0];
        Assert.AreEqual(0, secondLineFirstWord.Left, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#inline-formatting")]
    [Spec("css-inline-3", "#invisible-line-boxes")]
    public void ALineBoxWithoutContent_HasZeroHeight()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='d'><span></span></div><div id='n' style='height:10px'></div>"));

        var d = LayoutHarness.FindById(root, "d")!;

        Assert.AreEqual(0, d.ActualBottom - d.Location.Y, Delta);
    }

    private static CssRect? FindWord(CssBox box, string text)
    {
        foreach (var word in box.Words)
        {
            if (word.Text == text)
                return word;
        }

        foreach (var child in box.Boxes)
        {
            var found = FindWord(child, text);
            if (found is not null)
                return found;
        }

        return null;
    }
}
