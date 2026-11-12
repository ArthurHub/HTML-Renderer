using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Only checks that vertical-align is parsed and stored, read back from a laid-out box.
/// </summary>
[TestClass]
public sealed class VerticalAlignPropertyTests
{
    private static CssBox GetTargetBox(string value)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<span id='target' style='vertical-align: {value}'>x</span>"));
        var target = LayoutHarness.FindById(root, "target");
        Assert.IsNotNull(target);
        return target;
    }

    [TestMethod]
    [DataRow("baseline")]
    [DataRow("sub")]
    [DataRow("super")]
    [DataRow("top")]
    [DataRow("text-top")]
    [DataRow("middle")]
    [DataRow("bottom")]
    [DataRow("text-bottom")]
    public void VerticalAlignKeywordLegal(string keyword)
    {
        var target = GetTargetBox(keyword);

        Assert.AreEqual(keyword, target.VerticalAlign);
    }

    [TestMethod]
    public void VerticalAlignLengthLegal()
    {
        var target = GetTargetBox("3px");

        Assert.AreEqual("3px", target.VerticalAlign);
    }

    [TestMethod]
    public void VerticalAlignPercentLegal()
    {
        var target = GetTargetBox("25%");

        Assert.AreEqual("25%", target.VerticalAlign);
    }

    [TestMethod]
    public void VerticalAlignNegativeLengthLegal()
    {
        var target = GetTargetBox("-3px");

        Assert.AreEqual("-3px", target.VerticalAlign);
    }

    [TestMethod]
    public void VerticalAlignInvalidKeywordIllegal()
    {
        var target = GetTargetBox("wavy");

        Assert.AreEqual("baseline", target.VerticalAlign);
    }
}
