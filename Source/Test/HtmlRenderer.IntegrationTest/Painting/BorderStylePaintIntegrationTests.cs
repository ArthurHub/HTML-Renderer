using System;
using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class BorderStylePaintIntegrationTests
{
    [TestMethod]
    [DataRow("dotted")]
    [DataRow("dashed")]
    [DataRow("solid")]
    [DataRow("double")]
    [DataRow("groove")]
    [DataRow("ridge")]
    [DataRow("inset")]
    [DataRow("outset")]
    public void BorderStyle_AllCss1Keywords_DoNotThrowWhenPainted(string style)
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            $"<div id='b' style='border: 12px {style} rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        PaintHarness.PaintBox(container, div);
    }

    [TestMethod]
    [DataRow("inset", 50, 100)]
    [DataRow("outset", 100, 50)]
    [Spec("css21", "box.html#border-style-properties")]
    public void InsetAndOutset_DarkenOppositeSides(string style, int topLeft, int bottomRight)
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            $"<div id='b' style='border:12px {style} rgb(100,100,100)'>x</div>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "b")!);

        // Painted in the order top, left, bottom, right
        var colors = g.Log.OfType<RecordingGraphics.DrawPolygonCall>().Select(p => p.Color.R).ToList();
        CollectionAssert.AreEqual(new[] { topLeft, topLeft, bottomRight, bottomRight }, colors.Select(c => (int)c).ToList());
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "box.html#border-style-properties")]
    public void BorderStyleDouble_DrawsTwoEqualWidthStripesWithGap()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='border-top-style: double; border-top-width: 12px; border-top-color: rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        var lines = g.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();
        Assert.AreEqual(2, lines.Count);
        Assert.AreNotEqual(lines[0].Y1, lines[1].Y1, "expected two visually distinct stripes, not one merged line");
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "box.html#border-style-properties")]
    public void BorderStyleGroove_OuterStripeIsDarker_InnerStripeIsBaseColor()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='border-top-style: groove; border-top-width: 12px; border-top-color: rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        var lines = g.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();
        Assert.AreEqual(2, lines.Count);
        Assert.AreEqual(RColor.FromArgb(25, 25, 25), lines[0].Color);
        Assert.AreEqual(RColor.FromArgb(51, 51, 51), lines[1].Color);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "box.html#border-style-properties")]
    public void BorderRightStyleDouble_DrawsTwoEqualWidthVerticalStripesWithGap()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='border-right-style: double; border-right-width: 12px; border-right-color: rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        var lines = g.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();
        Assert.AreEqual(2, lines.Count);
        Assert.IsTrue(Math.Abs(lines[0].X1 - lines[0].X2) < 0.01, "expected a vertical (right-edge) line");
        Assert.AreNotEqual(lines[0].X1, lines[1].X1, "expected two visually distinct vertical stripes");
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "box.html#border-style-properties")]
    public void BorderLeftStyleGroove_OuterStripeIsDarker_InnerStripeIsBaseColor()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='border-left-style: groove; border-left-width: 12px; border-left-color: rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        var lines = g.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();
        Assert.AreEqual(2, lines.Count);
        Assert.AreEqual(RColor.FromArgb(25, 25, 25), lines[0].Color);
        Assert.AreEqual(RColor.FromArgb(51, 51, 51), lines[1].Color);
    }

    [TestMethod]
    [Spec("css21", "box.html#border-color-properties")]
    public void BorderColorPerSide_ResolvesDistinctColorPerEdge()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:40px; height:40px; border-style:solid; border-width:4px; " +
            "border-top-color: rgb(1,0,0); border-right-color: rgb(0,1,0); " +
            "border-bottom-color: rgb(0,0,1); border-left-color: rgb(90,90,90)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        var lines = g.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();
        Assert.AreEqual(4, lines.Count);

        var horizontal = lines.Where(l => Math.Abs(l.Y1 - l.Y2) < 0.01).ToList();
        var vertical = lines.Where(l => Math.Abs(l.X1 - l.X2) < 0.01).ToList();
        Assert.AreEqual(2, horizontal.Count);
        Assert.AreEqual(2, vertical.Count);

        var top = horizontal.OrderBy(l => l.Y1).First();
        var bottom = horizontal.OrderByDescending(l => l.Y1).First();
        var left = vertical.OrderBy(l => l.X1).First();
        var right = vertical.OrderByDescending(l => l.X1).First();

        Assert.AreEqual(RColor.FromArgb(1, 0, 0), top.Color);
        Assert.AreEqual(RColor.FromArgb(0, 1, 0), right.Color);
        Assert.AreEqual(RColor.FromArgb(0, 0, 1), bottom.Color);
        Assert.AreEqual(RColor.FromArgb(90, 90, 90), left.Color);
    }

    [Ignore("Gap: currentColor isn't supported in border-color")]
    [TestMethod]
    public void BorderColorPerSide_CurrentColor_ResolvesToElementColor()
    {
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:40px; height:40px; border-style:solid; border-width:4px; color: rgb(90,90,90); " +
            "border-left-color: currentcolor'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        Assert.AreEqual(RColor.FromArgb(90, 90, 90), div.ActualBorderLeftColor);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "box.html#border-style-properties")]
    public void BorderStyleRidge_IsMirrorImageOfGroove()
    {
        var (grooveRoot, grooveContainer) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='border-top-style: groove; border-top-width: 12px; border-top-color: rgb(51,51,51)'>x</div>"));
        var grooveDiv = PaintHarness.FindById(grooveRoot, "b")!;
        var grooveG = PaintHarness.PaintBox(grooveContainer, grooveDiv);
        var grooveLines = grooveG.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();

        var (ridgeRoot, ridgeContainer) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='border-top-style: ridge; border-top-width: 12px; border-top-color: rgb(51,51,51)'>x</div>"));
        var ridgeDiv = PaintHarness.FindById(ridgeRoot, "b")!;
        var ridgeG = PaintHarness.PaintBox(ridgeContainer, ridgeDiv);
        var ridgeLines = ridgeG.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();

        Assert.AreEqual(2, grooveLines.Count);
        Assert.AreEqual(2, ridgeLines.Count);

        Assert.AreEqual(grooveLines[0].Color, ridgeLines[1].Color);
        Assert.AreEqual(grooveLines[1].Color, ridgeLines[0].Color);
        Assert.AreNotEqual(grooveLines[0].Color, grooveLines[1].Color);
    }

    [TestMethod]
    public void BorderStyleDoubleWithRoundedCorners_FallsBackToASingleUnstripedStroke()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='border-top-style: double; border-top-width: 12px; border-top-color: rgb(51,51,51); border-radius: 8px'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;
        Assert.IsTrue(div.IsRounded);

        var g = PaintHarness.PaintBox(container, div);

        // RecordingGraphics doesn't log DrawPath, the absence of lines and polygons shows the rounded path was taken
        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawLineCall>().Any());
        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawPolygonCall>().Any());
    }

    [TestMethod]
    [Spec("css21", "box.html#border-style-properties")]
    public void BorderStyleTwoValueShorthand_OnlyPaintsTheSolidSides()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:40px; height:40px; border-width:4px; border-color:rgb(51,51,51); border-style: none solid'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        Assert.AreEqual("none", div.BorderTopStyle);
        Assert.AreEqual("solid", div.BorderRightStyle);
        Assert.AreEqual("none", div.BorderBottomStyle);
        Assert.AreEqual("solid", div.BorderLeftStyle);

        var g = PaintHarness.PaintBox(container, div);

        var lines = g.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();
        Assert.AreEqual(2, lines.Count);
        Assert.IsTrue(lines.All(l => Math.Abs(l.X1 - l.X2) < 0.01), "expected only vertical (left/right) border lines");
        Assert.IsTrue(lines.All(l => l.Color == RColor.FromArgb(51, 51, 51)));
    }

    [TestMethod]
    [Spec("css21", "box.html#border-color-properties")]
    public void BorderColorFourValueShorthand_ResolvesTopRightBottomLeftPerSide()
    {
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='border-style:solid; border-width:1px; border-color: rgb(1,0,0) rgb(0,1,0) rgb(0,0,1) rgb(1,1,0)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        Assert.AreEqual("rgb(1, 0, 0)", div.BorderTopColor);
        Assert.AreEqual("rgb(0, 1, 0)", div.BorderRightColor);
        Assert.AreEqual("rgb(0, 0, 1)", div.BorderBottomColor);
        Assert.AreEqual("rgb(1, 1, 0)", div.BorderLeftColor);
    }

    [TestMethod]
    [Spec("css21", "box.html#border-width-properties")]
    public void BorderWidthTwoValueShorthand_ThenLaterOneValue_OverridesAllSidesPerSpecificity()
    {
        var (root, _) = PaintHarness.Layout(
            "<html><head><style>#b { border-width: 0 2em; } #b { border-width: 1em; }</style></head>" +
            "<body style='margin:0'><div id='b' style='border-style:solid'></div></body></html>");
        var div = PaintHarness.FindById(root, "b")!;

        Assert.AreEqual("1em", div.BorderTopWidth);
        Assert.AreEqual("1em", div.BorderRightWidth);
        Assert.AreEqual("1em", div.BorderBottomWidth);
        Assert.AreEqual("1em", div.BorderLeftWidth);
    }

    [TestMethod]
    public void PresentationalBorderAttribute_OnAPlainElement_ForcesSolidOnAllSides()
    {
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap("<div id='b' border='1'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        Assert.AreEqual("solid", div.BorderTopStyle);
        Assert.AreEqual("solid", div.BorderRightStyle);
        Assert.AreEqual("solid", div.BorderBottomStyle);
        Assert.AreEqual("solid", div.BorderLeftStyle);
    }
}
