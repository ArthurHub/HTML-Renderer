using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssAlignTests
{
    [TestMethod]
    [Spec("css-align-3", "#positional-values")]
    [DataRow("align-items: self-start")]
    [DataRow("align-self: flex-end")]
    [DataRow("justify-content: left")]
    [DataRow("justify-items: end")]
    [DataRow("justify-self: center")]
    public void PositionalKeyword_IsAccepted(string declaration) => AssertAccepted(declaration);

    [TestMethod]
    [Spec("css-align-3", "#baseline-values")]
    [DataRow("align-content: baseline")]
    [DataRow("align-items: first baseline")]
    [DataRow("align-self: last baseline")]
    [DataRow("justify-items: baseline")]
    [DataRow("justify-self: first baseline")]
    public void BaselineKeyword_IsAccepted(string declaration) => AssertAccepted(declaration);

    [TestMethod]
    [Spec("css-align-3", "#distribution-values")]
    [DataRow("align-content: space-between")]
    [DataRow("align-content: stretch")]
    [DataRow("justify-content: space-around")]
    [DataRow("justify-content: space-evenly")]
    public void DistributionKeyword_IsAccepted(string declaration) => AssertAccepted(declaration);

    [TestMethod]
    [Spec("css-align-3", "#overflow-values")]
    [DataRow("align-content: safe center")]
    [DataRow("justify-content: unsafe center")]
    [DataRow("align-items: safe center")]
    [DataRow("justify-self: unsafe center")]
    public void OverflowKeywordBeforeCenter_IsAccepted(string declaration) => AssertAccepted(declaration);

    [TestMethod]
    [Spec("css-align-3", "#justify-self-property")]
    [Spec("css-align-3", "#justify-items-property")]
    [DataRow("justify-self: auto")]
    [DataRow("justify-self: normal")]
    [DataRow("justify-self: stretch")]
    [DataRow("justify-items: normal")]
    [DataRow("justify-items: stretch")]
    public void JustifySelfAndItems_AreAccepted(string declaration) => AssertAccepted(declaration);

    [TestMethod]
    [Spec("css-align-3", "#place-content")]
    [Spec("css-align-3", "#place-items-property")]
    [Spec("css-align-3", "#place-self-property")]
    [DataRow("place-content: center", "align-content", "center", "justify-content", "center")]
    [DataRow("place-content: space-between end", "align-content", "space-between", "justify-content", "end")]
    [DataRow("place-items: stretch", "align-items", "stretch", "justify-items", "stretch")]
    [DataRow("place-items: center start", "align-items", "center", "justify-items", "start")]
    [DataRow("place-self: auto", "align-self", "auto", "justify-self", "auto")]
    [DataRow("place-self: auto center", "align-self", "auto", "justify-self", "center")]
    public void PlaceShorthand_SetsBothLonghands(string declaration, string align, string alignValue, string justify, string justifyValue)
    {
        var style = Rule(declaration);

        Assert.AreEqual(alignValue, style.GetPropertyValue(align));
        Assert.AreEqual(justifyValue, style.GetPropertyValue(justify));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-align-3.yml")]
    [Spec("css-align-3", "#overflow-values")]
    [DataRow("align-content: safe start")]
    [DataRow("justify-content: unsafe left")]
    [DataRow("align-items: safe self-end")]
    [DataRow("align-self: unsafe flex-start")]
    [DataRow("justify-items: safe end")]
    public void OverflowKeywordBeforeAnyPosition_IsAccepted(string declaration) => AssertAccepted(declaration);

    [TestMethod]
    [Ignore("Gap, see Spec/css-align-3.yml")]
    [Spec("css-align-3", "#positional-values")]
    [Spec("css-align-3", "#justify-items-property")]
    [DataRow("justify-items: legacy")]
    [DataRow("justify-items: legacy center")]
    [DataRow("justify-items: right")]
    [DataRow("justify-self: left")]
    public void LeftRightAndLegacy_AreAcceptedForJustifyItemsAndSelf(string declaration) => AssertAccepted(declaration);

    [TestMethod]
    [Spec("css-align-3", "#overflow-values")]
    [DataRow("align-items: safe")]
    public void OverflowKeywordWithoutAPosition_IsRejected(string declaration)
    {
        Assert.IsFalse(CssConstructionFunctions.ParseDeclaration(declaration).HasValue);
    }

    [TestMethod]
    [Spec("css-align-3", "#baseline-values")]
    [DataRow("align-content: first")]
    [DataRow("justify-content: first baseline")]
    public void BaselinePrefixWithoutBaselineAlignment_IsRejected(string declaration)
    {
        Assert.IsFalse(CssConstructionFunctions.ParseDeclaration(declaration).HasValue);
    }

    [TestMethod]
    [DataRow("align-items: safe")]
    [DataRow("justify-self: safe right")]
    [DataRow("place-items: start legacy")]
    public void PrefixedValue_DoesntBreakTheDocument(string declaration)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<div id='x' style='{declaration}'>text</div>"));

        Assert.IsNotNull(LayoutHarness.FindById(root, "x"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-align-3.yml")]
    [Spec("css-align-3", "#place-content")]
    [Spec("css-align-3", "#place-items-property")]
    [Spec("css-align-3", "#place-self-property")]
    [DataRow("place-content: baseline", "align-content", "baseline", "justify-content", "start")]
    [DataRow("place-items: start legacy", "align-items", "start", "justify-items", "legacy")]
    [DataRow("place-self: center left", "align-self", "center", "justify-self", "left")]
    public void PlaceShorthand_WithValidValues_SetsBothLonghands(string declaration, string align, string alignValue, string justify, string justifyValue)
        => PlaceShorthand_SetsBothLonghands(declaration, align, alignValue, justify, justifyValue);

    [TestMethod]
    [Ignore("Gap, see Spec/css-align-3.yml")]
    [Spec("css-align-3", "#place-content")]
    public void PlaceContentWithAnInvalidAlignContent_IsRejected()
    {
        Assert.IsFalse(CssConstructionFunctions.ParseDeclaration("place-content: left").HasValue);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-align-3.yml")]
    [Spec("css-align-3", "#distribution-block")]
    public void AlignContent_CentersTheContentOfABlockContainer()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='p' style='height:100px;align-content:center'><div id='c' style='height:20px'></div></div>"));

        var parent = LayoutHarness.FindById(root, "p")!;
        Assert.AreEqual(parent.Location.Y + 40, LayoutHarness.FindById(root, "c")!.Location.Y, 0.5);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-align-3.yml")]
    [Spec("css-align-3", "#justify-block")]
    public void JustifySelfEnd_MovesABlockLevelBoxToTheEnd()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:300px'><div id='c' style='width:100px;height:20px;justify-self:end'></div></div>"));

        Assert.AreEqual(200, LayoutHarness.FindById(root, "c")!.Location.X, 0.5);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-align-3.yml")]
    [Spec("css-align-3", "#justify-abspos")]
    [Spec("css-align-3", "#align-abspos")]
    public void PlaceSelfCenter_CentersAnAbsolutelyPositionedBox()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='position:relative;width:300px;height:100px'>" +
            "<div id='c' style='position:absolute;left:0;right:0;top:0;bottom:0;margin:0;width:100px;height:20px;place-self:center'></div></div>"));

        var box = LayoutHarness.FindById(root, "c")!;
        Assert.AreEqual(100, box.Location.X, 0.5);
        Assert.AreEqual(40, box.Location.Y, 0.5);
    }

    private static void AssertAccepted(string declaration)
    {
        var property = CssConstructionFunctions.ParseDeclaration(declaration);

        Assert.IsTrue(property.HasValue, declaration);
        Assert.AreEqual(declaration.Substring(declaration.IndexOf(':') + 1).Trim(), property.Value);
    }

    private static StyleDeclaration Rule(string declaration)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".t {{ {declaration}; }}");
        return ((StyleRule)sheet.StyleRules.First()).Style;
    }
}
