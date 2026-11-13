using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css.Selectors;

[TestClass]
public sealed class RelationalPseudoClassSelectorTests
{
    // ── :not() ─────────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("selectors-4", "#negation")]
    public void Not_ExcludesElementsMatchingTheArgument()
    {
        var html = Html(
            "p:not(.exclude) { background-color: #ff0000; }",
            "<div><p class='exclude'>1</p><p>2</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#negation")]
    public void NotNot_MatchesWhatTheInnerArgumentMatches()
    {
        var html = Html(
            "p:not(:not(.foo)) { background-color: #ff0000; }",
            "<div><p class='foo'>1</p></div>");
        var box = FindBoxByTag(html, "p");
        Assert.AreNotEqual(RColor.Transparent, box.ActualBackgroundColor);
    }

    // ── :is() / :matches() ────────────────────────────────────────────────────

    [TestMethod]
    [Spec("selectors-4", "#matches")]
    public void Is_MatchesEitherBranch()
    {
        var html = Html(
            "p:is(.a, .b) { background-color: #ff0000; }",
            "<div><p class='a'>1</p><p class='b'>2</p><p>3</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(3, boxes.Count);
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[2].ActualBackgroundColor);
    }

    [TestMethod]
    [Spec("selectors-4", "#matches")]
    [Spec("selectors-4", "#legacy-aliasing")]
    public void Matches_LegacyAliasBehavesTheSameAsIs()
    {
        var html = Html(
            "p:matches(.a, .b) { background-color: #ff0000; }",
            "<div><p class='a'>1</p><p>2</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
    }

    [TestMethod]
    [Spec("selectors-4", "#matches")]
    [Spec("selectors-4", "#specificity-rules")]
    public void Is_SpecificityIsTheStaticMaxOfItsArguments_NotJustTheMatchedBranch()
    {
        // Only .a matches, but :is() counts as #b, which beats .a.c.
        var html = Html(
            ":is(.a, #b) { color: #0000ff; } .a.c { color: #ff0000; }",
            "<div class='a c'>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual(RColor.FromArgb(0, 0, 255), box.ActualColor);
    }

    // ── :has() ─────────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("selectors-4", "#relational")]
    [Spec("selectors-4", "#relative")]
    public void Has_MatchesWhenADirectChildSatisfiesTheArgument()
    {
        var html = Html(
            "div:has(.foo) { background-color: #ff0000; }",
            "<div id='yes'><span class='foo'>x</span></div><div id='no'><span>x</span></div>");
        var yes = FindBoxById(html, "yes");
        var no = FindBoxById(html, "no");
        Assert.AreNotEqual(RColor.Transparent, yes.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, no.ActualBackgroundColor);
    }

    [TestMethod]
    [Spec("selectors-4", "#relational")]
    public void Has_MatchesWhenADeeplyNestedDescendantSatisfiesTheArgument()
    {
        var html = Html(
            "div:has(.foo) { background-color: #ff0000; }",
            "<div id='outer'><section><article><span class='foo'>deep</span></article></section></div>");
        var outer = FindBoxById(html, "outer");
        Assert.AreNotEqual(RColor.Transparent, outer.ActualBackgroundColor);
    }

    [TestMethod]
    [Spec("selectors-4", "#relational")]
    public void Has_CommaSeparatedArgument_MatchesEitherBranch()
    {
        var html = Html(
            "div:has(.a, .b) { background-color: #ff0000; }",
            "<div id='yes'><span class='b'>x</span></div><div id='no'><span class='c'>x</span></div>");
        var yes = FindBoxById(html, "yes");
        var no = FindBoxById(html, "no");
        Assert.AreNotEqual(RColor.Transparent, yes.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, no.ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#relational")]
    [Spec("selectors-4", "#relative")]
    public void Has_ChildCombinator_MatchesOnlyDirectChild()
    {
        // ":has(> .foo)" must NOT behave like plain ":has(.foo)" - a .foo that's only a grandchild
        // (nested one level deeper than a direct child) must not satisfy the child-combinator form.
        var html = Html(
            "div:has(> .foo) { background-color: #ff0000; }",
            "<div id='yes'><span class='foo'>x</span></div><div id='no'><section><span class='foo'>x</span></section></div>");
        var yes = FindBoxById(html, "yes");
        var no = FindBoxById(html, "no");
        Assert.AreNotEqual(RColor.Transparent, yes.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, no.ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#relative")]
    public void Has_ChildCombinator_WithMultiCompoundArgument_AnchorsOnlyItsFirstCompoundToTheChild()
    {
        // The child has to match .a, and .b has to be anywhere inside that child.
        var html = Html(
            "div:has(> .a .b) { background-color: #ff0000; }",
            "<div id='yes'><section class='a'><em><span class='b'>x</span></em></section></div>" +
            "<div id='no'><div><section class='a'><em><span class='b'>x</span></em></section></div></div>" +
            "<div id='no-descendant'><section class='a'><em>no b here</em></section></div>");
        var yes = FindBoxById(html, "yes");
        var no = FindBoxById(html, "no");
        var noDescendant = FindBoxById(html, "no-descendant");
        Assert.AreNotEqual(RColor.Transparent, yes.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, no.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, noDescendant.ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#relational")]
    public void Has_ChildCombinator_WithUniversalSelector_DoesNotMatchATextOnlyChild()
    {
        var html = Html(
            "div:has(> *) { background-color: #ff0000; }",
            "<div id='yes'><span>x</span></div><div id='no'>just text</div>");
        var yes = FindBoxById(html, "yes");
        var no = FindBoxById(html, "no");
        Assert.AreNotEqual(RColor.Transparent, yes.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, no.ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#relative")]
    public void Has_AdjacentSiblingCombinator_MatchesOnlyImmediateNextSibling()
    {
        var html = Html(
            "div:has(+ .foo) { background-color: #ff0000; }",
            "<div id='yes'></div><span class='foo'>x</span>" +
            "<div id='no'></div><section></section><span class='foo'>x</span>");
        var yes = FindBoxById(html, "yes");
        var no = FindBoxById(html, "no");
        Assert.AreNotEqual(RColor.Transparent, yes.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, no.ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#relative")]
    public void Has_GeneralSiblingCombinator_MatchesAnyFollowingSibling()
    {
        // Same markup shape as the adjacent-sibling test's "no" case (.foo is a later, not immediate,
        // sibling) - "~" must succeed where "+" fails.
        var html = Html(
            "div:has(~ .foo) { background-color: #ff0000; }",
            "<div id='yes'></div><section></section><span class='foo'>x</span>" +
            "<div id='no'></div>");
        var yes = FindBoxById(html, "yes");
        var no = FindBoxById(html, "no");
        Assert.AreNotEqual(RColor.Transparent, yes.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, no.ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#relative")]
    public void Has_MixedLeadingCombinatorsInCommaList_MatchesEitherBranch()
    {
        // Each comma-separated alternative tracks its own leading combinator independently.
        var html = Html(
            "div:has(> .a, + .b) { background-color: #ff0000; }",
            "<div id='child'><span class='a'>x</span></div>" +
            "<div id='sibling'></div><span class='b'>x</span>" +
            "<div id='none'><section><span class='a'>x</span></section></div>");
        var child = FindBoxById(html, "child");
        var sibling = FindBoxById(html, "sibling");
        var none = FindBoxById(html, "none");
        Assert.AreNotEqual(RColor.Transparent, child.ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, sibling.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, none.ActualBackgroundColor);
    }

    [TestMethod]
    [DataRow("div:has(.foo)")]
    public void Has_LeadingCombinator_RoundTripsThroughSelectorText(string selector)
    {
        var sheet = ParseStyleSheet($"{selector} {{ }}");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual(selector, ((StyleRule)sheet.Rules[0]).SelectorText);
    }

    [TestMethod]
    [DataRow("div:has(> .foo)")]
    [DataRow("div:has(+ .foo)")]
    [DataRow("div:has(~ .foo)")]
    [Spec("selectors-4", "#relative")]
    public void Has_LeadingCombinator_RoundTripsThroughSelectorText_LeadingCombinatorForms(string selector)
    {
        var sheet = ParseStyleSheet($"{selector} {{ }}");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual(selector, ((StyleRule)sheet.Rules[0]).SelectorText);
    }

    [TestMethod]
    [Spec("selectors-4", "#relative")]
    public void Has_MixedLeadingCombinatorsInCommaList_RoundTripsThroughSelectorText()
    {
        // ListSelector.ToCss writes no space after the comma.
        var sheet = ParseStyleSheet("div:has(> .a, + .b) { }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual("div:has(> .a,+ .b)", ((StyleRule)sheet.Rules[0]).SelectorText);
    }

    // ── :where() ───────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("selectors-4", "#zero-matches")]
    public void Where_MatchesEitherBranch_LikeIs()
    {
        var html = Html(
            "p:where(.a, .b) { background-color: #ff0000; }",
            "<div><p class='a'>1</p><p class='b'>2</p><p>3</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(3, boxes.Count);
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[2].ActualBackgroundColor);
    }

    [TestMethod]
    [Spec("selectors-4", "#zero-matches")]
    [Spec("selectors-4", "#specificity-rules")]
    public void Where_ContributesZeroSpecificity_LosesToATypeSelector()
    {
        var html = Html(
            ":where(#b) { color: #0000ff; } p { color: #ff0000; }",
            "<p id='b'>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(RColor.FromArgb(255, 0, 0), box.ActualColor);
    }

    [TestMethod]
    [Spec("selectors-4", "#zero-matches")]
    public void Where_StillMatches_WhenUncontested()
    {
        var html = Html(
            ":where(#b) { color: #0000ff; }",
            "<p id='b'>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(RColor.FromArgb(0, 0, 255), box.ActualColor);
    }

    private static string Html(string css, string body) =>
        $"<!DOCTYPE html><html><head><style>{css}</style></head><body>{body}</body></html>";

    private static CssBox FindBoxByTag(string html, string tag)
    {
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.Descendants(root).FirstOrDefault(b => b.HtmlTag != null && b.HtmlTag.Name == tag);
        Assert.IsNotNull(box);
        return box!;
    }

    private static List<CssBox> FindAllBoxesByTag(string html, string tag)
    {
        var (root, _) = LayoutHarness.Layout(html);
        return LayoutHarness.Descendants(root).Where(b => b.HtmlTag != null && b.HtmlTag.Name == tag).ToList();
    }

    private static CssBox FindBoxById(string html, string id)
    {
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box);
        return box!;
    }
}
