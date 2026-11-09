using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.BoxModel;

[DoNotParallelize]
[TestClass]
public sealed class PaddingIntegrationTests
{
    private const double Delta = 0.5;

    [TestMethod]
    [Spec("css21", "box.html#padding-properties")]
    public void PercentagePadding_ResolvesAgainstTheContainingBlockWidth()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:200px'><div id='b' style='padding-top:10%;padding-left:5%;height:10px'></div></div>"));
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(20, b.ActualPaddingTop, Delta);
        Assert.AreEqual(10, b.ActualPaddingLeft, Delta);
        Assert.AreEqual(30, b.Size.Height, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "box.html#padding-properties")]
    public void NegativePadding_IsIgnored()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='b' style='padding-left:-10px'></div>"));
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(0, b.ActualPaddingLeft);
    }
}
