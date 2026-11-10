using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class PaddingPropertyTests
{
    private static string GetProperty(string declaration, string propertyName)
    {
        var rule = new CssParser(new MockAdapter()).ParseInlineStyle(declaration);
        Assert.IsNotNull(rule);
        return rule.Style[propertyName];
    }

    [TestMethod]
    [Spec("css-box-3", "#padding-shorthand")]
    public void CssPaddingAllZeroLegal()
    {
        const string declaration = "padding: 0";

        Assert.AreEqual("0", GetProperty(declaration, "padding-left"));
        Assert.AreEqual("0", GetProperty(declaration, "padding-top"));
        Assert.AreEqual("0", GetProperty(declaration, "padding-right"));
        Assert.AreEqual("0", GetProperty(declaration, "padding-bottom"));
    }

    [TestMethod]
    [Spec("css-box-3", "#padding-shorthand")]
    public void CssPaddingAllPercentLegal()
    {
        const string declaration = "padding: 25%";

        Assert.AreEqual("25%", GetProperty(declaration, "padding-left"));
        Assert.AreEqual("25%", GetProperty(declaration, "padding-top"));
        Assert.AreEqual("25%", GetProperty(declaration, "padding-right"));
        Assert.AreEqual("25%", GetProperty(declaration, "padding-bottom"));
    }

    [TestMethod]
    [Spec("css-box-3", "#padding-shorthand")]
    public void CssPaddingSidesLengthLegal()
    {
        const string declaration = "padding: 10px 3em";

        Assert.AreEqual("3em", GetProperty(declaration, "padding-left"));
        Assert.AreEqual("10px", GetProperty(declaration, "padding-top"));
        Assert.AreEqual("3em", GetProperty(declaration, "padding-right"));
        Assert.AreEqual("10px", GetProperty(declaration, "padding-bottom"));
    }

    [TestMethod]
    [Spec("css-box-3", "#padding-shorthand")]
    public void CssPaddingAutoIllegal()
    {
        const string declaration = "padding: auto";

        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "padding-left")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "padding-top")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "padding-right")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "padding-bottom")));
    }

    [TestMethod]
    [Spec("css-box-3", "#padding-shorthand")]
    public void CssPaddingThreeValuesLegal()
    {
        const string declaration = "padding: 10px 3em 5px";

        Assert.AreEqual("3em", GetProperty(declaration, "padding-left"));
        Assert.AreEqual("10px", GetProperty(declaration, "padding-top"));
        Assert.AreEqual("3em", GetProperty(declaration, "padding-right"));
        Assert.AreEqual("5px", GetProperty(declaration, "padding-bottom"));
    }

    [TestMethod]
    [Spec("css-box-3", "#padding-shorthand")]
    public void CssPaddingAllValuesWithPercentLegal()
    {
        const string declaration = "padding: 10px 5% 8px 2%";

        Assert.AreEqual("2%", GetProperty(declaration, "padding-left"));
        Assert.AreEqual("10px", GetProperty(declaration, "padding-top"));
        Assert.AreEqual("5%", GetProperty(declaration, "padding-right"));
        Assert.AreEqual("8px", GetProperty(declaration, "padding-bottom"));
    }

    [TestMethod]
    [Spec("css-box-3", "#padding-shorthand")]
    public void CssPaddingTooManyValuesIllegal()
    {
        const string declaration = "padding: 10px 5% 8px 2% 3px";

        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "padding-left")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "padding-top")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "padding-right")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "padding-bottom")));
    }
}
