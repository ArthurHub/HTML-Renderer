using System.Globalization;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Fragmentation;

/// <summary>
/// CSS Fragmentation 3 behavior that the CSS 2.1 paged media tests don't already cover.
/// </summary>
[TestClass]
public sealed class CssBreakTests
{
    private const double Delta = 0.5;
    private const double PageHeight = 300;

    private static (CssBox Root, HtmlContainerInt Container) Paginate(string body, string css = "") =>
        LayoutHarness.Layout(
            $"<html><head><style>{css}</style></head><body style='margin:0'>{body}</body></html>",
            400, pageHeight: PageHeight, margin: 0);

    private static CssBox Find(CssBox root, string id) => LayoutHarness.FindById(root, id)!;

    private static int PageOf(HtmlContainerInt container, CssBox box) => container.PageIndexOf(box.Location.Y);

    private static string Px(double value) => value.ToString("0.###", CultureInfo.InvariantCulture) + "px";

    private static string Paragraph(string id, int lines, string style = "") =>
        $"<div id='{id}' style='{style}'>" + string.Join("<br>", Enumerable.Range(1, lines).Select(i => $"L{i}")) + "</div>";

    private static readonly Lazy<double> LineHeightValue = new(() =>
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(Paragraph("p", 1)));
        return Find(root, "p").Size.Height;
    });

    private static double LineHeight => LineHeightValue.Value;

    [TestMethod]
    [Spec("css-break-3", "#break-between")]
    public void ColumnBreak_DoesNotBreakThePage()
    {
        var (root, container) = Paginate("<div style='height:50px'></div><div id='b' style='break-before:column;height:10px'></div>");

        Assert.AreEqual(50, Find(root, "b").Location.Y, Delta);
        Assert.AreEqual(1, container.FragmentTree!.Fragmentainers.Count);
    }

    [TestMethod]
    [Spec("css-break-3", "#break-between")]
    [DataRow("avoid", true)]
    [DataRow("avoid-page", true)]
    [DataRow("avoid-column", false)]
    public void AvoidValues_KeepTheBoxWithTheNextOnlyForPageBreaks(string value, bool kept)
    {
        var (root, container) = Paginate(
            "<div style='height:200px'></div>"
            + $"<div id='h' style='break-after:{value}'>Head</div>"
            + Paragraph("p", 8, "break-inside:avoid"));

        Assert.AreEqual(1, PageOf(container, Find(root, "p")));
        Assert.AreEqual(kept ? 1 : 0, PageOf(container, Find(root, "h")));
    }

    [TestMethod]
    [Spec("css-break-3", "#break-within")]
    [DataRow("avoid", 1)]
    [DataRow("avoid-page", 1)]
    [DataRow("avoid-column", 0)]
    [DataRow("avoid-region", 0)]
    public void BreakInside_AvoidsOnlyPageBreaks(string value, int expectedPage)
    {
        var (root, container) = Paginate("<div style='height:250px'></div>" + Paragraph("p", 6, $"break-inside:{value}"));

        Assert.AreEqual(expectedPage, PageOf(container, Find(root, "p")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-break-3.yml")]
    [Spec("css-break-3", "#break-propagation")]
    public void BreakBeforeAvoidOnAFirstChild_AppliesBeforeItsContainer()
    {
        var (root, container) = Paginate(
            "<div style='height:200px'></div>"
            + "<div id='h'>Head</div>"
            + "<div id='w'>" + Paragraph("p", 8, "break-before:avoid;break-inside:avoid") + "</div>");

        Assert.AreEqual(1, PageOf(container, Find(root, "p")));
        Assert.AreEqual(1, PageOf(container, Find(root, "h")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-break-3.yml")]
    [Spec("css-break-3", "#possible-breaks")]
    public void ForcedBreakInsideAMonolithicBox_IsIgnored()
    {
        var (root, container) = Paginate(
            "<div style='overflow:auto;height:100px'><div style='height:20px'></div><div id='b' style='break-before:page;height:10px'></div></div>"
            + "<div id='after' style='height:10px'></div>");

        Assert.AreEqual(0, PageOf(container, Find(root, "b")));
        Assert.AreEqual(0, PageOf(container, Find(root, "after")));
    }

    [TestMethod]
    [Spec("css-break-3", "#fragmentation-model")]
    public void BoxSpanningTwoPages_HasOneFragmentPerPage()
    {
        var (root, container) = Paginate("<div style='height:250px'></div><div id='b' style='height:100px'></div>");

        var b = Find(root, "b");
        var fragments = container.FragmentTree!.Fragmentainers.Select(f => FindFragment(f.Root, b)).ToList();
        Assert.AreEqual(2, fragments.Count);
        Assert.AreEqual(50, fragments[0]!.Rect.Height, Delta);
        Assert.AreEqual(50, fragments[1]!.Rect.Height, Delta);
        Assert.AreEqual(0, fragments[1]!.Rect.Top, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-break-3.yml")]
    [Spec("css-break-3", "#forced-breaks")]
    [Spec("css-page-3", "#using-named-pages")]
    public void DifferentPageValues_ForceABreak()
    {
        var (root, container) = Paginate(
            "<div style='height:50px'></div><div id='b' style='page:chapter;height:10px'></div>");

        Assert.AreEqual(1, PageOf(container, Find(root, "b")));
    }

    [TestMethod]
    [Spec("css-break-3", "#break-between")]
    public void AbsolutelyPositionedBox_IgnoresBreakBefore()
    {
        var (root, container) = Paginate(
            "<div style='height:50px'></div><div id='b' style='position:absolute;top:60px;break-before:page;height:10px'></div>");

        Assert.AreEqual(0, PageOf(container, Find(root, "b")));
    }

    [TestMethod]
    [Spec("css-break-3", "#break-between")]
    public void Float_HonorsBreakBefore()
    {
        var (root, container) = Paginate(
            "<div style='height:50px'></div><div id='b' style='float:left;width:50px;break-before:page;height:10px'></div>");

        Assert.AreEqual(1, PageOf(container, Find(root, "b")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-break-3.yml")]
    [Spec("css-break-3", "#break-between")]
    [DataRow("recto")]
    [DataRow("verso")]
    public void RectoAndVerso_ForceABreak(string value)
    {
        var (root, container) = Paginate($"<div style='height:50px'></div><div id='b' style='break-before:{value};height:10px'></div>");

        Assert.IsTrue(PageOf(container, Find(root, "b")) >= 1);
    }

    [TestMethod]
    [Spec("css-break-3", "#page-break-properties")]
    public void BreakBefore_OverridesAnEarlierPageBreakBefore()
    {
        var (root, container) = Paginate(
            "<div style='height:50px'></div><div id='b' class='x' style='height:10px'></div>",
            "div { page-break-before: always } .x { break-before: auto }");

        Assert.AreEqual(0, PageOf(container, Find(root, "b")));
    }

    [TestMethod]
    [Spec("css-break-3", "#page-break-properties")]
    public void PageBreakBefore_OverridesAnEarlierBreakBefore()
    {
        var (root, container) = Paginate(
            "<div style='height:50px'></div><div id='b' class='x' style='height:10px'></div>",
            "div { break-before: page } .x { page-break-before: auto }");

        Assert.AreEqual(0, PageOf(container, Find(root, "b")));
    }

    [TestMethod]
    [Spec("css-break-3", "#page-break-properties")]
    [DataRow("page-break-before:always")]
    [DataRow("break-before:page")]
    public void PageBreakAlwaysAndBreakPage_ForceTheSameBreak(string declaration)
    {
        var (root, container) = Paginate($"<div style='height:50px'></div><div id='b' style='{declaration};height:10px'></div>");

        Assert.AreEqual(container.PageTopOf(1), Find(root, "b").Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css-break-3", "#forced-breaks")]
    public void ForcedBreak_HasNoEffectWithoutPages()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='height:50px'></div><div id='b' style='break-before:page;height:10px'></div>"));

        Assert.AreEqual(50, Find(root, "b").Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css-break-3", "#parallel-flows")]
    public void ForcedBreakInsideAFloat_DoesNotMoveContentBesideIt()
    {
        var (root, container) = Paginate(
            "<div style='float:left;width:100px'><div style='height:50px'></div><div id='f' style='break-before:page;height:10px'></div></div>"
            + "<div id='o' style='height:10px'>o</div>");

        Assert.AreEqual(0, PageOf(container, Find(root, "o")));
    }

    [TestMethod]
    [Spec("css-break-3", "#parallel-flows")]
    public void TableCellsInOneRow_BreakIndependently()
    {
        var filler = PageHeight - 1.5 * LineHeight;
        var (root, container) = Paginate(
            $"<div style='height:{Px(filler)}'></div>"
            + "<table cellspacing='0' cellpadding='0'><tr><td id='a'>" + string.Join("<br>", Enumerable.Range(1, 4).Select(i => $"A{i}"))
            + "</td><td id='b'>B1</td></tr></table>");

        var a = Find(root, "a");
        foreach (var line in a.LineBoxes)
            Assert.AreEqual(container.PageIndexOf(line.LineTop), container.PageIndexOf(line.LineBottom - 0.01));
    }

    [TestMethod]
    [Spec("css-break-3", "#best-breaks")]
    public void ZeroHeightBoxAtTheEndOfAFullPage_StaysOnThatPage()
    {
        var (root, container) = Paginate($"<div style='height:{Px(PageHeight - 1)}'></div><div id='z'></div>");

        Assert.AreEqual(0, PageOf(container, Find(root, "z")));
        Assert.AreEqual(1, container.FragmentTree!.Fragmentainers.Count);
    }

    [TestMethod]
    [Spec("css-break-3", "#box-splitting")]
    public void SplitBox_FillsTheRestOfThePage()
    {
        var filler = PageHeight - 1.5 * LineHeight;
        var (root, container) = Paginate($"<div style='height:{Px(filler)}'></div>" + Paragraph("p", 4, "orphans:1;widows:1;background:red"));

        var page0 = container.FragmentTree!.Fragmentainers[0];
        var p = Find(root, "p");
        var fragment = FindFragment(page0.Root, p);
        Assert.IsNotNull(fragment);
        Assert.AreEqual(page0.Rect.Height, fragment.Rect.Bottom, Delta);
    }

    [TestMethod]
    [Spec("css-break-3", "#box-splitting")]
    public void BreakGap_CountsTowardsAFixedHeight()
    {
        var filler = PageHeight - 1.5 * LineHeight;
        var (root, _) = Paginate($"<div style='height:{Px(filler)}'></div>" + Paragraph("p", 3, "orphans:1;widows:1;height:100px"));

        var p = Find(root, "p");
        Assert.AreEqual(filler + 100, p.ActualBottom, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-break-3.yml")]
    [Spec("css-break-3", "#break-decoration")]
    public void Clone_RepeatsTheBorderAndPaddingAfterTheBreak()
    {
        var filler = PageHeight - 1.5 * LineHeight;
        var (root, container) = Paginate(
            $"<div style='height:{Px(filler - 10)}'></div>"
            + Paragraph("p", 4, "orphans:1;widows:1;border-top:5px solid black;padding-top:5px;box-decoration-break:clone"));

        var p = Find(root, "p");
        var firstOnPage1 = p.LineBoxes.First(l => container.PageIndexOf(l.LineTop) == 1);
        Assert.AreEqual(container.PageTopOf(1) + 10, firstOnPage1.LineTop, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-break-3.yml")]
    [Spec("css-break-3", "#transforms")]
    public void RelativeOffset_IsAppliedAfterFragmentation()
    {
        var filler = PageHeight - 1.5 * LineHeight;
        var (root, container) = Paginate(
            $"<div style='height:{Px(filler)}'></div>" + Paragraph("p", 4, "orphans:1;widows:1;position:relative;top:20px"));

        var p = Find(root, "p");
        Assert.AreEqual(filler + 20, p.LineBoxes[0].LineTop, Delta);
        Assert.AreEqual(container.PageTopOf(1) + 20, p.LineBoxes[1].LineTop, Delta);
    }

    [TestMethod]
    [Spec("css-break-3", "#transforms")]
    public void AbsoluteBoxSpanningABreak_IsSplitAcrossBothPages()
    {
        var (root, container) = Paginate(
            "<div style='height:10px'></div><div id='a' style='position:absolute;top:250px;height:100px;width:50px;background:red'></div>"
            + "<div style='height:500px'></div>");

        var a = Find(root, "a");
        var fragments = container.FragmentTree!.Fragmentainers.Select(f => FindFragment(f.Root, a)).ToList();
        Assert.AreEqual(50, fragments[0]!.Rect.Height, Delta);
        Assert.AreEqual(50, fragments[1]!.Rect.Height, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-break-3.yml")]
    [Spec("css-break-3", "#transforms")]
    public void AbsoluteBoxReachingPastTheLastPage_GetsAPageOfItsOwn()
    {
        var (_, container) = Paginate(
            "<div style='height:10px'></div><div style='position:absolute;top:250px;height:100px;width:50px;background:red'></div>");

        Assert.AreEqual(2, container.FragmentTree!.Fragmentainers.Count);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-break-3.yml")]
    [Spec("css-break-3", "#parallel-flows")]
    public void ContentOverflowingAFixedHeightBox_SpillsOntoMorePages()
    {
        var (_, container) = Paginate(Paragraph("p", 60, "height:50px"));

        Assert.IsTrue(container.FragmentTree!.Fragmentainers.Count >= 2);
    }

    private static TheArtOfDev.HtmlRenderer.Core.Fragments.BoxFragment? FindFragment(
        TheArtOfDev.HtmlRenderer.Core.Fragments.BoxFragment fragment, CssBox box)
    {
        if (ReferenceEquals(fragment.Box, box))
            return fragment;
        foreach (var child in fragment.Children)
        {
            var found = FindFragment(child, box);
            if (found != null)
                return found;
        }
        return null;
    }
}
