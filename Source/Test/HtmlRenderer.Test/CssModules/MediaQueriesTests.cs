using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.CssModules;

/// <summary>
/// Evaluates media queries in the CSS engine against an 800x600 screen, a &lt;style&gt; element
/// can't hold the '&lt;' of the range syntax.
/// </summary>
[TestClass]
public sealed class MediaQueriesTests
{
    // ── 2 Media queries ──────────────────────────────────────────────────

    [TestMethod]
    [Spec("mediaqueries-4", "#media")]
    [Spec("mediaqueries-5", "#media")]
    [DataRow("screen and (min-width: 100px)", true)]
    [DataRow("print and (min-width: 100px)", false)]
    [DataRow("screen and (min-width: 1000px)", false)]
    [DataRow("(min-width: 100px) and (min-height: 100px)", true)]
    public void MediaQuery_IsTrueWhenTheTypeAndTheConditionMatch(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#mq-list")]
    [Spec("mediaqueries-5", "#mq-list")]
    [DataRow("print, screen", true)]
    [DataRow("print, (max-width: 100px)", false)]
    public void MediaQueryList_IsTrueWhenAnyQueryIs(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#mq-not")]
    [Spec("mediaqueries-5", "#mq-not")]
    [DataRow("not print", true)]
    [DataRow("not screen", false)]
    [DataRow("not screen and (min-width: 100px)", false)]
    [DataRow("not screen and (max-width: 100px)", true)]
    public void Not_NegatesTheWholeQuery(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#mq-only")]
    [Spec("mediaqueries-5", "#mq-only")]
    [DataRow("only screen", true)]
    [DataRow("only screen and (min-width: 100px)", true)]
    [DataRow("only print", false)]
    public void Only_DoesNotChangeTheResult(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#media-types")]
    [Spec("mediaqueries-5", "#media-types")]
    [DataRow("all", "screen", true)]
    [DataRow("all", "print", true)]
    [DataRow("screen", "screen", true)]
    [DataRow("print", "print", true)]
    [DataRow("screen", "print", false)]
    [DataRow("tv", "screen", false)]
    [DataRow("not tv", "screen", true)]
    [DataRow("speech", "print", false)]
    public void MediaType_MatchesTheDevice(string query, string mediaType, bool expected)
    {
        Assert.AreEqual(expected, Matches(query, mediaType: mediaType));
    }

    // ── 2.4 Media features ───────────────────────────────────────────────

    [TestMethod]
    [Spec("mediaqueries-4", "#mq-boolean-context")]
    [Spec("mediaqueries-5", "#mq-boolean-context")]
    [DataRow("(color)", true)]
    [DataRow("(monochrome)", false)]
    [DataRow("(grid)", false)]
    [DataRow("(update)", false)]
    [DataRow("(pointer)", false)]
    public void BooleanContext_IsTrueForAnythingButZeroOrNone(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [Ignore("Gap, see Spec/mediaqueries-4.yml")]
    [TestMethod]
    [Spec("mediaqueries-4", "#mq-boolean-context")]
    [Spec("mediaqueries-5", "#mq-boolean-context")]
    [DataRow("(width)")]
    [DataRow("(height)")]
    [DataRow("(orientation)")]
    [DataRow("(aspect-ratio)")]
    [DataRow("(resolution)")]
    public void BooleanContext_IsValidForEveryFeature(string query)
    {
        Assert.IsTrue(Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#mq-range-context")]
    [Spec("mediaqueries-5", "#mq-range-context")]
    [DataRow("(width >= 800px)", true)]
    [DataRow("(width > 800px)", false)]
    [DataRow("(width <= 800px)", true)]
    [DataRow("(width < 800px)", false)]
    [DataRow("(height > 500px)", true)]
    public void RangeContext_ComparesTheFeature(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [Ignore("Gap, see Spec/mediaqueries-4.yml")]
    [TestMethod]
    [Spec("mediaqueries-4", "#mq-range-context")]
    [Spec("mediaqueries-5", "#mq-range-context")]
    [DataRow("(width = 800px)")]
    [DataRow("(600px <= width)")]
    [DataRow("(400px < width < 1000px)")]
    public void RangeContext_AcceptsEqualityValueFirstAndDoubleRanges(string query)
    {
        Assert.IsTrue(Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#mq-range-context")]
    [Spec("mediaqueries-5", "#mq-range-context")]
    [DataRow("(900px < width)")]
    [DataRow("(1000px > width > 900px)")]
    public void RangeContext_ValueFirstAndDoubleRanges_OutsideTheRange_DoNotMatch(string query)
    {
        Assert.IsFalse(Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#mq-range-context")]
    [Spec("mediaqueries-5", "#mq-range-context")]
    [DataRow("not all and (width <= -100px)", true)]
    [DataRow("(height > -100px)", true)]
    [DataRow("(width: -100px)", false)]
    public void RangeContext_NegativeValues_AreFalseInTheNegativeRange(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#mq-min-max")]
    [Spec("mediaqueries-5", "#mq-min-max")]
    [DataRow("(min-width: 800px)", true)]
    [DataRow("(min-width: 801px)", false)]
    [DataRow("(max-width: 800px)", true)]
    [DataRow("(max-width: 799px)", false)]
    [DataRow("(min-width)", false)]
    [DataRow("(min-grid: 0)", false)]
    [Spec("mediaqueries-4", "#mq-ranges")]
    [Spec("mediaqueries-5", "#mq-ranges")]
    public void MinMaxPrefixes_AreInclusiveRanges(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [Ignore("Gap, see Spec/mediaqueries-4.yml")]
    [TestMethod]
    [Spec("mediaqueries-4", "#media-conditions")]
    [Spec("mediaqueries-5", "#media-conditions")]
    [DataRow("(color) or (grid)")]
    [DataRow("not (grid)")]
    [DataRow("(not (color)) or (min-width: 100px)")]
    [DataRow("screen and ((grid) or (color))")]
    public void MediaConditions_CombineWithAndOrNot(string query)
    {
        Assert.IsTrue(Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#media-conditions")]
    [Spec("mediaqueries-5", "#media-conditions")]
    public void NotInFrontOfAMatchingFeature_DoesNotMatch()
    {
        Assert.IsFalse(Matches("not (color)"));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#media-conditions")]
    [Spec("mediaqueries-5", "#media-conditions")]
    [DataRow("(color) and (pointer) or (hover)")]
    public void MixingAndWithOr_WithoutParentheses_IsInvalid(string query)
    {
        Assert.IsFalse(Matches(query));
    }

    // ── 3 Syntax ─────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("mediaqueries-4", "#mq-syntax")]
    [Spec("mediaqueries-5", "#mq-syntax")]
    [DataRow("SCREEN AND (WIDTH: 800px)", true)]
    [DataRow("screen and(min-width: 100px)", false)]
    [DataRow("or and (color)", false)]
    public void Syntax_IsCaseInsensitiveAndNeedsWhitespaceBeforeParentheses(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#mq-syntax")]
    [Spec("mediaqueries-5", "#mq-syntax")]
    public void MinMaxPrefixes_AreCaseInsensitive()
    {
        Assert.IsTrue(Matches("(MIN-WIDTH: 100px)"));
    }

    [Ignore("Gap, see Spec/mediaqueries-4.yml")]
    [TestMethod]
    [Spec("mediaqueries-4", "#mq-syntax")]
    [Spec("mediaqueries-5", "#mq-syntax")]
    public void Units_AreCaseInsensitive()
    {
        Assert.IsTrue(Matches("(width: 800PX)"));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#evaluating")]
    [Spec("mediaqueries-5", "#evaluating")]
    [DataRow("not (max-weight: 3kg)")]
    [DataRow("not all and (max-weight: 3kg)")]
    public void Unknown_IsFalse_EvenNegated(string query)
    {
        Assert.IsFalse(Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#error-handling")]
    [Spec("mediaqueries-5", "#error-handling")]
    [DataRow("unknown", false)]
    [DataRow("not unknown", true)]
    public void UnknownMediaType_DoesNotMatch(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#error-handling")]
    [Spec("mediaqueries-5", "#error-handling")]
    [DataRow("(max-weight: 3kg)")]
    [DataRow("(min-orientation: portrait)")]
    [DataRow("(color: 20example)")]
    public void UnknownFeatureOrValue_IsFalse(string query)
    {
        Assert.IsFalse(Matches(query));
    }

    [Ignore("Gap, see Spec/mediaqueries-4.yml")]
    [TestMethod]
    [Spec("mediaqueries-4", "#error-handling")]
    [Spec("mediaqueries-5", "#error-handling")]
    [DataRow("screen and (max-weight: 3kg) and (color), (color)")]
    [DataRow("&test, screen")]
    [DataRow("(example, all,), screen")]
    public void InvalidQuery_OnlyReplacesThatQueryWithNotAll(string query)
    {
        Assert.IsTrue(Matches(query));
    }

    // ── 4 Viewport dimensions ────────────────────────────────────────────

    [TestMethod]
    [Spec("mediaqueries-4", "#width")]
    [Spec("mediaqueries-5", "#width")]
    [Spec("mediaqueries-4", "#units")]
    [Spec("mediaqueries-5", "#units")]
    [DataRow("(min-width: 50em)", true)]
    [DataRow("(min-width: 51em)", false)]
    [DataRow("(max-width: 21.17cm)", true)]
    [DataRow("(width: 800px)", true)]
    public void Width_IsTheViewportWidth(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#height")]
    [Spec("mediaqueries-5", "#height")]
    [DataRow("(height: 600px)", true)]
    [DataRow("(min-height: 601px)", false)]
    public void Height_IsTheViewportHeight(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#aspect-ratio")]
    [Spec("mediaqueries-5", "#aspect-ratio")]
    [DataRow("(aspect-ratio: 4/3)", true)]
    [DataRow("(aspect-ratio: 8/6)", true)]
    [DataRow("(min-aspect-ratio: 1/1)", true)]
    [DataRow("(max-aspect-ratio: 1/1)", false)]
    [DataRow("(aspect-ratio > 2/1)", false)]
    public void AspectRatio_IsWidthOverHeight(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#orientation")]
    [Spec("mediaqueries-5", "#orientation")]
    [DataRow(800d, 600d, "landscape")]
    [DataRow(600d, 800d, "portrait")]
    [DataRow(600d, 600d, "portrait")]
    public void Orientation_IsPortraitWhenHeightIsAtLeastWidth(double width, double height, string orientation)
    {
        Assert.IsTrue(Matches($"(orientation: {orientation})", width, height));
    }

    // ── 5 Display quality ────────────────────────────────────────────────

    [TestMethod]
    [Spec("mediaqueries-4", "#resolution")]
    [Spec("mediaqueries-5", "#resolution")]
    [DataRow("(resolution: 96dpi)", true)]
    [DataRow("(resolution: 1dppx)", true)]
    [DataRow("(min-resolution: 2dppx)", false)]
    [DataRow("(max-resolution: 1dppx)", true)]
    public void Resolution_IsOneDevicePixelPerCssPixel(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [Ignore("Gap, see Spec/mediaqueries-4.yml")]
    [TestMethod]
    [Spec("mediaqueries-4", "#resolution")]
    [Spec("mediaqueries-5", "#resolution")]
    public void Resolution_AcceptsInfinite()
    {
        Assert.IsTrue(Matches("(resolution < infinite)"));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#scan")]
    [Spec("mediaqueries-5", "#scan")]
    [DataRow("(scan)")]
    [DataRow("(scan: interlace)")]
    public void Scan_DoesNotApply(string query)
    {
        Assert.IsFalse(Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#grid")]
    [Spec("mediaqueries-5", "#grid")]
    [DataRow("(grid: 0)", true)]
    [DataRow("(grid: 1)", false)]
    public void Grid_IsZero(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#update")]
    [Spec("mediaqueries-5", "#update")]
    [DataRow("(update: none)", true)]
    [DataRow("(update: fast)", false)]
    public void Update_IsNone(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    // ── 6 Color ──────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("mediaqueries-4", "#color")]
    [Spec("mediaqueries-5", "#color")]
    [DataRow("(min-color: 1)", true)]
    [DataRow("(color: 8)", true)]
    [DataRow("(color > 8)", false)]
    public void Color_IsEightBitsPerComponent(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#color-index")]
    [Spec("mediaqueries-5", "#color-index")]
    [DataRow("(color-index)", false)]
    [DataRow("(color-index: 0)", true)]
    [DataRow("(min-color-index: 1)", false)]
    public void ColorIndex_IsZero(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#monochrome")]
    [Spec("mediaqueries-5", "#monochrome")]
    [DataRow("(monochrome: 0)", true)]
    [DataRow("(min-monochrome: 1)", false)]
    public void Monochrome_IsZero(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    // ── 7 Interaction ────────────────────────────────────────────────────

    [TestMethod]
    [Spec("mediaqueries-4", "#pointer")]
    [Spec("mediaqueries-5", "#pointer")]
    [DataRow("(pointer: none)", true)]
    [DataRow("(pointer: fine)", false)]
    public void Pointer_IsNone(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#hover")]
    [Spec("mediaqueries-5", "#hover")]
    [DataRow("(hover: none)", true)]
    [DataRow("(hover)", false)]
    public void Hover_IsNone(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-4", "#@media/any-input")]
    [Spec("mediaqueries-5", "#any-input")]
    [DataRow("(any-pointer: none)", true)]
    [DataRow("(any-pointer)", false)]
    [DataRow("(any-hover: none)", true)]
    [DataRow("(any-hover: hover)", false)]
    public void AnyPointerAndAnyHover_AreNone(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    private static bool Matches(string query, double width = 800, double height = 600, string mediaType = "screen")
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@media " + query + " { }");
        if (sheet.Rules.Length != 1 || sheet.Rules[0] is not MediaRule rule)
            return false;

        var context = MediaQueryContext.FromAdapter(new MockAdapter { MediaType = mediaType }, width, height);
        return MediaQueryMatcher.Matches(new[] { rule.Media }, context);
    }
}
