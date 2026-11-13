using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>Which &lt;basic-shape&gt; values the CSS engine accepts, checked through clip-path.</summary>
[TestClass]
public sealed class BasicShapeTests
{
    private static string Parse(string value) =>
        CssConstructionFunctions.ParseDeclarations($"clip-path: {value}").GetPropertyValue("clip-path");

    [TestMethod]
    [Spec("css-shapes-1", "#supported-basic-shapes")]
    [DataRow("polygon(0 0, 100% 0, 50% 100%)")]
    [DataRow("polygon(evenodd, 0 0, 10px 0, 0 10px)")]
    [DataRow("polygon(nonzero, 0 0, 10px 0, 0 10px)")]
    [DataRow("polygon(50% 50%)")]
    [DataRow("inset(10px)")]
    [DataRow("inset(10px 20px)")]
    [DataRow("inset(10px 20px 30px)")]
    [DataRow("inset(1px 2px 3px 4px)")]
    [DataRow("inset(10px round 5px)")]
    [DataRow("inset(10% 20%)")]
    [DataRow("circle()")]
    [DataRow("circle(farthest-side)")]
    [DataRow("circle(50px at 10px 20px)")]
    [DataRow("circle(closest-side at left top)")]
    [DataRow("circle(at right bottom)")]
    [DataRow("ellipse()")]
    [DataRow("ellipse(40px 20% at 25% 75%)")]
    [DataRow("ellipse(closest-side farthest-side at right bottom)")]
    public void ValidShape_IsAccepted(string value)
    {
        Assert.AreNotEqual(string.Empty, Parse(value));
    }

    [TestMethod]
    [Spec("css-shapes-1", "#supported-basic-shapes")]
    [DataRow("polygon()")]
    [DataRow("polygon(0)")]
    [DataRow("polygon(0 0, 100%)")]
    [DataRow("polygon(banana, 0 0)")]
    [DataRow("polygon(evenodd)")]
    [DataRow("polygon(0 0,, 10px 10px)")]
    [DataRow("polygon(red 0, 10px 0)")]
    [DataRow("inset()")]
    [DataRow("inset(1px 2px 3px 4px 5px)")]
    [DataRow("inset(10px round)")]
    [DataRow("inset(red)")]
    [DataRow("circle(10px 20px)")]
    [DataRow("circle(at)")]
    [DataRow("circle(banana)")]
    [DataRow("circle(-5px)")]
    [DataRow("circle(-10% at center)")]
    [DataRow("ellipse(-5px 10px)")]
    [DataRow("ellipse(40px 20px 10px)")]
    public void InvalidShape_IsDropped(string value)
    {
        Assert.AreEqual(string.Empty, Parse(value));
    }

    [TestMethod]
    [DataRow("polygon(0% calc(100% * 0.65), 100% 100%, 0% 100%)")]
    [DataRow("polygon(min(10px, 20px) 0, 100% 0, 0 100%)")]
    [DataRow("polygon(0 max(10%, 5px), 100% 0, 0 100%)")]
    [DataRow("polygon(clamp(0px, 50%, 100px) 0, 100% 0, 0 100%)")]
    [Spec("css-shapes-1", "#supported-basic-shapes")]
    public void Polygon_MathFunctions_AreAccepted(string value)
    {
        Assert.AreNotEqual(string.Empty, Parse(value));
    }

    [Ignore("Gap, see Spec/css-shapes-1.yml")]
    [TestMethod]
    [DataRow("ellipse(40px)")]
    [DataRow("ellipse(closest-side)")]
    [Spec("css-shapes-1", "#supported-basic-shapes")]
    public void Ellipse_OneRadiusIsValid(string value)
    {
        Assert.AreNotEqual(string.Empty, Parse(value));
    }

    [TestMethod]
    [DataRow("rect(0 auto 10px 5%)")]
    [DataRow("xywh(10px 10px 50% 20px round 4px)")]
    [DataRow("path(\"M 0 0 L 10 0 L 0 10 Z\")")]
    [Spec("css-shapes-1", "#supported-basic-shapes")]
    public void RectXywhAndPath_AreBasicShapes(string value)
    {
        Assert.AreNotEqual(string.Empty, Parse(value));
    }

    [Ignore("Gap, see Spec/css-shapes-1.yml")]
    [TestMethod]
    [DataRow("polygon(round 5px, 0 0, 100% 0, 50% 100%)")]
    [Spec("css-shapes-1", "#supported-basic-shapes")]
    public void RoundedPolygon_IsABasicShape(string value)
    {
        Assert.AreNotEqual(string.Empty, Parse(value));
    }

    [Ignore("Gap, see Spec/css-shapes-1.yml")]
    [TestMethod]
    [Spec("css-shapes-1", "#shape-function")]
    public void ShapeFunction_IsABasicShape()
    {
        Assert.AreNotEqual(string.Empty, Parse("shape(from 0 0, line to 100% 0, line to 50% 100%, close)"));
    }

    [TestMethod]
    [DataRow("none")]
    [DataRow("polygon(0 0, 100% 0, 50% 100%)")]
    [DataRow("inset(10px 20px round 4px)")]
    [DataRow("circle(50px at center)")]
    [DataRow("ellipse(closest-side farthest-side)")]
    [Spec("css-masking-1", "#the-clip-path")]
    public void ClipPath_ValidValue_IsKeptAsWritten(string value)
    {
        Assert.AreEqual(value, Parse(value));
    }

    [TestMethod]
    [DataRow("banana")]
    [DataRow("polygon(0)")]
    [DataRow("circle(10px 20px)")]
    [Spec("css-masking-1", "#the-clip-path")]
    public void ClipPath_InvalidValue_IsDropped(string value)
    {
        Assert.AreEqual(string.Empty, Parse(value));
    }

    [TestMethod]
    [Spec("css-masking-1", "#the-clip-path")]
    public void ClipPath_InStyleSheet_ValidSurvives_InvalidDropped()
    {
        var valid = CssConstructionFunctions.ParseStyleSheet("div{clip-path:polygon(0 0, 100% 0, 50% 100%)}");
        var validRule = valid.Rules.OfType<StyleRule>().Single();
        Assert.AreEqual("polygon(0 0, 100% 0, 50% 100%)", validRule.Style.GetPropertyValue("clip-path"));

        var invalid = CssConstructionFunctions.ParseStyleSheet("div{clip-path:banana}");
        var invalidRule = invalid.Rules.OfType<StyleRule>().Single();
        Assert.AreEqual(string.Empty, invalidRule.Style.GetPropertyValue("clip-path"));
    }
}
