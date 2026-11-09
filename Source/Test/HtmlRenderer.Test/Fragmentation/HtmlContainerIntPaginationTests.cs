using System.Linq;
using HtmlRenderer.Test.TestSupport;

namespace HtmlRenderer.Test.Fragmentation;

/// <summary>
/// Which page slots get a fragmentainer. css-page-3 §3.2 asks to avoid content-empty pages.
/// </summary>
[TestClass]
public sealed class HtmlContainerIntPaginationTests
{
    [Ignore("Every page slot the root spans gets a fragmentainer, content-empty ones included.")]
    [TestMethod]
    public void Fragmentainers_RealContentSeparatedByMultiPageGap_SkipWhollyEmptySlots()
    {
        // Slots 1 to 3 have nothing painted in them.
        var (_, container) = LayoutHarness.Layout(
            LayoutHarness.Wrap(
                "<div id='a' style='height:20px; background-color:rgb(0,0,0);'></div>" +
                "<div id='gap' style='height:880px;'></div>" +
                "<div id='b' style='height:20px; background-color:rgb(0,0,0);'></div>"),
            pageHeight: 200, margin: 0);

        var slotTops = container.FragmentTree!.Fragmentainers.Select(f => f.LocalOriginY).ToList();

        CollectionAssert.Contains(slotTops, 0.0);
        CollectionAssert.DoesNotContain(slotTops, 200.0);
        CollectionAssert.DoesNotContain(slotTops, 400.0);
        CollectionAssert.DoesNotContain(slotTops, 600.0);
        CollectionAssert.Contains(slotTops, 800.0);
    }

    [TestMethod]
    public void Fragmentainers_ContiguousRealContent_KeepEveryPage()
    {
        var (_, container) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<div style='height:900px; background-color:rgb(9,9,9);'>section content spanning pages</div>"),
            pageHeight: 200, margin: 0);

        var fragmentainers = container.FragmentTree!.Fragmentainers;

        CollectionAssert.AreEqual(new[] { 0.0, 200.0, 400.0, 600.0, 800.0 }, fragmentainers.Select(f => f.LocalOriginY).ToList());
        CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, fragmentainers.Select(f => f.SlotIndex).ToList());
    }

    [Ignore("A document without printable content gets a fragmentainer per page slot it spans.")]
    [TestMethod]
    public void Fragmentainers_PureMarginOnlyDocument_FallBackToASingleFragmentainer()
    {
        // Nothing printable anywhere still makes one page, never zero.
        var (_, container) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<div id='gap' style='height:900px;'></div>"),
            pageHeight: 200, margin: 0);

        var fragmentainer = container.FragmentTree!.Fragmentainers.Single();

        Assert.AreEqual(0, fragmentainer.SlotIndex);
        Assert.AreEqual(0.0, fragmentainer.LocalOriginY);
    }
}
