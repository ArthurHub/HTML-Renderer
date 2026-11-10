using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssStyleAttributeTests
{
    private const string Green = "rgb(0, 128, 0)";

    [TestMethod]
    [Spec("css-style-attr", "#syntax")]
    [DataRow(" color : green ")]
    [DataRow("color: green;")]
    [DataRow(" ; ; color: green ;; ")]
    [DataRow("/* a */ color /* b */ : /* c */ green /* d */")]
    [DataRow("color: red; color: green")]
    [DataRow("color: red; colour: blue; color: green")]
    [DataRow("color: red; color green; color: green")]
    public void StyleAttribute_IsADeclarationList(string style)
    {
        Assert.AreEqual(Green, Styled("", style).Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-style-attr.yml")]
    [Spec("css-style-attr", "#syntax")]
    public void CloseBrace_DoesntEndTheStyleAttribute()
    {
        Assert.AreEqual(Green, Styled("", "color: red; } ; color: green").Color);
    }

    [TestMethod]
    [Spec("css-style-attr", "#syntax")]
    public void StyleRuleSyntax_IsInvalid()
    {
        var box = Styled("", "#x { color: red } background-color: green");

        Assert.AreNotEqual("rgb(255, 0, 0)", box.Color);
    }

    [TestMethod]
    [Spec("css-style-attr", "#interpret")]
    public void StyleAttribute_BeatsAnyNormalDeclaration()
    {
        Assert.AreEqual(Green, Styled("#x#x#x.a.b { color: red }", "color: green").Color);
    }

    [TestMethod]
    [Spec("css-style-attr", "#interpret")]
    public void ImportantAuthorDeclaration_BeatsANormalStyleAttribute()
    {
        Assert.AreEqual(Green, Styled("#x { color: green !important }", "color: red").Color);
    }

    [TestMethod]
    [Spec("css-style-attr", "#interpret")]
    public void ImportantStyleAttribute_BeatsAnImportantAuthorDeclaration()
    {
        Assert.AreEqual(Green, Styled("#x#x#x { color: red !important }", "color: green !important").Color);
    }

    [TestMethod]
    [Spec("css-style-attr", "#interpret")]
    public void StyleAttribute_IsInterpretedLikeAStyleRule()
    {
        var box = Styled("", "--c: green; color: var(--c); border: 1px solid; margin: 0 auto; width: 100px", "<div id=p style='font-size: 20px'><div id=x>a</div></div>");

        Assert.AreEqual(Green, box.Color);
        Assert.AreEqual("solid", box.BorderTopStyle);
        Assert.AreEqual("100px", box.Width);
    }

    [TestMethod]
    [Spec("css-style-attr", "#interpret")]
    public void RelativeUrl_IsResolvedLikeInAStyleElement()
    {
        var image = Styled("", "background-image: url(a.png)").BackgroundImage;

        StringAssert.Contains(image, "a.png");
        Assert.AreEqual(Styled("#x { background-image: url(a.png) }", "").BackgroundImage, image);
    }

    private static CssBox Styled(string css, string style, string body = "<div id=x class='a b'>a</div>")
    {
        body = body.Replace("<div id=x", $"<div style=\"{style}\" id=x");
        var (root, _) = LayoutHarness.Layout($"<html><head><style>{css}</style></head><body style='margin:0'>{body}</body></html>");
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box;
    }
}
