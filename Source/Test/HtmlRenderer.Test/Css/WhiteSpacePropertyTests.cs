using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class WhiteSpacePropertyTests
{
    private static string GetWhiteSpace(string declaration)
    {
        var rule = new CssParser(new MockAdapter()).ParseInlineStyle(declaration);
        Assert.IsNotNull(rule);
        return rule.Style["white-space"];
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-prop")]
    [Spec("css-text-3", "#white-space-property")]
    [Spec("css-text-4", "#white-space-property")]
    [DataRow("normal")]
    [DataRow("pre")]
    [DataRow("nowrap")]
    [DataRow("pre-wrap")]
    [DataRow("pre-line")]
    public void WhiteSpaceKeywordLegal(string keyword)
    {
        Assert.AreEqual(keyword, GetWhiteSpace($"white-space: {keyword}"));
    }

    [TestMethod]
    [Spec("css21", "text.html#white-space-prop")]
    [Spec("css-text-3", "#white-space-property")]
    [Spec("css-text-4", "#white-space-property")]
    public void WhiteSpaceInvalidKeywordIllegal()
    {
        Assert.IsTrue(string.IsNullOrEmpty(GetWhiteSpace("white-space: wavy")));
    }
}
