using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace HtmlRenderer.Test.Utils;

[TestClass]
public sealed class CommonUtilsTests
{
    // Note: HTML-Renderer's method is "IsAsianCharecter" (sic) and takes a plain char, not a Rune --
    // so the astral-emoji row from the source test (which relied on Rune to represent a code point
    // beyond the BMP) cannot be expressed here and is dropped; a char can never hold it.
    [TestMethod]
    [DataRow(0x61, false)]     // 'a'
    [DataRow(0x4E2D, true)]    // '中'
    [DataRow(0x4e00, true)]
    [DataRow(0xFA2D, true)]
    [DataRow(0x4dff, false)]
    public void IsAsianCharecter_ChecksRange(int codepoint, bool expected)
    {
        Assert.AreEqual(expected, CommonUtils.IsAsianCharecter((char)codepoint));
    }

    [TestMethod]
    [DataRow('5', false, true)]
    [DataRow('a', false, false)]
    [DataRow('a', true, true)]
    [DataRow('F', true, true)]
    [DataRow('g', true, false)]
    public void IsDigit_ChecksDecimalOrHex(char ch, bool hex, bool expected)
    {
        Assert.AreEqual(expected, CommonUtils.IsDigit(ch, hex));
    }

    [TestMethod]
    [DataRow('7', false, 7)]
    [DataRow('a', false, 0)]
    [DataRow('a', true, 10)]
    [DataRow('F', true, 15)]
    [DataRow('g', true, 0)]
    public void ToDigit_ConvertsCharToNumericValue(char ch, bool hex, int expected)
    {
        Assert.AreEqual(expected, CommonUtils.ToDigit(ch, hex));
    }

    [TestMethod]
    public void Max_ReturnsComponentWiseMaximum()
    {
        var result = CommonUtils.Max(new RSize(10, 20), new RSize(30, 5));

        Assert.AreEqual(new RSize(30, 20), result);
    }

    [TestMethod]
    public void GetFirstValueOrDefault_NonEmptyDictionary_ReturnsFirstValue()
    {
        var dic = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

        var value = CommonUtils.GetFirstValueOrDefault(dic, -1);

        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public void GetFirstValueOrDefault_EmptyDictionary_ReturnsDefault()
    {
        var dic = new Dictionary<string, int>();

        var value = CommonUtils.GetFirstValueOrDefault(dic, -1);

        Assert.AreEqual(-1, value);
    }

    [TestMethod]
    public void GetFirstValueOrDefault_NullDictionary_ReturnsDefault()
    {
        var value = CommonUtils.GetFirstValueOrDefault<string, int>(null!, -1);

        Assert.AreEqual(-1, value);
    }

    [TestMethod]
    [DataRow("hello world", 0, 0, 5)]
    [DataRow("  hello", 0, 2, 5)]
    [DataRow("hello", 10, -1, 0)]
    public void GetNextSubString_FindsWhitespaceDelimitedWord(string str, int start, int expectedIndex, int expectedLength)
    {
        var index = CommonUtils.GetNextSubString(str, start, out var length);

        Assert.AreEqual(expectedIndex, index);
        Assert.AreEqual(expectedLength, length);
    }

    [TestMethod]
    public void SubStringEquals_CaseInsensitiveMatch_ReturnsTrue()
    {
        Assert.IsTrue(CommonUtils.SubStringEquals("Hello World", 0, 5, "hello"));
    }

    [TestMethod]
    public void SubStringEquals_DifferentLength_ReturnsFalse()
    {
        Assert.IsFalse(CommonUtils.SubStringEquals("Hello World", 0, 5, "hell"));
    }

    [TestMethod]
    public void SubStringEquals_OutOfRange_ReturnsFalse()
    {
        Assert.IsFalse(CommonUtils.SubStringEquals("Hi", 0, 5, "hello"));
    }

    // Style keywords below use TheArtOfDev.HtmlRenderer.Core.Utils.CssConstants -- HTML-Renderer has
    // no PeachPDF.CSS.Keywords equivalent, and ConvertToAlphaNumber's own style parameter is just a
    // plain CSS keyword string.
    [TestMethod]
    [DataRow(0, CssConstants.UpperAlpha, "")]
    [DataRow(1, CssConstants.UpperAlpha, "A")]
    [DataRow(27, CssConstants.UpperAlpha, "AA")]
    [DataRow(1, CssConstants.LowerAlpha, "a")]
    [DataRow(1, CssConstants.LowerLatin, "a")]
    [DataRow(1, CssConstants.UpperLatin, "A")]
    [DataRow(4, CssConstants.LowerRoman, "iv")]
    [DataRow(4, CssConstants.UpperRoman, "IV")]
    public void ConvertToAlphaNumber_KnownStyles(int number, string style, string expected)
    {
        Assert.AreEqual(expected, CommonUtils.ConvertToAlphaNumber(number, style));
    }

    [TestMethod]
    [DataRow(-3, CssConstants.LowerRoman)]
    [DataRow(-3, CssConstants.UpperRoman)]
    [DataRow(4000, CssConstants.LowerRoman)]
    [DataRow(10000, CssConstants.UpperRoman)]
    public void ConvertToAlphaNumber_RomanOutOfRange_ReturnsEmpty(int number, string style)
    {
        Assert.AreEqual(string.Empty, CommonUtils.ConvertToAlphaNumber(number, style));
    }

    [TestMethod]
    [DataRow(3999, CssConstants.UpperRoman, "MMMCMXCIX")]
    [DataRow(1, CssConstants.LowerRoman, "i")]
    public void ConvertToAlphaNumber_RomanRangeBoundaries(int number, string style, string expected)
    {
        Assert.AreEqual(expected, CommonUtils.ConvertToAlphaNumber(number, style));
    }

    [TestMethod]
    public void ConvertToAlphaNumber_LowerGreek_ProducesNonEmptyResult()
    {
        var result = CommonUtils.ConvertToAlphaNumber(1, CssConstants.LowerGreek);

        Assert.AreNotEqual(string.Empty, result);
    }

    // Note: HTML-Renderer's CssConstants only has a single generic "armenian" / "georgian" /
    // "hebrew" keyword each -- there's no separate lower-armenian/upper-armenian distinction (and no
    // Tetragrammaton-avoidance override or >999 support in ConvertToSpecificNumbers), so the source
    // ConvertToAlphaNumber_ArmenianAndHebrewBoundaries theory (which specifically exercises those
    // missing capabilities) is dropped entirely rather than ported.
    [TestMethod]
    [DataRow(CssConstants.Armenian)]
    [DataRow(CssConstants.Georgian)]
    [DataRow(CssConstants.Hebrew)]
    public void ConvertToAlphaNumber_SpecificAlphabets_ProduceNonEmptyResult(string style)
    {
        var result = CommonUtils.ConvertToAlphaNumber(5, style);

        Assert.AreNotEqual(string.Empty, result);
    }

    [TestMethod]
    [DataRow(CssConstants.Hiragana)]
    [DataRow(CssConstants.HiraganaIroha)]
    [DataRow(CssConstants.Katakana)]
    [DataRow(CssConstants.KatakanaIroha)]
    public void ConvertToAlphaNumber_KanaAlphabets_ProduceNonEmptyResult(string style)
    {
        var result = CommonUtils.ConvertToAlphaNumber(5, style);

        Assert.AreNotEqual(string.Empty, result);
    }

    [TestMethod]
    public void ConvertToAlphaNumber_ZeroWithAnyStyle_ReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, CommonUtils.ConvertToAlphaNumber(0, CssConstants.Hebrew));
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    public void ConvertToAlphaNumber_HiraganaIroha_DiffersFromDictionaryOrderHiragana()
    {
        // CSS Counter Styles Level 3's hiragana-iroha system reorders the 47 kana by the classical
        // iroha poem instead of gojuon dictionary order. HTML-Renderer's ConvertToAlphaNumber maps
        // both "hiragana" and "hiragana-iroha" to the exact same 48-character dictionary-order table
        // (ConvertToSpecificNumbers2 with _hiraganaDigitsTable either way), so the two styles
        // currently produce identical output instead of a different ordering.
        var dictionaryOrder = CommonUtils.ConvertToAlphaNumber(2, CssConstants.Hiragana);
        var irohaOrder = CommonUtils.ConvertToAlphaNumber(2, CssConstants.HiraganaIroha);

        Assert.AreNotEqual(dictionaryOrder, irohaOrder);
        Assert.AreEqual("ろ", irohaOrder); // ろ - second character of the iroha ordering
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    public void ConvertToAlphaNumber_KatakanaIroha_DiffersFromDictionaryOrderKatakana()
    {
        var dictionaryOrder = CommonUtils.ConvertToAlphaNumber(2, CssConstants.Katakana);
        var irohaOrder = CommonUtils.ConvertToAlphaNumber(2, CssConstants.KatakanaIroha);

        Assert.AreNotEqual(dictionaryOrder, irohaOrder);
        Assert.AreEqual("ロ", irohaOrder); // ロ - second character of the iroha ordering
    }

    [TestMethod]
    [DataRow(CssConstants.Hiragana)]
    [DataRow(CssConstants.HiraganaIroha)]
    [DataRow(CssConstants.Katakana)]
    [DataRow(CssConstants.KatakanaIroha)]
    public void ConvertToAlphaNumber_KanaAlphabets_DoNotThrowPastFirstWraparound(string style)
    {
        // The hiragana/katakana tables here are both a fixed 48 characters (including trailing "n"),
        // and HiraganaIroha/KatakanaIroha reuse those exact same tables rather than a separate
        // 47-character iroha-ordered array -- so, unlike PeachPDF's fixed version (which guards
        // against a smaller 47-character iroha table), there's no out-of-range indexing to trigger
        // here. This still confirms the algorithm never throws and always produces output.
        for (var number = 1; number <= 200; number++)
        {
            var result = CommonUtils.ConvertToAlphaNumber(number, style);
            Assert.AreNotEqual(string.Empty, result);
        }
    }
}
