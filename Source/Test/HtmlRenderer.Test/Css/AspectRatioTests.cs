using HtmlRenderer.Test.CssEngineSupport;

namespace HtmlRenderer.Test.Css;

/// <summary>Which aspect-ratio values the CSS engine accepts.</summary>
[TestClass]
public sealed class AspectRatioTests
{
    private static string Parse(string value) =>
        CssConstructionFunctions.ParseDeclarations($"aspect-ratio: {value}").GetPropertyValue("aspect-ratio");

    [TestMethod]
    [DataRow("2")]
    [DataRow("16 / 9")]
    [DataRow("16/9")]
    [DataRow("1.5")]
    [DataRow("auto")]
    [DataRow("auto 21 / 9")]
    [DataRow("21 / 9 auto")]
    [DataRow("1 / 0")]
    [DataRow("0")]
    [Spec("css-sizing-4", "#aspect-ratio")]
    public void ValidValue_IsAccepted(string value)
    {
        Assert.AreNotEqual(string.Empty, Parse(value));
    }

    [TestMethod]
    [DataRow("banana")]
    [DataRow("2 3")]
    [DataRow("16 /")]
    [DataRow("/ 9")]
    [DataRow("-2")]
    [DataRow("2 / -3")]
    [DataRow("auto auto")]
    [DataRow("2px")]
    [Spec("css-sizing-4", "#aspect-ratio")]
    public void InvalidValue_IsDropped(string value)
    {
        Assert.AreEqual(string.Empty, Parse(value));
    }
}
