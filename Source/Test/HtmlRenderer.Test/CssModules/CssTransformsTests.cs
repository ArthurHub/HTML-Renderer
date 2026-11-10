using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.CssModules;

/// <summary>Which CSS Transforms values the CSS engine accepts.</summary>
[TestClass]
public sealed class CssTransformsTests
{
    [TestMethod]
    [Spec("css-transforms-1", "#transform-property")]
    [DataRow("none rotate(5deg)")]
    [DataRow("rotate(5deg) none")]
    [DataRow("scale(2) banana(1)")]
    [DataRow("matrix(1, 2, 3, 4, 5)")]
    [DataRow("matrix(1 2 3 4 5 6)")]
    public void Transform_InvalidList_IsDropped(string value)
    {
        Assert.AreEqual("", Value("transform", value));
    }

    [TestMethod]
    [Spec("css-transforms-1", "#two-d-transform-functions")]
    [DataRow("translate(10px)")]
    [DataRow("skew(30deg, 10deg)")]
    [DataRow("translateX(calc(10% + 5px))")]
    public void TwoDimensionalFunctions_AreParsed(string value)
    {
        Assert.AreEqual(value, Value("transform", value));
    }

    [Ignore("Gap, see Spec/css-transforms-1.yml")]
    [TestMethod]
    [Spec("css-transforms-1", "#two-d-transform-functions")]
    public void Skew_WithOneAngle_IsParsed()
    {
        Assert.AreEqual("skew(30deg)", Value("transform", "skew(30deg)"));
    }

    [Ignore("Gap, see Spec/css-transforms-1.yml")]
    [TestMethod]
    [Spec("css-transforms-1", "#two-d-transform-functions")]
    [DataRow("rotate(0)")]
    [DataRow("skewX(0)")]
    [DataRow("skewY(0)")]
    public void UnitlessZeroAngle_IsParsed(string value)
    {
        Assert.AreNotEqual("", Value("transform", value));
    }

    [Ignore("Gap, see Spec/css-transforms-1.yml")]
    [TestMethod]
    [Spec("css-transforms-1", "#transform-box")]
    [DataRow("content-box")]
    [DataRow("border-box")]
    [DataRow("fill-box")]
    [DataRow("stroke-box")]
    [DataRow("view-box")]
    public void TransformBox_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("transform-box", value));
    }

    [TestMethod]
    [Spec("css-transforms-2", "#backface-visibility-property")]
    [DataRow("visible")]
    [DataRow("hidden")]
    public void BackfaceVisibility_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("backface-visibility", value));
    }

    [Ignore("Gap, see Spec/css-transforms-2.yml")]
    [TestMethod]
    [Spec("css-transforms-2", "#individual-transforms")]
    [DataRow("translate", "10px 20%")]
    [DataRow("translate", "10px 20px 5px")]
    [DataRow("rotate", "45deg")]
    [DataRow("rotate", "x 45deg")]
    [DataRow("rotate", "1 1 0 45deg")]
    [DataRow("scale", "2")]
    [DataRow("scale", "50% 2 1")]
    public void IndividualTransformProperties_AreParsed(string property, string value)
    {
        Assert.AreEqual(value, Value(property, value));
    }

    [Ignore("Gap, see Spec/css-transforms-2.yml")]
    [TestMethod]
    [Spec("css-transforms-2", "#perspective-property")]
    public void Perspective_NegativeLength_IsRejected()
    {
        Assert.AreEqual("", Value("perspective", "-10px"));
    }

    [Ignore("Gap, see Spec/css-transforms-2.yml")]
    [TestMethod]
    [Spec("css-transforms-2", "#perspective-origin-property")]
    [DataRow("left 10% bottom")]
    [DataRow("left 10px top 5px")]
    public void PerspectiveOrigin_ThreeAndFourValuePositions_AreParsed(string value)
    {
        Assert.AreNotEqual("", Value("perspective-origin", value));
    }

    [Ignore("Gap, see Spec/css-transforms-2.yml")]
    [TestMethod]
    [Spec("css-transforms-2", "#two-d-transform-functions")]
    [DataRow("scale(50%)")]
    [DataRow("scaleX(50%)")]
    [DataRow("scaleY(50%)")]
    public void ScaleFunctions_AcceptPercentages(string value)
    {
        Assert.AreEqual(value, Value("transform", value));
    }

    [Ignore("Gap, see Spec/css-transforms-2.yml")]
    [TestMethod]
    [Spec("css-transforms-2", "#three-d-transform-functions")]
    [DataRow("scale3d(50%, 50%, 50%)")]
    [DataRow("scaleZ(50%)")]
    [DataRow("rotateX(0)")]
    [DataRow("rotate3d(1, 0, 0, 0)")]
    [DataRow("perspective(none)")]
    public void ThreeDimensionalFunctions_AreParsed(string value)
    {
        Assert.AreNotEqual("", Value("transform", value));
    }

    [Ignore("Gap, see Spec/css-transforms-2.yml")]
    [TestMethod]
    [Spec("css-transforms-2", "#three-d-transform-functions")]
    [DataRow("translateZ(10%)")]
    [DataRow("translate3d(1px, 2px, 3%)")]
    [DataRow("translate3d(1px, 2px)")]
    [DataRow("perspective(-10px)")]
    public void InvalidThreeDimensionalFunctions_AreRejected(string value)
    {
        Assert.AreEqual("", Value("transform", value));
    }

    private static string Value(string property, string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {property}: {value}; }}");
        return sheet.Rules.OfType<StyleRule>().Single().Style.GetPropertyValue(property);
    }
}
