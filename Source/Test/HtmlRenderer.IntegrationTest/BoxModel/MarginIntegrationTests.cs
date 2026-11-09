using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.BoxModel;

[DoNotParallelize]
[TestClass]
public sealed class MarginIntegrationTests
{
    private const double Delta = 0.5;

    [TestMethod]
    [Spec("css21", "box.html#margin-properties")]
    public void NegativeMargin_PullsTheBoxOutOfTheContentArea()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:200px;padding-left:50px'><div id='b' style='margin-left:-20px;height:10px'></div></div>"));
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(30, b.Location.X, Delta);
        Assert.AreEqual(220, b.Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css21", "box.html#margin-properties")]
    public void VerticalPercentageMargin_ResolvesAgainstTheContainingBlockWidth()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:200px'><div id='b' style='margin-top:10%;width:50px;height:10px'></div></div>"));
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(20, b.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "box.html#margin-properties")]
    public void HorizontalPercentageMargin_ResolvesAgainstTheContainingBlockWidth()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:200px'><div id='b' style='margin-left:10%;width:50px;height:10px'></div></div>"));
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(20, b.Location.X, Delta);
    }

    [TestMethod]
    [Spec("css21", "box.html#margin-properties")]
    public void InlineHorizontalMargin_MovesTheText()
    {
        var (plainRoot, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div>x<span id='s'>y</span></div>"));
        var (marginRoot, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div>x<span id='s' style='margin-left:30px'>y</span></div>"));

        var plain = FirstWord(LayoutHarness.FindById(plainRoot, "s")!)!;
        var withMargin = FirstWord(LayoutHarness.FindById(marginRoot, "s")!)!;

        Assert.AreEqual(30, withMargin.Left - plain.Left, Delta);
    }

    [TestMethod]
    [Spec("css21", "box.html#margin-properties")]
    [Spec("css21", "visudet.html#inline-non-replaced")]
    public void InlineVerticalMargins_DontChangeTheLineHeight()
    {
        var (plainRoot, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='p'>x <span>y</span> z</div>"));
        var (marginRoot, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='p'>x <span style='margin-top:50px;margin-bottom:50px'>y</span> z</div>"));

        var plain = LayoutHarness.FindById(plainRoot, "p")!;
        var withMargins = LayoutHarness.FindById(marginRoot, "p")!;

        Assert.AreEqual(plain.Size.Height, withMargins.Size.Height, Delta);
    }

    private static CssRect? FirstWord(CssBox box) =>
        box.Words.Count > 0 ? box.Words[0] : box.Boxes.Select(FirstWord).FirstOrDefault(w => w != null);
}
