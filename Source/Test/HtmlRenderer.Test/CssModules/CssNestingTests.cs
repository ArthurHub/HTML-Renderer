using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssNestingTests
{
    private const string Green = "rgb(0, 128, 0)";
    private const string Red = "rgb(255, 0, 0)";

    [TestMethod]
    [Spec("css-nesting-1", "#nesting")]
    public void NestedRule_IsRelativeToTheParentsElements()
    {
        const string body = "<div class=a><p id=x>a</p></div><p id=y>b</p>";

        Assert.AreEqual(Green, Styled(".a { p { color: green } }", body).Color);
        Assert.AreNotEqual(Green, Styled(".a { p { color: green } }", body, "y").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#syntax")]
    [DataRow(".a { > #x { color: green } }")]
    [DataRow(".a { + #x { color: green } }")]
    [DataRow(".a { ~ #x { color: green } }")]
    [DataRow(".a { #x { color: green } }")]
    public void RelativeSelector_IsRelativeToTheNestingSelector(string css)
    {
        var body = css.Contains('>') || !css.Contains('+') && !css.Contains('~')
            ? "<div class=a><p id=x>a</p></div>"
            : "<div class=a>a</div><p id=x>b</p>";

        Assert.AreEqual(Green, Styled(css, body).Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#syntax")]
    public void SelectorContainingTheNestingSelector_IsntRelative()
    {
        Assert.AreEqual(Green, Styled(".a { .p & { color: green } }", "<div class=p><div id=x class=a>a</div></div>").Color);
        Assert.AreNotEqual(Green, Styled(".a { .p & { color: green } }", "<div class=a><div class=p><div id=x>a</div></div></div>").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#syntax")]
    public void InvalidNestedRule_DoesntInvalidateTheParent()
    {
        var box = Styled("#x { color: green; ##bad { color: red } background-color: green }");

        Assert.AreEqual(Green, box.Color);
        Assert.AreEqual(Green, box.BackgroundColor);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#syntax")]
    public void RelativeSelector_HasTheSpecificityOfTheNestingSelector()
    {
        Assert.AreEqual(Green, Styled("#p { > .x { color: green } } div > .x.x { color: red }", "<div id=p><div id=x class=x>a</div></div>").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-nesting-1.yml")]
    [Spec("css-nesting-1", "#conditionals")]
    [Spec("css-nesting-1", "#nested-declarations")]
    public void DeclarationsInANestedGroupRule_ApplyToTheParentsElements()
    {
        var box = Styled("#x { color: red; @media all { color: green } }");

        Assert.AreEqual(Green, box.Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-nesting-1.yml")]
    [Spec("css-nesting-1", "#conditionals")]
    public void StyleRuleInANestedGroupRule_IsNested()
    {
        Assert.AreEqual(Green, Styled(".a { @media all { & #x { color: green } } }", "<div class=a><p id=x>a</p></div>").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#mixing")]
    public void NestedRule_ComesAfterItsParent()
    {
        Assert.AreEqual(Green, Styled("#x { color: red; & { color: green } }").Color);
        Assert.AreEqual(Green, Styled("#x { color: green; :where(&) { color: red } }").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-nesting-1.yml")]
    [Spec("css-nesting-1", "#mixing")]
    [Spec("css-nesting-1", "#nested-declarations")]
    public void DeclarationsAfterANestedRule_ComeAfterIt()
    {
        Assert.AreEqual(Green, Styled("#x { color: red; & { color: red } color: green }").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#nested-declarations")]
    public void DeclarationsAfterANestedRule_MatchLikeTheParent()
    {
        var box = Styled("#x, #x::before { content: 'b'; & .none { color: red } color: green }");

        Assert.AreEqual(Green, box.Boxes.First(b => b.IsBeforePseudoElement).Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#nest-selector")]
    [DataRow(".a { &.b { color: green } }", "<div id=x class='a b'>a</div>")]
    [DataRow(".a { div& { color: green } }", "<div id=x class=a>a</div>")]
    [DataRow(".a { && { color: green } }", "<div id=x class=a>a</div>")]
    [DataRow(".a { & > & { color: green } }", "<div class=a><div id=x class=a>a</div></div>")]
    [DataRow(".a { :not(&) > #x { color: green } }", "<div><p id=x>a</p></div>")]
    [DataRow(".a { & :is(.b, &.c) { color: green } }", "<div class=a><p id=x class='a c'>a</p></div>")]
    [DataRow(".a, .b { & #x { color: green } }", "<div class=b><p id=x>a</p></div>")]
    public void NestingSelector_RepresentsTheParentsElements(string css, string body)
    {
        Assert.AreEqual(Green, Styled(css, body).Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-nesting-1.yml")]
    [Spec("css-nesting-1", "#nest-selector")]
    public void NestingSelector_IsInvalidBeforeATypeSelector()
    {
        Assert.AreNotEqual(Red, Styled(".a { &div { color: red } }", "<div id=x class=a>a</div>").Color);
    }

    [TestMethod]
    [Spec("css-nesting-1", "#nest-selector")]
    public void NestingSelector_HasTheLargestSpecificityOfTheParentList()
    {
        Assert.AreEqual(Green, Styled("#a, b { & c { color: green } } .foo c { color: red }", "<b class=foo><c id=x>a</c></b>").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-nesting-1.yml")]
    [Spec("css-nesting-1", "#nest-selector")]
    public void TopLevelNestingSelector_IsTheScopingRoot()
    {
        Assert.AreEqual(Green, Styled("& { color: green }").Color);
    }

    private static CssBox Styled(string css, string body = "<div id=x>a</div>", string id = "x")
    {
        var (root, _) = LayoutHarness.Layout($"<html><head><style>{css}</style></head><body style='margin:0'>{body}</body></html>");
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box);
        return box;
    }
}
