using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Dom;

[TestClass]
public sealed class CssCounterEngineTests
{
    [TestMethod]
    [Spec("css21", "generate.html#counter-styles")]
    [DataRow(1, "decimal", "1")]
    [DataRow(12, "decimal", "12")]
    [DataRow(1, "decimal-leading-zero", "01")]
    [DataRow(9, "decimal-leading-zero", "09")]
    [DataRow(12, "decimal-leading-zero", "12")]
    [DataRow(100, "decimal-leading-zero", "100")]
    [DataRow(4, "lower-roman", "iv")]
    [DataRow(4, "upper-roman", "IV")]
    [DataRow(1, "lower-alpha", "a")]
    [DataRow(3, "upper-alpha", "C")]
    public void FormatCounterValue_KnownStyles_FormatAsExpected(int number, string style, string expected)
    {
        Assert.AreEqual(expected, CssCounterEngine.FormatCounterValue(number, style));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(7)]
    [DataRow(42)]
    public void FormatCounterValue_UnknownStyle_FallsBackToDecimal(int number)
    {
        // CSS Counter Styles 3 §2
        Assert.AreEqual(number.ToString(System.Globalization.CultureInfo.InvariantCulture),
            CssCounterEngine.FormatCounterValue(number, "not-a-real-style"));
    }

    [TestMethod]
    [DataRow(0, "upper-roman", "0")]
    [DataRow(-5, "lower-alpha", "-5")]
    [DataRow(0, "lower-greek", "0")]
    [DataRow(-3, "lower-roman", "-3")]
    [DataRow(-3, "upper-roman", "-3")]
    [DataRow(4000, "lower-roman", "4000")]
    public void FormatCounterValue_AlphabeticStyleOutOfRange_FallsBackToDecimal(int number, string style, string expected)
    {
        // CSS Counter Styles 3 §2, values out of the style's range use the fallback style
        Assert.AreEqual(expected, CssCounterEngine.FormatCounterValue(number, style));
    }

    [TestMethod]
    public void FormatCounterValue_StyleMatchIsCaseInsensitive()
    {
        Assert.AreEqual("01", CssCounterEngine.FormatCounterValue(1, "DECIMAL-LEADING-ZERO"));
        Assert.AreEqual("iv", CssCounterEngine.FormatCounterValue(4, "Lower-Roman"));
    }
}
