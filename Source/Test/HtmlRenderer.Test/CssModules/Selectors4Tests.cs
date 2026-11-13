using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class Selectors4Tests
{
    private static readonly RColor Red = RColor.FromArgb(255, 255, 0, 0);
    private static readonly RColor Blue = RColor.FromArgb(255, 0, 0, 255);

    // ── 3 Selector Syntax and Structure ──────────────────────────────────

    [TestMethod]
    [Spec("selectors-4", "#structure")]
    public void ComplexSelector_ChainsCompoundSelectorsWithCombinators()
    {
        const string selector = "section.a > div + p#x[title]:last-child";
        Assert.IsTrue(Matches(selector, "<section class='a'><div></div><p id='x' title='t'>a</p></section>"));
        Assert.IsFalse(Matches(selector, "<section><div></div><p id='x' title='t'>a</p></section>"));
        Assert.IsFalse(Matches(selector, "<section class='a'><span></span><p id='x' title='t'>a</p></section>"));
        Assert.IsFalse(Matches(selector, "<section class='a'><div></div><p id='x'>a</p></section>"));
    }

    [TestMethod]
    [Spec("selectors-4", "#pseudo-classes")]
    [Spec("selectors-4", "#grammar")]
    public void FunctionalPseudoClass_AllowsWhitespaceAroundItsArguments()
    {
        Assert.IsTrue(Matches("p:nth-child( 2 )", "<div><span></span><p id='x'>a</p></div>"));
        Assert.IsTrue(Matches("p:not( .a )", "<p id='x'>a</p>"));
        Assert.IsTrue(Matches("p:is( .a , .b )", "<p id='x' class='b'>a</p>"));
    }

    [TestMethod]
    [Spec("selectors-4", "#pseudo-classes")]
    [Spec("selectors-4", "#grammar")]
    public void WhitespaceAfterTheColon_InvalidatesTheRule()
    {
        Assert.IsFalse(IsRed(Document("p: first-child, p { background-color: #ff0000 }", "<div><p id='x'>a</p></div>"), "x"));
    }

    [TestMethod]
    [Spec("selectors-4", "#pseudo-element-syntax")]
    [DataRow("p::marker, p")]
    [DataRow("p::selection, p")]
    public void DoubleColonPseudoElements_AreValid(string selector)
    {
        Assert.IsTrue(Matches(selector, "<p id='x'>a</p>"));
    }

    [TestMethod]
    [Spec("selectors-4", "#pseudo-element-syntax")]
    [DataRow("p:marker, p")]
    [DataRow("p:selection, p")]
    public void SingleColonSyntax_IsOnlyAcceptedForTheLevel2PseudoElements(string selector)
    {
        Assert.IsFalse(Matches(selector, "<p id='x'>a</p>"));
    }

    [TestMethod]
    [Spec("selectors-4", "#pseudo-element-attachment")]
    public void PseudoElement_BindsToTheElementMatchedByTheSelectorBeforeIt()
    {
        var (root, _) = LayoutHarness.Layout(Document("div a::before { content: 'N' }",
            "<div><a id='in' href='#'>a</a></div><a id='out' href='#'>b</a>"));

        Assert.AreEqual("N", BeforeText(LayoutHarness.FindById(root, "in")!));
        Assert.AreEqual("", BeforeText(LayoutHarness.FindById(root, "out")!));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#pseudo-element-attachment")]
    [DataRow("::before { content: 'N' }")]
    [DataRow(":before { content: 'N' }")]
    public void PseudoElement_WithoutACompoundSelector_BindsToEveryElement(string css)
    {
        var (root, _) = LayoutHarness.Layout(Document(css, "<p id='x'>a</p>"));

        Assert.IsTrue(HasBefore(LayoutHarness.FindById(root, "x")!));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#pseudo-element-states")]
    public void UserActionPseudoClass_AfterAPseudoElement_IsValid()
    {
        Assert.IsTrue(Matches("p::before:hover, p", "<p id='x'>a</p>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#pseudo-element-states")]
    [DataRow("p::before.a, p")]
    [DataRow("p::before:first-child, p")]
    public void OtherSimpleSelectors_AfterAPseudoElement_AreInvalid(string selector)
    {
        Assert.IsFalse(Matches(selector, "<p id='x'>a</p>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#sub-pseudo-elements")]
    [DataRow("p::before::before, p")]
    [DataRow("p::marker::marker, p")]
    public void PseudoElement_CompoundedWithAnUndefinedSubPseudoElement_IsInvalid(string selector)
    {
        Assert.IsFalse(Matches(selector, "<p id='x'>a</p>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#pseudo-element-structure")]
    [DataRow("p::first-letter em, p")]
    [DataRow("p::before + span, p")]
    public void Combinator_AfterAPseudoElementWithoutInternalStructure_IsInvalid(string selector)
    {
        Assert.IsFalse(Matches(selector, "<p id='x'>a</p>"));
    }

    [TestMethod]
    [Spec("selectors-4", "#case-sensitive")]
    public void EscapedCharacters_LoseTheirSpecialMeaning()
    {
        Assert.IsTrue(Matches("#foo\\>a", "<p id='foo>a'>a</p>", "foo>a"));
    }

    [TestMethod]
    [Spec("selectors-4", "#case-sensitive")]
    public void FormFeed_IsWhitespace_EmSpaceIsNot()
    {
        Assert.IsTrue(Matches("div\fp", "<div><p id='x'>a</p></div>"));
        Assert.IsFalse(Matches(".a .b", "<div class='a'><p id='x' class='b'>a</p></div>"));
    }

    [TestMethod]
    [Spec("selectors-4", "#invalid")]
    [Spec("selectors-4", "#grouping")]
    [DataRow("h1, h2..foo, p")]
    [DataRow("h1, p:foo")]
    [DataRow("h1, p::foo")]
    [DataRow("h1, p >")]
    public void InvalidSelectorInAList_DropsTheWholeRule(string selector)
    {
        Assert.IsFalse(Matches(selector, "<p id='x'>a</p>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#invalid")]
    [DataRow("h1, , p")]
    [DataRow(", p")]
    public void EmptySelectorInAList_DropsTheWholeRule(string selector)
    {
        Assert.IsFalse(Matches(selector, "<p id='x'>a</p>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#invalid")]
    [Spec("selectors-4", "#type-nmsp")]
    [Spec("selectors-4", "#attrnmsp")]
    [DataRow("p, foo|p")]
    [DataRow("p, [foo|title]")]
    public void UndeclaredNamespacePrefix_InvalidatesTheSelector(string selector)
    {
        Assert.IsFalse(Matches(selector, "<p id='x' title='a'>a</p>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#invalid")]
    public void UnknownWebkitPseudoElement_IsValidButMatchesNothing()
    {
        Assert.IsTrue(Matches("p::-webkit-foo, p", "<p id='x'>a</p>"));
    }

    // ── 4 Logical Combinations ───────────────────────────────────────────

    [TestMethod]
    [Spec("selectors-4", "#matches")]
    public void Is_TakesComplexSelectors()
    {
        Assert.IsTrue(Matches(":is(div p, .a)", "<div><p id='x'>a</p></div>"));
        Assert.IsFalse(Matches(":is(div p, .a)", "<section><p id='x'>a</p></section>"));
    }

    [TestMethod]
    [Spec("selectors-4", "#matches")]
    [Spec("selectors-4", "#zero-matches")]
    [Spec("selectors-4", "#forgiving-selector")]
    [DataRow(":is(p, :foo)")]
    [DataRow(":where(p, :foo)")]
    public void IsAndWhere_DropInvalidArguments(string selector)
    {
        Assert.IsTrue(Matches(selector, "<p id='x'>a</p>"));
    }

    [TestMethod]
    [Spec("selectors-4", "#negation")]
    public void Not_TakesAListOfComplexSelectors()
    {
        Assert.IsFalse(Matches("p:not(.a, .b)", "<p id='x' class='b'>a</p>"));
        Assert.IsTrue(Matches("p:not(.a, .b)", "<p id='x' class='c'>a</p>"));
        Assert.IsFalse(Matches("p:not(div p)", "<div><p id='x'>a</p></div>"));
        Assert.IsTrue(Matches("p:not(div p)", "<section><p id='x'>a</p></section>"));
    }

    [TestMethod]
    [Spec("selectors-4", "#negation")]
    public void Not_WithAnInvalidArgument_InvalidatesTheRule()
    {
        Assert.IsFalse(Matches("p:not(:foo), p", "<p id='x'>a</p>"));
        Assert.IsFalse(Matches("p:not(::before), p", "<p id='x'>a</p>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#relational")]
    public void Has_IsNotValidInsideHas()
    {
        Assert.IsFalse(Matches("div:has(:has(p)), div:has(p)", "<div id='x'><section><p>a</p></section></div>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#relational")]
    public void PseudoElements_AreNotValidInsideHas()
    {
        Assert.IsFalse(Matches("div:has(::before), div:has(p)", "<div id='x'><p>a</p></div>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#relational")]
    public void Has_IgnoresTextChildren()
    {
        Assert.IsFalse(Matches("div:has(*)", "<div id='x'>just text</div>"));
    }

    // ── 5 Elemental selectors ────────────────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#type-nmsp")]
    [DataRow("*|p", true)]
    [DataRow("*|*", true)]
    [DataRow("|p", false)]
    public void NamespacePrefixes_OnTypeSelectors(string selector, bool matches)
    {
        Assert.AreEqual(matches, Matches(selector, "<p id='x'>a</p>"));
    }

    // ── 6 Attribute selectors ────────────────────────────────────────────

    [TestMethod]
    [Spec("selectors-4", "#attribute-substrings")]
    public void AttributeContains_MatchesASubstring()
    {
        Assert.IsTrue(Matches("[title*=b]", "<p id='x' title='abc'>a</p>"));
        Assert.IsFalse(Matches("[title*=x]", "<p id='x' title='abc'>a</p>"));
        Assert.IsFalse(Matches("[title*=b]", "<p id='x'>a</p>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#attribute-substrings")]
    [DataRow("[title^='']")]
    [DataRow("[title$='']")]
    [DataRow("[title*='']")]
    public void SubstringSelector_WithAnEmptyValue_MatchesNothing(string selector)
    {
        Assert.IsFalse(Matches(selector, "<p id='x' title='a'>a</p>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#attribute-substrings")]
    [DataRow("[title^=AB]")]
    [DataRow("[title$=BC]")]
    [DataRow("[title*=B]")]
    public void SubstringSelector_IsCaseSensitive(string selector)
    {
        Assert.IsFalse(Matches(selector, "<p id='x' title='abc'>a</p>"));
    }

    [TestMethod]
    [Spec("selectors-4", "#attribute-case")]
    [DataRow("[title=foo i]", "FOO")]
    [DataRow("[title='foo' I]", "FOO")]
    [DataRow("[title^=f i]", "FOO")]
    [DataRow("[title=foo s]", "foo")]
    public void CaseSensitivityFlags_Match(string selector, string value)
    {
        Assert.IsTrue(Matches(selector, $"<p id='x' title='{value}'>a</p>"));
    }

    [TestMethod]
    [Spec("selectors-4", "#attribute-case")]
    public void SensitiveFlag_DoesNotMatchADifferentCase()
    {
        Assert.IsFalse(Matches("[title=foo s]", "<p id='x' title='FOO'>a</p>"));
    }

    [TestMethod]
    [Spec("selectors-4", "#attrnmsp")]
    public void AttributeWithoutNamespace_MatchesWithAnEmptyPrefix()
    {
        Assert.IsTrue(Matches("[|title]", "<p id='x' title='a'>a</p>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#attrnmsp")]
    public void AttributeWithAnyNamespace_Matches()
    {
        Assert.IsTrue(Matches("[*|title]", "<p id='x' title='a'>a</p>"));
    }

    // ── 7 Linguistic Pseudo-classes ──────────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#the-lang-pseudo")]
    [DataRow(":lang(fr, de)", "de")]
    [DataRow(":lang('*-CH')", "de-CH")]
    [DataRow(":lang(de-DE)", "de-Latn-DE")]
    public void Lang_TakesListsAndWildcards(string selector, string lang)
    {
        Assert.IsTrue(Matches(selector, $"<p id='x' lang='{lang}'>a</p>"));
    }

    // ── 8 Location Pseudo-classes ────────────────────────────────────────

    [TestMethod]
    [Spec("selectors-4", "#the-target-pseudo")]
    public void Target_IsValidButNeverMatches_WithoutAUrlFragment()
    {
        Assert.IsFalse(Matches(":target", "<p id='x'>a</p>"));
        Assert.IsTrue(Matches("p, :target", "<p id='x'>a</p>"));
    }

    // ── 9 User Action Pseudo-classes ─────────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#the-hover-pseudo")]
    [DataRow("p, a:hover")]
    [DataRow("p:hover::before, p")]
    public void HoverInOneAlternative_DoesntHoldBackTheOthers(string selector)
    {
        Assert.IsTrue(Matches(selector, "<p id='x'>a</p>"));
    }

    // ── 13 Tree-Structural pseudo-classes ────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#child-index")]
    [Spec("selectors-4", "#typed-child-index")]
    [DataRow("html:first-child p")]
    [DataRow("html:only-child p")]
    [DataRow("html:first-of-type p")]
    [DataRow("html:only-of-type p")]
    public void RootElement_IsItsOwnOnlySibling(string selector)
    {
        Assert.IsTrue(Matches(selector, "<p id='x'>a</p>"));
    }

    [TestMethod]
    [Spec("selectors-4", "#the-nth-child-pseudo")]
    public void NthChildOf_CountsComplexSelectors()
    {
        Assert.IsTrue(Matches("p:nth-child(2 of div p)", "<div><p>a</p><span></span><p id='x'>a</p></div>"));
        Assert.IsFalse(Matches("p:nth-child(1 of .a, #x)", "<div><p class='a'>a</p><p id='x'>a</p></div>"));
    }

    [TestMethod]
    [Spec("selectors-4", "#the-nth-last-of-type-pseudo")]
    [Spec("selectors-4", "#the-only-of-type-pseudo")]
    public void TypedChildIndex_CountsOnlySiblingsOfTheSameType()
    {
        Assert.IsTrue(Matches("p:nth-last-of-type(2)", "<div><p id='x'>a</p><span></span><p>b</p><span></span></div>"));
        Assert.IsTrue(Matches("p:only-of-type", "<div><span></span><p id='x'>a</p><span></span></div>"));
    }

    // ── 14 Combinators ───────────────────────────────────────────────────

    [TestMethod]
    [Spec("selectors-4", "#general-sibling-combinators")]
    [Spec("selectors-4", "#adjacent-sibling-combinators")]
    public void SiblingCombinators_SkipTextAndChain()
    {
        Assert.IsTrue(Matches("h1 ~ pre", "<h1>a</h1>text<p>b</p><pre id='x'>c</pre>"));
        Assert.IsTrue(Matches(".a ~ span ~ p", "<span></span><div class='a'></div><p></p><span></span><p id='x'>a</p>"));
        Assert.IsTrue(Matches(".a ~ p + p", "<div class='a'></div><span></span><p>a</p><p id='x'>a</p>"));
    }

    // ── 15 Specificity ───────────────────────────────────────────────────

    [TestMethod]
    [Spec("selectors-4", "#specificity-rules")]
    [DataRow("p:not(.q, #y) { color: #ff0000 } p.a.b { color: #0000ff }", "<p id='x' class='a b'>a</p>")]
    [DataRow("div:has(#y, .q) { color: #ff0000 } div.a.b { color: #0000ff }", "<div id='x' class='a b'><p id='y'>a</p></div>")]
    [DataRow(".a.a { color: #ff0000 } .a { color: #0000ff }", "<p id='x' class='a'>a</p>")]
    [DataRow("p.a { color: #0000ff } p:where(#x#x), .a.b { color: #ff0000 }", "<p id='x' class='a b'>a</p>")]
    public void Specificity_OfLogicalCombinationsAndRepeatedSelectors(string css, string body)
    {
        Assert.AreEqual(Red, Box(css, body).ActualColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#specificity-rules")]
    [Spec("selectors-4", "#the-nth-child-pseudo")]
    public void NthChildOf_AddsTheSpecificityOfItsSelectorList()
    {
        Assert.AreEqual(Red, Box(":nth-child(1 of #x) { color: #ff0000 } p.a.b.c { color: #0000ff }",
            "<div><p id='x' class='a b c'>a</p></div>").ActualColor);
    }

    [TestMethod]
    [Spec("selectors-4", "#specificity-rules")]
    public void NthChild_WithoutASelectorList_CountsAsOnePseudoClass()
    {
        Assert.AreEqual(Blue, Box(":nth-child(1) { color: #ff0000 } p.a { color: #0000ff }",
            "<div><p id='x' class='a'>a</p></div>").ActualColor);
    }

    private static string Document(string css, string body) =>
        $"<!DOCTYPE html><html><head><style>{css}</style></head><body>{body}</body></html>";

    [TestMethod]
    [Spec("selectors-4", "#video-state")]
    [Spec("selectors-4", "#media-loading-state")]
    [Spec("selectors-4", "#sound-state")]
    [Spec("selectors-4", "#popover-open-state")]
    [Spec("selectors-4", "#modal-state")]
    [Spec("selectors-4", "#fullscreen-state")]
    [Spec("selectors-4", "#pip-state")]
    [Spec("selectors-4", "#autofill")]
    [Spec("selectors-4", "#user-pseudos")]
    [DataRow(":playing")]
    [DataRow(":paused")]
    [DataRow(":seeking")]
    [DataRow(":buffering")]
    [DataRow(":stalled")]
    [DataRow(":muted")]
    [DataRow(":volume-locked")]
    [DataRow(":popover-open")]
    [DataRow(":modal")]
    [DataRow(":fullscreen")]
    [DataRow(":picture-in-picture")]
    [DataRow(":autofill")]
    [DataRow(":user-valid")]
    [DataRow(":user-invalid")]
    public void StatesThatNeverOccur_MatchNothingAndKeepTheRule(string pseudoClass)
    {
        var body = "<p id='x'>a</p><p id='y'>b</p>";

        Assert.IsFalse(Matches("p" + pseudoClass, body));
        Assert.IsTrue(Matches("p" + pseudoClass + ", #y", body, "y"));
    }

    private static bool Matches(string selector, string body, string id = "x") =>
        IsRed(Document(selector + " { background-color: #ff0000 }", body), id);

    private static bool IsRed(string html, string id)
    {
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box, $"No box with id '{id}'");
        return box.ActualBackgroundColor == Red;
    }

    private static CssBox Box(string css, string body)
    {
        var (root, _) = LayoutHarness.Layout(Document(css, body));
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box;
    }

    private static bool HasBefore(CssBox box) => BeforeText(box).Length > 0;

    private static string BeforeText(CssBox box) =>
        string.Concat(box.Boxes.Where(b => b.IsBeforePseudoElement)
            .SelectMany(LayoutHarness.Descendants).SelectMany(b => b.Words).Select(w => w.Text.Trim()));
}
