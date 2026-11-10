using System.Collections.Generic;
using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.CssModules;

[DoNotParallelize]
[TestClass]
public sealed class CssOverflowTests
{
    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);

    [TestMethod]
    [Spec("css-overflow-3", "#ink")]
    public void InkOverflow_OfAVisibleBox_IsPainted()
    {
        var (_, container) = PaintHarness.Layout(Doc(
            "<div style='width:50px;height:20px'><div style='width:150px;height:150px;background:rgb(255,0,0)'></div></div>"));
        var g = PaintHarness.PaintPage(container);

        var rect = g.Log.OfType<RecordingGraphics.DrawRectCall>().Single(r => r.Color == Red);
        Assert.AreEqual(150, rect.Width, 0.5);
        Assert.AreEqual(150, rect.Height, 0.5);
    }

    [TestMethod]
    [Spec("css-overflow-3", "#scrollable")]
    public void ScrollableOverflow_ExtendsTheDocumentSize()
    {
        var (_, container) = PaintHarness.Layout(Doc(
            "<div style='height:20px'><div style='height:500px'></div></div>"
            + "<div style='position:relative'><div style='position:absolute;left:0;top:0;width:700px;height:10px'></div></div>"), 300, 300);

        Assert.IsGreaterThanOrEqualTo(500, container.ActualSize.Height);
        Assert.IsGreaterThanOrEqualTo(700, container.ActualSize.Width);
    }

    [TestMethod]
    [Spec("css-overflow-3", "#scrolling")]
    public void ScrollPosition_MovesTheContentWithinTheViewport()
    {
        var (_, container) = PaintHarness.Layout(Doc(
            "<div style='height:200px'></div><div style='height:20px;background:rgb(255,0,0)'></div><div style='height:500px'></div>"), 300, 300);

        container.ScrollOffset = new RPoint(0, -150);
        var g = PaintHarness.PaintPage(container);

        Assert.AreEqual(50, g.Log.OfType<RecordingGraphics.DrawRectCall>().Single(r => r.Color == Red).Y, 0.5);
    }

    [TestMethod]
    [Spec("css-overflow-3", "#overflow-control")]
    public void OverflowHidden_ClipsToTheBox()
    {
        var clip = ClipOfRedBox(
            "<div style='overflow:hidden;width:50px;height:60px'><div style='width:150px;height:150px;background:rgb(255,0,0)'></div></div>");

        Assert.AreEqual(50, clip.Right, 0.5);
        Assert.AreEqual(60, clip.Bottom, 0.5);
    }

    [Ignore("Gap, see Spec/css-overflow-3.yml")]
    [TestMethod]
    [DataRow("overflow:clip")]
    [DataRow("overflow:hidden hidden")]
    [DataRow("overflow-x:hidden;overflow-y:hidden")]
    [Spec("css-overflow-3", "#overflow-control")]
    public void ClippingValues_ClipTheContent(string style)
    {
        var clip = ClipOfRedBox(
            $"<div style='{style};width:50px;height:60px'><div style='width:150px;height:150px;background:rgb(255,0,0)'></div></div>");

        Assert.AreEqual(50, clip.Right, 0.5);
        Assert.AreEqual(60, clip.Bottom, 0.5);
    }

    [Ignore("Gap, see Spec/css-overflow-3.yml")]
    [TestMethod]
    [Spec("css-overflow-3", "#overflow-control")]
    public void OverflowXHidden_ClipsOnlyHorizontallyAsHiddenAndAuto()
    {
        var clip = ClipOfRedBox(
            "<div style='overflow-x:hidden;width:50px;height:60px'><div style='width:150px;height:150px;background:rgb(255,0,0)'></div></div>");

        Assert.AreEqual(50, clip.Right, 0.5);
        Assert.AreEqual(60, clip.Bottom, 0.5);
    }

    [Ignore("Gap, see Spec/css-overflow-3.yml")]
    [TestMethod]
    [Spec("css-overflow-3", "#overflow-control")]
    public void OverflowHidden_EstablishesAnIndependentFormattingContext()
    {
        var (root, _) = LayoutHarness.Layout(Doc(
            "<div id='b' style='overflow:hidden'><div style='float:left;width:20px;height:50px'></div>x</div>"));

        Assert.AreEqual(50, LayoutHarness.FindById(root, "b")!.Size.Height, 0.5);
    }

    [Ignore("Gap, see Spec/css-overflow-3.yml")]
    [TestMethod]
    [Spec("css-overflow-3", "#overflow-clip-margin")]
    public void OverflowClipMargin_ExpandsTheClip()
    {
        var clip = ClipOfRedBox(
            "<div style='overflow:clip;overflow-clip-margin:10px;width:50px;height:60px'><div style='width:150px;height:150px;background:rgb(255,0,0)'></div></div>");

        Assert.AreEqual(60, clip.Right, 0.5);
        Assert.AreEqual(70, clip.Bottom, 0.5);
    }

    [Ignore("Gap, see Spec/css-overflow-3.yml")]
    [TestMethod]
    [Spec("css-overflow-3", "#text-overflow")]
    public void TextOverflowEllipsis_EndsTheClippedLineWithAnEllipsis()
    {
        var (_, container) = PaintHarness.Layout(Doc(
            "<div style='overflow:hidden;white-space:nowrap;text-overflow:ellipsis;width:60px'>aaaa bbbb cccc dddd</div>"));
        var g = PaintHarness.PaintPage(container);

        Assert.IsTrue(g.DrawStringCalls.Any(c => c.Text.Contains('…')));
    }

    [Ignore("Gap, see Spec/css-overflow-3.yml")]
    [TestMethod]
    [Spec("css-overflow-3", "#scrollbar-layout")]
    [Spec("css-overflow-3", "#scrollbar-sizing")]
    public void OverflowScroll_TakesTheScrollbarFromTheContentWidth()
    {
        var (root, _) = LayoutHarness.Layout(Doc("<div style='overflow:scroll;width:100px;height:50px'><div id='c'>x</div></div>"));

        Assert.IsLessThan(100, LayoutHarness.FindById(root, "c")!.Size.Width);
    }

    private static string Doc(string body) => $"<!DOCTYPE html><html><head></head><body style='margin:0'>{body}</body></html>";

    private static RRect ClipOfRedBox(string body)
    {
        var (_, container) = PaintHarness.Layout(Doc(body));
        var g = PaintHarness.PaintPage(container);

        return ClipAt(g, e => e is RecordingGraphics.DrawRectCall r && r.Color == Red);
    }

    private static RRect ClipAt(RecordingGraphics g, System.Func<object, bool> match)
    {
        var stack = new List<RRect>();
        foreach (var entry in g.Log)
        {
            switch (entry)
            {
                case RecordingGraphics.PushClipCall push:
                    stack.Add(push.Rect);
                    break;
                case RecordingGraphics.PopClipCall:
                    stack.RemoveAt(stack.Count - 1);
                    break;
                default:
                    if (match(entry))
                        return stack.Aggregate(RRect.Intersect);
                    break;
            }
        }

        Assert.Fail("No matching draw call was recorded");
        return RRect.Empty;
    }
}
