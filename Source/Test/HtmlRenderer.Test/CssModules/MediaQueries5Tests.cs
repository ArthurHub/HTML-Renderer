using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.CssModules;

/// <summary>
/// The features Media Queries 5 adds, evaluated against an 800x600 screen.
/// </summary>
[TestClass]
public sealed class MediaQueries5Tests
{
    // ── 4 Viewport/Page Characteristics ──────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/mediaqueries-5.yml")]
    [Spec("mediaqueries-5", "#mf-horizontal-viewport-segments")]
    [Spec("mediaqueries-5", "#mf-vertical-viewport-segments")]
    [DataRow("(horizontal-viewport-segments: 1)")]
    [DataRow("(vertical-viewport-segments: 1)")]
    public void ViewportSegments_AreOne(string query)
    {
        Assert.IsTrue(Matches(query));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/mediaqueries-5.yml")]
    [Spec("mediaqueries-5", "#display-modes")]
    public void DisplayMode_IsBrowser()
    {
        Assert.IsTrue(Matches("(display-mode: browser)"));
    }

    // ── 5 Display Quality ────────────────────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/mediaqueries-5.yml")]
    [Spec("mediaqueries-5", "#environment-blending")]
    public void EnvironmentBlending_IsOpaque()
    {
        Assert.IsTrue(Matches("(environment-blending: opaque)"));
    }

    // ── 6 Color ──────────────────────────────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/mediaqueries-5.yml")]
    [Spec("mediaqueries-5", "#dynamic-range")]
    [Spec("mediaqueries-5", "#video-dynamic-range")]
    [DataRow("(dynamic-range: standard)")]
    [DataRow("(video-dynamic-range: standard)")]
    public void DynamicRange_IsStandard(string query)
    {
        Assert.IsTrue(Matches(query));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/mediaqueries-5.yml")]
    [Spec("mediaqueries-5", "#video-color-gamut")]
    public void VideoColorGamut_IsSrgb()
    {
        Assert.IsTrue(Matches("(video-color-gamut: srgb)"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/mediaqueries-5.yml")]
    [Spec("mediaqueries-5", "#inverted")]
    public void InvertedColors_IsNone()
    {
        Assert.IsTrue(Matches("(inverted-colors: none)"));
    }

    // ── 7 Interaction ────────────────────────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/mediaqueries-5.yml")]
    [Spec("mediaqueries-5", "#nav-controls")]
    public void NavControls_IsNone()
    {
        Assert.IsTrue(Matches("(nav-controls: none)"));
    }

    // ── 9 Scripting ──────────────────────────────────────────────────────

    [TestMethod]
    [Spec("mediaqueries-5", "#scripting")]
    [DataRow("(scripting: none)", true)]
    [DataRow("(scripting: initial-only)", false)]
    [DataRow("(scripting: enabled)", false)]
    [DataRow("(scripting)", false)]
    public void Scripting_IsNone(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    // ── 10 Custom Media Queries ──────────────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/mediaqueries-5.yml")]
    [Spec("mediaqueries-5", "#custom-mq")]
    public void CustomMediaQuery_IsEvaluatedWhereItIsUsed()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@custom-media --wide (min-width: 100px); @media (--wide) { }");
        var rule = sheet.Rules.OfType<MediaRule>().SingleOrDefault();

        Assert.IsNotNull(rule, "The @media rule is valid");
        Assert.IsTrue(MediaQueryMatcher.Matches(new[] { rule.Media }, MediaQueryContext.FromAdapter(new MockAdapter(), 800, 600)));
    }

    // ── 12 User Preferences ──────────────────────────────────────────────

    [TestMethod]
    [Spec("mediaqueries-5", "#prefers-reduced-motion")]
    [DataRow("(prefers-reduced-motion: reduce)", true)]
    [DataRow("(prefers-reduced-motion: no-preference)", false)]
    [DataRow("(prefers-reduced-motion)", true)]
    public void PrefersReducedMotion_IsReduce(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-5", "#prefers-reduced-transparency")]
    [DataRow("(prefers-reduced-transparency: no-preference)", true)]
    [DataRow("(prefers-reduced-transparency: reduce)", false)]
    [DataRow("(prefers-reduced-transparency)", false)]
    public void PrefersReducedTransparency_IsNoPreference(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-5", "#prefers-contrast")]
    [DataRow("(prefers-contrast: no-preference)", true)]
    [DataRow("(prefers-contrast: more)", false)]
    [DataRow("(prefers-contrast: less)", false)]
    [DataRow("(prefers-contrast: custom)", false)]
    [DataRow("(prefers-contrast)", false)]
    public void PrefersContrast_IsNoPreference(string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/mediaqueries-5.yml")]
    [Spec("mediaqueries-5", "#forced-colors")]
    [DataRow("(forced-colors: none)")]
    [DataRow("not all and (forced-colors)")]
    public void ForcedColors_IsNone(string query)
    {
        Assert.IsTrue(Matches(query));
    }

    [TestMethod]
    [Spec("mediaqueries-5", "#forced-colors")]
    public void ForcedColors_Active_DoesNotMatch()
    {
        Assert.IsFalse(Matches("(forced-colors: active)"));
    }

    [TestMethod]
    [Spec("mediaqueries-5", "#prefers-color-scheme")]
    [DataRow(RColorScheme.Light, "(prefers-color-scheme: light)", true)]
    [DataRow(RColorScheme.Light, "(prefers-color-scheme: dark)", false)]
    [DataRow(RColorScheme.Dark, "(prefers-color-scheme: dark)", true)]
    [DataRow(RColorScheme.Dark, "(prefers-color-scheme: light)", false)]
    [DataRow(RColorScheme.Dark, "(prefers-color-scheme)", true)]
    public void PrefersColorScheme_FollowsTheAdapter(RColorScheme scheme, string query, bool expected)
    {
        Assert.AreEqual(expected, Matches(query, scheme));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/mediaqueries-5.yml")]
    [Spec("mediaqueries-5", "#prefers-color-scheme")]
    [Spec("mediaqueries-5", "#prefers-contrast")]
    [DataRow("not all and (prefers-color-scheme: foo)")]
    [DataRow("not all and (prefers-contrast: foo)")]
    public void UnknownPreferenceValue_IsFalse_EvenNegated(string query)
    {
        Assert.IsFalse(Matches(query));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/mediaqueries-5.yml")]
    [Spec("mediaqueries-5", "#prefers-reduced-data")]
    public void PrefersReducedData_IsNoPreference()
    {
        Assert.IsTrue(Matches("(prefers-reduced-data: no-preference)"));
    }

    private static bool Matches(string query, RColorScheme scheme = RColorScheme.Light)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@media " + query + " { }");
        if (sheet.Rules.Length != 1 || sheet.Rules[0] is not MediaRule rule)
            return false;

        var context = MediaQueryContext.FromAdapter(new MockAdapter { ColorScheme = scheme }, 800, 600);
        return MediaQueryMatcher.Matches(new[] { rule.Media }, context);
    }
}
