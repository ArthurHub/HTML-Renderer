using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Dom;

/// <summary>
/// Ported from PeachPDF.Tests/Html/Core/Dom/CssCounterEngineTests.cs. Unit tests for
/// <see cref="CssCounterEngine.FormatCounterValue"/>, the counter-style resolver <c>content: counter()</c>
/// uses. Pins the CSS Counter Styles Level 3 §2 requirement that an unknown/invalid style falls back to
/// <c>decimal</c> rather than rendering nothing.
/// </summary>
[TestClass]
public sealed class CssCounterEngineTests
{
    [TestMethod]
    [DataRow(1, "decimal", "1")]
    [DataRow(12, "decimal", "12")]
    [DataRow(1, "decimal-leading-zero", "01")]
    [DataRow(9, "decimal-leading-zero", "09")]
    [DataRow(12, "decimal-leading-zero", "12")] // already two digits - no over-padding
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
        // CSS Counter Styles Level 3 §2: an unknown/invalid counter style renders as decimal, never empty.
        Assert.AreEqual(number.ToString(System.Globalization.CultureInfo.InvariantCulture),
            CssCounterEngine.FormatCounterValue(number, "not-a-real-style"));
    }

    [TestMethod]
    [DataRow(0, "upper-roman", "0")]
    [DataRow(-5, "lower-alpha", "-5")]
    [DataRow(0, "lower-greek", "0")]
    public void FormatCounterValue_AlphabeticStyleOutOfRange_FallsBackToDecimal(int number, string style, string expected)
    {
        // Alphabetic/symbolic styles can't represent 0 or negatives; CSS Counter Styles L3 §2 says such
        // out-of-range values render with the fallback style (decimal), not empty.
        Assert.AreEqual(expected, CssCounterEngine.FormatCounterValue(number, style));
    }

    [TestMethod]
    public void FormatCounterValue_StyleMatchIsCaseInsensitive()
    {
        Assert.AreEqual("01", CssCounterEngine.FormatCounterValue(1, "DECIMAL-LEADING-ZERO"));
        Assert.AreEqual("iv", CssCounterEngine.FormatCounterValue(4, "Lower-Roman"));
    }
}
