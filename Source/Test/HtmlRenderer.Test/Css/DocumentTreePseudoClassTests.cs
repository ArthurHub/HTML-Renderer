using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class DocumentTreePseudoClassTests
{
    private const string Blue = "rgb(0, 0, 255)";
    private const string IgnoreReason = "Gap, see Spec/selectors-4.yml";

    // ── :empty ─────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Ignore(IgnoreReason)]
    // A run of newlines and tabs does reach the box tree as a text box.
    [DataRow("<p id='p'></p>")]
    [DataRow("<p id='p'>   </p>")]
    [DataRow("<p id='p'>\n\t\r\n </p>")]
    [DataRow("<p id='p'><!-- a comment --></p>")]
    [DataRow("<p id='p'>\n  <!-- a comment -->\n</p>")]
    [Spec("selectors-4", "#the-empty-pseudo")]
    public void Empty_MatchesAnElementWithOnlyWhiteSpaceAndComments(string body)
    {
        var box = Box(Html("p:empty { color: #0000ff; }", body), "p");
        Assert.AreEqual(Blue, box.Color);
    }

    [TestMethod]
    [DataRow("<p id='p'>x</p>")]
    [DataRow("<p id='p'> x </p>")]
    [DataRow("<p id='p'>&nbsp;</p>")]
    [DataRow("<p id='p'><span></span></p>")]
    [DataRow("<p id='p'><br></p>")]
    [Spec("selectors-4", "#the-empty-pseudo")]
    public void Empty_DoesNotMatchAnElementWithTextOrElementChildren(string body)
    {
        var box = Box(Html("p:empty { color: #0000ff; }", body), "p");
        Assert.AreNotEqual(Blue, box.Color);
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    [Spec("selectors-4", "#the-empty-pseudo")]
    public void Empty_IsUnaffectedByGeneratedContent()
    {
        // The ::before box is a child in the box tree, not in the document tree.
        var box = Box(Html("p::before { content: 'x'; } p:empty { color: #0000ff; }", "<p id='p'></p>"), "p");
        Assert.AreEqual(Blue, box.Color);
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    [Spec("selectors-4", "#the-empty-pseudo")]
    public void Empty_IsUnaffectedByASynthesizedMarker()
    {
        // An <li>'s ::marker box is generated too, so an empty list item is still :empty.
        var box = Box(Html("li:empty { color: #0000ff; }", "<ul><li id='li'></li></ul>"), "li");
        Assert.AreEqual(Blue, box.Color);
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    [Spec("selectors-4", "#the-empty-pseudo")]
    public void Empty_ComposesWithCombinatorsAndNegation()
    {
        var html = Html(
            ".row :empty { color: #0000ff; } .row p:not(:empty) { color: #ff0000; }",
            "<div class='row'><p id='blank'></p><p id='filled'>text</p></div>");

        Assert.AreEqual(Blue, Box(html, "blank").Color);
        Assert.AreEqual("rgb(255, 0, 0)", Box(html, "filled").Color);
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    [Spec("selectors-4", "#the-empty-pseudo")]
    public void Empty_SharingAListWithAMatchableSelector_StillApplies()
    {
        var html = Html("h1, p:empty { color: #0000ff; }", "<h1 id='h'>Title</h1><p id='p'></p>");
        Assert.AreEqual(Blue, Box(html, "h").Color);
        Assert.AreEqual(Blue, Box(html, "p").Color);
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    [Spec("selectors-4", "#the-empty-pseudo")]
    public void Empty_SeesThroughAnAnonymousBox()
    {
        // After the cascade, the mixed content is wrapped in anonymous boxes.
        var root = BuildRoot(Html(
            "p { display: block; }",
            "<div id='mixed'>loose text<p>block</p></div><div id='blank'></div>"));

        var mixed = LayoutHarness.FindById(root, "mixed")!;
        var blank = LayoutHarness.FindById(root, "blank")!;

        Assert.IsTrue(mixed.Boxes.Any(b => b.HtmlTag is null && b.Boxes.Count > 0));
        Assert.IsFalse(Matches(mixed, ":empty"));
        Assert.IsTrue(Matches(blank, ":empty"));
    }

    [TestMethod]
    public void Empty_NeverMatchesANonElementBox()
    {
        var root = BuildRoot(Html("p { display: block; }", "<div id='mixed'>loose text<p>block</p></div>"));
        var anonymous = LayoutHarness.FindById(root, "mixed")!.Boxes.First(b => b.HtmlTag is null);

        Assert.IsNull(anonymous.HtmlTag);
        Assert.IsFalse(Matches(anonymous, ":empty"));
    }

    // ── :any-link ──────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Ignore(IgnoreReason)]
    [Spec("selectors-4", "#the-any-link-pseudo")]
    public void AnyLink_MatchesAnAnchorWithAnHref_AndNothingElse()
    {
        var html = Html(
            ":any-link { color: #0000ff; }",
            "<a id='link' href='https://example.com'>link</a><a id='anchor' name='top'>anchor</a><p id='p'>text</p>");

        Assert.AreEqual(Blue, Box(html, "link").Color);
        Assert.AreNotEqual(Blue, Box(html, "anchor").Color);
        Assert.AreNotEqual(Blue, Box(html, "p").Color);
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    [Spec("selectors-4", "#the-any-link-pseudo")]
    public void AnyLink_AndLink_SelectTheSameElements()
    {
        // :visited never matches, so :any-link and :link select the same elements.
        var body = "<a id='link' href='#x'>link</a><a id='anchor'>anchor</a>";
        var withAnyLink = Html("a:any-link { color: #0000ff; }", body);
        var withLink = Html("a:link { color: #0000ff; }", body);

        foreach (var id in new[] { "link", "anchor" })
        {
            Assert.AreEqual(
                Box(withLink, id).Color == Blue,
                Box(withAnyLink, id).Color == Blue);
        }
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    [Spec("selectors-4", "#the-any-link-pseudo")]
    public void AnyLink_ComposesWithACompoundSelector()
    {
        var html = Html(
            "a.cta:any-link { color: #0000ff; }",
            "<a id='cta' class='cta' href='#x'>go</a><a id='plain' href='#y'>plain</a>");

        Assert.AreEqual(Blue, Box(html, "cta").Color);
        Assert.AreNotEqual(Blue, Box(html, "plain").Color);
    }

    // ── :scope ─────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Ignore(IgnoreReason)]
    [Spec("selectors-4", "#the-scope-pseudo")]
    public void Scope_MatchesTheRootElement_LikeRoot()
    {
        var root = BuildRoot(Html(":scope { color: #0000ff; }", "<p id='p'>text</p>"));
        var htmlBox = LayoutHarness.Descendants(root).First(b => b.HtmlTag != null && b.HtmlTag.Name == "html");

        Assert.AreEqual(Blue, htmlBox.Color);
        Assert.IsTrue(Matches(htmlBox, ":scope"));
        Assert.IsTrue(Matches(htmlBox, ":root"));

        foreach (var tag in new[] { "body", "p" })
        {
            var box = LayoutHarness.Descendants(root).First(b => b.HtmlTag != null && b.HtmlTag.Name == tag);
            Assert.IsFalse(Matches(box, ":scope"));
            Assert.IsFalse(Matches(box, ":root"));
        }
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    [Spec("selectors-4", "#the-scope-pseudo")]
    public void Scope_WorksAsTheLeftHandSideOfACombinator()
    {
        var html = Html(
            ":scope p { color: #0000ff; } :scope > body > p.direct { font-weight: bold; }",
            "<p id='deep' class='direct'>text</p>");

        var box = Box(html, "deep");
        Assert.AreEqual(Blue, box.Color);
        Assert.AreEqual("bold", box.FontWeight);
    }

    [TestMethod]
    public void Scope_AndRoot_ShareTheirDeclarationsInAList()
    {
        var box = Box(Html(":root, :scope { --brand: #0000ff; } p { color: var(--brand); }", "<p id='p'>text</p>"), "p");
        Assert.AreEqual(Blue, box.Color);
    }

    private static bool Matches(CssBox box, string selector)
    {
        var cssData = new CssParser(new MockAdapter()).ParseStyleSheet($"{selector} {{ color: red; }}", false);
        return cssData.GetAuthorStyleRules(MediaQueryContext.TypeOnly("print"), box).Any();
    }

    private static string Html(string css, string body) =>
        $"<!DOCTYPE html><html><head><style>{css}</style></head><body>{body}</body></html>";

    private static CssBox Box(string html, string id)
    {
        var root = BuildRoot(html);
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box);
        return box!;
    }

    private static CssBox BuildRoot(string html) => LayoutHarness.Layout(html).Root;
}
