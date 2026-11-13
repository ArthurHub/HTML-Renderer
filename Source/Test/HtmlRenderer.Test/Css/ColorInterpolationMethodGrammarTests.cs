using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// <see cref="ColorInterpolationMethodGrammar"/> only takes the <c>in …</c> part out of a gradient's
/// arguments, the remainder is validated by the gradient, e.g. a hue method after a rectangular space.
/// </summary>
[TestClass]
public sealed class ColorInterpolationMethodGrammarTests
{
    private static (bool Ok, bool HasIn, string? Remainder) Extract(string value)
    {
        var ok = ColorInterpolationMethodGrammar.TryExtractInterpolationMethod(
            CssValueParser.GetCssTokens(value), out var remainder, out var hasIn);
        // GetCssTokens does not emit whitespace tokens, so rejoin the remainder's tokens with a space.
        var text = ok && remainder != null ? string.Join(" ", remainder.Select(t => t.ToValue())) : null;
        return (ok, hasIn, text);
    }

    [TestMethod]
    public void NoInClause_PassesThroughUnchanged()
    {
        var (ok, hasIn, remainder) = Extract("to right");
        Assert.IsTrue(ok);
        Assert.IsFalse(hasIn);
        Assert.AreEqual("to right", remainder);
    }

    [TestMethod]
    [Spec("css-color-4", "#interpolation-space")]
    [DataRow("in srgb")]
    [DataRow("in srgb-linear")]
    [DataRow("in display-p3")]
    [DataRow("in lab")]
    [DataRow("in oklab")]
    [DataRow("in xyz")]
    [DataRow("in xyz-d65")]
    [DataRow("in xyz-d50")]
    [DataRow("in hsl")]
    [DataRow("in hwb")]
    [DataRow("in lch")]
    [DataRow("in oklch")]
    [DataRow("in oklch shorter hue")]
    [DataRow("in oklch longer hue")]
    [DataRow("in hwb increasing hue")]
    [DataRow("in lch decreasing hue")]
    [DataRow("in a98-rgb")]
    [DataRow("in prophoto-rgb")]
    [DataRow("in rec2020")]
    public void ValidMethodOnly_HasInWithEmptyRemainder(string value)
    {
        var (ok, hasIn, remainder) = Extract(value);
        Assert.IsTrue(ok);
        Assert.IsTrue(hasIn);
        Assert.AreEqual("", remainder);
    }

    [TestMethod]
    [DataRow("in oklab to right", "to right")]
    [DataRow("to right in oklab", "to right")]
    [DataRow("in oklch longer hue to right", "to right")]
    // A hue method on a rectangular space is not rejected by the grammar - the leftover "longer hue"
    // is returned as the remainder for the caller's direction validation to reject.
    [DataRow("in oklab longer hue", "longer hue")]
    public void ValidMethodWithRemainder_ExtractsDirectionRemainder(string value, string expectedRemainder)
    {
        var (ok, hasIn, remainder) = Extract(value);
        Assert.IsTrue(ok);
        Assert.IsTrue(hasIn);
        Assert.AreEqual(expectedRemainder, remainder);
    }

    [TestMethod]
    [DataRow("in nonsense")]            // unknown space
    [DataRow("in")]                     // "in" with nothing after
    [DataRow("in oklch longer")]        // polar hue direction without the trailing "hue"
    public void InvalidMethod_ReturnsFalse(string value)
    {
        var ok = ColorInterpolationMethodGrammar.TryExtractInterpolationMethod(
            CssValueParser.GetCssTokens(value), out _, out _);
        Assert.IsFalse(ok);
    }

    [TestMethod]
    [DataRow("srgb")]
    [DataRow("oklab")]
    [DataRow("oklch")]
    [DataRow("a98-rgb")]
    [DataRow("prophoto-rgb")]
    public void IsColorSpace_RecognizesSupportedSpaces(string name) =>
        Assert.IsTrue(ColorInterpolationMethodGrammar.IsColorSpace(name));

    [TestMethod]
    [DataRow("banana")]
    public void IsColorSpace_RejectsUnsupportedOrUnknown(string name) =>
        Assert.IsFalse(ColorInterpolationMethodGrammar.IsColorSpace(name));

    [TestMethod]
    [DataRow("oklch", true)]
    [DataRow("hsl", true)]
    [DataRow("oklab", false)]
    [DataRow("srgb", false)]
    public void IsPolarColorSpace_ClassifiesPolarVsRectangular(string name, bool polar) =>
        Assert.AreEqual(polar, ColorInterpolationMethodGrammar.IsPolarColorSpace(name));
}
