using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.CssModules;

[DoNotParallelize]
[TestClass]
public sealed class CssTransformsTests
{
    private const double Delta = 0.5;

    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);

    [Ignore("Gap, see Spec/css-transforms-1.yml")]
    [TestMethod]
    [Spec("css-transforms-1", "#transform-rendering")]
    [Spec("css-transforms-1", "#transform-property")]
    public void Translate_MovesThePaintedBox()
    {
        var rect = RedRect("<div style='transform:translate(30px, 20px);width:50px;height:50px;background:rgb(255,0,0)'></div>");

        Assert.AreEqual(30, rect.X, Delta);
        Assert.AreEqual(20, rect.Y, Delta);
    }

    [Ignore("Gap, see Spec/css-transforms-1.yml")]
    [TestMethod]
    [Spec("css-transforms-1", "#transform-origin-property")]
    public void Scale_IsAppliedAroundTheTransformOrigin()
    {
        var rect = RedRect("<div style='margin-left:100px;transform:scale(2);width:50px;height:50px;background:rgb(255,0,0)'></div>");

        Assert.AreEqual(75, rect.X, Delta);
        Assert.AreEqual(100, rect.Width, Delta);
    }

    [Ignore("Gap, see Spec/css-transforms-1.yml")]
    [TestMethod]
    [Spec("css-transforms-1", "#transform-rendering")]
    public void TransformedBox_IsTheContainingBlockForAbsoluteDescendants()
    {
        var root = Layout("<div style='height:50px'></div><div id='t' style='transform:translate(0, 0);margin-left:30px;width:100px;height:100px'>"
                          + "<div id='a' style='position:absolute;top:10px;left:10px;width:10px;height:10px'></div></div>");

        Assert.AreEqual(Box(root, "t").Location.X + 10, Box(root, "a").Location.X, Delta);
        Assert.AreEqual(Box(root, "t").Location.Y + 10, Box(root, "a").Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css-transforms-1.yml")]
    [TestMethod]
    [Spec("css-transforms-1", "#transform-rendering")]
    public void TransformedBox_IsTheContainingBlockForFixedDescendants()
    {
        var root = Layout("<div style='height:50px'></div><div id='t' style='position:relative;transform:translate(0, 0);margin-left:30px;width:100px;height:100px'>"
                          + "<div id='f' style='position:fixed;top:10px;left:10px;width:10px;height:10px'></div></div>");

        Assert.AreEqual(Box(root, "t").Location.X + 10, Box(root, "f").Location.X, Delta);
        Assert.AreEqual(Box(root, "t").Location.Y + 10, Box(root, "f").Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css-transforms-2.yml")]
    [TestMethod]
    [Spec("css-transforms-2", "#individual-transforms")]
    public void TranslateProperty_MovesThePaintedBox()
    {
        var rect = RedRect("<div style='translate:30px 20px;width:50px;height:50px;background:rgb(255,0,0)'></div>");

        Assert.AreEqual(30, rect.X, Delta);
        Assert.AreEqual(20, rect.Y, Delta);
    }

    [Ignore("Gap, see Spec/css-transforms-2.yml")]
    [TestMethod]
    [DataRow("perspective:100px")]
    [DataRow("transform-style:preserve-3d")]
    [Spec("css-transforms-2", "#perspective-property")]
    [Spec("css-transforms-2", "#transform-style-property")]
    public void Box_IsTheContainingBlockForFixedDescendants(string style)
    {
        AssertContainingBlockForFixedDescendants(style);
    }

    [Ignore("Gap, see Spec/motion-1.yml")]
    [TestMethod]
    [Spec("motion-1", "#offset-path-property")]
    public void OffsetPath_IsTheContainingBlockForFixedDescendants()
    {
        AssertContainingBlockForFixedDescendants("offset-path:ray(0deg)");
    }

    [Ignore("Gap, see Spec/css-will-change-1.yml")]
    [TestMethod]
    [Spec("css-will-change-1", "#will-change")]
    public void WillChangeTransform_IsTheContainingBlockForFixedDescendants()
    {
        AssertContainingBlockForFixedDescendants("will-change:transform");
    }

    private static void AssertContainingBlockForFixedDescendants(string style)
    {
        var root = Layout($"<div style='height:50px'></div><div id='t' style='{style};margin-left:30px;width:100px;height:100px'>"
                          + "<div id='f' style='position:fixed;top:10px;left:10px;width:10px;height:10px'></div></div>");

        Assert.AreEqual(Box(root, "t").Location.X + 10, Box(root, "f").Location.X, Delta);
        Assert.AreEqual(Box(root, "t").Location.Y + 10, Box(root, "f").Location.Y, Delta);
    }

    private static RecordingGraphics.DrawRectCall RedRect(string body)
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(body), 400, 400);
        var g = PaintHarness.PaintPage(container);

        return g.Log.OfType<RecordingGraphics.DrawRectCall>().First(r => r.Color == Red);
    }

    private static CssBox Layout(string body) => LayoutHarness.Layout(LayoutHarness.Wrap(body), maxWidth: 400, maxHeight: 400).Root;

    private static CssBox Box(CssBox root, string id) => LayoutHarness.FindById(root, id)!;
}
