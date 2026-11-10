using System;
using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

[TestClass]
[DoNotParallelize]
public sealed class BoxDecorationBreakIntegrationTests
{
    private static readonly RColor BorderColor = RColor.FromArgb(255, 1, 2, 3);

    private static int VerticalEdges(RecordingGraphics recording) =>
        recording.Log.OfType<RecordingGraphics.DrawLineCall>()
            .Count(l => l.Color == BorderColor && Math.Abs(l.X1 - l.X2) < 0.01);

    private static int HorizontalEdges(RecordingGraphics recording) =>
        recording.Log.OfType<RecordingGraphics.DrawLineCall>()
            .Count(l => l.Color == BorderColor && Math.Abs(l.Y1 - l.Y2) < 0.01);

    [TestMethod]
    [Spec("css-break-3", "#break-decoration")]
    public void Slice_InlineBrokenAcrossLines_HasNoBorderAtTheBrokenEdges()
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div><span style='border:3px solid rgb(1,2,3)'>first<br>second<br>third</span></div>"));

        var recording = PaintHarness.PaintPage(container, 0);

        Assert.AreEqual(2, VerticalEdges(recording));
        Assert.AreEqual(6, HorizontalEdges(recording));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-break-3.yml")]
    [Spec("css-break-3", "#break-decoration")]
    public void Clone_InlineBrokenAcrossLines_HasBordersOnEveryFragment()
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div><span style='border:3px solid rgb(1,2,3);box-decoration-break:clone'>first<br>second<br>third</span></div>"));

        var recording = PaintHarness.PaintPage(container, 0);

        Assert.AreEqual(6, VerticalEdges(recording));
    }
}
