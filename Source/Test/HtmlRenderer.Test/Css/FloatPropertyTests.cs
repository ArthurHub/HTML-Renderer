using HtmlRenderer.Test.TestSupport;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/FloatClearProperty.cs.
/// HTML-Renderer does have a `clear` CSS property (CssEngine.ClearProperty, CssBoxProperties.Clear) -
/// the original note claiming otherwise predates that property being added. Both the "invalid keyword"
/// float and clear cases are dropped: TheArtOfDev.HtmlRenderer.Core.Parse.CssParser does not validate
/// `float`/`clear` values against a keyword set, and CssUtils.SetPropertyValue assigns whatever string
/// was parsed straight to CssBox.Float/CssBox.Clear with no rejection path, so there is no "illegal
/// keyword" outcome to observe in this fork.
/// Exercised via the real box tree (LayoutHarness + inline style) rather than raw property parsing, so
/// the assertion is against the actual CssBoxProperties.Float/Clear value a laid-out box ends up with.
/// </summary>
[TestClass]
public sealed class FloatPropertyTests
{
    [TestMethod]
    [DataRow("left")]
    [DataRow("right")]
    [DataRow("none")]
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
    public void ClearKeywordLegal_SetsBoxClear(string keyword)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<div id='target' style='clear: {keyword}'>content</div>"));

        var target = LayoutHarness.FindById(root, "target");

        Assert.IsNotNull(target);
        Assert.AreEqual(keyword, target.Clear);
    }
}
