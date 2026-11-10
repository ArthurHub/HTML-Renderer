using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class CoordinatePropertyTests
{
    private static string GetProperty(string declaration, string propertyName)
    {
        var rule = new CssParser(new MockAdapter()).ParseInlineStyle(declaration);
        Assert.IsNotNull(rule);
        return rule.Style[propertyName];
    }

    [TestMethod]
    [Spec("css-sizing-3", "#preferred-size-properties")]
    public void CssHeightLegalPercentage()
    {
        Assert.AreEqual("28%", GetProperty("height: 28%", "height"));
    }

    [TestMethod]
    [Spec("css-sizing-3", "#preferred-size-properties")]
    public void CssHeightLegalLengthInEm()
    {
        Assert.AreEqual("0.3em", GetProperty("height: 0.3em", "height"));
    }

    [TestMethod]
    [Spec("css-sizing-3", "#preferred-size-properties")]
    public void CssHeightLegalLengthInPx()
    {
        Assert.AreEqual("144px", GetProperty("height: 144px", "height"));
    }

    [TestMethod]
    [Spec("css-sizing-3", "#preferred-size-properties")]
    public void CssHeightLegalAutoUppercase()
    {
        Assert.AreEqual("auto", GetProperty("height: AUTO", "height"));
    }

    [TestMethod]
    [Spec("css-sizing-3", "#preferred-size-properties")]
    public void CssWidthLegalLengthInCm()
    {
        Assert.AreEqual("0.5cm", GetProperty("width: 0.5cm", "width"));
    }

    [TestMethod]
    [Spec("css-sizing-3", "#preferred-size-properties")]
    public void CssWidthLegalLengthInMm()
    {
        Assert.AreEqual("1.5mm", GetProperty("width: 1.5mm", "width"));
    }

    [TestMethod]
    [Spec("css-sizing-3", "#preferred-size-properties")]
    public void CssWidthIllegalLength()
    {
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty("width: 1.5 meter", "width")));
    }

    [TestMethod]
    [Spec("css-sizing-3", "#preferred-size-properties")]
    public void CssWidthPercentLegal()
    {
        Assert.AreEqual("20.5%", GetProperty("width: 20.5%", "width"));
    }

    [TestMethod]
    [Spec("css-sizing-3", "#preferred-size-properties")]
    public void CssWidthLegalLengthInInches()
    {
        Assert.AreEqual("3in", GetProperty("width: 3in", "width"));
    }

    [TestMethod]
    [Spec("css-sizing-3", "#preferred-size-properties")]
    public void CssHeightAngleIllegal()
    {
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty("height: 3deg", "height")));
    }

    [TestMethod]
    [Spec("css-sizing-3", "#preferred-size-properties")]
    public void CssHeightResolutionIllegal()
    {
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty("height: 3dpi", "height")));
    }

    [TestMethod]
    [Spec("css-sizing-3", "#max-size-properties")]
    [DataRow("max-width")]
    [DataRow("max-height")]
    public void MaxSizeNone_IsAccepted(string property)
    {
        Assert.AreEqual("none", GetProperty($"{property}: none", property));
    }

    [Ignore("Gap, see Spec/css-sizing-3.yml")]
    [TestMethod]
    [Spec("css-sizing-3", "#min-size-properties")]
    [DataRow("min-width")]
    [DataRow("min-height")]
    public void MinSizeAuto_IsAccepted(string property)
    {
        Assert.AreEqual("auto", GetProperty($"{property}: auto", property));
    }

    [Ignore("Gap, see Spec/css-sizing-3.yml")]
    [TestMethod]
    [Spec("css-sizing-3", "#sizing-values")]
    [DataRow("width")]
    [DataRow("height")]
    [DataRow("min-width")]
    [DataRow("max-height")]
    public void NegativeSize_IsRejected(string property)
    {
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty($"{property}: -10px", property)));
    }
}
