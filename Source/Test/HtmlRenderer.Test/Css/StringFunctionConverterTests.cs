using System.Linq;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Parsing of string(). GetCssTokens already drops whitespace tokens, the whitespace filters below change nothing.
/// </summary>
[TestClass]
public sealed class StringFunctionConverterTests
{
    [TestMethod]
    [Spec("css-syntax-3", "#consume-function")]
    public void StringFunction_WithNameOnly_ParsesCorrectly()
    {
        const string input = "string(chapter)";
        var tokens = CssValueParser.GetCssTokens(input);

        Assert.AreEqual(1, tokens.Count);
        var token = tokens[0];
        Assert.IsInstanceOfType<FunctionToken>(token);

        var functionToken = (FunctionToken)token;
        Assert.AreEqual("string", functionToken.Data);
        Assert.IsTrue(functionToken.ArgumentTokens.Any());

        // First argument should be the identifier "chapter"
        var firstArg = functionToken.ArgumentTokens.First(t => t.Type != TokenType.Whitespace);
        Assert.IsInstanceOfType<KeywordToken>(firstArg);
        Assert.AreEqual("chapter", ((KeywordToken)firstArg).Data);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-function")]
    public void StringFunction_WithFirstKeyword_ParsesCorrectly()
    {
        const string input = "string(chapter, first)";
        var tokens = CssValueParser.GetCssTokens(input);

        Assert.AreEqual(1, tokens.Count);
        var token = tokens[0];
        Assert.IsInstanceOfType<FunctionToken>(token);

        var functionToken = (FunctionToken)token;
        Assert.AreEqual("string", functionToken.Data);

        var args = functionToken.ArgumentTokens
            .Where(t => t.Type != TokenType.Whitespace && t.Type != TokenType.Comma)
            .ToArray();

        Assert.AreEqual(2, args.Length);
        Assert.AreEqual("chapter", ((KeywordToken)args[0]).Data);
        Assert.AreEqual("first", ((KeywordToken)args[1]).Data);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-function")]
    public void StringFunction_WithLastKeyword_ParsesCorrectly()
    {
        const string input = "string(chapter, last)";
        var tokens = CssValueParser.GetCssTokens(input);

        var functionToken = (FunctionToken)tokens[0];
        var args = functionToken.ArgumentTokens
            .Where(t => t.Type != TokenType.Whitespace && t.Type != TokenType.Comma)
            .ToArray();

        Assert.AreEqual(2, args.Length);
        Assert.AreEqual("chapter", ((KeywordToken)args[0]).Data);
        Assert.AreEqual("last", ((KeywordToken)args[1]).Data);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-function")]
    public void StringFunction_WithStartKeyword_ParsesCorrectly()
    {
        const string input = "string(chapter, start)";
        var tokens = CssValueParser.GetCssTokens(input);

        var functionToken = (FunctionToken)tokens[0];
        var args = functionToken.ArgumentTokens
            .Where(t => t.Type != TokenType.Whitespace && t.Type != TokenType.Comma)
            .ToArray();

        Assert.AreEqual(2, args.Length);
        Assert.AreEqual("chapter", ((KeywordToken)args[0]).Data);
        Assert.AreEqual("start", ((KeywordToken)args[1]).Data);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-function")]
    public void StringFunction_WithFirstExceptKeyword_ParsesCorrectly()
    {
        const string input = "string(chapter, first-except)";
        var tokens = CssValueParser.GetCssTokens(input);

        var functionToken = (FunctionToken)tokens[0];
        var args = functionToken.ArgumentTokens
            .Where(t => t.Type != TokenType.Whitespace && t.Type != TokenType.Comma)
            .ToArray();

        Assert.AreEqual(2, args.Length);
        Assert.AreEqual("chapter", ((KeywordToken)args[0]).Data);
        Assert.AreEqual("first-except", ((KeywordToken)args[1]).Data);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-function")]
    public void StringFunction_WithHyphenatedName_ParsesCorrectly()
    {
        const string input = "string(my-chapter-title)";
        var tokens = CssValueParser.GetCssTokens(input);

        var functionToken = (FunctionToken)tokens[0];
        var args = functionToken.ArgumentTokens
            .Where(t => t.Type != TokenType.Whitespace)
            .ToArray();

        Assert.AreEqual(1, args.Length);
        Assert.AreEqual("my-chapter-title", ((KeywordToken)args[0]).Data);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-function")]
    public void StringFunction_WithUnderscoreName_ParsesCorrectly()
    {
        const string input = "string(chapter_title)";
        var tokens = CssValueParser.GetCssTokens(input);

        var functionToken = (FunctionToken)tokens[0];
        var args = functionToken.ArgumentTokens
            .Where(t => t.Type != TokenType.Whitespace)
            .ToArray();

        Assert.AreEqual(1, args.Length);
        Assert.AreEqual("chapter_title", ((KeywordToken)args[0]).Data);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-function")]
    [Spec("css-gcpm-3", "#using-named-strings")]
    public void StringFunction_InContentProperty_ParsesCorrectly()
    {
        const string input = "content: string(chapter)";
        var parser = new StylesheetParser();
        var stylesheet = parser.Parse($"div {{ {input} }}");
        var rule = stylesheet.Rules[0] as StyleRule;

        Assert.IsNotNull(rule);
        Assert.AreEqual("string(chapter)", rule.Style.Content);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-function")]
    [Spec("css-gcpm-3", "#using-named-strings")]
    public void StringFunction_CombinedWithStringLiteral_ParsesCorrectly()
    {
        const string input = "content: \"Chapter: \" string(chapter)";
        var parser = new StylesheetParser();
        var stylesheet = parser.Parse($"div {{ {input} }}");
        var rule = stylesheet.Rules[0] as StyleRule;

        Assert.IsNotNull(rule);
        Assert.AreEqual("\"Chapter: \" string(chapter)", rule.Style.Content);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-function")]
    [Spec("css-gcpm-3", "#using-named-strings")]
    public void StringFunction_CombinedWithCounter_ParsesCorrectly()
    {
        const string input = "content: string(chapter) \" - Page \" counter(page)";
        var parser = new StylesheetParser();
        var stylesheet = parser.Parse($"div {{ {input} }}");
        var rule = stylesheet.Rules[0] as StyleRule;

        Assert.IsNotNull(rule);
        Assert.AreEqual("string(chapter) \" - Page \" counter(page)", rule.Style.Content);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-function")]
    [Spec("css-gcpm-3", "#using-named-strings")]
    public void StringFunction_MultipleInContent_ParsesCorrectly()
    {
        const string input = "content: string(chapter) \" / \" string(section)";
        var parser = new StylesheetParser();
        var stylesheet = parser.Parse($"div {{ {input} }}");
        var rule = stylesheet.Rules[0] as StyleRule;

        Assert.IsNotNull(rule);
        Assert.AreEqual("string(chapter) \" / \" string(section)", rule.Style.Content);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-function")]
    public void StringFunction_WithWhitespaceAroundComma_ParsesCorrectly()
    {
        const string input = "string(chapter , last)";
        var tokens = CssValueParser.GetCssTokens(input);

        var functionToken = (FunctionToken)tokens[0];
        var args = functionToken.ArgumentTokens
            .Where(t => t.Type != TokenType.Whitespace && t.Type != TokenType.Comma)
            .ToArray();

        Assert.AreEqual(2, args.Length);
        Assert.AreEqual("chapter", ((KeywordToken)args[0]).Data);
        Assert.AreEqual("last", ((KeywordToken)args[1]).Data);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-function")]
    public void StringFunction_CaseInsensitiveKeyword_ParsesCorrectly()
    {
        const string input = "string(chapter, LAST)";
        var tokens = CssValueParser.GetCssTokens(input);

        var functionToken = (FunctionToken)tokens[0];
        var args = functionToken.ArgumentTokens
            .Where(t => t.Type != TokenType.Whitespace && t.Type != TokenType.Comma)
            .ToArray();

        Assert.AreEqual(2, args.Length);
        // Keywords are typically lowercased during parsing
        Assert.AreEqual("LAST", ((KeywordToken)args[1]).Data);
    }
}
