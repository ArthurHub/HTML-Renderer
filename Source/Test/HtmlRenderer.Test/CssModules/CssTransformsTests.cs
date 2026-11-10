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

    private static string Value(string property, string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {property}: {value}; }}");
        return sheet.Rules.OfType<StyleRule>().Single().Style.GetPropertyValue(property);
    }
}
