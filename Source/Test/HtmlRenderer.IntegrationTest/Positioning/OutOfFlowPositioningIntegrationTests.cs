using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Positioning;

[DoNotParallelize]
[TestClass]
public sealed class OutOfFlowPositioningIntegrationTests
{
    private const double Delta = 1.0;

    [TestMethod]
    [Spec("css21", "visuren.html#absolute-positioning")]
    public void PositionAbsolute_HasNoImpactOnLaterSiblings()
    {
        var html = LayoutHarness.Wrap(
            "<div style='position:relative'>"
            + "<div id='a' style='height:20px'></div>"
            + "<div style='position:absolute;top:0;left:0;width:50px;height:100px'></div>"
            + "<div id='b' style='height:20px'></div></div>");
        var (root, _) = LayoutHarness.Layout(html);

        Assert.AreEqual(LayoutHarness.FindById(root, "a")!.ActualBottom, LayoutHarness.FindById(root, "b")!.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "box.html#collapsing-margins")]
    public void PositionAbsolute_MarginsDoNotCollapse()
    {
        var html = LayoutHarness.Wrap(
            "<div id='cb' style='position:relative;width:100px;height:100px'>"
            + "<div id='t' style='position:absolute;top:0;left:0;margin-top:20px;width:10px;height:10px'>"
            + "<div id='c' style='margin-top:30px;height:10px'></div></div></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var cb = LayoutHarness.FindById(root, "cb")!;
        var t = LayoutHarness.FindById(root, "t")!;
        var c = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(cb.Location.Y + 20, t.Location.Y, Delta);
        Assert.AreEqual(t.Location.Y + 30, c.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#absolute-positioning")]
    public void PositionAbsolute_EstablishesTheContainingBlockForAbsoluteDescendants()
    {
        var html = LayoutHarness.Wrap(
            "<div id='outer' style='position:absolute;top:50px;left:60px;width:200px;height:200px'>"
            + "<div id='inner' style='position:absolute;top:10px;left:20px;width:10px;height:10px'></div></div>");
        var (root, _) = LayoutHarness.Layout(html);

        var inner = LayoutHarness.FindById(root, "inner")!;
        Assert.AreEqual(80, inner.Location.X, Delta);
        Assert.AreEqual(60, inner.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#absolute-positioning")]
    public void PositionAbsolute_WithoutPositionedAncestor_UsesTheInitialContainingBlock()
    {
        var html = "<html><head></head><body style='margin:30px'><div style='margin:40px'>"
                   + "<div id='t' style='position:absolute;top:5px;left:7px;width:10px;height:10px'></div></div></body></html>";
        var (root, _) = LayoutHarness.Layout(html);

        var t = LayoutHarness.FindById(root, "t")!;
        Assert.AreEqual(7, t.Location.X, Delta);
        Assert.AreEqual(5, t.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#absolute-positioning")]
    public void PositionAbsolute_ContentDoesNotFlowAroundOutsideFloats()
    {
        var html = LayoutHarness.Wrap(
            "<div style='position:relative;width:300px'>"
            + "<div style='float:left;width:100px;height:100px'></div>"
            + "<div style='position:absolute;top:0;left:0;width:300px'>text</div></div>");
        var (root, _) = LayoutHarness.Layout(html);

        Assert.IsLessThan(10, FindWord(root, "text")!.Left);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#position-props")]
    public void PositionAbsolute_RightOffset_PositionsRelativeToContainingBlockRightEdge()
    {
        var html = LayoutHarness.Wrap(
            "<div id='cb' style='position:relative;width:100px;height:100px;border:5px solid black'>"
            + "<div id='t' style='position:absolute;right:10px;top:0;width:10px;height:10px'></div></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var cb = LayoutHarness.FindById(root, "cb")!;
        var t = LayoutHarness.FindById(root, "t")!;

        Assert.AreEqual(cb.Location.X + 5 + 100 - 10, t.ActualRight, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#position-props")]
    public void PositionAbsolute_PercentageOffsets_ReferToTheContainingBlock()
    {
        var html = LayoutHarness.Wrap(
            "<div id='cb' style='position:relative;width:200px;height:400px'>"
            + "<div id='t' style='position:absolute;left:10%;top:10%;width:10px;height:10px'></div></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var cb = LayoutHarness.FindById(root, "cb")!;
        var t = LayoutHarness.FindById(root, "t")!;

        Assert.AreEqual(cb.Location.X + 20, t.Location.X, Delta);
        Assert.AreEqual(cb.Location.Y + 40, t.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#fixed-positioning")]
    public void PositionFixed_IgnoresPositionedAncestors()
    {
        var html = LayoutHarness.Wrap(
            "<div style='position:relative;margin:100px'>"
            + "<div id='t' style='position:fixed;top:10px;left:20px;width:10px;height:10px'></div></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var t = LayoutHarness.FindById(root, "t")!;

        Assert.AreEqual(20, t.Location.X, Delta);
        Assert.AreEqual(10, t.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#fixed-positioning")]
    public void PositionFixed_RightAndBottom_AnchorToTheViewport()
    {
        var html = LayoutHarness.Wrap("<div id='t' style='position:fixed;right:10px;bottom:20px;width:30px;height:40px'></div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 500, maxHeight: 600);
        var t = LayoutHarness.FindById(root, "t")!;

        Assert.AreEqual(500 - 10, t.ActualRight, Delta);
        Assert.AreEqual(600 - 20, t.ActualBottom, Delta);
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
