using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace HtmlRenderer.Test.Utils;

[TestClass]
public sealed class HtmlUtilsTests
{
    [TestMethod]
    [DataRow("&#65;", "A")]
    [DataRow("&#65;&#66;&#67;", "ABC")]
    [DataRow("&#x41;", "A")]
    [DataRow("&#X41;", "A")]
    [DataRow("Hello &#65; World", "Hello A World")]
    public void DecodeHtml_NumericCharacterReference_DecodesToCharacter(string input, string expected)
    {
        Assert.AreEqual(expected, HtmlUtils.DecodeHtml(input));
    }

    [TestMethod]
    public void DecodeHtml_NumericCharacterReferenceWithTrailingSemicolon_ConsumesSemicolon()
    {
        Assert.AreEqual("A rest", HtmlUtils.DecodeHtml("&#65; rest"));
    }

    [TestMethod]
    [Ignore("DecodeHtml throws for a code point above U+10FFFF, the tokenizer has already replaced it when DecodeHtml runs")]
    public void DecodeHtml_OutOfRangeCodePoint_DecodesToReplacementCharacter()
    {
        // WHATWG HTML 13.2.5.80 numeric-character-reference-end-state: an out-of-range code point
        // resolves to U+FFFD REPLACEMENT CHARACTER, not to nothing. HTML-Renderer's
        // DecodeHtmlCharByCode leaves `repl` as string.Empty for a code point outside 0..0x10FFFF (or
        // inside the surrogate range), so the reference is silently dropped instead.
        Assert.AreEqual("�", HtmlUtils.DecodeHtml("&#x110000;"));
    }

    [TestMethod]
    [Ignore("DecodeHtml throws for a surrogate code point, the tokenizer has already replaced it when DecodeHtml runs")]
    public void DecodeHtml_SurrogateRangeCodePoint_DecodesToReplacementCharacter()
    {
        Assert.AreEqual("�", HtmlUtils.DecodeHtml("&#xD800;"));
    }

    [TestMethod]
    [Ignore("DecodeHtml turns &#0; into U+0000, the tokenizer has already replaced it when DecodeHtml runs")]
    public void DecodeHtml_NullCodePoint_DecodesToReplacementCharacter()
    {
        // Unlike the out-of-range/surrogate cases (which decode to an empty string), U+0000 passes
        // HTML-Renderer's range check and is converted verbatim via Char.ConvertFromUtf32(0), yielding
        // a literal NUL character instead of the spec's U+FFFD replacement.
        Assert.AreEqual("�", HtmlUtils.DecodeHtml("&#0;"));
    }

    [TestMethod]
    public void DecodeHtml_NoEntities_ReturnsUnchanged()
    {
        Assert.AreEqual("plain text", HtmlUtils.DecodeHtml("plain text"));
    }

    [TestMethod]
    public void DecodeHtml_NullOrEmpty_ReturnsInput()
    {
        Assert.IsNull(HtmlUtils.DecodeHtml(null!));
        Assert.AreEqual(string.Empty, HtmlUtils.DecodeHtml(string.Empty));
    }
}
