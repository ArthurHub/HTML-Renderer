using System.Collections.Generic;
using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// The tag, class and id rule index of <c>CssData</c> only narrows the candidates, a bucketing bug shows up as
/// a rule that doesn't apply, or applies where it shouldn't.
/// </summary>
[TestClass]
public sealed class CssDataRuleIndexingTests
{
    // ── Id selector (its own bucket) ──────────────────────────────────────────

    [TestMethod]
    public void IdSelector_Matches_ElementWithThatId()
    {
        var html = Html("#target { background-color: #ff0000; }", "<div id='target'>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreNotEqual("transparent", box.BackgroundColor);
    }

    [TestMethod]
    public void IdSelector_DoesNotMatch_DifferentId()
    {
        var html = Html("#target { background-color: #ff0000; }", "<div id='other'>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual("transparent", box.BackgroundColor);
    }

    // ── Multi-class compound selector (bucketed by one of its classes) ───────

    [TestMethod]
    public void MultiClassCompound_Matches_WhenElementHasBothClasses()
    {
        var html = Html(".a.b { background-color: #ff0000; }", "<div class='a b'>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreNotEqual("transparent", box.BackgroundColor);
    }

    [TestMethod]
    public void MultiClassCompound_DoesNotMatch_WhenOnlyOneClassPresent()
    {
        var html = Html(".a.b { background-color: #ff0000; }", "<div class='a'>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual("transparent", box.BackgroundColor);
    }

    [TestMethod]
    public void ElementWithNoClassAttribute_DoesNotMatchClassSelector()
    {
        // Regression for the class bucket lookup: a box with no "class" attribute at all must
        // not blow up or accidentally match, it should just never be a class-bucket candidate.
        var html = Html(".a { background-color: #ff0000; }", "<div>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual("transparent", box.BackgroundColor);
    }

    // ── List (comma) selector spanning multiple buckets ───────────────────────

    [TestMethod]
    public void ListSelector_TagAlternative_MatchesViaTagBucket()
    {
        var html = Html("div, .foo { background-color: #ff0000; }", "<div>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreNotEqual("transparent", box.BackgroundColor);
    }

    [TestMethod]
    public void ListSelector_ClassAlternative_MatchesViaClassBucket()
    {
        var html = Html("span, .foo { background-color: #ff0000; }", "<div class='foo'>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreNotEqual("transparent", box.BackgroundColor);
    }

    [TestMethod]
    public void ListSelector_MatchingBothAlternatives_AppliesOnceWithoutCorruption()
    {
        // "div, .foo" on a <div class="foo"> is reachable through both the tag bucket AND the
        // class bucket - regression for the dedup in CssData.GetStyleRulesByOrigin, which must
        // yield the rule exactly once so cascade application isn't run twice for it.
        var html = Html("div, .foo { background-color: #ff0000; color: #00ff00; }", "<div class='foo'>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual("rgb(255, 0, 0)", box.BackgroundColor);
        Assert.AreEqual("rgb(0, 255, 0)", box.Color);
    }

    // ── Universal selector ────────────────────────────────────────────────────

    [TestMethod]
    public void UniversalSelector_MatchesAnyElement()
    {
        var html = Html("* { background-color: #ff0000; }", "<span>text</span>");
        var box = FindBoxByTag(html, "span");
        Assert.AreNotEqual("transparent", box.BackgroundColor);
    }

    // ── Pseudo-element compound (falls back to the unindexed/universal bucket) ─

    [TestMethod]
    public void PseudoElement_CombinedWithClassSelector_StillGeneratesContent()
    {
        var html = Html(".label::before { content: 'X: '; }", "<span class='label'>text</span>");
        var (root, _) = LayoutHarness.Layout(html);
        var span = LayoutHarness.Descendants(root).FirstOrDefault(b => b.HtmlTag != null && b.HtmlTag.Name == "span");
        Assert.IsNotNull(span);

        var beforeBox = span!.Boxes.FirstOrDefault(b => b.IsBeforePseudoElement);
        Assert.IsNotNull(beforeBox);
        Assert.AreEqual("X: ", beforeBox!.Text);
    }

    // ── :nth-child(1) compound (falls back to the unindexed/universal bucket) ──

    [TestMethod]
    public void NthChildCompound_MatchesExactlyOneChild()
    {
        // Note: bare ":first-child" parses as a generic PseudoClassSelector in this engine's parser
        // (SelectorConstructor only maps the "nth-child(...)" function form to FirstChildSelector), and
        // DoesSelectorMatch(PseudoClassSelector,...) only recognizes ":link"/"hover"/"root" - so plain
        // ":first-child" never matches anything here, a pre-existing gap unrelated to this change.
        // ":nth-child(1)" does produce a real ChildSelector, which is what CollectIndexKeys routes to the
        // universal fallback bucket (rather than indexing by tag/class/id) - "*" for the other compound
        // member sidesteps a separate, pre-existing quirk.
        var html = Html(
            "*:nth-child(1) { background-color: #ff0000; }",
            "<div><p>first</p><p>second</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");

        Assert.AreEqual(2, boxes.Count);
        Assert.AreNotEqual("transparent", boxes[0].BackgroundColor);
        Assert.AreEqual("transparent", boxes[1].BackgroundColor);
    }

    // ── Media query rules (kept as an unindexed linear scan) ──────────────────

    [TestMethod]
    public void MediaQueryRule_ForPrintMedia_StillApplies()
    {
        var html = Html("@media print { div { background-color: #ff0000; } }", "<div>text</div>");
        var adapter = new MockAdapter { MediaType = "print" };
        var (root, _) = LayoutHarness.Layout(html, adapter: adapter);
        var box = LayoutHarness.Descendants(root).FirstOrDefault(b => b.HtmlTag != null && b.HtmlTag.Name == "div");
        Assert.IsNotNull(box);
        Assert.AreNotEqual("transparent", box!.BackgroundColor);
    }

    [TestMethod]
    public void MediaQueryRule_ForScreenMedia_DoesNotApplyToPrintOutput()
    {
        var html = Html("@media screen { div { background-color: #ff0000; } }", "<div>text</div>");
        var adapter = new MockAdapter { MediaType = "print" };
        var (root, _) = LayoutHarness.Layout(html, adapter: adapter);
        var box = LayoutHarness.Descendants(root).FirstOrDefault(b => b.HtmlTag != null && b.HtmlTag.Name == "div");
        Assert.IsNotNull(box);
        Assert.AreEqual("transparent", box!.BackgroundColor);
    }

    // ── Specificity/cascade order across differently-bucketed rules ──────────

    [TestMethod]
    public void ClassSelector_BeatsTagSelector_RegardlessOfBucket()
    {
        // Standard CSS specificity: a class selector (0,1,0) outranks a type selector (0,0,1),
        // even though the two rules are now looked up via completely different index buckets.
        var html = Html("div { color: #0000ff; } .highlight { color: #ff0000; }", "<div class='highlight'>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual("rgb(255, 0, 0)", box.Color);
    }

    [TestMethod]
    public void IdSelector_BeatsClassSelector_RegardlessOfBucket()
    {
        var html = Html("#target { color: #ff0000; } .highlight { color: #0000ff; }", "<div id='target' class='highlight'>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual("rgb(255, 0, 0)", box.Color);
    }

    [TestMethod]
    public void AuthorTagRule_OverridesUserAgentDefault()
    {
        // UA stylesheet sets h1's font-size; an author tag-selector rule (a different bucket
        // lookup than the UA rule's own tag bucket, but the same bucket *kind*) must still win.
        var html = Html("h1 { font-size: 10px; }", "<h1>heading</h1>");
        var box = FindBoxByTag(html, "h1");
        Assert.AreEqual("10px", box.FontSize);
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

    private static List<CssBox> FindAllBoxesByTag(string html, string tag)
    {
        var (root, _) = LayoutHarness.Layout(html);
        return LayoutHarness.Descendants(root).Where(b => b.HtmlTag != null && b.HtmlTag.Name == tag).ToList();
    }
}
