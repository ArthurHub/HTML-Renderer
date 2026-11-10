using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.CssModules;

[DoNotParallelize]
[TestClass]
public sealed class CssPositionTests
{
    private const double Delta = 0.5;

    private const string Container = "<div style='position:relative;width:200px;height:100px'>{0}</div>";

    [TestMethod]
    [Spec("css-position-3", "#position-property")]
    public void Sticky_WithAutoInsets_StaysInFlow()
    {
        var root = Layout("<div id='a' style='height:20px'></div><div id='s' style='position:sticky;height:20px'></div><div id='b' style='height:20px'></div>");

        Assert.AreEqual(20, Box(root, "s").Location.Y, Delta);
        Assert.AreEqual(40, Box(root, "b").Location.Y, Delta);
    }

    [TestMethod]
    [DataRow("relative")]
    [DataRow("absolute")]
    [DataRow("fixed")]
    [DataRow("sticky")]
    [Spec("css-position-3", "#position-property")]
    [Spec("css-position-3", "#def-cb")]
    public void PositionedBox_EstablishesTheContainingBlockForAbsoluteDescendants(string position)
    {
        var root = Layout($"<div style='height:50px'></div><div id='p' style='position:{position};top:50px;left:30px;width:100px;height:100px'>"
                          + "<div id='a' style='position:absolute;top:10px;left:10px;width:10px;height:10px'></div></div>");
        var p = Box(root, "p");
        var a = Box(root, "a");

        Assert.AreEqual(p.Location.X + 10, a.Location.X, Delta);
        Assert.AreEqual(p.Location.Y + 10, a.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css-position-3.yml")]
    [TestMethod]
    [Spec("css-position-3", "#stickypos-insets")]
    public void Sticky_IsShiftedIntoTheStickyViewRectangle()
    {
        var root = Layout("<div style='height:300px'><div id='s' style='position:sticky;top:50px;height:20px'></div></div>");

        Assert.AreEqual(50, Box(root, "s").Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css-position-3.yml")]
    [TestMethod]
    [Spec("css-position-3", "#stickypos-insets")]
    public void Sticky_StaysInsideItsContainingBlock()
    {
        var root = Layout("<div style='height:40px'><div id='s' style='position:sticky;top:100px;height:20px'></div></div>");

        Assert.AreEqual(20, Box(root, "s").Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css-position-3", "#inset-shorthands")]
    public void Inset_SetsTheFourPhysicalInsetsLikeMargin()
    {
        var root = Layout(string.Format(Container, "<div id='a' style='position:absolute;inset:10px auto auto 20px;width:30px;height:30px'></div>"
                                                   + "<div id='b' style='position:absolute;inset:auto 5px 15px auto;width:30px;height:30px'></div>"));

        Assert.AreEqual(20, Box(root, "a").Location.X, Delta);
        Assert.AreEqual(10, Box(root, "a").Location.Y, Delta);
        Assert.AreEqual(200 - 5 - 30, Box(root, "b").Location.X, Delta);
        Assert.AreEqual(100 - 15 - 30, Box(root, "b").Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css-position-3", "#inset-shorthands")]
    public void InsetBlockAndInsetInline_SetTheirAxis()
    {
        var root = Layout(string.Format(Container, "<div id='a' style='position:absolute;inset-block:10px auto;inset-inline:20px auto;width:30px;height:30px'></div>"));

        Assert.AreEqual(20, Box(root, "a").Location.X, Delta);
        Assert.AreEqual(10, Box(root, "a").Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css-position-3", "#insets")]
    public void FlowRelativeInsets_MapToThePhysicalSidesInLtr()
    {
        var root = Layout(string.Format(Container, "<div id='a' style='position:absolute;inset-block-end:10px;inset-inline-end:20px;width:30px;height:30px'></div>"));

        Assert.AreEqual(200 - 20 - 30, Box(root, "a").Location.X, Delta);
        Assert.AreEqual(100 - 10 - 30, Box(root, "a").Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css-position-3.yml")]
    [TestMethod]
    [Spec("css-position-3", "#insets")]
    public void InsetInlineStart_IsTheRightSideInRtl()
    {
        var root = Layout("<div style='position:relative;direction:rtl;width:200px;height:100px'>"
                          + "<div id='a' style='position:absolute;top:0;inset-inline-start:20px;width:30px;height:30px'></div></div>");

        Assert.AreEqual(200 - 20 - 30, Box(root, "a").Location.X, Delta);
    }

    [Ignore("Gap, see Spec/css-position-3.yml")]
    [TestMethod]
    [Spec("css-position-3", "#relpos-insets")]
    public void RelativeTableRow_ShiftsItsCells()
    {
        var root = Layout("<table style='border-spacing:0'><tr><td id='a'>a</td></tr><tr style='position:relative;left:40px;top:5px'><td id='b'>b</td></tr></table>");

        Assert.AreEqual(Box(root, "a").Location.X + 40, Box(root, "b").Location.X, Delta);
    }

    [Ignore("Gap, see Spec/css-position-3.yml")]
    [TestMethod]
    [Spec("css-position-3", "#staticpos-rect")]
    [Spec("css-position-3", "#resolving-insets")]
    public void AbsoluteBox_WithAutoInsets_IsPlacedAtItsStaticPosition()
    {
        var root = Layout("<div style='position:relative;padding-left:15px'><div style='height:30px'></div>"
                          + "<div id='a' style='position:absolute;width:10px;height:10px'></div></div>");
        var a = Box(root, "a");

        Assert.AreEqual(15, a.Location.X, Delta);
        Assert.AreEqual(30, a.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css-position-3", "#resolving-insets")]
    [Spec("css-position-3", "#abspos-alignment")]
    public void AbsoluteBox_WithOneAutoInset_IsAlignedToTheOtherInset()
    {
        var root = Layout(string.Format(Container, "<div id='a' style='position:absolute;right:10px;bottom:20px;width:30px;height:30px'></div>"));

        Assert.AreEqual(160, Box(root, "a").Location.X, Delta);
        Assert.AreEqual(50, Box(root, "a").Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css-position-3.yml")]
    [TestMethod]
    [Spec("css-position-3", "#abspos-alignment")]
    public void AbsoluteBox_IsAlignedByJustifySelfAndAlignSelf()
    {
        var root = Layout(string.Format(Container, "<div id='a' style='position:absolute;inset:0;justify-self:end;align-self:center;width:30px;height:30px'></div>"));

        Assert.AreEqual(170, Box(root, "a").Location.X, Delta);
        Assert.AreEqual(35, Box(root, "a").Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css-position-3.yml")]
    [TestMethod]
    [Spec("css-position-3", "#abspos-margins")]
    public void AbsoluteBox_AutoMarginsCenterItVertically()
    {
        var root = Layout(string.Format(Container, "<div id='a' style='position:absolute;top:0;bottom:0;left:0;margin:auto 0;width:30px;height:30px'></div>"));

        Assert.AreEqual(35, Box(root, "a").Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css-position-3", "#abspos-margins")]
    public void AbsoluteBox_AutoMarginsAreZeroWhenAnInsetIsAuto()
    {
        var root = Layout(string.Format(Container, "<div id='a' style='position:absolute;top:10px;left:20px;margin:auto;width:30px;height:30px'></div>"));

        Assert.AreEqual(20, Box(root, "a").Location.X, Delta);
        Assert.AreEqual(10, Box(root, "a").Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css-position-3", "#abspos-breaking")]
    public void AbsoluteBox_IsPositionedIgnoringPageBreaks()
    {
        var (root, _) = PaintHarness.LayoutPaginated(PaintHarness.Wrap(
            "<div style='position:relative'><div style='height:150px'></div><div id='a' style='position:absolute;top:120px;left:0;width:10px;height:10px'></div></div>"),
            pageHeight: 100);

        Assert.AreEqual(120, PaintHarness.FindById(root, "a")!.Location.Y, Delta);
    }

    private static CssBox Layout(string body) => LayoutHarness.Layout(LayoutHarness.Wrap(body), maxWidth: 400, maxHeight: 400).Root;

    private static CssBox Box(CssBox root, string id) => LayoutHarness.FindById(root, id)!;
}
