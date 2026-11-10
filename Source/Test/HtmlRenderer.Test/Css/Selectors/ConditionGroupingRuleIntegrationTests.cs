using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css.Selectors;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/ConditionGroupingRuleIntegrationTests.cs.
/// <c>Supports_SvgOnlyProperty_AppliesToAnInlineSvgElement</c> is not ported, HTML-Renderer has no SVG
/// rendering pipeline. The cascade doesn't apply <c>@container</c>'s inner rules yet.
/// </summary>
[TestClass]
public sealed class ConditionGroupingRuleIntegrationTests
{
    private static readonly RColor Blue = RColor.FromArgb(0, 0, 255);

    [TestMethod]
    public void Supports_InnerRules_Apply_WhenConditionIsSupported()
    {
        var html = Html(
            "p { color: #ff0000; } @supports (display: flex) { p { color: #0000ff; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(Blue, box.ActualColor);
    }

    [TestMethod]
    public void SupportsNot_Applies_WhenDeclarationIsInvalid()
    {
        var html = Html(
            "@supports not (display: no-such-value) { p { color: #0000ff; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(Blue, box.ActualColor);
    }

    [TestMethod]
    [Ignore("not yet spec compliant - DeclarationCondition.Check() reports animation-name as supported, " +
            "see SupportsTests.SupportsAnimationNameRule_NoLongerFalsePositive")]
    public void SupportsNot_Fallback_Applies_WhenFeatureIsUnsupported()
    {
        var html = Html(
            "@supports not (animation-name: spin) { p { color: #0000ff; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(Blue, box.ActualColor);
    }

    // The exact scenario the pre-evaluation stopgap PeachPDF#283 was written to avoid: an enhanced
    // block and its not-guarded fallback wrapping the same declaration, both present in one
    // stylesheet. Real evaluation must let exactly one of them win, never both and never neither.
    [TestMethod]
    public void SupportsEnhancedBlockAndNotGuardedFallback_ExactlyOneApplies()
    {
        var html = Html(
            "@supports (display: flex) { p { color: #0000ff; } } " +
            "@supports not (display: flex) { p { color: #00ff00; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(Blue, box.ActualColor);
        Assert.AreNotEqual(RColor.FromArgb(0, 255, 0), box.ActualColor);
    }

    [TestMethod]
    public void Supports_InnerRules_DoNotApply_WhenConditionIsNotSupported()
    {
        var html = Html(
            "p { color: #ff0000; } @supports (display: no-such-value) { p { color: #0000ff; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(RColor.FromArgb(255, 0, 0), box.ActualColor);
    }

    [TestMethod]
    public void Supports_NestedWithMedia_AppliesOnlyWhenAllConditionsHold()
    {
        var html = Html(
            "p { color: #ff0000; } " +
            "@supports (display: block) { @media screen { @supports (display: block) { p { color: #0000ff; } } } } " +
            "@media screen { @supports (display: no-such-value) { p { color: #00ff00; } } } " +
            "@supports (display: block) { @media print { p { color: #00ff00; } } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(Blue, box.ActualColor);
    }

    [TestMethod]
    public void Supports_InsideLayer_AppliesOnlyWhenConditionIsSupported()
    {
        var html = Html(
            "@layer base { @supports (display: block) { p { color: #0000ff; } } " +
            "@supports (display: no-such-value) { p { color: #00ff00; } } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(Blue, box.ActualColor);
    }

    [TestMethod]
    [Ignore("not yet spec compliant - the cascade never applies @container's inner rules")]
    public void Container_InnerRules_Apply_WhenAnEligibleAncestorContainerMatches()
    {
        var html = Html(
            "#box { container-type: inline-size; width: 300px; } " +
            "@container (min-width: 200px) { p { color: #0000ff; } }",
            "<div id=\"box\"><p>text</p></div>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(Blue, box.ActualColor);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static string Html(string css, string body) =>
        $"<!DOCTYPE html><html><head><style>{css}</style></head><body>{body}</body></html>";

    private static CssBox FindBoxByTag(string html, string tag)
    {
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.Descendants(root).FirstOrDefault(b => b.HtmlTag != null && b.HtmlTag.Name == tag);
        Assert.IsNotNull(box);
        return box!;
    }
}
