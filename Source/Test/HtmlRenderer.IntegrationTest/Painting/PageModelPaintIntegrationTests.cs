using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;

namespace HtmlRenderer.IntegrationTest.Painting;

[TestClass]
[DoNotParallelize]
public sealed class PageModelPaintIntegrationTests
{
    private const double PageHeight = 200;
    private const double Margin = 20;
    private static readonly RColor Canvas = RColor.FromArgb(255, 1, 2, 3);

    private static HtmlContainerInt Layout(string style, string body) =>
        PaintHarness.LayoutPaginated(
            $"<html><head><style>{style}</style></head><body style='margin:0'>{body}</body></html>",
            pageWidth: 400, pageHeight: PageHeight, margin: Margin).Container;

    private static RecordingGraphics.DrawRectCall[] CanvasRects(HtmlContainerInt container, int page) =>
        PaintHarness.PaintPage(container, page).Log.OfType<RecordingGraphics.DrawRectCall>().Where(r => r.Color == Canvas).ToArray();

    [TestMethod]
    [Spec("css-page-3", "#painting")]
    public void DocumentBackground_IsPaintedInThePageAreaOfEveryPage()
    {
        var container = Layout("html { background: rgb(1,2,3) }", "<div style='height:300px'></div>");

        Assert.AreEqual(2, container.FragmentTree!.Fragmentainers.Count);
        var first = CanvasRects(container, 0).Single();
        Assert.AreEqual(Margin, first.Y, 0.5);
        Assert.AreEqual(PageHeight, first.Height, 0.5);
        Assert.AreEqual(Margin, CanvasRects(container, 1).Single().Y, 0.5);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-page-3.yml")]
    [Spec("css-page-3", "#painting")]
    public void DocumentBackground_FillsTheWholePageAreaOfTheLastPage()
    {
        var container = Layout("html { background: rgb(1,2,3) }", "<div style='height:300px'></div>");

        Assert.AreEqual(PageHeight, CanvasRects(container, 1).Sum(r => r.Height), 0.5);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-page-3.yml")]
    [Spec("css-page-3", "#painting")]
    public void PageBackground_IsPainted()
    {
        var container = Layout("@page { background: rgb(1,2,3) }", "<div style='height:50px'></div>");

        Assert.IsTrue(CanvasRects(container, 0).Length > 0);
    }
}
