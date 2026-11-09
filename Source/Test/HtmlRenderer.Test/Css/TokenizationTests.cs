using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class TokenizationTests
{
    [TestMethod]
    [Spec("css21", "grammar.html#scanner")]
    public void CssParserIdentifier()
    {
        var teststring = "h1 { background: blue; }";
        var tokenizer = new Lexer(new TextSource(teststring));
        var token = tokenizer.Get();
        Assert.AreEqual(TokenType.Ident, token.Type);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#scanner")]
    public void CssParserAtRule()
    {
        var teststring = "@media { background: blue; }";
        var tokenizer = new Lexer(new TextSource(teststring));
        var token = tokenizer.Get();
        Assert.AreEqual(TokenType.AtKeyword, token.Type);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#scanner")]
    public void CssParserUrlUnquoted()
    {
        var url = "http://someurl";
        var teststring = "url(" + url + ")";
        var tokenizer = new Lexer(new TextSource(teststring));
        var token = tokenizer.Get();
        Assert.AreEqual(url, token.Data);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#scanner")]
    public void CssParserUrlDoubleQuoted()
    {
        var url = "http://someurl";
        var teststring = "url(\"" + url + "\")";
        var tokenizer = new Lexer(new TextSource(teststring));
        var token = tokenizer.Get();
        Assert.AreEqual(url, token.Data);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#scanner")]
    public void CssParserUrlSingleQuoted()
    {
        var url = "http://someurl";
        var teststring = "url('" + url + "')";
        var tokenizer = new Lexer(new TextSource(teststring));
        var token = tokenizer.Get();
        Assert.AreEqual(url, token.Data);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#scanner")]
    public void CssParserUrlBad_UnexpectedQuoteInUnquotedUrl_RecoversAtClosingParen()
    {
        var teststring = "url(bad\"value) next";
        var tokenizer = new Lexer(new TextSource(teststring));

        var urlToken = tokenizer.Get();
        Assert.AreEqual(TokenType.Url, urlToken.Type);

        Assert.AreEqual(TokenType.Whitespace, tokenizer.Get().Type);

        var nextToken = tokenizer.Get();
        Assert.AreEqual(TokenType.Ident, nextToken.Type);
        Assert.AreEqual("next", nextToken.Data);
    }

    // In a value, an all-hex hash is a color literal, any other name stays an id hash.
    [TestMethod]
    [Spec("css21", "grammar.html#scanner")]
    public void ValueContextHash()
    {
        static void Check(string input, TokenType expectedType, string expectedData)
        {
            var lexer = new Lexer(new TextSource(input)) { IsInValue = true };
            var token = lexer.Get();
            Assert.AreEqual(expectedType, token.Type);
            Assert.AreEqual(expectedData, token.Data);
            Assert.AreEqual(TokenType.EndOfFile, lexer.Get().Type); // whole name is one token, no trailing ident
        }

        Check("#f00", TokenType.Color, "f00");
        Check("#abc123", TokenType.Color, "abc123");
        Check("#deadbeef", TokenType.Color, "deadbeef");
        Check("#hero", TokenType.Hash, "hero");
        Check("#top", TokenType.Hash, "top");
        Check("#f00bar", TokenType.Hash, "f00bar");
        Check("#\\41", TokenType.Hash, "A"); // an escape in the name → id hash-token (not a color)

        // '#' not followed by a name code point or valid escape is a plain '#' delimiter, not a hash-token.
        var delim = new Lexer(new TextSource("# ")) { IsInValue = true };
        Assert.AreEqual(TokenType.Delim, delim.Get().Type);
    }

    // CSS Syntax 3 §4.3.7: zero, a surrogate or a code point above U+10FFFF becomes U+FFFD
    [TestMethod]
    [DataRow(@"'\0'", "�")]
    [DataRow(@"'\D800'", "�")]
    [DataRow(@"'\DABC'", "�")]
    [DataRow(@"'\DFFF'", "�")]
    [DataRow(@"'\110000'", "�")]
    [DataRow(@"'\D7FF\E000'", "퟿")]
    [DataRow(@"'\10FFFF'", "􏿿")]
    public void LexerInvalidEscapedCodePointInString(string input, string expected)
    {
        var token = new Lexer(new TextSource(input)).Get();
        Assert.AreEqual(TokenType.String, token.Type);
        Assert.AreEqual(expected, token.Data);
    }

    [TestMethod]
    [DataRow(@"a\D800 b")]
    [DataRow(@"a\DFFF b")]
    public void LexerInvalidEscapedCodePointInIdent(string input)
    {
        var token = new Lexer(new TextSource(input)).Get();
        Assert.AreEqual(TokenType.Ident, token.Type);
        Assert.AreEqual("a�b", token.Data);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#scanner")]
    public void LexerOnlyCarriageReturn()
    {
        var teststring = "\r";
        var tokenizer = new Lexer(new TextSource(teststring));
        var token = tokenizer.Get();
        Assert.AreEqual("\n", token.Data);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#scanner")]
    public void LexerCarriageReturnLineFeed()
    {
        var teststring = "\r\n";
        var tokenizer = new Lexer(new TextSource(teststring));
        var token = tokenizer.Get();
        Assert.AreEqual("\n", token.Data);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#scanner")]
    public void LexerOnlyLineFeed()
    {
        var teststring = "\n";
        var tokenizer = new Lexer(new TextSource(teststring));
        var token = tokenizer.Get();
        Assert.AreEqual("\n", token.Data);
    }
}
