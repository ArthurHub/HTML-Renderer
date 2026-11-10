using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css.Selectors;

[TestClass]
public sealed class Css21SelectorTests
{
    private static readonly RColor Red = RColor.FromArgb(255, 255, 0, 0);

    [TestMethod]
    [Spec("css21", "selector.html#pattern-matching")]
    [Spec("css21", "selector.html#type-selectors")]
    [Spec("selectors-4", "#type-selectors")]
    [Spec("selectors-4", "#case-sensitive")]
    public void TypeSelector_MatchesElementNamesCaseInsensitively()
    {
        Assert.IsTrue(Matches("DIV", "<div id='x'>a</div>"));
        Assert.IsTrue(Matches("div", "<DIV id='x'>a</DIV>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#type-selectors")]
    [Spec("selectors-4", "#type-selectors")]
    public void TypeSelector_MatchesOnlyThatElementType()
    {
        Assert.IsTrue(Matches("p", "<p id='x'>a</p>"));
        Assert.IsFalse(Matches("p", "<div id='x'>a</div>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#selector-syntax")]
    [Spec("selectors-4", "#structure")]
    public void SimpleSelector_MatchesOnlyWhenAllComponentsMatch()
    {
        Assert.IsTrue(Matches("p.a#x[title]:first-child", "<div><p id='x' class='a' title='t'>a</p></div>"));
        Assert.IsFalse(Matches("p.a#x[title]:first-child", "<div><p id='x' class='a'>a</p></div>"));
        Assert.IsFalse(Matches("p.a#x[title]:first-child", "<div><p id='x' title='t'>a</p></div>"));
        Assert.IsFalse(Matches("p.a#x[title]:first-child", "<div><span></span><p id='x' class='a' title='t'>a</p></div>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#selector-syntax")]
    [Spec("selectors-4", "#child-combinators")]
    [Spec("selectors-4", "#adjacent-sibling-combinators")]
    public void Combinators_WorkWithAndWithoutSurroundingWhitespace()
    {
        Assert.IsTrue(Matches("div>p", "<div><p id='x'>a</p></div>"));
        Assert.IsTrue(Matches("div  >  p", "<div><p id='x'>a</p></div>"));
        Assert.IsTrue(Matches("h1+p", "<h1>h</h1><p id='x'>a</p>"));
        Assert.IsTrue(Matches("h1  +  p", "<h1>h</h1><p id='x'>a</p>"));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "selector.html#selector-syntax")]
    [Spec("selectors-4", "#descendant-combinators")]
    public void CombinatorChain_TriesFartherAncestors()
    {
        Assert.IsTrue(Matches(".a > div span", "<section class='a'><div><div><span id='x'>a</span></div></div></section>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#grouping")]
    [Spec("selectors-4", "#grouping")]
    public void SelectorGroup_AppliesTheDeclarationsToEveryAlternative()
    {
        var html = Document("h1, h2, p { background-color: #ff0000 }", "<h1 id='a'>a</h1><h2 id='b'>b</h2><p id='c'>c</p><div id='d'>d</div>");

        Assert.IsTrue(IsRed(html, "a"));
        Assert.IsTrue(IsRed(html, "b"));
        Assert.IsTrue(IsRed(html, "c"));
        Assert.IsFalse(IsRed(html, "d"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#universal-selector")]
    [Spec("selectors-4", "#the-universal-selector")]
    public void UniversalSelector_MatchesAnyElement()
    {
        Assert.IsTrue(Matches("*", "<p id='x'>a</p>"));
        Assert.IsTrue(Matches("*", "<span id='x'>a</span>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#universal-selector")]
    [Spec("selectors-4", "#the-universal-selector")]
    public void UniversalSelector_CanBeOmittedInFrontOfOtherComponents()
    {
        Assert.IsTrue(Matches("*[lang=fr]", "<p id='x' lang='fr'>a</p>"));
        Assert.IsTrue(Matches("*.warning", "<p id='x' class='warning'>a</p>"));
        Assert.IsTrue(Matches("*#x", "<p id='x'>a</p>"));
        Assert.IsFalse(Matches("*.warning", "<p id='x'>a</p>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#descendant-selectors")]
    [Spec("selectors-4", "#descendant-combinators")]
    public void DescendantSelector_MatchesAtAnyDepth()
    {
        Assert.IsTrue(Matches("h1 em", "<h1>This <span class='myclass'>is <em id='x'>very</em></span></h1>"));
        Assert.IsFalse(Matches("h1 em", "<p>This <em id='x'>very</em></p>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#descendant-selectors")]
    [Spec("selectors-4", "#descendant-combinators")]
    public void DescendantSelector_WithUniversalInBetween_SkipsChildren()
    {
        var html = Document("div * p { background-color: #ff0000 }",
            "<div><p id='child'>a</p><section><p id='grandchild'>b</p></section></div>");

        Assert.IsFalse(IsRed(html, "child"));
        Assert.IsTrue(IsRed(html, "grandchild"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#descendant-selectors")]
    [Spec("selectors-4", "#descendant-combinators")]
    public void DescendantSelector_CombinesWithAttributeSelectors()
    {
        Assert.IsTrue(Matches("div p *[href]", "<div><p>a <a id='x' href='#'>link</a></p></div>"));
        Assert.IsFalse(Matches("div p *[href]", "<div><p>a <a id='x'>link</a></p></div>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#child-selectors")]
    [Spec("selectors-4", "#child-combinators")]
    public void ChildSelector_MatchesChildrenOfBody()
    {
        var html = Document("body > p { background-color: #ff0000 }", "<p id='a'>a</p><div><p id='b'>b</p></div>");

        Assert.IsTrue(IsRed(html, "a"));
        Assert.IsFalse(IsRed(html, "b"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#child-selectors")]
    [Spec("selectors-4", "#child-combinators")]
    public void ChildSelector_MixedWithDescendantSelectors()
    {
        Assert.IsTrue(Matches("div ol>li p", "<div><section><ol><li><span><p id='x'>a</p></span></li></ol></section></div>"));
        Assert.IsFalse(Matches("div ol>li p", "<div><ol><div><li><p id='x'>a</p></li></div></ol></div>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#adjacent-selectors")]
    [Spec("selectors-4", "#adjacent-sibling-combinators")]
    public void AdjacentSelector_IgnoresTextAndComments()
    {
        Assert.IsTrue(Matches("h1 + h2", "<div><h1>a</h1> text <!-- c --> <h2 id='x'>b</h2></div>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#adjacent-selectors")]
    [Spec("selectors-4", "#adjacent-sibling-combinators")]
    public void AdjacentSelector_WithClassOnTheFirstElement()
    {
        Assert.IsTrue(Matches("h1.opener + h2", "<h1 class='opener'>a</h1><h2 id='x'>b</h2>"));
        Assert.IsFalse(Matches("h1.opener + h2", "<h1>a</h1><h2 id='x'>b</h2>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#matching-attrs")]
    [Spec("selectors-4", "#attribute-representation")]
    public void AttributePresence_MatchesWhateverTheValue()
    {
        Assert.IsTrue(Matches("h1[title]", "<h1 id='x' title='t'>a</h1>"));
        Assert.IsTrue(Matches("h1[title]", "<h1 id='x' title=''>a</h1>"));
        Assert.IsFalse(Matches("h1[title]", "<h1 id='x'>a</h1>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#matching-attrs")]
    [Spec("selectors-4", "#attribute-representation")]
    public void AttributeEquals_MatchesTheExactValue()
    {
        Assert.IsTrue(Matches("span[class=example]", "<span id='x' class='example'>a</span>"));
        Assert.IsFalse(Matches("span[class=example]", "<span id='x' class='example other'>a</span>"));
        Assert.IsTrue(Matches("a[href=\"http://www.w3.org/\"]", "<a id='x' href='http://www.w3.org/'>a</a>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#matching-attrs")]
    [Spec("selectors-4", "#attribute-representation")]
    public void SeveralAttributeSelectors_MustAllMatch()
    {
        const string selector = "span[hello=\"Cleveland\"][goodbye=\"Columbus\"]";
        Assert.IsTrue(Matches(selector, "<span id='x' hello='Cleveland' goodbye='Columbus'>a</span>"));
        Assert.IsFalse(Matches(selector, "<span id='x' hello='Cleveland'>a</span>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#matching-attrs")]
    [Spec("selectors-4", "#attribute-representation")]
    public void AttributeIncludes_MatchesOneWordOfAWhitespaceSeparatedList()
    {
        Assert.IsTrue(Matches("a[rel~=\"copyright\"]", "<a id='x' rel='copyright copyleft copyeditor'>a</a>"));
        Assert.IsFalse(Matches("a[rel~=\"copy\"]", "<a id='x' rel='copyright copyleft'>a</a>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#matching-attrs")]
    [Spec("selectors-4", "#attribute-representation")]
    public void AttributeIncludes_WithWhitespaceOrEmptyValue_NeverMatches()
    {
        Assert.IsFalse(Matches("a[rel~=\"a b\"]", "<a id='x' rel='a b'>a</a>"));
        Assert.IsFalse(Matches("a[rel~=\"\"]", "<a id='x' rel=''>a</a>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#matching-attrs")]
    [Spec("selectors-4", "#attribute-representation")]
    public void AttributeDashMatch_MatchesTheValueOrTheValueFollowedByAHyphen()
    {
        Assert.IsTrue(Matches("*[lang|=\"en\"]", "<p id='x' lang='en'>a</p>"));
        Assert.IsTrue(Matches("*[lang|=\"en\"]", "<p id='x' lang='en-US'>a</p>"));
        Assert.IsTrue(Matches("*[lang|=\"en\"]", "<p id='x' lang='en-cockney'>a</p>"));
        Assert.IsFalse(Matches("*[lang|=\"en\"]", "<p id='x' lang='english'>a</p>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#matching-attrs")]
    [Spec("selectors-4", "#case-sensitive")]
    public void AttributeNames_AreCaseInsensitiveInHtml()
    {
        Assert.IsTrue(Matches("[TITLE]", "<p id='x' title='t'>a</p>"));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "selector.html#matching-attrs")]
    [Spec("selectors-4", "#attribute-representation")]
    [Spec("selectors-4", "#case-sensitive")]
    public void AttributeValues_AreCaseSensitiveInHtml()
    {
        Assert.IsFalse(Matches("[title=Foo]", "<p id='x' title='foo'>a</p>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#class-html")]
    [Spec("selectors-4", "#class-html")]
    public void ClassSelector_MatchesOneOfTheClasses()
    {
        Assert.IsTrue(Matches(".pastoral", "<h1 id='x' class='pastoral'>a</h1>"));
        Assert.IsTrue(Matches("H1.pastoral", "<h1 id='x' class='pastoral'>a</h1>"));
        Assert.IsFalse(Matches("H1.pastoral", "<h1 id='x'>a</h1>"));
        Assert.IsFalse(Matches("H1.pastoral", "<p id='x' class='pastoral'>a</p>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#class-html")]
    [Spec("selectors-4", "#class-html")]
    public void SeveralClassSelectors_MustAllMatch()
    {
        Assert.IsTrue(Matches("p.marine.pastoral", "<p id='x' class='pastoral blue aqua marine'>a</p>"));
        Assert.IsFalse(Matches("p.marine.pastoral", "<p id='x' class='pastoral blue'>a</p>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#class-html")]
    [Spec("selectors-4", "#class-html")]
    public void ClassSelector_IsTheSameAsAttributeIncludes()
    {
        Assert.IsTrue(Matches("div[class~=value]", "<div id='x' class='a value b'>a</div>"));
        Assert.IsTrue(Matches("div.value", "<div id='x' class='a value b'>a</div>"));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "selector.html#class-html")]
    [Spec("selectors-4", "#class-html")]
    public void ClassSelector_IsCaseSensitiveInHtml()
    {
        Assert.IsFalse(Matches(".Foo", "<p id='x' class='foo'>a</p>"));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "selector.html#class-html")]
    [Spec("selectors-4", "#class-html")]
    public void ClassSelector_SplitsTheClassAttributeOnAnyWhitespace()
    {
        Assert.IsTrue(Matches(".b", "<p id='x' class='a\tb'>a</p>"));
        Assert.IsTrue(Matches(".b", "<p id='x' class='a\nb'>a</p>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#id-selectors")]
    [Spec("selectors-4", "#id-selectors")]
    public void IdSelector_MatchesTheElementWithThatId()
    {
        Assert.IsTrue(Matches("*#z98y", "<p id='z98y'>a</p>", "z98y"));
        Assert.IsTrue(Matches("#z98y", "<p id='z98y'>a</p>", "z98y"));
        Assert.IsFalse(Matches("h1#z98y", "<p id='z98y'>a</p>", "z98y"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#id-selectors")]
    [Spec("selectors-4", "#specificity-rules")]
    public void IdSelector_IsMoreSpecificThanAnAttributeSelector()
    {
        var html = Document("#p123 { background-color: #ff0000 } [id=p123] { background-color: #0000ff }", "<p id='p123'>a</p>");
        var (root, _) = LayoutHarness.Layout(html);

        Assert.AreEqual(Red, LayoutHarness.FindById(root, "p123")!.ActualBackgroundColor);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "selector.html#id-selectors")]
    [Spec("selectors-4", "#id-selectors")]
    public void IdSelector_IsCaseSensitiveInHtml()
    {
        Assert.IsFalse(Matches("#Foo", "<p id='foo'>a</p>", "foo"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#pseudo-elements")]
    [Spec("selectors-4", "#pseudo-classes")]
    public void PseudoClasses_CanAppearAnywhereInTheSelector()
    {
        Assert.IsTrue(Matches("div:first-child > p", "<div><p id='x'>a</p></div>"));
        Assert.IsFalse(Matches("div:first-child > p", "<span></span><div><p id='x'>a</p></div>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#pseudo-elements")]
    [Spec("selectors-4", "#pseudo-classes")]
    public void SeveralPseudoClasses_CanApplyToTheSameElement()
    {
        Assert.IsTrue(Matches("a:link:first-child", "<p><a id='x' href='#'>a</a></p>"));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "selector.html#pseudo-elements")]
    [Spec("selectors-4", "#pseudo-classes")]
    [Spec("selectors-4", "#case-sensitive")]
    public void PseudoClassNames_AreCaseInsensitive()
    {
        Assert.IsTrue(Matches("p:FIRST-CHILD", "<div><p id='x'>a</p></div>"));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "selector.html#pseudo-elements")]
    [Spec("selectors-4", "#pseudo-element-syntax")]
    public void PseudoElementNames_AreCaseInsensitive()
    {
        var p = Layout("p:BEFORE { content: 'N' }", "<p id='x'>a</p>");

        Assert.AreEqual("N", GeneratedText(p, before: true));
    }

    [TestMethod]
    [Spec("css21", "selector.html#first-child")]
    [Spec("selectors-4", "#the-first-child-pseudo")]
    public void FirstChild_MatchesTheFirstChildElement()
    {
        Assert.IsTrue(Matches("div > p:first-child", "<p>before</p><div class='note'><p id='x'>first</p></div>"));
        Assert.IsFalse(Matches("div > p:first-child", "<p>before</p><div class='note'><h2>Note</h2><p id='x'>first</p></div>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#first-child")]
    [Spec("selectors-4", "#the-first-child-pseudo")]
    [Spec("selectors-4", "#structural-pseudos")]
    public void FirstChild_IgnoresLeadingText()
    {
        Assert.IsTrue(Matches("em:first-child", "<p>abc <em id='x'>default</em></p>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#first-child")]
    [Spec("selectors-4", "#the-first-child-pseudo")]
    public void FirstChild_InADescendantChain()
    {
        Assert.IsTrue(Matches("p:first-child em", "<div><p>a <span><em id='x'>b</em></span></p></div>"));
        Assert.IsFalse(Matches("p:first-child em", "<div><span></span><p>a <em id='x'>b</em></p></div>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#link-pseudo-classes")]
    [Spec("selectors-4", "#link")]
    public void Link_MatchesAnchorsWithAnHref()
    {
        Assert.IsTrue(Matches("a:link", "<a id='x' href='http://out.side/'>a</a>"));
        Assert.IsTrue(Matches(":link", "<a id='x' href='http://out.side/'>a</a>"));
        Assert.IsFalse(Matches("a:link", "<a id='x' name='n'>a</a>"));
        Assert.IsFalse(Matches(":link", "<span id='x'>a</span>"));
    }

    [TestMethod]
    [Spec("css21", "selector.html#link-pseudo-classes")]
    [Spec("selectors-4", "#link")]
    public void Visited_NeverMatches_AllLinksCountAsUnvisited()
    {
        Assert.IsFalse(Matches("a:visited", "<a id='x' href='http://out.side/'>a</a>"));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "selector.html#dynamic-pseudo-classes")]
    [Spec("selectors-4", "#the-hover-pseudo")]
    public void Hover_OnANonSubjectElement_DoesntMatchWhileNothingIsHovered()
    {
        Assert.IsFalse(Matches("div:hover p", "<div><p id='x'>a</p></div>"));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "selector.html#dynamic-pseudo-classes")]
    [Spec("selectors-4", "#the-hover-pseudo")]
    public void Hover_AppliesWhileTheMouseIsOverTheElement()
    {
        var adapter = new MockAdapter();
        var html = Document("p:hover { background-color: #ff0000 }", "<p id='x' style='height:20px'>a</p>");
        var (root, container) = LayoutHarness.Layout(html, adapter: adapter);
        var p = LayoutHarness.FindById(root, "x")!;

        container.HandleMouseMove(new MockControl(adapter), new RPoint(p.Location.X + 1, p.Location.Y + 1));

        Assert.AreEqual(Red, p.ActualBackgroundColor);
    }

    [TestMethod]
    [Spec("css21", "selector.html#matching-attrs")]
    [Spec("selectors-4", "#attribute-representation")]
    public void AttributeDashMatch_OnlyMatchesTheElementWithTheAttribute()
    {
        var html = Document("[lang|=fr] { background-color: #ff0000 }", "<div id='b' lang='fr'><p id='p'>Je suis Français.</p></div>");

        Assert.IsTrue(IsRed(html, "b"));
        Assert.IsFalse(IsRed(html, "p"));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "selector.html#lang")]
    [Spec("selectors-4", "#the-lang-pseudo")]
    public void Lang_MatchesElementsInThatLanguage()
    {
        var html = Document(":lang(fr) { background-color: #ff0000 }", "<div id='b' lang='fr-CA'><p id='p'>Je suis Français.</p></div><p id='en' lang='en'>a</p>");

        Assert.IsTrue(IsRed(html, "b"));
        Assert.IsTrue(IsRed(html, "p"));
        Assert.IsFalse(IsRed(html, "en"));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "selector.html#first-line-pseudo")]
    [Spec("css-pseudo-4", "#first-line-pseudo")]
    public void FirstLine_StylesTheFirstFormattedLine()
    {
        var p = Layout("p:first-line { color: #ff0000 }", "<p id='x'>Hello</p>");

        Assert.AreEqual(Red, FirstWord(p)!.OwnerBox.ActualColor);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "selector.html#first-letter")]
    [Spec("css-pseudo-4", "#first-letter-pseudo")]
    public void FirstLetter_StylesTheFirstLetter()
    {
        var p = Layout("p:first-letter { color: #ff0000 }", "<p id='x'>Hello</p>");
        var first = FirstWord(p)!;

        Assert.AreEqual("H", first.Text);
        Assert.AreEqual(Red, first.OwnerBox.ActualColor);
    }

    [TestMethod]
    [Spec("css21", "selector.html#before-and-after")]
    [Spec("selectors-4", "#pseudo-element-syntax")]
    [Spec("css-pseudo-4", "#css2-compat")]
    public void BeforeAndAfter_WithASingleColon_GenerateContent()
    {
        var p = Layout("p:before { content: 'Note: ' } p:after { content: '!' }", "<p id='x'>a</p>");

        Assert.AreEqual("Note:", GeneratedText(p, before: true));
        Assert.AreEqual("!", GeneratedText(p, before: false));
    }

    private static string Document(string css, string body) =>
        $"<!DOCTYPE html><html><head><style>{css}</style></head><body>{body}</body></html>";

    private static bool Matches(string selector, string body, string id = "x") =>
        IsRed(Document(selector + " { background-color: #ff0000 }", body), id);

    private static bool IsRed(string html, string id)
    {
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box, $"No box with id '{id}'");
        return box.ActualBackgroundColor == Red;
    }

    private static CssBox Layout(string css, string body)
    {
        var (root, _) = LayoutHarness.Layout(Document(css, body));
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box;
    }

    private static string GeneratedText(CssBox box, bool before)
    {
        var generated = box.Boxes.FirstOrDefault(b => before ? b.IsBeforePseudoElement : b.IsAfterPseudoElement);
        Assert.IsNotNull(generated);
        return string.Join(" ", LayoutHarness.Descendants(generated).SelectMany(b => b.Words).Select(w => w.Text.Trim()));
    }

    private static CssRect? FirstWord(CssBox box) =>
        box.Words.Count > 0 ? box.Words[0] : box.Boxes.Select(FirstWord).FirstOrDefault(w => w != null);

    private sealed class MockControl(RAdapter adapter) : RControl(adapter)
    {
        public override bool LeftMouseButton => false;
        public override bool RightMouseButton => false;
        public override RPoint MouseLocation => RPoint.Empty;
        public override void SetCursorDefault() { }
        public override void SetCursorHand() { }
        public override void SetCursorIBeam() { }
        public override void DoDragDropCopy(object dragDropData) { }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            charFit = str.Length;
            charFitWidth = 0;
        }

        public override void Invalidate() { }
    }
}
