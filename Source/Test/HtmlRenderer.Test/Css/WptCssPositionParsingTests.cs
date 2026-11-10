using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class WptCssPositionParsingTests
{
    private static string DeclaredValue(string property, string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {property}: {value}; }}");
        var rule = sheet.Rules.OfType<StyleRule>().Single();
        return rule.Style.GetPropertyValue(property);
    }

    // ─── position: 'static | relative | absolute | sticky | fixed' ───────────

    [TestMethod]
    [DataRow("static")]
    [DataRow("relative")]
    [DataRow("absolute")]
    [DataRow("sticky")]
    [DataRow("fixed")]
    [Spec("css-position-3", "#position-property")]
    public void Position_AcceptsValidKeyword(string value)
    {
        Assert.AreEqual(value, DeclaredValue("position", value));
    }

    [TestMethod]
    [DataRow("auto")]
    [DataRow("static relative")]
    [Spec("css-position-3", "#position-property")]
    public void Position_RejectsInvalidValue(string value)
    {
        Assert.AreEqual(string.Empty, DeclaredValue("position", value));
    }

    // ─── top/right/bottom/left: 'auto | <length-percentage>' ─────────────────

    [TestMethod]
    [DataRow("top")]
    [DataRow("right")]
    [DataRow("bottom")]
    [DataRow("left")]
    [Spec("css-position-3", "#insets")]
    public void Coordinate_AcceptsAuto(string property)
    {
        Assert.AreEqual("auto", DeclaredValue(property, "auto"));
    }

    [TestMethod]
    [DataRow("top")]
    [DataRow("right")]
    [DataRow("bottom")]
    [DataRow("left")]
    [Spec("css-position-3", "#insets")]
    public void Coordinate_AcceptsNegativeLength(string property)
    {
        Assert.AreEqual("-10px", DeclaredValue(property, "-10px"));
    }

    [TestMethod]
    [DataRow("top")]
    [DataRow("right")]
    [DataRow("bottom")]
    [DataRow("left")]
    [Spec("css-position-3", "#insets")]
    public void Coordinate_AcceptsNegativePercentage(string property)
    {
        Assert.AreEqual("-20%", DeclaredValue(property, "-20%"));
    }

    [TestMethod]
    [DataRow("top")]
    [DataRow("right")]
    [DataRow("bottom")]
    [DataRow("left")]
    [Spec("css-position-3", "#insets")]
    public void Coordinate_AcceptsCalcExpression(string property)
    {
        Assert.IsFalse(string.IsNullOrEmpty(DeclaredValue(property, "calc(2em + 3ex)")));
    }

    [TestMethod]
    [DataRow("top")]
    [DataRow("right")]
    [DataRow("bottom")]
    [DataRow("left")]
    [Spec("css-position-3", "#insets")]
    public void Coordinate_RejectsNonLengthKeyword(string property)
    {
        Assert.AreEqual(string.Empty, DeclaredValue(property, "min-content"));
    }

    [TestMethod]
    [DataRow("top")]
    [DataRow("right")]
    [DataRow("bottom")]
    [DataRow("left")]
    [Spec("css-position-3", "#insets")]
    public void Coordinate_RejectsUnitlessNumber(string property)
    {
        Assert.AreEqual(string.Empty, DeclaredValue(property, "60"));
    }

    [TestMethod]
    [DataRow("top")]
    [DataRow("right")]
    [DataRow("bottom")]
    [DataRow("left")]
    [Spec("css-position-3", "#insets")]
    public void Coordinate_RejectsMultipleValues(string property)
    {
        Assert.AreEqual(string.Empty, DeclaredValue(property, "10px 20%"));
    }

    // ─── z-index: 'auto | <integer>' ──────────────────────────────────────────

    [TestMethod]
    [DataRow("auto")]
    [DataRow("-789")]
    [DataRow("0")]
    [DataRow("123")]
    [Spec("css21", "visuren.html#z-index")]
    public void ZIndex_AcceptsValidValue(string value)
    {
        Assert.AreEqual(value, DeclaredValue("z-index", value));
    }

    [TestMethod]
    [DataRow("none")]
    [DataRow("10px")]
    [DataRow("0.5")]
    [DataRow("auto 123")]
    [Spec("css21", "visuren.html#z-index")]
    public void ZIndex_RejectsInvalidValue(string value)
    {
        Assert.AreEqual(string.Empty, DeclaredValue("z-index", value));
    }
}
