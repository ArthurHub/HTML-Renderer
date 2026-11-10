using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssFlexboxTests
{
    [TestMethod]
    [Ignore("Gap, see Spec/css-flexbox-1.yml")]
    [Spec("css-flexbox-1", "#flex-containers")]
    [Spec("css-flexbox-1", "#flex-lines")]
    public void FlexItems_AreLaidOutSideBySide()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:flex;width:300px'>" +
            "<div id='a' style='width:100px;height:20px'></div><div id='b' style='width:100px;height:20px'></div></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;
        Assert.AreEqual(a.Location.Y, b.Location.Y, 0.5);
        Assert.AreEqual(a.Location.X + 100, b.Location.X, 0.5);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-flexbox-1.yml")]
    [Spec("css-flexbox-1", "#flexibility")]
    [Spec("css-flexbox-1", "#resolve-flexible-lengths")]
    public void FlexGrow_DistributesTheFreeSpace()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:flex;width:300px'>" +
            "<div id='a' style='flex:1;height:20px'></div><div id='b' style='flex:2;height:20px'></div></div>"));

        Assert.AreEqual(100, LayoutHarness.FindById(root, "a")!.Size.Width, 0.5);
        Assert.AreEqual(200, LayoutHarness.FindById(root, "b")!.Size.Width, 0.5);
    }

    [TestMethod]
    [Spec("css-flexbox-1", "#flex-property")]
    [DataRow("0", "0", "1", "0")]
    [DataRow("30px", "1", "1", "30px")]
    [DataRow("2 30px", "2", "1", "30px")]
    [DataRow("content", "1", "1", "content")]
    public void FlexShorthand_ExpandsOmittedValues(string value, string grow, string shrink, string basis)
    {
        var style = Rule($"flex: {value}");

        Assert.AreEqual(grow, style.FlexGrow);
        Assert.AreEqual(shrink, style.FlexShrink);
        Assert.AreEqual(basis, style.FlexBasis);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-flexbox-1.yml")]
    [Spec("css-flexbox-1", "#flex-property")]
    public void FlexNone_IsZeroZeroAuto()
    {
        var style = Rule("flex: none");

        Assert.AreEqual("0", style.FlexGrow);
        Assert.AreEqual("0", style.FlexShrink);
        Assert.AreEqual("auto", style.FlexBasis);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-flexbox-1.yml")]
    [Spec("css-flexbox-1", "#flex-property")]
    public void FlexBasisBeforeTheFactors_IsValid()
    {
        var style = Rule("flex: 30px 2");

        Assert.AreEqual("2", style.FlexGrow);
        Assert.AreEqual("30px", style.FlexBasis);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-flexbox-1.yml")]
    [Spec("css-flexbox-1", "#flex-property")]
    [Spec("css-flexbox-1", "#flex-grow-property")]
    [Spec("css-flexbox-1", "#flex-shrink-property")]
    [Spec("css-flexbox-1", "#flex-basis-property")]
    [DataRow("flex: -1")]
    [DataRow("flex: initial 1")]
    [DataRow("flex-grow: -1")]
    [DataRow("flex-shrink: -1")]
    [DataRow("flex-basis: -5px")]
    public void InvalidValue_IsRejected(string declaration)
    {
        Assert.IsFalse(CssConstructionFunctions.ParseDeclaration(declaration).HasValue);
    }

    private static StyleDeclaration Rule(string declaration)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".t {{ {declaration}; }}");
        return ((StyleRule)sheet.StyleRules.First()).Style;
    }
}
