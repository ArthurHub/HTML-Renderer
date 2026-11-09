using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css.Selectors;

[TestClass]
public sealed class StructuralPseudoClassSelectorTests
{
    // ── :first-child ──────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css21", "selector.html#first-child")]
    public void FirstChild_Matches_FirstElement()
    {
        var html = Html(":first-child { background-color: #ff0000; }", "<div><p>first</p><p>second</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
    }

    // ── :last-child ────────────────────────────────────────────────────────────

    [TestMethod]
    public void LastChild_Matches_LastElement()
    {
        var html = Html(":last-child { background-color: #ff0000; }", "<div><p>first</p><p>second</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
    }

    // ── :only-child ────────────────────────────────────────────────────────────

    [TestMethod]
    public void OnlyChild_Matches_WhenNoSiblings()
    {
        var html = Html(":only-child { background-color: #ff0000; }", "<div><p>alone</p></div>");
        var box = FindBoxByTag(html, "p");
        Assert.AreNotEqual(RColor.Transparent, box.ActualBackgroundColor);
    }

    [TestMethod]
    public void OnlyChild_DoesNotMatch_WhenSiblingsPresent()
    {
        var html = Html(":only-child { background-color: #ff0000; }", "<div><p>first</p><p>second</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        foreach (var b in boxes) Assert.AreEqual(RColor.Transparent, b.ActualBackgroundColor);
    }

    // ── :root ──────────────────────────────────────────────────────────────────

    [TestMethod]
    public void Root_Matches_HtmlElement()
    {
        var html = Html(":root { background-color: #ff0000; }", "<p>content</p>");
        var box = FindBoxByTag(html, "html");
        Assert.AreNotEqual(RColor.Transparent, box.ActualBackgroundColor);
    }

    [TestMethod]
    public void Root_DoesNotMatch_BodyOrDescendants()
    {
        var html = Html(":root { background-color: #ff0000; }", "<div><p>content</p></div>");
        var body = FindBoxByTag(html, "body");
        var div = FindBoxByTag(html, "div");
        Assert.AreEqual(RColor.Transparent, body.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, div.ActualBackgroundColor);
    }

    [TestMethod]
    public void Root_CompoundWithTypeSelector_StillMatches()
    {
        var html = Html("html:root { background-color: #ff0000; }", "<p>content</p>");
        var box = FindBoxByTag(html, "html");
        Assert.AreNotEqual(RColor.Transparent, box.ActualBackgroundColor);
    }

    [TestMethod]
    public void Root_Specificity_OutranksTypeSelector_RegardlessOfSourceOrder()
    {
        // ":root" is (0,0,1,0), "html" is (0,0,0,1) - :root must win the cascade even though it's
        // declared after "html", where a naive source-order-only cascade would pick "html"'s blue.
        var html = Html(
            "html { background-color: #0000ff; } :root { background-color: #ff0000; }",
            "<p>content</p>");
        var box = FindBoxByTag(html, "html");
        Assert.AreEqual(RColor.FromArgb(255, 0, 0), box.ActualBackgroundColor);
    }

    // ── :nth-child() — literal, formula, keywords, negative offset ────────────

    [TestMethod]
    public void NthChild_Literal_MatchesOnlyThatPosition()
    {
        var html = Html(":nth-child(2) { background-color: #ff0000; }", "<div><p>1</p><p>2</p><p>3</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(3, boxes.Count);
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[2].ActualBackgroundColor);
    }

    [TestMethod]
    public void NthChild_2n_MatchesEvenPositions()
    {
        var html = Html(":nth-child(2n) { background-color: #ff0000; }", "<div><p>1</p><p>2</p><p>3</p><p>4</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[2].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[3].ActualBackgroundColor);
    }

    [TestMethod]
    public void NthChild_2nPlus1_MatchesOddPositions()
    {
        var html = Html(":nth-child(2n+1) { background-color: #ff0000; }", "<div><p>1</p><p>2</p><p>3</p><p>4</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[2].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[3].ActualBackgroundColor);
    }

    [TestMethod]
    public void NthChild_2nPlus1_WhitespaceSeparatedSign_MatchesSameAsCompactForm()
    {
        // CSS Syntax §5.6: when whitespace separates the sign from B, the '+' arrives as a standalone
        // delim token (`2n + 1`) rather than a signed number (`2n+1`). Both must select the same elements.
        var html = Html(":nth-child(2n + 1) { background-color: #ff0000; }", "<div><p>1</p><p>2</p><p>3</p><p>4</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[2].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[3].ActualBackgroundColor);
    }

    [TestMethod]
    public void NthOfType_10nPlus1_WhitespaceSeparatedSign_MatchesFirstOfType()
    {
        // 10n + 1 over 3 elements matches only position 1; proves the spaced sign is parsed (not silently
        // invalidating the selector, which would leave every element transparent).
        var html = Html("p:nth-of-type(10n + 1) { background-color: #ff0000; }", "<div><p>1</p><p>2</p><p>3</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[2].ActualBackgroundColor);
    }

    [TestMethod]
    public void NthChild_2nMinus1_WhitespaceSeparatedNegativeSign_MatchesOddPositions()
    {
        // A spaced negative sign (`2n - 1`) must also parse: 2n-1 selects positions 1 and 3.
        var html = Html(":nth-child(2n - 1) { background-color: #ff0000; }", "<div><p>1</p><p>2</p><p>3</p><p>4</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[2].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[3].ActualBackgroundColor);
    }

    [TestMethod]
    public void NthChild_Odd_MatchesOddPositions()
    {
        var html = Html(":nth-child(odd) { background-color: #ff0000; }", "<div><p>1</p><p>2</p><p>3</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[2].ActualBackgroundColor);
    }

    [TestMethod]
    public void NthChild_Even_MatchesEvenPositions()
    {
        var html = Html(":nth-child(even) { background-color: #ff0000; }", "<div><p>1</p><p>2</p><p>3</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[2].ActualBackgroundColor);
    }

    [TestMethod]
    public void NthChild_NegativeStep_MatchesFirstNPositions()
    {
        // "-n+3" matches positions 1, 2, 3 and nothing beyond.
        var html = Html(":nth-child(-n+3) { background-color: #ff0000; }", "<div><p>1</p><p>2</p><p>3</p><p>4</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[2].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[3].ActualBackgroundColor);
    }

    [TestMethod]
    public void NthLastChild_1_MatchesLastElement()
    {
        var html = Html(":nth-last-child(1) { background-color: #ff0000; }", "<div><p>1</p><p>2</p><p>3</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[2].ActualBackgroundColor);
    }

    // ── :first-of-type / :last-of-type / :only-of-type / :nth-of-type() ──────
    // Mixed-tag siblings prove type-filtering isn't just reusing :nth-child's all-elements scope.

    [TestMethod]
    public void FirstOfType_Matches_FirstOfItsOwnTagOnly()
    {
        var html = Html("p:first-of-type { background-color: #ff0000; }", "<div><span>s</span><p>1</p><p>2</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
    }

    [TestMethod]
    public void LastOfType_Matches_LastOfItsOwnTagOnly()
    {
        var html = Html("p:last-of-type { background-color: #ff0000; }", "<div><p>1</p><p>2</p><span>s</span></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
    }

    [TestMethod]
    public void OnlyOfType_Matches_WhenNoOtherSiblingSharesTag()
    {
        var html = Html("p:only-of-type { background-color: #ff0000; }", "<div><span>s</span><p>1</p></div>");
        var box = FindBoxByTag(html, "p");
        Assert.AreNotEqual(RColor.Transparent, box.ActualBackgroundColor);
    }

    [TestMethod]
    public void OnlyOfType_DoesNotMatch_WhenAnotherSameTagSiblingExists()
    {
        var html = Html("p:only-of-type { background-color: #ff0000; }", "<div><p>1</p><p>2</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        foreach (var b in boxes) Assert.AreEqual(RColor.Transparent, b.ActualBackgroundColor);
    }

    [TestMethod]
    public void NthOfType_2_MatchesSecondOfItsOwnTagOnly()
    {
        var html = Html("p:nth-of-type(2) { background-color: #ff0000; }", "<div><span>s</span><p>1</p><span>s2</span><p>2</p><p>3</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(3, boxes.Count);
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[2].ActualBackgroundColor);
    }

    [TestMethod]
    public void NthLastOfType_1_MatchesLastOfItsOwnTagOnly()
    {
        var html = Html("p:nth-last-of-type(1) { background-color: #ff0000; }", "<div><p>1</p><span>s</span><p>2</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
    }

    // ── Compound forms ─────────────────────────────────────────────────────────

    [TestMethod]
    public void TagPlusNthChild_MatchesOnlyMatchingTagAtThatPosition()
    {
        var html = Html("p:nth-child(2n+1) { background-color: #ff0000; }", "<div><p>1</p><span>2</span><p>3</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor); // <p> at position 1 (odd) - matches
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor); // <p> at position 3 (odd) - matches
    }

    [TestMethod]
    [Spec("css21", "selector.html#first-child")]
    public void ClassPlusFirstChild_MatchesOnlyWhenBothConditionsHold()
    {
        var html = Html(".item:first-child { background-color: #ff0000; }", "<div><p class='item'>1</p><p class='item'>2</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
    }

    [TestMethod]
    [Spec("css21", "selector.html#first-child")]
    public void ClassPlusFirstChild_DoesNotMatch_WhenFirstChildLacksClass()
    {
        var html = Html(".item:first-child { background-color: #ff0000; }", "<div><p>1</p><p class='item'>2</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        foreach (var b in boxes) Assert.AreEqual(RColor.Transparent, b.ActualBackgroundColor);
    }

    // ── :nth-column() / :nth-last-column() ────────────────────────────────────

    [TestMethod]
    public void NthColumn_MatchesAnyOccupiedColumnOfAColspanCell()
    {
        // Row occupies 4 columns total: A=col0, B(colspan=2)=cols1-2, C=col3.
        // nth-column(2) should match B, since one of its occupied columns is position 2.
        var html = Html(
            "td:nth-column(2) { background-color: #ff0000; }",
            "<table><tr><td>A</td><td colspan='2'>B</td><td>C</td></tr></table>");
        var boxes = FindAllBoxesByTag(html, "td");
        Assert.AreEqual(3, boxes.Count);
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor); // A
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor); // B
        Assert.AreEqual(RColor.Transparent, boxes[2].ActualBackgroundColor); // C
    }

    [TestMethod]
    public void NthLastColumn_1_MatchesLastColumnOnly()
    {
        var html = Html(
            "td:nth-last-column(1) { background-color: #ff0000; }",
            "<table><tr><td>A</td><td colspan='2'>B</td><td>C</td></tr></table>");
        var boxes = FindAllBoxesByTag(html, "td");
        Assert.AreEqual(3, boxes.Count);
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor); // A
        Assert.AreEqual(RColor.Transparent, boxes[1].ActualBackgroundColor); // B
        Assert.AreNotEqual(RColor.Transparent, boxes[2].ActualBackgroundColor); // C, the last of 4 columns
    }

    // ── CSS4 "of <selector>" extension (:nth-child()/:nth-last-child() only) ──

    [TestMethod]
    public void NthChildOfSelector_1_MatchesOnlyTheFirstMatchingSibling()
    {
        // Position is counted among the ".foo"-matching subset only, skipping non-".foo" siblings.
        var html = Html(
            ":nth-child(1 of .foo) { background-color: #ff0000; }",
            "<div><p>1</p><p class='foo'>2</p><p>3</p><p class='foo'>4</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(4, boxes.Count);
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor); // "1", not .foo
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor); // "2", first .foo
        Assert.AreEqual(RColor.Transparent, boxes[2].ActualBackgroundColor); // "3", not .foo
        Assert.AreEqual(RColor.Transparent, boxes[3].ActualBackgroundColor); // "4", second .foo
    }

    [TestMethod]
    public void NthChildOfSelector_2nPlus1_AppliesFormulaWithinTheFilteredSubset()
    {
        var html = Html(
            ":nth-child(2n+1 of .foo) { background-color: #ff0000; }",
            "<div><p>skip</p><p class='foo'>foo-1</p><p>skip</p><p class='foo'>foo-2</p><p class='foo'>foo-3</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(5, boxes.Count);
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor); // "skip"
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor); // "foo-1" - .foo position 1 (odd)
        Assert.AreEqual(RColor.Transparent, boxes[2].ActualBackgroundColor); // "skip"
        Assert.AreEqual(RColor.Transparent, boxes[3].ActualBackgroundColor); // "foo-2" - .foo position 2 (even)
        Assert.AreNotEqual(RColor.Transparent, boxes[4].ActualBackgroundColor); // "foo-3" - .foo position 3 (odd)
    }

    [TestMethod]
    public void NthLastChildOfSelector_1_MatchesTheLastMatchingSibling()
    {
        var html = Html(
            ":nth-last-child(1 of .foo) { background-color: #ff0000; }",
            "<div><p class='foo'>1</p><p>2</p><p class='foo'>3</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(3, boxes.Count);
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[2].ActualBackgroundColor); // last .foo
    }

    [TestMethod]
    public void NthChildOfSelector_ElementNotMatchingS_NeverMatchesRegardlessOfPosition()
    {
        // The structurally-first element doesn't itself have class "foo", so it can never satisfy
        // ":nth-child(1 of .foo)" even though it's first among ALL children.
        var html = Html(
            ":nth-child(1 of .foo) { background-color: #ff0000; }",
            "<div><p>first, not foo</p><p class='foo'>second, is foo</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
    }

    [TestMethod]
    public void NthChildOfSelector_CommaSeparatedList_MatchesEitherBranch()
    {
        var html = Html(
            ":nth-child(1 of .foo, .bar) { background-color: #ff0000; }",
            "<div><p>skip</p><p class='bar'>first-of-either</p><p class='foo'>second-of-either</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(3, boxes.Count);
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor); // first among .foo-or-.bar
        Assert.AreEqual(RColor.Transparent, boxes[2].ActualBackgroundColor);
    }

    [TestMethod]
    public void OfSelector_OnDisallowedFunction_InvalidatesTheWholeRule()
    {
        // The parser only allows "of S" on nth-child/nth-last-child - CSS spec has no such clause
        // for nth-of-type/nth-last-of-type/nth-column/nth-last-column. Writing it anyway doesn't
        // silently drop just the clause: it invalidates the entire enclosing selector (parses to
        // UnknownSelector), so the whole rule matches nothing. Locking in this existing, correct
        // parser behavior as a regression guard.
        var html = Html(
            "td:nth-of-type(1 of .foo) { background-color: #ff0000; }",
            "<table><tr><td class='foo'>A</td></tr></table>");
        var box = FindBoxByTag(html, "td");
        Assert.AreEqual(RColor.Transparent, box.ActualBackgroundColor);
    }

    // ── Helpers (mirrors CssSpecificityOrderingTests.cs conventions) ─────────

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
