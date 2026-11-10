using System.Globalization;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Fragmentation;

[DoNotParallelize]
[TestClass]
public sealed class PagedMediaIntegrationTests
{
    private const double Delta = 0.5;
    private const double PageWidth = 400;
    private const double PageHeight = 300;

    private const string OnePixelPng =
        "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";

    private static (CssBox Root, HtmlContainerInt Container) Paginate(string body, double pageHeight = PageHeight, double margin = 0) =>
        LayoutHarness.Layout(LayoutHarness.Wrap(body), PageWidth, pageHeight, margin: margin);

    private static CssBox Find(CssBox root, string id) => LayoutHarness.FindById(root, id)!;

    private static int PageOf(HtmlContainerInt container, CssBox box) => container.PageIndexOf(box.Location.Y);

    private static int LastPageOf(HtmlContainerInt container, CssBox box) =>
        container.PageIndexOf(Math.Max(box.Location.Y, box.ActualBottom - 0.01));

    /// <summary>One line per &lt;br&gt;, each <see cref="LineHeight"/> tall.</summary>
    private static string Paragraph(string id, int lines, string style = "") =>
        $"<div id='{id}' style='font-size:12px;{style}'>"
        + string.Join("<br>", Enumerable.Range(1, lines).Select(i => $"L{i}"))
        + "</div>";

    private static readonly Lazy<double> LineHeightValue = new(() =>
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(Paragraph("p", 1)));
        return Find(root, "p").Size.Height;
    });

    private static double LineHeight => LineHeightValue.Value;

    private static string Px(double value) => value.ToString("0.###", CultureInfo.InvariantCulture) + "px";

    private static int[] LinesPerPage(HtmlContainerInt container, CssBox block, int pages)
    {
        var perPage = new int[pages];
        foreach (var line in block.LineBoxes)
            perPage[container.PageIndexOf(line.LineTop)]++;
        return perPage;
    }

    #region 13.2 Page boxes

    [TestMethod]
    [Spec("css21", "page.html#page-box")]
    public void PageArea_IsTheInitialContainingBlock()
    {
        var (root, _) = Paginate("<div id='d' style='width:50%;height:10px'></div>", margin: 20);
        var d = Find(root, "d");

        Assert.AreEqual(20, d.Location.X, Delta);
        Assert.AreEqual(20, d.Location.Y, Delta);
        Assert.AreEqual(PageWidth / 2, d.Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css21", "page.html#page-box")]
    public void EachPageArea_StartsBelowItsPageMargin()
    {
        var (root, container) = Paginate(
            "<div style='height:100px'></div><div id='b' style='page-break-before:always;height:10px'></div>", margin: 20);
        var b = Find(root, "b");

        Assert.AreEqual(20 + PageHeight, b.Location.Y, Delta);
        Assert.AreEqual(2, container.FragmentTree!.Fragmentainers.Count);
    }

    #endregion

    #region 13.2.3 Content outside the page box

    [TestMethod]
    [Spec("css21", "page.html#outside-page-box")]
    public void FarAwayAbsoluteBox_DoesNotProduceBlankPages()
    {
        var (_, container) = Paginate(
            "<div style='height:10px'></div><div style='position:absolute;top:30000px;height:10px'>far</div>");

        Assert.IsTrue(container.FragmentTree!.Fragmentainers.Count <= 2,
            $"{container.FragmentTree.Fragmentainers.Count} pages for two small boxes");
    }

    #endregion

    #region 13.3.1 Page break properties

    [TestMethod]
    [Spec("css21", "page.html#page-break-props")]
    [Spec("css-break-3", "#page-break-properties")]
    public void PageBreakBeforeAlways_StartsTheBoxOnANewPage()
    {
        var (root, container) = Paginate(
            "<div style='height:50px'></div><div id='b' style='page-break-before:always;height:10px'></div>");

        Assert.AreEqual(container.PageTopOf(1), Find(root, "b").Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "page.html#page-break-props")]
    [Spec("css-break-3", "#page-break-properties")]
    public void PageBreakAfterAlways_StartsTheNextBoxOnANewPage()
    {
        var (root, container) = Paginate(
            "<div style='height:50px;page-break-after:always'></div><div id='b' style='height:10px'></div>");

        Assert.AreEqual(container.PageTopOf(1), Find(root, "b").Location.Y, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "page.html#page-break-props")]
    [Spec("css-break-3", "#break-between")]
    [DataRow("<div style='height:50px'></div><div id='b' style='page-break-before:left;height:10px'></div>")]
    [DataRow("<div style='height:50px'></div><div id='b' style='page-break-before:right;height:10px'></div>")]
    [DataRow("<div style='height:50px;page-break-after:left'></div><div id='b' style='height:10px'></div>")]
    [DataRow("<div style='height:50px;page-break-after:right'></div><div id='b' style='height:10px'></div>")]
    public void LeftAndRight_ForceABreak(string body)
    {
        var (root, container) = Paginate(body);

        Assert.IsTrue(PageOf(container, Find(root, "b")) >= 1);
    }

    [TestMethod]
    [Spec("css21", "page.html#page-break-props")]
    [Spec("css-break-3", "#forced-breaks")]
    [DataRow("<div style='height:50px;page-break-after:avoid'></div><div id='b' style='page-break-before:always;height:10px'></div>")]
    [DataRow("<div style='height:50px;page-break-after:always'></div><div id='b' style='page-break-before:avoid;height:10px'></div>")]
    public void Always_TakesPrecedenceOverAvoid(string body)
    {
        var (root, container) = Paginate(body);

        Assert.AreEqual(container.PageTopOf(1), Find(root, "b").Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "page.html#page-break-props")]
    [Spec("css-break-3", "#break-within")]
    public void PageBreakInsideAvoid_KeepsTheBoxOnOnePage()
    {
        var (root, container) = Paginate(
            "<div style='height:250px'></div>" + Paragraph("p", 4, "page-break-inside:avoid"));
        var p = Find(root, "p");

        Assert.AreEqual(1, PageOf(container, p));
        Assert.AreEqual(1, LastPageOf(container, p));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "page.html#page-break-props")]
    [Spec("css-break-3", "#break-within")]
    public void PageBreakInsideAvoid_KeepsABoxWithoutTextOnOnePage()
    {
        var (root, container) = Paginate(
            "<div style='height:250px'></div><div id='b' style='height:80px;page-break-inside:avoid'></div>");

        Assert.AreEqual(container.PageTopOf(1), Find(root, "b").Location.Y, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "page.html#page-break-props")]
    [Spec("css-break-3", "#break-decoration")]
    public void ASplitBox_HasNoBorderWhereItIsSplit()
    {
        var (_, container) = PaintHarness.LayoutPaginated(PaintHarness.Wrap(
            "<div style='height:250px'></div><div style='border:5px solid rgb(1,2,3);height:100px'></div>"),
            PageWidth, PageHeight);
        var color = RColor.FromArgb(255, 1, 2, 3);

        for (var page = 0; page < 2; page++)
        {
            var horizontalEdges = PaintHarness.PaintPage(container, page).Log
                .OfType<RecordingGraphics.DrawLineCall>()
                .Count(l => l.Color == color && Math.Abs(l.Y1 - l.Y2) < 0.01);

            Assert.AreEqual(1, horizontalEdges, $"page {page}");
        }
    }

    #endregion

    #region 13.3.2 Breaks inside elements: 'orphans', 'widows'

    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    [Spec("css-break-3", "#widows-orphans")]
    public void OrphansAndWidows_DefaultToTwoAndInherit()
    {
        var (root, _) = Paginate("<div style='orphans:5;widows:4'><p id='c'>c</p></div><p id='d'>d</p>");

        Assert.AreEqual(5, Find(root, "c").ActualOrphans);
        Assert.AreEqual(4, Find(root, "c").ActualWidows);
        Assert.AreEqual(2, Find(root, "d").ActualOrphans);
        Assert.AreEqual(2, Find(root, "d").ActualWidows);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "page.html#break-inside")]
    [Spec("css-break-3", "#widows-orphans")]
    [DataRow("orphans:0;widows:0")]
    [DataRow("orphans:-3;widows:-3")]
    public void NonPositiveOrphansAndWidows_AreIgnored(string declaration)
    {
        var (root, _) = Paginate($"<div style='orphans:5;widows:4'><p id='c' style='{declaration}'>c</p></div>");

        Assert.AreEqual(5, Find(root, "c").ActualOrphans);
        Assert.AreEqual(4, Find(root, "c").ActualWidows);
    }

    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    [Spec("css-break-3", "#widows-orphans")]
    public void Orphans_MovesTheParagraphWhenTooFewLinesFit()
    {
        var filler = PageHeight - 2.5 * LineHeight;
        var (root, container) = Paginate($"<div style='height:{Px(filler)}'></div>" + Paragraph("p", 6, "orphans:3"));

        CollectionAssert.AreEqual(new[] { 0, 6 }, LinesPerPage(container, Find(root, "p"), 2));
    }

    [TestMethod]
    [Spec("css21", "page.html#break-inside")]
    [Spec("css-break-3", "#widows-orphans")]
    public void Widows_PullsLinesToTheNextPage()
    {
        var filler = PageHeight - 5.5 * LineHeight;
        var (root, container) = Paginate($"<div style='height:{Px(filler)}'></div>" + Paragraph("p", 6, "widows:3"));

        CollectionAssert.AreEqual(new[] { 3, 3 }, LinesPerPage(container, Find(root, "p"), 2));
    }

    #endregion

    #region 13.3.3 Allowed page breaks

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    [Spec("css-break-3", "#break-margins")]
    public void UnforcedBreakInTheMargin_SetsTheMarginToZero()
    {
        var (root, container) = Paginate(
            "<div style='height:250px;margin-bottom:10px'></div>"
            + "<div id='b' style='margin-top:30px;height:80px;page-break-inside:avoid'>text</div>");

        Assert.AreEqual(container.PageTopOf(1), Find(root, "b").Location.Y, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "page.html#allowed-page-breaks")]
    [Spec("css-break-3", "#possible-breaks")]
    public void ABlockWhoseLinesAllMoveToTheNextPage_MovesWithThem()
    {
        var filler = PageHeight - 1.5 * LineHeight;
        var (root, container) = Paginate(
            $"<div style='height:{Px(filler)}'></div>" + Paragraph("p", 4, "border-top:3px solid black;orphans:3"));

        Assert.AreEqual(container.PageTopOf(1), Find(root, "p").Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    [Spec("css-break-3", "#possible-breaks")]
    public void Breaks_FallBetweenLineBoxes()
    {
        var (root, container) = Paginate("<div style='height:5px'></div>" + Paragraph("p", 40));
        var p = Find(root, "p");

        foreach (var line in p.LineBoxes)
            Assert.AreEqual(container.PageIndexOf(line.LineTop), container.PageIndexOf(line.LineBottom - 0.01));
        Assert.IsTrue(LastPageOf(container, p) >= 2);
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    [Spec("css-break-3", "#unforced-breaks")]
    [DataRow("page-break-after:avoid", "")]
    [DataRow("", "page-break-before:avoid")]
    public void AvoidBetweenSiblings_MovesTheFirstAlongWithTheSecond(string headStyle, string nextStyle)
    {
        var (root, container) = Paginate(
            "<div style='height:200px'></div>"
            + $"<div id='h' style='{headStyle}'>Head</div>"
            + Paragraph("p", 8, "page-break-inside:avoid;" + nextStyle));

        Assert.AreEqual(1, PageOf(container, Find(root, "p")));
        Assert.AreEqual(container.PageTopOf(1), Find(root, "h").Location.Y, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "page.html#allowed-page-breaks")]
    [Spec("css-break-3", "#unforced-breaks")]
    public void AvoidBetweenSiblings_AlsoMovesABlockWithoutText()
    {
        var (root, container) = Paginate(
            "<div style='height:200px'></div>"
            + "<div id='h' style='height:30px;page-break-after:avoid'></div>"
            + Paragraph("p", 8, "page-break-inside:avoid"));

        Assert.AreEqual(container.PageTopOf(1), Find(root, "h").Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    [Spec("css-break-3", "#unforced-breaks")]
    public void PageBreakInsideAvoidOnAnAncestor_ForbidsBreaksBetweenItsChildren()
    {
        var (root, container) = Paginate(
            "<div style='height:250px'></div>"
            + "<div id='w' style='page-break-inside:avoid'><div style='height:30px'></div><div style='height:30px'></div></div>");
        var w = Find(root, "w");

        Assert.AreEqual(container.PageTopOf(1), w.Location.Y, Delta);
        Assert.AreEqual(1, LastPageOf(container, w));
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    [Spec("css-break-3", "#unforced-breaks")]
    public void AvoidBoxTallerThanAPage_IsStillBroken()
    {
        var (root, container) = Paginate("<div style='height:100px'></div>" + Paragraph("p", 25, "page-break-inside:avoid"));
        var p = Find(root, "p");

        Assert.AreEqual(0, PageOf(container, p));
        Assert.IsTrue(LastPageOf(container, p) >= 1);
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    [Spec("css-break-3", "#unforced-breaks")]
    public void OrphansAndWidowsThatCannotBeSatisfied_AreDropped()
    {
        // Fewer than 20 lines fit on a page, so no split of 25 lines satisfies both.
        var (root, container) = Paginate(Paragraph("p", 25, "orphans:20;widows:20"));
        var p = Find(root, "p");

        Assert.IsTrue(LastPageOf(container, p) >= 1);
        foreach (var line in p.LineBoxes)
            Assert.AreEqual(container.PageIndexOf(line.LineTop), container.PageIndexOf(line.LineBottom - 0.01));
    }

    [TestMethod]
    [Spec("css21", "page.html#allowed-page-breaks")]
    [Spec("css-break-3", "#break-margins")]
    public void ForcedBreak_RetainsTheMarginTop()
    {
        var (root, container) = Paginate(
            "<div style='height:50px'></div><div id='b' style='page-break-before:always;margin-top:20px;height:10px'></div>");

        Assert.AreEqual(container.PageTopOf(1) + 20, Find(root, "b").Location.Y, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "page.html#allowed-page-breaks")]
    [Spec("css-break-3", "#break-margins")]
    public void ForcedBreak_SetsTheMarginBottomToZero()
    {
        var (root, container) = Paginate(
            "<div style='height:50px;margin-bottom:40px'></div><div id='b' style='page-break-before:always;margin-top:20px;height:10px'></div>");

        Assert.IsTrue(Find(root, "b").Location.Y <= container.PageTopOf(1) + 20 + Delta);
    }

    #endregion

    #region 13.3.4 Forced page breaks

    [TestMethod]
    [Spec("css21", "page.html#forced")]
    [Spec("css-break-3", "#forced-breaks")]
    public void ForcedBreak_OccursEvenWhereTheContentWouldFit()
    {
        var (root, container) = Paginate(
            "<div style='height:10px'></div><div id='b' style='page-break-before:always;height:10px'></div>"
            + "<div id='c' style='height:10px;page-break-before:always'></div>");

        Assert.AreEqual(1, PageOf(container, Find(root, "b")));
        Assert.AreEqual(2, PageOf(container, Find(root, "c")));
    }

    #endregion

    #region 13.3.5 "Best" page breaks

    [TestMethod]
    [Spec("css21", "page.html#best-page-breaks")]
    [Spec("css-break-3", "#possible-breaks")]
    public void ReplacedElement_IsNotBroken()
    {
        var (root, container) = Paginate(
            $"<div style='height:250px'></div><p style='margin:0'><img src='{OnePixelPng}' style='width:20px;height:100px'></p>");

        var image = LayoutHarness.Descendants(root).SelectMany(b => b.Words).Single(w => w.IsImage);

        Assert.AreEqual(container.PageTopOf(1), image.Top, Delta);
    }

    [TestMethod]
    [Spec("css21", "page.html#best-page-breaks")]
    [Spec("css-break-3", "#best-breaks")]
    [DataRow(20, new[] { 20, 0 })]
    [DataRow(21, new[] { 19, 2 })]
    [DataRow(22, new[] { 20, 2 })]
    [DataRow(25, new[] { 20, 5 })]
    public void Example_Orphans4Widows2With20LinesAvailable(int lines, int[] expected)
    {
        var pageHeight = 20 * LineHeight + 1;
        var (root, container) = Paginate(Paragraph("p", lines, "orphans:4;widows:2"), pageHeight);

        CollectionAssert.AreEqual(expected, LinesPerPage(container, Find(root, "p"), 2));
    }

    [TestMethod]
    [Spec("css21", "page.html#best-page-breaks")]
    [Spec("css-break-3", "#best-breaks")]
    [DataRow(8, new[] { 8, 0, 0 })]
    [DataRow(9, new[] { 0, 9, 0 })]
    [DataRow(30, new[] { 0, 10, 20 })]
    public void Example_Orphans10Widows20With8LinesAvailable(int lines, int[] expected)
    {
        var pageHeight = 20 * LineHeight + 1;
        var filler = pageHeight - 8 * LineHeight - 0.5;
        var (root, container) = Paginate(
            $"<div style='height:{Px(filler)}'></div>" + Paragraph("p", lines, "orphans:10;widows:20"), pageHeight);

        CollectionAssert.AreEqual(expected, LinesPerPage(container, Find(root, "p"), 3));
    }

    #endregion
}
