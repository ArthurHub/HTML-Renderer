using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css.Selectors;

[TestClass]
public sealed class SelectorMatchingTests
{
    // ── Attribute: starts-with [attr^=value] ─────────────────────────────────

    [TestMethod]
    public void AttrBegins_Matches_WhenValueStartsWith()
    {
        var html = Html("[class^='btn'] { background-color: #ff0000; }", "<span class='btn-primary'>text</span>");
        var box = FindBoxByTag(html, "span");
        Assert.AreNotEqual(RColor.Transparent, box.ActualBackgroundColor);
    }

    [TestMethod]
    public void AttrBegins_DoesNotMatch_WhenValueDoesNotStartWith()
    {
        var html = Html("[class^='btn'] { background-color: #ff0000; }", "<span class='input-btn'>text</span>");
        var box = FindBoxByTag(html, "span");
        Assert.AreEqual(RColor.Transparent, box.ActualBackgroundColor);
    }

    // ── Attribute: ends-with [attr$=value] ───────────────────────────────────

    [TestMethod]
    public void AttrEnds_Matches_WhenValueEndsWith()
    {
        var html = Html("[lang$='-US'] { background-color: #ff0000; }", "<span lang='en-US'>text</span>");
        var box = FindBoxByTag(html, "span");
        Assert.AreNotEqual(RColor.Transparent, box.ActualBackgroundColor);
    }

    [TestMethod]
    public void AttrEnds_DoesNotMatch_WhenValueDoesNotEndWith()
    {
        var html = Html("[lang$='-US'] { background-color: #ff0000; }", "<span lang='US-en'>text</span>");
        var box = FindBoxByTag(html, "span");
        Assert.AreEqual(RColor.Transparent, box.ActualBackgroundColor);
    }

    // ── Attribute: hyphen-prefix [attr|=value] ───────────────────────────────

    [TestMethod]
    [Spec("css21", "selector.html#matching-attrs")]
    public void AttrHyphen_Matches_ExactValue()
    {
        var html = Html("[lang|='en'] { background-color: #ff0000; }", "<span lang='en'>text</span>");
        var box = FindBoxByTag(html, "span");
        Assert.AreNotEqual(RColor.Transparent, box.ActualBackgroundColor);
    }

    [TestMethod]
    [Spec("css21", "selector.html#matching-attrs")]
    public void AttrHyphen_Matches_HyphenPrefixed()
    {
        var html = Html("[lang|='en'] { background-color: #ff0000; }", "<span lang='en-US'>text</span>");
        var box = FindBoxByTag(html, "span");
        Assert.AreNotEqual(RColor.Transparent, box.ActualBackgroundColor);
    }

    [TestMethod]
    [Spec("css21", "selector.html#matching-attrs")]
    public void AttrHyphen_DoesNotMatch_SubstringOnly()
    {
        var html = Html("[lang|='en'] { background-color: #ff0000; }", "<span lang='english'>text</span>");
        var box = FindBoxByTag(html, "span");
        Assert.AreEqual(RColor.Transparent, box.ActualBackgroundColor);
    }

    // ── Combinator: adjacent sibling (div + p) ───────────────────────────────

    [TestMethod]
    [Spec("css21", "selector.html#adjacent-selectors")]
    public void AdjacentSibling_Matches_ImmediatelyFollowingSibling()
    {
        var html = Html("div + p { background-color: #ff0000; }", "<div>d</div><p id='t'>first</p>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
    }

    [TestMethod]
    [Spec("css21", "selector.html#adjacent-selectors")]
    public void AdjacentSibling_DoesNotMatch_SecondSibling()
    {
        var html = Html("div + p { background-color: #ff0000; }", "<div>d</div><p>first</p><p>second</p>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
    }

    [TestMethod]
    [Spec("css21", "selector.html#adjacent-selectors")]
    public void AdjacentSibling_DoesNotMatch_PrecedingSibling()
    {
        var html = Html("div + p { background-color: #ff0000; }", "<p>before</p><div>d</div>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(RColor.Transparent, box.ActualBackgroundColor);
    }

    // ── Combinator: general sibling (div ~ p) ────────────────────────────────

    [TestMethod]
    public void GeneralSibling_Matches_AllFollowingSiblings()
    {
        var html = Html("div ~ p { background-color: #ff0000; }", "<div>d</div><p>first</p><p>second</p>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
    }

    [TestMethod]
    public void GeneralSibling_DoesNotMatch_PrecedingSibling()
    {
        var html = Html("div ~ p { background-color: #ff0000; }", "<p>before</p><div>d</div>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(RColor.Transparent, box.ActualBackgroundColor);
    }

    // ── Combinator: child (div > p) and descendant (div p) ──────────────────

    [TestMethod]
    [Spec("css21", "selector.html#child-selectors")]
    public void ChildCombinator_StillMatches_DirectChild()
    {
        var html = Html("div > p { background-color: #ff0000; }", "<div><p>direct</p><section><p>nested</p></section></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
    }

    [TestMethod]
    [Spec("css21", "selector.html#descendant-selectors")]
    public void DescendantCombinator_StillMatches_AnyDescendant()
    {
        var html = Html("div p { background-color: #ff0000; }", "<div><section><p>deep</p></section></div><p>outside</p>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

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
