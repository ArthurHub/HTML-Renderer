using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssVariablesTests
{
    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);
    private static readonly RColor Green = RColor.FromArgb(0, 128, 0);

    // ── 2 Custom properties ──────────────────────────────────────────────

    [TestMethod]
    [Spec("css-variables-1", "#defining-variables")]
    public void CustomProperty_IsInheritedAndCascaded()
    {
        var x = Box(
            "<style>:root { --c: red } div { --c: green } #y { --c: red } p { color: var(--c) }</style>" +
            "<div><p id=x>a</p></div><p id=y>b</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css-variables-1", "#defining-variables")]
    public void CustomPropertyNames_AreCaseSensitive()
    {
        var x = Box("<p id=x style='--c: green; --C: red; color: var(--c)'>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css-variables-1", "#defining-variables")]
    public void CustomProperty_CanBeImportant()
    {
        var x = Box("<style>p { --c: green !important; color: var(--c) }</style><p id=x style='--c: red'>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css-variables-1", "#defining-variables")]
    public void CustomProperty_TakesCssWideKeywords()
    {
        var x = Box(
            "<div style='--c: green'><p id=x style='--c: red; --c: inherit; color: var(--c)'>a</p></div>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css-variables-1", "#syntax")]
    public void CustomPropertyValue_KeepsArbitraryTokens()
    {
        var x = Box("<p id=x style='--m: 3px 7px; margin: var(--m)'>a</p>");

        Assert.AreEqual("3px", x.MarginTop);
        Assert.AreEqual("7px", x.MarginLeft);
    }

    [TestMethod]
    [Spec("css-variables-1", "#guaranteed-invalid")]
    public void Initial_ResetsACustomPropertyToTheGuaranteedInvalidValue()
    {
        var x = Box("<div style='--c: red'><p id=x style='--c: initial; color: var(--c, green)'>a</p></div>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [Ignore("Gap, see Spec/css-variables-1.yml")]
    [TestMethod]
    [Spec("css-variables-1", "#guaranteed-invalid")]
    public void EmptyCustomProperty_IsValidAndSubstitutesNothing()
    {
        var x = Box("<p id=x style='--e: ; margin-left: var(--e) 5px'>a</p>");

        Assert.AreEqual("5px", x.MarginLeft);
    }

    [TestMethod]
    [Spec("css-variables-1", "#cycles")]
    public void CustomPropertiesInACycle_AreInvalid()
    {
        var x = Box("<p id=x style='--a: var(--b); --b: var(--a); color: var(--a, green)'>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css-variables-1", "#cycles")]
    public void CustomProperty_IsInheritedWithItsVariablesSubstituted()
    {
        var x = Box(
            "<div style='--foo: 10px'><div style='--bar: calc(var(--foo) + 10px)'>" +
            "<p id=x style='--foo: calc(var(--bar) + 10px); margin-left: var(--foo)'>a</p></div></div>");

        Assert.AreEqual(30, x.ActualMarginLeft, 0.01);
    }

    // ── 3 var() ──────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-variables-1", "#using-variables")]
    [Spec("css-values-5", "#var-notation")]
    public void Var_UsesTheFallback_WhenThePropertyIsMissing()
    {
        var x = Box("<p id=x style='color: var(--missing, green)'>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css-variables-1", "#using-variables")]
    public void VarFallback_CanContainCommasAndVariables()
    {
        var x = Box("<p id=x style='--c: green; font-family: var(--f, Arial, serif); color: var(--missing, var(--c))'>a</p>");
        var direct = Box("<p id=x style='font-family: Arial, serif'>a</p>");

        Assert.AreEqual(direct.FontFamily, x.FontFamily);
        Assert.AreEqual(Green, x.ActualColor);
    }

    [Ignore("Gap, see Spec/css-variables-1.yml")]
    [TestMethod]
    [Spec("css-variables-1", "#using-variables")]
    public void Var_WithAnEmptyFallback_SubstitutesNothing()
    {
        var x = Box("<p id=x style='margin-left: var(--missing,) 5px'>a</p>");

        Assert.AreEqual("5px", x.MarginLeft);
    }

    [TestMethod]
    [Spec("css-variables-1", "#using-variables")]
    public void Var_InACustomProperty_IsSubstitutedAtTheUse()
    {
        var x = Box("<p id=x style='--b: var(--a); --a: green; color: var(--b)'>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [Ignore("Gap, see Spec/css-variables-1.yml")]
    [TestMethod]
    [Spec("css-variables-1", "#using-variables")]
    public void Var_IsSubstitutedAsTokens_NotAsText()
    {
        var x = Box("<style>p { margin-left: 7px } #x { --gap: 20; margin-left: var(--gap)px }</style><p id=x>a</p>");

        Assert.AreEqual(0, x.ActualMarginLeft, 0.01);
    }

    [Ignore("Gap, see Spec/css-variables-1.yml")]
    [TestMethod]
    [Spec("css-variables-1", "#using-variables")]
    public void Var_ResolvingToACssWideKeyword_ActsAsThatKeyword()
    {
        var x = Box("<div style='color: red'><p id=x style='color: var(--missing, initial)'>a</p></div>");

        Assert.AreEqual(RColor.Black, x.ActualColor);
    }

    [TestMethod]
    [Spec("css-variables-1", "#invalid-variables")]
    public void MissingVariableWithoutFallback_MakesThePropertyUnset()
    {
        var x = Box(
            "<style>p { color: red; margin-left: 5px } #x { color: var(--missing); margin-left: var(--missing) }</style>" +
            "<div style='color: green'><p id=x>a</p></div>");

        Assert.AreEqual(Green, x.ActualColor);
        Assert.AreEqual("0", x.MarginLeft);
    }

    [Ignore("Gap, see Spec/css-variables-1.yml")]
    [TestMethod]
    [Spec("css-variables-1", "#invalid-variables")]
    public void InvalidValueAfterSubstitution_MakesThePropertyUnset()
    {
        var x = Box(
            "<style>:root { --not-a-color: 20px } p { color: red } #x { color: var(--not-a-color) }</style>" +
            "<div style='color: green'><p id=x>a</p></div>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css-variables-1", "#variables-in-shorthands")]
    public void Shorthand_WithVar_IsExpandedAfterSubstitution()
    {
        var x = Box("<p id=x style='--w: 3px; --c: green; border: var(--w) solid var(--c)'>a</p>");

        Assert.AreEqual("3px", x.BorderLeftWidth);
        Assert.AreEqual("solid", x.BorderLeftStyle);
        Assert.AreEqual(Green, x.ActualBorderLeftColor);
    }

    [TestMethod]
    [Spec("css-variables-1", "#using-variables")]
    public void DeclarationWithVar_LosesToALaterDeclarationOfTheSameLonghand()
    {
        Assert.AreEqual("2px", Styled(":root{--m:10px} div{margin-inline-start:var(--m)} #x{margin-left:2px}").MarginLeft);
        Assert.AreEqual("normal", Styled(":root{--w:pre} div{white-space:var(--w)} #x{white-space:normal}").WhiteSpace);
        Assert.AreEqual("left", Styled(":root{--a:right} div{text-align:var(--a)} #x{text-align:left}").TextAlign);
    }

    [TestMethod]
    [Spec("css-variables-1", "#variables-in-shorthands")]
    public void ShorthandWithVar_KeepsTheLonghandsNoLaterDeclarationSets()
    {
        var x = Styled(":root{--m:10px} div{margin:var(--m)} #x{margin-left:2px}");

        Assert.AreEqual("2px", x.MarginLeft);
        Assert.AreEqual("10px", x.MarginTop);
        Assert.AreEqual("10px", x.MarginRight);
    }

    private static CssBox Styled(string css)
    {
        var (root, _) = LayoutHarness.Layout($"<html><head><style>{css}</style></head><body><div id=x>a</div></body></html>", 800, 600);
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box;
    }

    private static CssBox Box(string body, string id = "x")
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(body));
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box, $"No box with id '{id}'");
        return box;
    }
}
