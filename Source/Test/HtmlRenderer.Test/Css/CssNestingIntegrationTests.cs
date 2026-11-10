using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Nested rules are resolved at parse time into rules with absolute selectors (<c>&amp;</c> becomes
/// <c>:is(parent)</c>), these check that the resolved rules match and cascade, and that the look-ahead
/// that tells a nested rule from a declaration leaves declarations intact.
/// </summary>
[TestClass]
public sealed class CssNestingIntegrationTests
{
    [TestMethod]
    [Spec("css-nesting-1", "#nest-selector")]
    public void ImplicitDescendant_And_ParentDeclaration()
    {
        var html = Html(
            ".box { color: #ff0000; & span { color: #0000ff; } }",
            "<div class='box' id='d'>text<span id='s'>inner</span></div>");
        var d = Box(html, "d");
        var s = Box(html, "s");
        Assert.AreEqual("rgb(255, 0, 0)", d.Color);
        Assert.AreEqual("rgb(0, 0, 255)", s.Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#nest-selector")]
    public void AmpersandCompound_MatchesSameElement()
    {
        var html = Html(
            ".box { &.active { color: #0000ff; } }",
            "<div class='box active' id='d'>x</div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "d").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#nesting")]
    public void TypeSelectorNestedRule_IsNotMistakenForADeclaration()
    {
        // Starts with an ident like a declaration, the '{' before any ';' makes it a rule.
        var html = Html(
            ".card { p { color: #0000ff; } }",
            "<div class='card'><p id='p'>x</p></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "p").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#syntax")]
    public void HexColorInNestedValue_SurvivesTheLookahead()
    {
        // The look-ahead lexes outside value mode, where '#123456' isn't a color.
        var html = Html(
            ".card { p { color: #123456; } }",
            "<div class='card'><p id='p'>x</p></div>");
        Assert.AreEqual("rgb(18, 52, 86)", Box(html, "p").Color);
    }

    [TestMethod]
    public void HexColorInTopLevelValue_Unaffected()
    {
        var html = Html(".card { color: #123456; }", "<div class='card' id='d'>x</div>");
        Assert.AreEqual("rgb(18, 52, 86)", Box(html, "d").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#syntax")]
    public void ChildCombinatorLeading()
    {
        var html = Html(
            ".card { > p { color: #0000ff; } }",
            "<div class='card'><p id='child'>x</p><section><p id='grandchild'>y</p></section></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "child").Color);
        Assert.AreNotEqual("rgb(0, 0, 255)", Box(html, "grandchild").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#mixing")]
    public void DeclarationAfterNestedRule_StillApplies()
    {
        var html = Html(
            ".card { color: #ff0000; & span { color: #00ff00; } color: #0000ff; }",
            "<div class='card' id='d'>t<span id='s'>x</span></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "d").Color);
        Assert.AreEqual("rgb(0, 255, 0)", Box(html, "s").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#nesting")]
    public void ThreeLevelNesting()
    {
        var html = Html(
            ".a { & .b { & .c { color: #0000ff; } } }",
            "<div class='a'><div class='b'><div class='c' id='c'>x</div></div></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "c").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#nest-selector")]
    public void ParentSelectorList_ResolvesToIs()
    {
        var html = Html(
            ".x, .y { & span { color: #0000ff; } }",
            "<div class='x'><span id='sx'>1</span></div><div class='y'><span id='sy'>2</span></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "sx").Color);
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "sy").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#nesting")]
    public void NestedRule_InheritsEnclosingMedia()
    {
        // Box() renders with the print media type.
        var html = Html(
            "@media print { .card { & p { color: #0000ff; } } }",
            "<div class='card'><p id='p'>x</p></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "p").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-cascade-5.yml")]
    [Spec("css-cascade-5", "#layering")]
    public void NestedRule_InheritsEnclosingLayer()
    {
        var html = Html(
            ".card p { color: #0000ff; } @layer base { .card { & p { color: #ff0000; } } }",
            "<div class='card'><p id='p'>x</p></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "p").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#syntax")]
    public void CustomProperty_CoexistsWithNestedRule()
    {
        var html = Html(
            ".card { --c: #0000ff; p { color: var(--c); } }",
            "<div class='card'><p id='p'>x</p></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "p").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#syntax")]
    public void TypePseudoNestedRule_IsConsumed_NotMistakenForADeclaration()
    {
        // 'a:hover { }' could be the declaration 'a: hover', :hover never matches, so the following
        // nested rule applying shows it was consumed as a rule.
        var html = Html(
            ".card { a:hover { color: #ff0000; } p { color: #0000ff; } }",
            "<div class='card'><p id='p'>x</p></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "p").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#syntax")]
    public void BraceInsideUrl_DoesNotTriggerNestedRule()
    {
        var html = Html(
            ".card { background: url(\"a{b}.png\"); & p { color: #0000ff; } }",
            "<div class='card'><p id='p'>x</p></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "p").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#nest-selector")]
    public void AmpersandInsideAttributeValue_IsPreserved_NotTreatedAsNestingSelector()
    {
        // The only '&' is in a string, so the rule is still relative to .card.
        var html = Html(
            ".card { .item[data-state=\"on&off\"] { color: #0000ff; } }",
            "<div class='card'><span class='item' id='m' data-state='on&off'>x</span>" +
            "<span class='item' id='n' data-state='off'>y</span></div>" +
            "<span class='item' id='outside' data-state='on&off'>z</span>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "m").Color);
        Assert.AreNotEqual("rgb(0, 0, 255)", Box(html, "n").Color);
        Assert.AreNotEqual("rgb(0, 0, 255)", Box(html, "outside").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#nest-selector")]
    public void RealAmpersand_WithAmpersandAlsoInsideAttributeValue()
    {
        var html = Html(
            ".card { &[data-state=\"a&b\"] { color: #0000ff; } }",
            "<div class='card' id='d' data-state='a&b'>x</div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "d").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#syntax")]
    public void EscapedQuoteInsideAttributeValue_DoesNotEndTheStringEarly()
    {
        var html = Html(
            ".card { .item[data-x=\"a\\\"b\"] { color: #0000ff; } }",
            "<div class='card'><span class='item' id='e' data-x='a\"b'>x</span></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "e").Color);
    }

    private static string Html(string css, string body) =>
        $"<!DOCTYPE html><html><head><style>{css}</style></head><body>{body}</body></html>";

    private static CssBox Box(string html, string id)
    {
        var (root, _) = LayoutHarness.Layout(html, adapter: new MockAdapter { MediaType = "print" });
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box);
        return box!;
    }
}
