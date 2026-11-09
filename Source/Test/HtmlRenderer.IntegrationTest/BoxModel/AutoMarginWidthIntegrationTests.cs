using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.BoxModel;

/// <remarks>
/// <c>ActualWidth</c> is the computed width and reads 0 for <c>width: auto</c>, the used border box width is
/// <c>Size.Width</c>.
/// </remarks>
[DoNotParallelize]
[TestClass]
public sealed class AutoMarginWidthIntegrationTests
{
    private const double Delta = 1.0;

    private const string ActualWidthOfAuto = "Asserts ActualWidth, which is the computed width and 0 for width: auto";

    [Ignore(ActualWidthOfAuto)]
    [TestMethod]
    public void AutoWidth_MarginZeroAuto_FillsContainingBlock()
    {
        var html = LayoutHarness.Wrap("<div id='c' style='max-width:5000px; margin:0 auto'>x</div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 400, maxHeight: 2000);
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(0, box.Location.X, Delta);
        Assert.AreEqual(400, box.ActualWidth, Delta);
    }

    [Ignore(ActualWidthOfAuto)]
    [TestMethod]
    public void AutoWidth_MarginZeroAuto_WithPadding_FillsAndOffsetsByBorderPadding()
    {
        var html = LayoutHarness.Wrap("<div id='c' style='max-width:5000px; margin:0 auto; padding:0 25px'>x</div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 400, maxHeight: 2000);
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(0, box.Location.X, Delta);
        Assert.AreEqual(400, box.ActualWidth, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#the-width-property")]
    public void WidthPercent100_Child_FillsInsideAutoMarginWrapper()
    {
        var html = LayoutHarness.Wrap(
            "<div id='wrap' style='max-width:5000px; margin:0 auto'>"
            + "<div id='inner' style='width:100%'>x</div></div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 400, maxHeight: 2000);
        var inner = LayoutHarness.FindById(root, "inner")!;

        Assert.AreEqual(0, inner.Location.X, Delta);
        Assert.AreEqual(400, inner.ActualWidth, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#blockwidth")]
    public void DefiniteWidth_MarginZeroAuto_Centers()
    {
        var html = LayoutHarness.Wrap("<div id='c' style='width:100px; margin:0 auto'>x</div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 400, maxHeight: 2000);
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(150, box.Location.X, Delta);
        Assert.AreEqual(100, box.ActualWidth, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#min-max-widths")]
    public void AutoWidth_MaxWidthBinds_MarginZeroAuto_Centers()
    {
        var html = LayoutHarness.Wrap("<div id='c' style='max-width:200px; margin:0 auto'>x</div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 400, maxHeight: 2000);
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(200, box.ActualWidth, Delta);
        Assert.AreEqual(100, box.Location.X, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#min-max-widths")]
    public void AutoWidth_MinWidthReWidensPastMaxWidth_CentersOnMinWidth()
    {
        var html = LayoutHarness.Wrap("<div id='c' style='max-width:200px; min-width:300px; margin:0 auto'>x</div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 400, maxHeight: 2000);
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(300, box.ActualWidth, Delta);
        Assert.AreEqual(50, box.Location.X, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#min-max-widths")]
    public void AutoWidth_MinWidthExceedsContainingBlock_MarginsCollapseToZero()
    {
        var html = LayoutHarness.Wrap("<div id='c' style='min-width:600px; margin:0 auto'>x</div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 400, maxHeight: 2000);
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(600, box.ActualWidth, Delta);
        Assert.AreEqual(0, box.Location.X, Delta);
    }

    [Ignore(ActualWidthOfAuto)]
    [TestMethod]
    public void AutoWidth_MarginZero_LeftAligns_Fills()
    {
        var html = LayoutHarness.Wrap("<div id='c' style='max-width:5000px; margin:0'>x</div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 400, maxHeight: 2000);
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(0, box.Location.X, Delta);
        Assert.AreEqual(400, box.ActualWidth, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#blockwidth")]
    public void Table_MarginZeroAuto_CentersAgainstResolvedWidth()
    {
        var html = LayoutHarness.Wrap(
            "<div id='wrap' style='width:200px'>"
            + "<table id='t' style='width:100px; margin:0 auto'><tr><td>A</td></tr></table></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var wrap = LayoutHarness.FindById(root, "wrap")!;
        var table = LayoutHarness.FindById(root, "t")!;

        var expected = wrap.ClientLeft + (200.0 - 100.0) / 2.0;

        Assert.AreEqual(expected, table.Location.X, Delta);
    }
}
