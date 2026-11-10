using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class MarginPropertyTests
{
    private static string GetProperty(string declaration, string propertyName)
    {
        var rule = new CssParser(new MockAdapter()).ParseInlineStyle(declaration);
        Assert.IsNotNull(rule);
        return rule.Style[propertyName];
    }

    [TestMethod]
    [Spec("css-box-3", "#margin-shorthand")]
    public void MarginAllZeroLegal()
    {
        const string declaration = "margin: 0";

        Assert.AreEqual("0", GetProperty(declaration, "margin-left"));
        Assert.AreEqual("0", GetProperty(declaration, "margin-top"));
        Assert.AreEqual("0", GetProperty(declaration, "margin-right"));
        Assert.AreEqual("0", GetProperty(declaration, "margin-bottom"));
    }

    [TestMethod]
    [Spec("css-box-3", "#margin-shorthand")]
    public void MarginAllPercentLegal()
    {
        const string declaration = "margin: 25%";

        Assert.AreEqual("25%", GetProperty(declaration, "margin-left"));
        Assert.AreEqual("25%", GetProperty(declaration, "margin-top"));
        Assert.AreEqual("25%", GetProperty(declaration, "margin-right"));
        Assert.AreEqual("25%", GetProperty(declaration, "margin-bottom"));
    }

    [TestMethod]
    [Spec("css-box-3", "#margin-shorthand")]
    public void MarginAutoLegal()
    {
        const string declaration = "margin: auto";

        Assert.AreEqual("auto", GetProperty(declaration, "margin-left"));
        Assert.AreEqual("auto", GetProperty(declaration, "margin-top"));
        Assert.AreEqual("auto", GetProperty(declaration, "margin-right"));
        Assert.AreEqual("auto", GetProperty(declaration, "margin-bottom"));
    }

    [TestMethod]
    [Spec("css-box-3", "#margin-shorthand")]
    public void MarginSidesLengthLegal()
    {
        const string declaration = "margin: 10px 3em";

        Assert.AreEqual("3em", GetProperty(declaration, "margin-left"));
        Assert.AreEqual("10px", GetProperty(declaration, "margin-top"));
        Assert.AreEqual("3em", GetProperty(declaration, "margin-right"));
        Assert.AreEqual("10px", GetProperty(declaration, "margin-bottom"));
    }

    [TestMethod]
    [Spec("css-box-3", "#margin-shorthand")]
    public void MarginSidesLengthAndAutoLegal()
    {
        const string declaration = "margin: 10px auto";

        Assert.AreEqual("auto", GetProperty(declaration, "margin-left"));
        Assert.AreEqual("10px", GetProperty(declaration, "margin-top"));
        Assert.AreEqual("auto", GetProperty(declaration, "margin-right"));
        Assert.AreEqual("10px", GetProperty(declaration, "margin-bottom"));
    }

    [TestMethod]
    [Spec("css-box-3", "#margin-shorthand")]
    public void MarginThreeValuesLegal()
    {
        const string declaration = "margin: 10px 3em 5px";

        Assert.AreEqual("3em", GetProperty(declaration, "margin-left"));
        Assert.AreEqual("10px", GetProperty(declaration, "margin-top"));
        Assert.AreEqual("3em", GetProperty(declaration, "margin-right"));
        Assert.AreEqual("5px", GetProperty(declaration, "margin-bottom"));
    }

    [TestMethod]
    [Spec("css-box-3", "#margin-shorthand")]
    public void MarginAllValuesWithPercentAndAutoLegal()
    {
        const string declaration = "margin: 10px 5% auto 2%";

        Assert.AreEqual("2%", GetProperty(declaration, "margin-left"));
        Assert.AreEqual("10px", GetProperty(declaration, "margin-top"));
        Assert.AreEqual("5%", GetProperty(declaration, "margin-right"));
        Assert.AreEqual("auto", GetProperty(declaration, "margin-bottom"));
    }

    [TestMethod]
    [Spec("css-box-3", "#margin-shorthand")]
    public void MarginTooManyValuesIllegal()
    {
        const string declaration = "margin: 10px 5% 8px 2% 3px auto";

        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "margin-left")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "margin-top")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "margin-right")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "margin-bottom")));
    }
}
