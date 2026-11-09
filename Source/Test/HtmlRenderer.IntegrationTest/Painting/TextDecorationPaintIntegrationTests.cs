using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class TextDecorationPaintIntegrationTests
{
    [TestMethod]
    [Spec("css21", "text.html#lining-striking-props")]
    public void Underline_DrawsLineBelowRectangleTop()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<span id='s' style='text-decoration:underline; color:rgb(0,0,255)'>text</span>"));
        var s = PaintHarness.FindById(root, "s")!;

        var g = PaintHarness.PaintBox(container, s);

        var line = g.Log.OfType<RecordingGraphics.DrawLineCall>().Single();
        Assert.AreEqual(RColor.FromArgb(0, 0, 255), line.Color);

        var rect = s.Rectangles.Values.Single();
        Assert.IsTrue(line.Y1 > rect.Top, "underline should sit below the rectangle's top edge");
        Assert.IsTrue(line.Y1 <= rect.Bottom, "underline should still sit within the rectangle");
    }

    [TestMethod]
    [Spec("css21", "text.html#lining-striking-props")]
    public void Overline_DrawsLineAtRectangleTop()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<span id='s' style='text-decoration:overline'>text</span>"));
        var s = PaintHarness.FindById(root, "s")!;

        var g = PaintHarness.PaintBox(container, s);

        var line = g.Log.OfType<RecordingGraphics.DrawLineCall>().Single();
        var rect = s.Rectangles.Values.Single();

        Assert.AreEqual(rect.Top, line.Y1, 1);
    }

    [TestMethod]
    [Spec("css21", "text.html#lining-striking-props")]
    public void LineThrough_DrawsLineNearRectangleMiddle()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<span id='s' style='text-decoration:line-through'>text</span>"));
        var s = PaintHarness.FindById(root, "s")!;

        var g = PaintHarness.PaintBox(container, s);

        var line = g.Log.OfType<RecordingGraphics.DrawLineCall>().Single();
        var rect = s.Rectangles.Values.Single();
        var expectedMiddle = rect.Top + rect.Height / 2;

        Assert.AreEqual(expectedMiddle, line.Y1, 1);
    }

    [TestMethod]
    [Spec("css21", "text.html#lining-striking-props")]
    public void Underline_OverlineLineThrough_ProduceDifferentYPositions()
    {
        var underlineY = GetDecorationY("underline");
        var overlineY = GetDecorationY("overline");
        var lineThroughY = GetDecorationY("line-through");

        Assert.IsTrue(overlineY < lineThroughY, "overline should sit above line-through");
        Assert.IsTrue(lineThroughY < underlineY, "line-through should sit above underline");
    }

    [TestMethod]
    [Spec("css21", "text.html#lining-striking-props")]
    public void TextDecorationNone_DrawsNoLine()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<span id='s' style='text-decoration:none'>text</span>"));
        var s = PaintHarness.FindById(root, "s")!;

        var g = PaintHarness.PaintBox(container, s);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawLineCall>().Any());
    }

    [TestMethod]
    [Spec("css21", "text.html#lining-striking-props")]
    public void SeveralLines_AreAllDrawn()
    {
        var lines = PageLines("<span style='text-decoration:underline overline'>text</span>");

        Assert.AreEqual(2, lines.Select(l => l.Y1).Distinct().Count());
    }

    [TestMethod]
    [Spec("css21", "text.html#lining-striking-props")]
    public void Blink_DrawsNoLine()
    {
        Assert.AreEqual(0, PageLines("<span style='text-decoration:blink'>text</span>").Count);
    }

    [TestMethod]
    [Spec("css21", "text.html#lining-striking-props")]
    public void Block_PropagatesTheDecorationToItsText()
    {
        Assert.AreEqual(1, PageLines("<div style='text-decoration:underline'>text</div>").Count);
    }

    [TestMethod]
    [Spec("css21", "text.html#lining-striking-props")]
    public void Inline_PropagatesTheDecorationToTheBlockThatSplitsIt()
    {
        Assert.AreEqual(3, PageLines("<span style='text-decoration:underline'>a<div>b</div>c</span>").Count);
    }

    [TestMethod]
    [Spec("css21", "text.html#lining-striking-props")]
    public void Underline_SkipsMarginAndPadding()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<span id='s' style='text-decoration:underline;padding-left:20px;padding-right:20px;margin-left:10px'>text</span>"));
        var s = PaintHarness.FindById(root, "s")!;
        var word = LayoutHarness.Descendants(s).SelectMany(b => b.Words).Single();

        var line = PaintHarness.PaintPage(container).Log.OfType<RecordingGraphics.DrawLineCall>().Single();

        Assert.AreEqual(word.Left, line.X1, 0.5);
        Assert.AreEqual(word.Right, line.X2, 0.5);
    }

    [TestMethod]
    [Spec("css21", "text.html#lining-striking-props")]
    public void Image_IsntUnderlined()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div style='text-decoration:underline'>a <img id='i' style='width:20px;height:20px'></div>"));
        var image = PaintHarness.FindById(root, "i")!.Words.Single();

        var lines = PaintHarness.PaintPage(container).Log.OfType<RecordingGraphics.DrawLineCall>().ToList();

        Assert.AreEqual(1, lines.Count);
        Assert.IsTrue(lines[0].X2 <= image.Left);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#lining-striking-props")]
    public void PropagatedDecoration_KeepsTheColorOfTheElementItIsSetOn()
    {
        var lines = PageLines("<div style='text-decoration:underline;color:rgb(0,0,255)'>a <span style='color:rgb(255,0,0)'>b</span></div>");

        Assert.AreEqual(2, lines.Count);
        Assert.IsTrue(lines.All(l => l.Color == RColor.FromArgb(0, 0, 255)));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#lining-striking-props")]
    public void Descendant_CantRemoveTheDecorationOfAnAncestor()
    {
        Assert.AreEqual(2, PageLines("<div style='text-decoration:underline'>a <span style='text-decoration:none'>b</span></div>").Count);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#lining-striking-props")]
    [DataRow("float:left")]
    [DataRow("position:absolute;top:100px")]
    [DataRow("display:inline-block")]
    public void Decoration_IsntPropagatedToFloatsAbsolutesAndInlineBlocks(string style)
    {
        Assert.AreEqual(1, PageLines($"<div style='text-decoration:underline'>a <span style='{style}'>b</span></div>").Count);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static List<RecordingGraphics.DrawLineCall> PageLines(string bodyHtml)
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(bodyHtml));
        return PaintHarness.PaintPage(container).Log.OfType<RecordingGraphics.DrawLineCall>().ToList();
    }

    private static double GetDecorationY(string decorationLine)
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            $"<span id='s' style='text-decoration:{decorationLine}'>text</span>"));
        var s = PaintHarness.FindById(root, "s")!;

        var g = PaintHarness.PaintBox(container, s);

        return g.Log.OfType<RecordingGraphics.DrawLineCall>().Single().Y1;
    }
}
