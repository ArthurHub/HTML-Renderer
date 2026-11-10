using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Parsing of content values, no layout.
/// </summary>
[TestClass]
public sealed class ContentPropertyTests
{
    private static StyleRule Parse(string source)
    {
        var parser = new StylesheetParser();
        var rule = parser.Parse(source).Rules[0];
        return (StyleRule)rule;
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-string-token")]
    public void CssContentParseStringWithDoubleQuoteEscape()
    {
        var source = "a{content:\"\\\"\"}";
        var parsed = Parse(source);
        Assert.AreEqual("\"\\\"\"", parsed.Style.Content);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-string-token")]
    public void CssContentParseStringWithSingleQuoteEscape()
    {
        var source = "a{content:'\\''}";
        var parsed = Parse(source);
        Assert.AreEqual("\"'\"", parsed.Style.Content);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-string-token")]
    public void CssContentParseStringWithDoubleQuoteMultipleEscapes()
    {
        var source = "a{content:\"abc\\\"\\\"d\\\"ef\"}";
        var parsed = Parse(source);
        Assert.AreEqual("\"abc\\\"\\\"d\\\"ef\"", parsed.Style.Content);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-string-token")]
    public void CssContentParseStringWithSingleQuoteMultipleEscapes()
    {
        var source = "a{content:'abc\\'\\'d\\'ef'}";
        var parsed = Parse(source);
        Assert.AreEqual("\"abc''d'ef\"", parsed.Style.Content);
    }

    [TestMethod]
    [Spec("css-content-3", "#content-function")]
    [Spec("css-gcpm-3", "#content-function-header")]
    public void CssContentParseContentFunctionText()
    {
        var source = "a::before{content:content(text)}";
        var parsed = Parse(source);
        Assert.AreEqual("content()", parsed.Style.Content);
    }

    [TestMethod]
    [Spec("css-content-3", "#content-function")]
    [Spec("css-gcpm-3", "#content-function-header")]
    public void CssContentParseContentFunctionBefore()
    {
        var source = "a::before{content:content(before)}";
        var parsed = Parse(source);
        Assert.AreEqual("content(before)", parsed.Style.Content);
    }

    [TestMethod]
    [Spec("css-content-3", "#content-function")]
    [Spec("css-gcpm-3", "#content-function-header")]
    public void CssContentParseContentFunctionAfter()
    {
        var source = "a::before{content:content(after)}";
        var parsed = Parse(source);
        Assert.AreEqual("content(after)", parsed.Style.Content);
    }

    [TestMethod]
    [Spec("css-content-3", "#content-function")]
    [Spec("css-gcpm-3", "#content-function-header")]
    public void CssContentParseContentFunctionFirstLetter()
    {
        var source = "a::before{content:content(first-letter)}";
        var parsed = Parse(source);
        Assert.AreEqual("content(first-letter)", parsed.Style.Content);
    }

    [TestMethod]
    [Spec("css-content-3", "#content-function")]
    [Spec("css-gcpm-3", "#content-function-header")]
    public void CssContentParseContentFunctionWithString()
    {
        var source = "a::before{content:\"Chapter \" content(text)}";
        var parsed = Parse(source);
        Assert.AreEqual("\"Chapter \" content()", parsed.Style.Content);
    }

    [TestMethod]
    [Spec("css-content-3", "#content-function")]
    [Spec("css-gcpm-3", "#content-function-header")]
    public void CssContentParseContentFunctionWithCounter()
    {
        var source = "a::before{content:content(before) \" - \" counter(page)}";
        var parsed = Parse(source);
        Assert.AreEqual("content(before) \" - \" counter(page)", parsed.Style.Content);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-function")]
    [Spec("css-gcpm-3", "#using-named-strings")]
    public void CssContentParseStringFunction()
    {
        var source = "a::before{content:string(chapter)}";
        var parsed = Parse(source);
        Assert.AreEqual("string(chapter)", parsed.Style.Content);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-function")]
    [Spec("css-gcpm-3", "#using-named-strings")]
    public void CssContentParseStringFunctionWithFirstKeyword()
    {
        var source = "a::before{content:string(chapter, first)}";
        var parsed = Parse(source);
        StringAssert.Contains(parsed.Style.Content, "string(chapter");
    }

    [TestMethod]
    [Spec("css-content-3", "#string-function")]
    [Spec("css-gcpm-3", "#using-named-strings")]
    public void CssContentParseStringFunctionWithLastKeyword()
    {
        var source = "a::before{content:string(chapter, last)}";
        var parsed = Parse(source);
        StringAssert.Contains(parsed.Style.Content, "string(chapter");
    }

    [TestMethod]
    [Spec("css-content-3", "#string-function")]
    [Spec("css-gcpm-3", "#using-named-strings")]
    public void CssContentParseStringFunctionWithStartKeyword()
    {
        var source = "a::before{content:string(chapter, start)}";
        var parsed = Parse(source);
        StringAssert.Contains(parsed.Style.Content, "string(chapter");
    }

    [TestMethod]
    [Spec("css-content-3", "#string-function")]
    [Spec("css-gcpm-3", "#using-named-strings")]
    public void CssContentParseStringFunctionWithFirstExceptKeyword()
    {
        var source = "a::before{content:string(chapter, first-except)}";
        var parsed = Parse(source);
        StringAssert.Contains(parsed.Style.Content, "string(chapter");
    }

    [TestMethod]
    [Spec("css-content-3", "#string-function")]
    [Spec("css-gcpm-3", "#using-named-strings")]
    public void CssContentParseStringFunctionWithStringLiteral()
    {
        var source = "a::before{content:\"Chapter: \" string(chapter)}";
        var parsed = Parse(source);
        Assert.AreEqual("\"Chapter: \" string(chapter)", parsed.Style.Content);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-function")]
    [Spec("css-gcpm-3", "#using-named-strings")]
    public void CssContentParseStringFunctionWithCounter()
    {
        var source = "a::before{content:string(chapter) \" - Page \" counter(page)}";
        var parsed = Parse(source);
        Assert.AreEqual("string(chapter) \" - Page \" counter(page)", parsed.Style.Content);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-function")]
    [Spec("css-gcpm-3", "#using-named-strings")]
    public void CssContentParseMultipleStringFunctions()
    {
        var source = "a::before{content:string(chapter) \" / \" string(section)}";
        var parsed = Parse(source);
        Assert.AreEqual("string(chapter) \" / \" string(section)", parsed.Style.Content);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-function")]
    [Spec("css-gcpm-3", "#using-named-strings")]
    public void CssContentParseStringFunctionWithContentFunction()
    {
        var source = "a::before{content:string(chapter) \": \" content(text)}";
        var parsed = Parse(source);
        Assert.AreEqual("string(chapter) \": \" content()", parsed.Style.Content);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-function")]
    [Spec("css-gcpm-3", "#using-named-strings")]
    public void CssContentParseStringFunctionWithAttr()
    {
        var source = "a::before{content:string(chapter) \" - \" attr(title)}";
        var parsed = Parse(source);
        Assert.AreEqual("string(chapter) \" - \" attr(title)", parsed.Style.Content);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-function")]
    [Spec("css-gcpm-3", "#using-named-strings")]
    public void CssContentParseComplexCombination()
    {
        var source = "a::before{content:\"Part \" string(part, first) \" - Chapter \" string(chapter) \" (Page \" counter(page) \")\"}";
        var parsed = Parse(source);
        // Verify the main components are present
        StringAssert.Contains(parsed.Style.Content, "\"Part \"");
        StringAssert.Contains(parsed.Style.Content, "string(part");
        StringAssert.Contains(parsed.Style.Content, "\" - Chapter \"");
        StringAssert.Contains(parsed.Style.Content, "string(chapter)");
        StringAssert.Contains(parsed.Style.Content, "counter(page)");
    }
}
