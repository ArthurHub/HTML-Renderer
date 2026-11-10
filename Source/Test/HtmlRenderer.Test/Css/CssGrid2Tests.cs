using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class CssGrid2Tests
{
    private static bool IsAccepted(string declaration) =>
        CssConstructionFunctions.ParseDeclaration(declaration)?.HasValue == true;

    // ── parsing ──────────────────────────────────────────────────────────

    [TestMethod]
    [DataRow("display: grid")]
    [DataRow("display: inline-grid")]
    [Spec("css-grid-2", "#grid-containers")]
    public void GridDisplayValues_AreAccepted(string declaration)
    {
        Assert.IsTrue(IsAccepted(declaration));
    }

    [TestMethod]
    [DataRow("grid-template-columns: none")]
    [DataRow("grid-template-columns: 100px 20% auto min-content max-content")]
    [DataRow("grid-template-columns: minmax(100px, 1fr) fit-content(200px)")]
    [DataRow("grid-template-rows: [header-start] 50px [header-end main-start] 1fr [main-end]")]
    [DataRow("grid-template-columns: repeat(3, 1fr 2fr)")]
    [DataRow("grid-template-columns: [a] repeat(auto-fill, minmax(100px, 1fr)) [b]")]
    [DataRow("grid-template-columns: 100px repeat(auto-fit, 50px) 100px")]
    [DataRow("grid-template-columns: 0.5fr 2.5fr")]
    [Spec("css-grid-2", "#track-sizing")]
    [Spec("css-grid-2", "#track-sizes")]
    [Spec("css-grid-2", "#named-lines")]
    [Spec("css-grid-2", "#repeat-syntax")]
    [Spec("css-grid-2", "#auto-repeat")]
    [Spec("css-grid-2", "#fr-unit")]
    public void TrackLists_AreAccepted(string declaration)
    {
        Assert.IsTrue(IsAccepted(declaration));
    }

    [TestMethod]
    [DataRow("grid-template-columns: repeat(auto-fill, 50px) repeat(auto-fit, 50px)")]
    [DataRow("grid-template-columns: minmax(1fr, 100px)")]
    [DataRow("grid-template-columns: -1fr")]
    [DataRow("grid-template-columns: repeat(0, 100px)")]
    [Spec("css-grid-2", "#track-sizing")]
    [Spec("css-grid-2", "#repeat-syntax")]
    public void InvalidTrackLists_AreDropped(string declaration)
    {
        Assert.IsFalse(IsAccepted(declaration));
    }

    [TestMethod]
    [DataRow("grid-template-areas: none")]
    [DataRow("grid-template-areas: \"head head\" \"nav main\" \". foot\"")]
    [Spec("css-grid-2", "#grid-template-areas-property")]
    public void TemplateAreas_AreAccepted(string declaration)
    {
        Assert.IsTrue(IsAccepted(declaration));
    }

    [TestMethod]
    [DataRow("grid-template-areas: \"a a\" \"a b a\"")]
    [DataRow("grid-template-areas: \"a b\" \"b a\"")]
    [Spec("css-grid-2", "#grid-template-areas-property")]
    public void NonRectangularTemplateAreas_AreDropped(string declaration)
    {
        Assert.IsFalse(IsAccepted(declaration));
    }

    [TestMethod]
    [DataRow("grid-auto-columns: 100px")]
    [DataRow("grid-auto-rows: minmax(10px, auto) 1fr")]
    [Spec("css-grid-2", "#auto-tracks")]
    public void AutoTracks_AreAccepted(string declaration)
    {
        Assert.IsTrue(IsAccepted(declaration));
    }

    [TestMethod]
    [DataRow("grid-auto-flow: row", true)]
    [DataRow("grid-auto-flow: dense column", true)]
    [DataRow("grid-auto-flow: row column", false)]
    [Spec("css-grid-2", "#grid-auto-flow-property")]
    public void AutoFlow_ParsesRowOrColumnWithDense(string declaration, bool accepted)
    {
        Assert.AreEqual(accepted, IsAccepted(declaration));
    }

    [TestMethod]
    [DataRow("grid-column-start: auto")]
    [DataRow("grid-column-end: -1")]
    [DataRow("grid-row-start: span 2")]
    [DataRow("grid-row-end: main-end")]
    [DataRow("grid-row-end: 2 col")]
    [Spec("css-grid-2", "#line-placement")]
    public void GridLines_AreAccepted(string declaration)
    {
        Assert.IsTrue(IsAccepted(declaration));
    }

    [TestMethod]
    [Spec("css-grid-2", "#placement-shorthands")]
    public void GridArea_WithOneName_CopiesItToAllFourLines()
    {
        var style = CssConstructionFunctions.ParseDeclarations("grid-area: main");

        Assert.AreEqual("main", style.GetPropertyValue("grid-row-start"));
        Assert.AreEqual("main", style.GetPropertyValue("grid-column-start"));
        Assert.AreEqual("main", style.GetPropertyValue("grid-row-end"));
        Assert.AreEqual("main", style.GetPropertyValue("grid-column-end"));
    }

    [TestMethod]
    [Spec("css-grid-2", "#placement-shorthands")]
    public void GridColumn_WithANumber_LeavesTheEndAuto()
    {
        var style = CssConstructionFunctions.ParseDeclarations("grid-column: 2");

        Assert.AreEqual("2", style.GetPropertyValue("grid-column-start"));
        Assert.AreEqual("auto", style.GetPropertyValue("grid-column-end"));
    }

    [TestMethod]
    [DataRow("grid-template-columns: subgrid")]
    [DataRow("grid-template-rows: subgrid [a] [b c]")]
    [Spec("css-grid-2", "#subgrids")]
    public void Subgrid_IsAccepted(string declaration)
    {
        Assert.IsTrue(IsAccepted(declaration));
    }

    [TestMethod]
    [DataRow("order: -1")]
    [Spec("css-grid-2", "#order-property")]
    public void Order_IsAccepted(string declaration)
    {
        Assert.IsTrue(IsAccepted(declaration));
    }

    [TestMethod]
    [DataRow("row-gap: 10px")]
    [DataRow("column-gap: 5%")]
    [DataRow("gap: 10px 20px")]
    [Spec("css-grid-2", "#gutters")]
    public void Gaps_AreAccepted(string declaration)
    {
        Assert.IsTrue(IsAccepted(declaration));
    }

    [TestMethod]
    [DataRow("justify-self: center", "#row-align")]
    [DataRow("justify-items: end", "#row-align")]
    [DataRow("align-self: stretch", "#column-align")]
    [DataRow("align-items: start", "#column-align")]
    [DataRow("justify-content: space-between", "#grid-align")]
    [DataRow("align-content: center", "#grid-align")]
    [Spec("css-grid-2", "#row-align")]
    [Spec("css-grid-2", "#column-align")]
    [Spec("css-grid-2", "#grid-align")]
    public void AlignmentProperties_AreAccepted(string declaration, string section)
    {
        Assert.IsTrue(IsAccepted(declaration), section);
    }

    // ── gaps ─────────────────────────────────────────────────────────────

    [Ignore("Gap, see Spec/css-grid-2.yml")]
    [TestMethod]
    [Spec("css-grid-2", "#grid-containers")]
    [Spec("css-grid-2", "#auto-placement-algo")]
    public void GridItems_AreAutoPlacedIntoTheColumnTracks()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:grid;grid-template-columns:100px 100px'><div id=a>a</div><div id=b>b</div></div>"));
        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(a.Location.Y, b.Location.Y, 0.5);
        Assert.AreEqual(a.Location.X + 100, b.Location.X, 0.5);
        Assert.AreEqual(100, a.Size.Width, 0.5);
    }

    [Ignore("Gap, see Spec/css-grid-2.yml")]
    [TestMethod]
    [Spec("css-grid-2", "#fr-unit")]
    [Spec("css-grid-2", "#algo-flex-tracks")]
    public void FlexibleTracks_ShareTheFreeSpaceByFactor()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:grid;width:400px;grid-template-columns:1fr 3fr'><div id=a>a</div><div id=b>b</div></div>"));

        Assert.AreEqual(100, LayoutHarness.FindById(root, "a")!.Size.Width, 0.5);
        Assert.AreEqual(300, LayoutHarness.FindById(root, "b")!.Size.Width, 0.5);
    }

    [Ignore("Gap, see Spec/css-grid-2.yml")]
    [TestMethod]
    [DataRow("grid-template-columns: -10px")]
    [DataRow("grid-template-columns: 10deg")]
    [Spec("css-grid-2", "#track-sizes")]
    public void NegativeOrNonLengthTrackSizes_AreDropped(string declaration)
    {
        Assert.IsFalse(IsAccepted(declaration));
    }

    [Ignore("Gap, see Spec/css-grid-2.yml")]
    [TestMethod]
    [DataRow("grid-template-columns: [span] 100px")]
    [DataRow("grid-template-columns: 100px [auto]")]
    [Spec("css-grid-2", "#named-lines")]
    public void SpanAndAutoLineNames_AreDropped(string declaration)
    {
        Assert.IsFalse(IsAccepted(declaration));
    }

    [Ignore("Gap, see Spec/css-grid-2.yml")]
    [TestMethod]
    [Spec("css-grid-2", "#repeat-syntax")]
    public void LineNamesInsideRepeat_AreAccepted()
    {
        Assert.IsTrue(IsAccepted("grid-template-columns: repeat(2, [col-start] 1fr [col-end])"));
    }

    [Ignore("Gap, see Spec/css-grid-2.yml")]
    [TestMethod]
    [DataRow("grid-template-columns: repeat(auto-fill, 1fr)")]
    [DataRow("grid-template-columns: repeat(auto-fit, minmax(auto, auto))")]
    [DataRow("grid-template-columns: repeat(auto-fill, 100px) 1fr")]
    [Spec("css-grid-2", "#auto-repeat")]
    public void AutoRepeat_WithIntrinsicOrFlexibleTracks_IsDropped(string declaration)
    {
        Assert.IsFalse(IsAccepted(declaration));
    }

    [Ignore("Gap, see Spec/css-grid-2.yml")]
    [TestMethod]
    [Spec("css-grid-2", "#grid-template-areas-property")]
    public void TemplateAreas_DotBetweenNames_IsItsOwnCell()
    {
        Assert.IsTrue(IsAccepted("grid-template-areas: \"a.b c\" \"a.b c\""));
    }

    [Ignore("Gap, see Spec/css-grid-2.yml")]
    [TestMethod]
    [Spec("css-grid-2", "#grid-template-areas-property")]
    public void TemplateAreas_WithATrashToken_IsDropped()
    {
        Assert.IsFalse(IsAccepted("grid-template-areas: \"a$ b\""));
    }

    [Ignore("Gap, see Spec/css-grid-2.yml")]
    [TestMethod]
    [DataRow("grid-row-end: span main")]
    [DataRow("grid-column-start: span 2 col")]
    [Spec("css-grid-2", "#line-placement")]
    public void SpanWithALineName_IsAccepted(string declaration)
    {
        Assert.IsTrue(IsAccepted(declaration));
    }

    [Ignore("Gap, see Spec/css-grid-2.yml")]
    [TestMethod]
    [Spec("css-grid-2", "#subgrids")]
    public void SubgridLineNameRepeat_IsAccepted()
    {
        Assert.IsTrue(IsAccepted("grid-template-columns: subgrid repeat(2, [a]) [b]"));
    }
}
