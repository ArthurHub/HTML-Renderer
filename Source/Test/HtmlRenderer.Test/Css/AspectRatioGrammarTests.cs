using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Parse;
using System.Linq;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class AspectRatioGrammarTests
{
    private const double Delta = 1e-5;

    private static bool TryParse(string value, out double? ratio) =>
        AspectRatioGrammar.TryParse(CssValueParser.GetCssTokens(value), out ratio);

    [TestMethod]
    [DataRow("2", 2.0)]
    [DataRow("16 / 9", 16.0 / 9.0)]
    [DataRow("16/9", 16.0 / 9.0)]
    [DataRow("1.5", 1.5)]
    [DataRow("3 / 2", 1.5)]
    [DataRow("auto 21 / 9", 21.0 / 9.0)]
    [DataRow("21 / 9 auto", 21.0 / 9.0)]
    [Spec("css-sizing-4", "#aspect-ratio")]
    public void ValidRatio_ParsesToWidthOverHeight(string value, double expected)
    {
        Assert.IsTrue(TryParse(value, out var ratio));
        Assert.IsNotNull(ratio);
        Assert.AreEqual(expected, ratio!.Value, Delta);
    }

    [TestMethod]
    [DataRow("auto")]
    [DataRow("1 / 0")] // a zero term => no usable ratio
    [DataRow("0")]
    [DataRow("0 / 5")]
    public void AutoOrZeroTerm_IsValidButHasNoUsableRatio(string value)
    {
        Assert.IsTrue(TryParse(value, out var ratio));
        Assert.IsNull(ratio);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("banana")]
    [DataRow("2 3")]        // two numbers without a slash
    [DataRow("16 /")]        // slash without a second number
    [DataRow("/ 9")]         // slash without a first number
    [DataRow("-2")]          // negative number
    [DataRow("2 / -3")]      // negative second term
    [DataRow("auto auto")]   // two autos
    [DataRow("2px")]         // a length, not a number
    public void Invalid_ReturnsFalse(string value)
    {
        Assert.IsFalse(TryParse(value, out _));
    }

    // ─── hasAuto: distinguishing `auto <ratio>` (natural-ratio fallback) from a bare `<ratio>` ───

    private static bool TryParse(string value, out double? ratio, out bool hasAuto) =>
        AspectRatioGrammar.TryParse(CssValueParser.GetCssTokens(value), out ratio, out hasAuto);

    [TestMethod]
    [DataRow("2", 2.0, false)]              // bare ratio: overrides any natural ratio
    [DataRow("16 / 9", 16.0 / 9.0, false)]
    [DataRow("auto 16 / 9", 16.0 / 9.0, true)]  // auto <ratio>: prefer natural, fall back to 16/9
    [DataRow("16 / 9 auto", 16.0 / 9.0, true)]  // the `||` allows either order
    public void HasAuto_DistinguishesAutoRatioFromBareRatio(string value, double expected, bool expectedAuto)
    {
        Assert.IsTrue(TryParse(value, out var ratio, out var hasAuto));
        Assert.IsNotNull(ratio);
        Assert.AreEqual(expected, ratio!.Value, Delta);
        Assert.AreEqual(expectedAuto, hasAuto);
    }

    [TestMethod]
    public void HasAuto_BareAuto_HasAutoTrueAndNullRatio()
    {
        Assert.IsTrue(TryParse("auto", out var ratio, out var hasAuto));
        Assert.IsNull(ratio);
        Assert.IsTrue(hasAuto);
    }

    [TestMethod]
    [DataRow("aspect-ratio: 16 / 9", true)]
    [DataRow("aspect-ratio: auto", true)]
    [DataRow("aspect-ratio: 2", true)]
    [DataRow("aspect-ratio: banana", false)]
    [DataRow("aspect-ratio: 2px", false)]
    [Spec("css-sizing-4", "#aspect-ratio")]
    public void LayerA_AcceptsValid_RejectsInvalid(string declaration, bool shouldApply)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($"div {{ {declaration}; }}");
        var style = sheet.Rules.OfType<StyleRule>().Single().Style;
        var applied = !string.IsNullOrEmpty(style.GetPropertyValue("aspect-ratio"));
        Assert.AreEqual(shouldApply, applied);
    }

    // ─── TryParseRatio: the pure <ratio> data type (no `auto`) used by @property ───

    private static bool TryParseRatio(string value, out double? ratio) =>
        AspectRatioGrammar.TryParseRatio(CssValueParser.GetCssTokens(value), out ratio);

    [TestMethod]
    [DataRow("16/9", 16.0 / 9.0)]
    [DataRow("16 / 9", 16.0 / 9.0)]
    [DataRow("1", 1.0)]
    [DataRow("2", 2.0)]
    [DataRow("3 / 2", 1.5)]
    public void TryParseRatio_ValidRatio_ParsesToWidthOverHeight(string value, double expected)
    {
        Assert.IsTrue(TryParseRatio(value, out var ratio));
        Assert.IsNotNull(ratio);
        Assert.AreEqual(expected, ratio!.Value, Delta);
    }

    [TestMethod]
    [DataRow("0/1")]  // a zero term => valid, but no usable ratio
    [DataRow("0")]
    [DataRow("5 / 0")]
    public void TryParseRatio_ZeroTerm_IsValidButHasNullRatio(string value)
    {
        Assert.IsTrue(TryParseRatio(value, out var ratio));
        Assert.IsNull(ratio);
    }

    [TestMethod]
    [DataRow("auto")]        // <ratio> — unlike aspect-ratio — does NOT permit `auto`
    [DataRow("auto 16 / 9")]
    [DataRow("16 / 9 auto")]
    [DataRow("")]
    [DataRow("banana")]
    [DataRow("2 3")]         // two numbers without a slash
    [DataRow("16 /")]        // slash without a second number
    [DataRow("-2")]          // negative number
    [DataRow("2 / -3")]      // negative second term
    [DataRow("2px")]         // a length, not a number
    public void TryParseRatio_Invalid_ReturnsFalse(string value)
    {
        Assert.IsFalse(TryParseRatio(value, out _));
    }
}
