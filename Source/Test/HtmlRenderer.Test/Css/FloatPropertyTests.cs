using HtmlRenderer.Test.TestSupport;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class FloatPropertyTests
{
    [TestMethod]
    [DataRow("left")]
    [DataRow("right")]
    [DataRow("none")]
    [Spec("css21", "visuren.html#float-position")]
    public void FloatKeywordLegal_SetsBoxFloat(string keyword)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<div id='target' style='float: {keyword}'>content</div>"));

        var target = LayoutHarness.FindById(root, "target");

        Assert.IsNotNull(target);
        Assert.AreEqual(keyword, target.Float);
    }

    [TestMethod]
    [DataRow("left")]
    [DataRow("right")]
    [DataRow("both")]
    [DataRow("none")]
    [Spec("css21", "visuren.html#flow-control")]
    public void ClearKeywordLegal_SetsBoxClear(string keyword)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<div id='target' style='clear: {keyword}'>content</div>"));

        var target = LayoutHarness.FindById(root, "target");

        Assert.IsNotNull(target);
        Assert.AreEqual(keyword, target.Clear);
    }
}
