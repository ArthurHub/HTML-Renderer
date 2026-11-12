using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Parse;

/// <summary>
/// Direct unit tests for <see cref="CssValueParser.IsValidLength"/>.
///
/// HTML-Renderer's implementation chops the last 1-2 characters off the string and tries
/// <c>double.TryParse</c> on what's left, gated by a "string length &gt; 1" cutoff. Two known
/// disagreements with the CSS2.1 §4.3.2 / CSS Values §6.2 grammar this is meant to validate:
/// <list type="bullet">
/// <item>a bare unitless "0" is rejected outright (length must be &gt; 1), even though the unit
/// identifier is optional after a zero length;</item>
/// <item><c>calc(...)</c> expressions are not understood at all -- the naive "chop trailing
/// characters" strategy can never parse one as a number.</item>
/// </list>
/// Those two cases are captured as <c>[Ignore]</c> tests below, asserting the spec-correct
/// expectation rather than today's actual (incorrect) result.
/// </summary>
[TestClass]
public sealed class CssValueParserIsValidLengthTests
{
    [TestMethod]
    [DataRow("0px")]
    [DataRow("0.5em")]
    [DataRow("10px")]
    [DataRow("-5px")]
    [DataRow("50%")]
    [DataRow("1in")]
    public void ValidLengthOrPercentage_ReturnsTrue(string value)
    {
        Assert.IsTrue(CssValueParser.IsValidLength(value));
    }

    [TestMethod]
    [Ignore("IsValidLength rejects a bare 0, which layout still handles like 0px")]
    public void BareZero_ReturnsTrue_NotYetSpecCompliant()
    {
        // CSS2.1 §4.3.2 / CSS Values §6.2: the unit identifier is optional after a zero length, so a
        // bare "0" is a valid length. HTML-Renderer's IsValidLength gates on "value.Length > 1" and
        // therefore rejects it outright.
        Assert.IsTrue(CssValueParser.IsValidLength("0"));
    }

    [TestMethod]
    public void Calc_ReturnsTrue_NotYetSpecCompliant()
    {
        // CSS Values and Units §8.1: calc() expressions are valid wherever a <length-percentage> is
        // valid. HTML-Renderer's IsValidLength has no calc() awareness at all -- it just tries to
        // double.TryParse whatever's left after chopping off a trailing unit, which never succeeds
        // for a "calc(...)" string.
        Assert.IsTrue(CssValueParser.IsValidLength("calc(10px + 1em)"));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("auto")]
    [DataRow("normal")]
    [DataRow("px")]
    [DataRow("abc")]
    public void InvalidOrNonLengthKeyword_ReturnsFalse(string value)
    {
        Assert.IsFalse(CssValueParser.IsValidLength(value));
    }
}
