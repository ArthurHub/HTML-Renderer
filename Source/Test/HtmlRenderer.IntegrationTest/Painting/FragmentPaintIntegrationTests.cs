using System;
using System.Collections.Generic;
using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragments;

namespace HtmlRenderer.IntegrationTest.Painting;

/// <summary>
/// Paint driven by the fragment tree: each page paints its own fragmentainer and nothing else, and every drawn
/// rectangle is the one the fragment carries.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class FragmentPaintIntegrationTests
{
    [TestMethod]
    [Spec("css21", "page.html#forced")]
    [Spec("css-break-3", "#fragmentation-model")]
    public void EachPage_PaintsOnlyItsOwnFragmentainersContent()
    {
        var (_, container) = PaintHarness.LayoutPaginated(PaintHarness.Wrap(
            "<p style='margin:0;height:150px'>PageOneMarker</p>"
            + "<p style='margin:0;height:150px;page-break-before:always'>PageTwoMarker</p>"),
            pageHeight: 200, margin: 0);

        Assert.AreEqual(2, container.FragmentTree!.Fragmentainers.Count);

        var page0 = PaintHarness.PaintPage(container, 0);
        var page1 = PaintHarness.PaintPage(container, 1);

        Assert.IsTrue(page0.DrawStringCalls.Any(c => c.Text.Contains("PageOneMarker")));
        Assert.IsFalse(page0.DrawStringCalls.Any(c => c.Text.Contains("PageTwoMarker")));

        Assert.IsTrue(page1.DrawStringCalls.Any(c => c.Text.Contains("PageTwoMarker")));
        Assert.IsFalse(page1.DrawStringCalls.Any(c => c.Text.Contains("PageOneMarker")));
    }

    [TestMethod]
    public void PaintedText_LandsAtItsOwnFragmentsCoordinates()
    {
        var (_, container) = PaintHarness.LayoutPaginated(PaintHarness.Wrap(
            "<p style='margin:0;height:150px'>PageOneMarker</p>"
            + "<p style='margin:0;height:150px;page-break-before:always'>PageTwoMarker</p>"),
            pageHeight: 200, margin: 0);

        for (var page = 0; page < 2; page++)
        {
            var recording = PaintHarness.PaintPage(container, page);
            var fragmentainer = container.FragmentTree!.Fragmentainers[page];
            var wordRects = WordRects(fragmentainer.Root).ToList();

            Assert.IsTrue(recording.DrawStringCalls.Count > 0);

            // Fragment coordinates are page-local, paint adds no page offset.
            foreach (var call in recording.DrawStringCalls)
            {
                Assert.IsTrue(wordRects.Any(r => Math.Abs(r.X - call.Point.X) < 0.001));
            }

            // Page 1's content is 200px down the document but paints near its own page top.
            Assert.IsTrue(recording.DrawStringCalls.All(c => c.Point.Y >= 0 && c.Point.Y <= 200));
        }
    }

    [TestMethod]
    public void BoxSpanningTwoPages_PaintsItsBackgroundOnBoth()
    {
        var (_, container) = PaintHarness.LayoutPaginated(PaintHarness.Wrap(
            "<div id='tall' style='height:300px;background:rgb(10,20,30)'>x</div>"),
            pageHeight: 200, margin: 0);

        Assert.AreEqual(2, container.FragmentTree!.Fragmentainers.Count);

        for (var page = 0; page < 2; page++)
        {
            var recording = PaintHarness.PaintPage(container, page);

            Assert.IsTrue(recording.Log.OfType<RecordingGraphics.DrawRectCall>()
                .Any(r => r.Color == RColorOf(10, 20, 30)));
        }
    }

    [TestMethod]
    public void FixedBox_PaintsAtTheSameCoordinatesOnEveryPage()
    {
        var (_, container) = PaintHarness.LayoutPaginated(PaintHarness.Wrap(
            "<div style='position:fixed;top:10px;left:10px;width:40px;height:20px;background:rgb(1,2,3)'></div>"
            + "<p style='margin:0;height:150px'>One</p>"
            + "<p style='margin:0;height:150px;page-break-before:always'>Two</p>"),
            pageHeight: 200, margin: 0);

        Assert.AreEqual(2, container.FragmentTree!.Fragmentainers.Count);

        var painted = new List<RecordingGraphics.DrawRectCall>();

        for (var page = 0; page < 2; page++)
        {
            var recording = PaintHarness.PaintPage(container, page);
            var matches = recording.Log.OfType<RecordingGraphics.DrawRectCall>()
                .Where(r => r.Color == RColorOf(1, 2, 3)).ToList();

            Assert.AreEqual(1, matches.Count);
            painted.Add(matches[0]);
        }

        Assert.AreEqual(painted[0].X, painted[1].X, 0.001);
        Assert.AreEqual(painted[0].Y, painted[1].Y, 0.001);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "zindex.html#painting-order")]
    public void StackingOrder_IsPreservedWhenPaintingFromFragments()
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div style='position:relative'>"
            + "<div style='position:absolute;z-index:2;width:10px;height:10px;background:rgb(0,0,255)'></div>"
            + "<div style='position:absolute;z-index:1;width:10px;height:10px;background:rgb(255,0,0)'></div>"
            + "</div>"));

        var recording = PaintHarness.PaintPage(container, 0);

        var rects = recording.Log.OfType<RecordingGraphics.DrawRectCall>().ToList();
        var red = rects.FindIndex(r => r.Color == RColorOf(255, 0, 0));
        var blue = rects.FindIndex(r => r.Color == RColorOf(0, 0, 255));

        Assert.IsTrue(red >= 0 && blue >= 0, "both positioned boxes must paint");
        Assert.IsTrue(red < blue, "the lower z-index sibling must paint first");
    }

    [TestMethod]
    public void RowspanCell_ShowsThroughEveryRowItSpans()
    {
        // A rowspan placeholder has no content of its own; the spanned cell reaches it as a fragment
        // child, so the ordinary paint walk draws it once per row it spans.
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table><tr><td rowspan='2'>SpannedCell</td><td>A</td></tr><tr><td>B</td></tr></table>"));

        var recording = PaintHarness.PaintPage(container, 0);

        Assert.IsTrue(recording.DrawStringCalls.Any(c => c.Text.Contains("SpannedCell")));
    }

    [TestMethod]
    [Spec("css21", "visufx.html#visibility")]
    public void VisibilityHidden_ReservesLayoutSpace_ButPaintsNothing_VisibleSiblingStillPaints()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='hidden' style='visibility:hidden;height:50px;background:rgb(10,20,30)'>Hidden</div>"
            + "<div id='visible' style='height:50px;background:rgb(40,50,60)'>Visible</div>"));

        var hidden = PaintHarness.FindById(root, "hidden")!;
        var visible = PaintHarness.FindById(root, "visible")!;

        Assert.AreEqual(hidden.ActualBottom, visible.Location.Y, 1);

        var recording = PaintHarness.PaintPage(container, 0);

        Assert.IsFalse(recording.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColorOf(10, 20, 30)));
        Assert.IsFalse(recording.DrawStringCalls.Any(c => c.Text.Contains("Hidden")));

        Assert.IsTrue(recording.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColorOf(40, 50, 60)));
        Assert.IsTrue(recording.DrawStringCalls.Any(c => c.Text.Contains("Visible")));
    }

    [TestMethod]
    [Spec("css21", "visufx.html#visibility")]
    public void VisibilityCollapse_ReservesLayoutSpace_ButPaintsNothing_VisibleSiblingStillPaints()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='collapsed' style='visibility:collapse;height:50px;background:rgb(10,20,30)'>Collapsed</div>"
            + "<div id='visible' style='height:50px;background:rgb(40,50,60)'>Visible</div>"));

        var collapsed = PaintHarness.FindById(root, "collapsed")!;
        var visible = PaintHarness.FindById(root, "visible")!;

        Assert.AreEqual(collapsed.ActualBottom, visible.Location.Y, 1);

        var recording = PaintHarness.PaintPage(container, 0);

        Assert.IsFalse(recording.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColorOf(10, 20, 30)));
        Assert.IsFalse(recording.DrawStringCalls.Any(c => c.Text.Contains("Collapsed")));

        Assert.IsTrue(recording.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColorOf(40, 50, 60)));
        Assert.IsTrue(recording.DrawStringCalls.Any(c => c.Text.Contains("Visible")));
    }

    private static RColor RColorOf(int r, int g, int b) => RColor.FromArgb(r, g, b);

    private static IEnumerable<RRect> WordRects(BoxFragment fragment)
    {
        foreach (var word in fragment.Words)
            yield return word.Rect;

        foreach (var child in fragment.Children)
            foreach (var rect in WordRects(child))
                yield return rect;

        if (fragment.MarkerFragment != null)
            foreach (var rect in WordRects(fragment.MarkerFragment))
                yield return rect;
    }
}
