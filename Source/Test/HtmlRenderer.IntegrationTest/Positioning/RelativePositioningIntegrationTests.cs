using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Positioning;

[DoNotParallelize]
[TestClass]
public sealed class RelativePositioningIntegrationTests
{
    private const double Delta = 1.0;

    [TestMethod]
    [Spec("css21", "visuren.html#choose-position")]
    public void PositionStatic_IgnoresTheBoxOffsets()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='t' style='position:static;top:20px;left:30px;height:10px'></div>"));

        var t = LayoutHarness.FindById(root, "t")!;

        Assert.AreEqual(0, t.Location.X, Delta);
        Assert.AreEqual(0, t.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#relative-positioning")]
    public void LeftAndTop_MoveTheBoxRightAndDown()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='t' style='position:relative;left:15px;top:25px;width:10px;height:10px'></div>"));

        var t = LayoutHarness.FindById(root, "t")!;

        Assert.AreEqual(15, t.Location.X, Delta);
        Assert.AreEqual(25, t.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#relative-positioning")]
    public void RightAlone_MovesTheBoxLeft()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='padding-left:50px'><div id='t' style='position:relative;right:15px;width:10px;height:10px'></div></div>"));

        Assert.AreEqual(35, LayoutHarness.FindById(root, "t")!.Location.X, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#relative-positioning")]
    public void LeftAndRight_LeftWinsInLtr()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='t' style='position:relative;left:15px;right:40px;width:10px;height:10px'></div>"));

        Assert.AreEqual(15, LayoutHarness.FindById(root, "t")!.Location.X, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#relative-positioning")]
    public void LeftAndRight_RightWinsInRtl()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='direction:rtl;padding-left:50px'><div id='t' style='position:relative;left:15px;right:40px;height:10px'></div></div>"));

        Assert.AreEqual(10, LayoutHarness.FindById(root, "t")!.Location.X, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#relative-positioning")]
    public void TopAndBottom_TopWins()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='t' style='position:relative;top:15px;bottom:40px;width:10px;height:10px'></div>"));

        Assert.AreEqual(15, LayoutHarness.FindById(root, "t")!.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#relative-positioning")]
    public void RelativeOffset_KeepsTheBoxSize()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:200px'><div id='t' style='position:relative;left:30px;height:10px'></div></div>"));

        Assert.AreEqual(200, LayoutHarness.FindById(root, "t")!.Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#position-props")]
    public void NegativeOffsets_AreAllowed()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='padding:50px'><div id='t' style='position:relative;left:-20px;top:-30px;width:10px;height:10px'></div></div>"));

        var t = LayoutHarness.FindById(root, "t")!;

        Assert.AreEqual(30, t.Location.X, Delta);
        Assert.AreEqual(20, t.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#position-props")]
    public void PercentageLeft_RefersToTheContainingBlockWidth()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:400px'><div id='t' style='position:relative;left:10%;width:100px;height:10px'></div></div>"));

        Assert.AreEqual(40, LayoutHarness.FindById(root, "t")!.Location.X, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#position-props")]
    public void PercentageTop_RefersToTheContainingBlockHeight()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='height:400px'><div id='t' style='position:relative;top:10%;width:100px;height:100px'></div></div>"));

        Assert.AreEqual(40, LayoutHarness.FindById(root, "t")!.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#relative-positioning")]
    public void RelativeInline_MovesItsText()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div>first <span style='position:relative;top:10px;left:20px'>second</span></div>"
            + "<div>first2 <span>second2</span></div>"));

        var moved = FindWord(root, "second")!;
        var reference = FindWord(root, "second2")!;
        var first = FindWord(root, "first")!;
        var first2 = FindWord(root, "first2")!;

        Assert.AreEqual(10, (moved.Top - first.Top) - (reference.Top - first2.Top), Delta);
        Assert.AreEqual(20, (moved.Left - first.Right) - (reference.Left - first2.Right), Delta);
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
