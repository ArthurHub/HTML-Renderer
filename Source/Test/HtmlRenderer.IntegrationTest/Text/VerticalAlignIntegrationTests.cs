using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Text;

// This fork's CssParser keeps a process-wide, non-thread-safe regex cache
// (RegexParserUtils.GetRegex's static Dictionary) that SetHtml/DefaultCssData populate lazily on first use per
// AppDomain; running HtmlContainerInt.SetHtml from more than one thread at once (as MSTestSettings.cs's
// assembly-wide [Parallelize(Scope = ExecutionScope.MethodLevel)] does by default) can corrupt it and throw
// "A concurrent update was performed on this collection". [DoNotParallelize] avoids tripping that pre-existing
// library race rather than masking it.
[DoNotParallelize]
[TestClass]
public sealed class VerticalAlignIntegrationTests
{
    private const double Delta = 0.5;

    private static double GetAlignedTop(string verticalAlign, string? lineHeight = null)
    {
        // "v" is deliberately smaller than its parent's own font (10px vs 16px) - text-top/text-bottom align
        // with the *parent's* font box, and if the aligned box were taller than that reference box, "top"
        // and "bottom" alignment could legitimately cross over.
        var lineHeightDecl = lineHeight is null ? "" : $"; line-height:{lineHeight}";
        var html = LayoutHarness.Wrap(
            "<p id='p' style='font-size:16px'><span style='font-size:60px'>TALL</span> " +
            $"<span id='v' style='vertical-align:{verticalAlign}; font-size:10px{lineHeightDecl}'>small</span></p>");

        var (root, _) = LayoutHarness.Layout(html);
        var v = LayoutHarness.FindById(root, "v")!;
        return LayoutHarness.Descendants(v).SelectMany(b => b.Words).First().Top;
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "visudet.html#leading")]
    [TestMethod]
    [Spec("css-inline-3", "#transverse-alignment")]
    public void Top_PositionsHigherThanBottom()
    {
        var topY = GetAlignedTop("top");
        var bottomY = GetAlignedTop("bottom");

        Assert.IsTrue(topY < bottomY, $"expected top-aligned span ({topY}) to sit above bottom-aligned span ({bottomY})");
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "visudet.html#leading")]
    [TestMethod]
    [Spec("css-inline-3", "#transverse-alignment")]
    public void Middle_PositionsBetweenTopAndBottom()
    {
        var topY = GetAlignedTop("top");
        var bottomY = GetAlignedTop("bottom");
        var middleY = GetAlignedTop("middle");

        Assert.IsTrue(middleY > topY && middleY < bottomY);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "visudet.html#leading")]
    [TestMethod]
    [Spec("css-inline-3", "#transverse-alignment")]
    public void TextTop_PositionsAboveTextBottom()
    {
        var textTopY = GetAlignedTop("text-top");
        var textBottomY = GetAlignedTop("text-bottom");

        Assert.IsTrue(textTopY < textBottomY, $"top={textTopY} bottom={textBottomY}");
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "visudet.html#leading")]
    [TestMethod]
    [Spec("css-inline-3", "#transverse-alignment")]
    public void Sub_PositionsBelowSuper()
    {
        var subY = GetAlignedTop("sub");
        var superY = GetAlignedTop("super");

        Assert.IsTrue(subY > superY, $"sub={subY} super={superY}");
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "visudet.html#leading")]
    [TestMethod]
    public void Bottom_DiffersFromDefaultBaselineAlignment()
    {
        var bottomY = GetAlignedTop("bottom");
        var baselineY = GetAlignedTop("baseline");

        Assert.AreNotEqual(baselineY, bottomY);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "visudet.html#leading")]
    [TestMethod]
    public void Middle_DiffersFromDefaultBaselineAlignment()
    {
        var middleY = GetAlignedTop("middle");
        var baselineY = GetAlignedTop("baseline");

        Assert.AreNotEqual(baselineY, middleY);
    }

    [TestMethod]
    public void TextTop_ReferencesParentFontAscent_NotJustLineTop()
    {
        // text-top aligns with the top of the *parent's* font (CSS1 §5.6.11), not the line's raw top extent
        // the way plain "top" does - changing only the parent's font-size (the target span stays fixed at
        // 10px in both builds) must still move the result, proving the parent's font metrics are actually
        // consulted rather than this collapsing to plain "top".
        var htmlSmallParent = LayoutHarness.Wrap(
            "<p id='p' style='font-size:10px'><span style='font-size:60px'>TALL</span> " +
            "<span id='v' style='vertical-align:text-top; font-size:10px'>small</span></p>");
        var htmlLargeParent = LayoutHarness.Wrap(
            "<p id='p' style='font-size:40px'><span style='font-size:60px'>TALL</span> " +
            "<span id='v' style='vertical-align:text-top; font-size:10px'>small</span></p>");

        var (rootSmall, _) = LayoutHarness.Layout(htmlSmallParent);
        var (rootLarge, _) = LayoutHarness.Layout(htmlLargeParent);

        var vSmall = LayoutHarness.FindById(rootSmall, "v")!;
        var vLarge = LayoutHarness.FindById(rootLarge, "v")!;
        var ySmall = LayoutHarness.Descendants(vSmall).SelectMany(b => b.Words).First().Top;
        var yLarge = LayoutHarness.Descendants(vLarge).SelectMany(b => b.Words).First().Top;

        Assert.AreNotEqual(ySmall, yLarge);
    }

    [TestMethod]
    [Spec("css21", "tables.html#height-layout")]
    public void Bottom_OnATableCell_PushesShortContentLowerThanTopAligned()
    {
        var htmlTop = LayoutHarness.Wrap(
            "<table><tr>"
            + "<td style='height:100px'>Tall</td>"
            + "<td id='v' style='vertical-align:top'>Short</td>"
            + "</tr></table>");
        var htmlBottom = LayoutHarness.Wrap(
            "<table><tr>"
            + "<td style='height:100px'>Tall</td>"
            + "<td id='v' style='vertical-align:bottom'>Short</td>"
            + "</tr></table>");

        var (rootTop, _) = LayoutHarness.Layout(htmlTop);
        var (rootBottom, _) = LayoutHarness.Layout(htmlBottom);

        var topY = LayoutHarness.Descendants(LayoutHarness.FindById(rootTop, "v")!).SelectMany(b => b.Words).First().Top;
        var bottomY = LayoutHarness.Descendants(LayoutHarness.FindById(rootBottom, "v")!).SelectMany(b => b.Words).First().Top;

        Assert.IsTrue(bottomY > topY,
            $"vertical-align:bottom ({bottomY}) should push the cell's content lower than vertical-align:top ({topY})");
    }

    [TestMethod]
    [Spec("css21", "tables.html#height-layout")]
    public void Middle_OnATableCellWithExplicitHeight_CentersShortContent()
    {
        var htmlTop = LayoutHarness.Wrap(
            "<table><tr>"
            + "<td style='height:100px'>Tall</td>"
            + "<td id='v' style='height:100px; vertical-align:top'>Short</td>"
            + "</tr></table>");
        var htmlMiddle = LayoutHarness.Wrap(
            "<table><tr>"
            + "<td style='height:100px'>Tall</td>"
            + "<td id='v' style='height:100px; vertical-align:middle'>Short</td>"
            + "</tr></table>");
        var htmlBottom = LayoutHarness.Wrap(
            "<table><tr>"
            + "<td style='height:100px'>Tall</td>"
            + "<td id='v' style='height:100px; vertical-align:bottom'>Short</td>"
            + "</tr></table>");

        var (rootTop, _) = LayoutHarness.Layout(htmlTop);
        var (rootMiddle, _) = LayoutHarness.Layout(htmlMiddle);
        var (rootBottom, _) = LayoutHarness.Layout(htmlBottom);

        var topY = LayoutHarness.Descendants(LayoutHarness.FindById(rootTop, "v")!).SelectMany(b => b.Words).First().Top;
        var middleY = LayoutHarness.Descendants(LayoutHarness.FindById(rootMiddle, "v")!).SelectMany(b => b.Words).First().Top;
        var bottomY = LayoutHarness.Descendants(LayoutHarness.FindById(rootBottom, "v")!).SelectMany(b => b.Words).First().Top;

        Assert.IsTrue(middleY > topY && middleY < bottomY,
            $"vertical-align:middle ({middleY}) should land strictly between vertical-align:top ({topY}) and vertical-align:bottom ({bottomY}) even with an explicit cell height");

        Assert.AreEqual((topY + bottomY) / 2, middleY, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "visudet.html#leading")]
    [TestMethod]
    [Spec("css-inline-3", "#transverse-alignment")]
    public void Length_PositiveValue_RaisesTheBoxAboveBaseline()
    {
        var baselineY = GetAlignedTop("baseline");
        var raisedY = GetAlignedTop("5px");

        Assert.IsTrue(raisedY < baselineY, $"raised={raisedY} baseline={baselineY}");
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "visudet.html#leading")]
    [TestMethod]
    public void Length_NegativeValue_LowersTheBoxBelowBaseline()
    {
        var baselineY = GetAlignedTop("baseline");
        var loweredY = GetAlignedTop("-5px");

        Assert.IsTrue(loweredY > baselineY, $"lowered={loweredY} baseline={baselineY}");
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "visudet.html#leading")]
    [TestMethod]
    public void Percentage_PositiveValue_RaisesTheBoxAboveBaseline()
    {
        var baselineY = GetAlignedTop("baseline");
        var raisedY = GetAlignedTop("50%");

        Assert.IsTrue(raisedY < baselineY, $"raised={raisedY} baseline={baselineY}");
    }

    [TestMethod]
    public void Percentage_ResolvesAgainstTheBoxsOwnLineHeight()
    {
        // A percentage is a fraction of the box's own line-height (CSS 2.1 §10.8.1) - doubling the
        // line-height (everything else unchanged) must double the raise relative to that line-height's own
        // baseline, proving the percentage is actually resolved against it rather than some other fixed
        // reference (e.g. font-size).
        var baseline20 = GetAlignedTop("baseline", lineHeight: "20px");
        var percent20 = GetAlignedTop("50%", lineHeight: "20px");
        var baseline40 = GetAlignedTop("baseline", lineHeight: "40px");
        var percent40 = GetAlignedTop("50%", lineHeight: "40px");

        var raise20 = baseline20 - percent20;
        var raise40 = baseline40 - percent40;

        Assert.AreEqual(raise20 * 2, raise40, Delta);
    }
}
