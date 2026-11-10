using System;
using System.Linq;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// <see cref="CalcEvaluator"/> on non-length calc() values: angles evaluate to radians, times to
/// milliseconds and resolutions to dppx.
/// </summary>
[TestClass]
public sealed class CalcEvaluatorTests
{
    private static double? EvaluateAngle(string calc)
    {
        var function = CssValueParser.GetCssTokens(calc).OfType<FunctionToken>().Single();
        var node = CalcParser.Parse(function);
        Assert.IsNotNull(node);
        // A full turn is 2π radians; em/rem factors are irrelevant to an angle calc.
        return CalcEvaluator.Evaluate(node!, new CalcContext(2.0 * Math.PI, 0, 0, false));
    }

    [TestMethod]
    [Spec("css-values-4", "#angles")]
    [DataRow("calc(1turn * 0.35)", 0.35)]
    [DataRow("calc(1turn * 0.5)", 0.5)]
    [DataRow("calc(0.25turn)", 0.25)]
    public void AngleCalc_TurnScaled_EvaluatesToRadians(string calc, double expectedTurns)
    {
        var radians = EvaluateAngle(calc);
        Assert.IsNotNull(radians);
        Assert.AreEqual(expectedTurns * 2.0 * Math.PI, radians!.Value, 1e-4);
    }

    [TestMethod]
    [Spec("css-values-4", "#angles")]
    public void AngleCalc_Degrees_EvaluatesToRadians()
    {
        var radians = EvaluateAngle("calc(90deg + 90deg)");
        Assert.IsNotNull(radians);
        Assert.AreEqual(Math.PI, radians!.Value, 1e-4); // 180deg
    }

    // Only @property syntax validation produces time and resolution calc() values.

    private static double? Evaluate(string calc)
    {
        var function = CssValueParser.GetCssTokens(calc).OfType<FunctionToken>().Single();
        var node = CalcParser.Parse(function);
        Assert.IsNotNull(node);
        return CalcEvaluator.Evaluate(node!, new CalcContext(0, 0, 0, false));
    }

    [TestMethod]
    [DataRow("calc(1s + 2s)", 3000.0)]    // canonical unit is milliseconds
    [DataRow("calc(500ms)", 500.0)]
    [DataRow("calc(2s * 3)", 6000.0)]
    public void TimeCalc_EvaluatesToMilliseconds(string calc, double expectedMs)
    {
        Assert.AreEqual(expectedMs, Evaluate(calc)!.Value, 1e-3);
    }

    [TestMethod]
    [Spec("css-values-4", "#resolution")]
    [DataRow("calc(2dppx * 2)", 4.0)]     // canonical unit is dots-per-pixel
    [DataRow("calc(96dpi)", 1.0)]         // 96dpi == 1dppx
    public void ResolutionCalc_EvaluatesToDotsPerPixel(string calc, double expectedDppx)
    {
        Assert.AreEqual(expectedDppx, Evaluate(calc)!.Value, 1e-3);
    }

    private static string Serialize(string calc, CalcCategory category)
    {
        var function = CssValueParser.GetCssTokens(calc).OfType<FunctionToken>().Single();
        var node = CalcParser.Parse(function);
        Assert.IsNotNull(node);
        return CalcSerializer.Serialize(node!, category);
    }

    [TestMethod]
    [DataRow("calc(1s + 2s)", "time", "calc(1s + 2s)")]
    [DataRow("calc(2dppx * 2)", "resolution", "calc(2dppx * 2)")]
    public void TimeResolutionCalc_SerializesToNormalizedText(string calc, string kind, string expected)
    {
        var category = kind == "time" ? CalcCategory.Time : CalcCategory.Resolution;
        Assert.AreEqual(expected, Serialize(calc, category));
    }
}
