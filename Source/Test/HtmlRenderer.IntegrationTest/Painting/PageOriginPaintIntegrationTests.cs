using System;
using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;

namespace HtmlRenderer.IntegrationTest.Painting;

/// <summary>
/// Where fragment content lands on the painted surface: a PDF page keeps its top margin, and the continuous
/// (non-paged) surface paints everything even when the container doesn't start at Y=0.
/// </summary>
[TestClass]
public sealed class PageOriginPaintIntegrationTests
{
    [TestMethod]
    public void EveryPage_KeepsTheTopMargin()
    {
        const double margin = 20;
        var (_, container) = PaintHarness.LayoutPaginated(PaintHarness.Wrap(
            "<p style='margin:0;height:150px'>PageOneMarker</p>"
            + "<p style='margin:0;height:150px;page-break-before:always'>PageTwoMarker</p>"),
            pageHeight: 200, margin: margin);

        Assert.AreEqual(2, container.FragmentTree!.Fragmentainers.Count);

        var first = PaintHarness.PaintPage(container, 0).DrawStringCalls.Single(c => c.Text.Contains("PageOneMarker"));
        var second = PaintHarness.PaintPage(container, 1).DrawStringCalls.Single(c => c.Text.Contains("PageTwoMarker"));

        Assert.IsGreaterThanOrEqualTo(margin - 0.001, first.Point.Y, "first page content starts above the top margin");
        Assert.AreEqual(first.Point.Y, second.Point.Y, 0.001, "both pages start their content at the same Y");
    }

    [TestMethod]
    public void ContinuousSurface_WithAnOffset_StillPaintsTheBottomOfTheDocument()
    {
        var container = new HtmlContainerInt(new MockAdapter())
        {
            MaxSize = new RSize(400, 0),
            Location = new RPoint(0, 300),
            PageSize = new RSize(99999, 99999)
        };
        container.SetHtml(PaintHarness.Wrap(
            "<div style='margin:0;height:400px'>top</div><p style='margin:0'>BottomMarker</p>"));

        using var layout = new RecordingGraphics();
        container.PerformLayout(layout);
        Assert.IsFalse(container.HasRealPageGrid);

        var g = new RecordingGraphics();
        container.PerformPaint(g);

        Assert.IsTrue(g.DrawStringCalls.Any(c => c.Text.Contains("BottomMarker")));
    }

    [TestMethod]
    public void Clear_DropsTheFragmentTree()
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap("<p>x</p>"));
        Assert.IsNotNull(container.FragmentTree);

        container.Clear();

        Assert.IsNull(container.FragmentTree);
    }
}
