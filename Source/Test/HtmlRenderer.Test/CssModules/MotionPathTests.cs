using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.CssModules;

/// <summary>Which Motion Path values the CSS engine accepts.</summary>
[TestClass]
public sealed class MotionPathTests
{
    [Ignore("Gap, see Spec/motion-1.yml")]
    [TestMethod]
    [Spec("motion-1", "#offset-path-property")]
    [Spec("motion-1", "#offset-distance-property")]
    [Spec("motion-1", "#offset-position-property")]
    [Spec("motion-1", "#offset-anchor-property")]
    [Spec("motion-1", "#offset-rotate-property")]
    [DataRow("offset-path", "ray(45deg closest-side)")]
    [DataRow("offset-path", "inset(10px) border-box")]
    [DataRow("offset-distance", "50%")]
    [DataRow("offset-position", "left top")]
    [DataRow("offset-anchor", "right bottom")]
    [DataRow("offset-rotate", "auto 90deg")]
    public void OffsetProperties_AreParsed(string property, string value)
    {
        Assert.AreEqual(value, Value(property, value));
    }

    [Ignore("Gap, see Spec/motion-1.yml")]
    [TestMethod]
    [Spec("motion-1", "#offset-shorthand")]
    public void OffsetShorthand_SetsItsLonghands()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet(".x { offset: left top ray(45deg) 20px / right bottom; }");
        var style = sheet.Rules.OfType<StyleRule>().Single().Style;

        Assert.AreEqual("20px", style.GetPropertyValue("offset-distance"));
        Assert.AreEqual("right bottom", style.GetPropertyValue("offset-anchor"));
    }

    private static string Value(string property, string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {property}: {value}; }}");
        return sheet.Rules.OfType<StyleRule>().Single().Style.GetPropertyValue(property);
    }
}
