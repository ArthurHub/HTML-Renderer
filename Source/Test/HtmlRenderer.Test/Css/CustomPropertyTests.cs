using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Parsing of custom properties and var(), and how custom properties inherit. The rest of the
/// cascade-level behavior is in CssVariablesTests.
/// </summary>
[TestClass]
public sealed class CustomPropertyTests
{
    [TestMethod]
    [Spec("css-variables-1", "#defining-variables")]
    public void CustomProperty_SimpleValue_ParsesAndRoundTrips()
    {
        var property = CssConstructionFunctions.ParseDeclaration("--main-color: red");
        Assert.AreEqual("--main-color", property.Name);
        Assert.IsInstanceOfType<CustomProperty>(property);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("red", property.Value);
    }

    [TestMethod]
    [Spec("css-variables-1", "#syntax")]
    public void CustomProperty_ArbitraryTokenSoup_NeverFailsToParse()
    {
        var property = CssConstructionFunctions.ParseDeclaration("--x: 1px solid   red , foo(bar)");
        Assert.AreEqual("--x", property.Name);
        Assert.IsInstanceOfType<CustomProperty>(property);
        Assert.IsTrue(property.HasValue);
    }

    [TestMethod]
    [DataRow("--x: #hero", "#hero")] // an id-shaped hash-token value (CSS Syntax accepts it)
    [DataRow("--x: #f00", "#f00")] // a hex color hash
    [Spec("css-variables-1", "#syntax")]
    public void CustomProperty_HashValue_ParsesAndRoundTrips(string snippet, string expected)
    {
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("--x", property.Name);
        Assert.IsInstanceOfType<CustomProperty>(property);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual(expected, property.Value);
    }

    [TestMethod]
    [Spec("css-variables-1", "#syntax")]
    public void CustomProperty_WithVarReference_RoundTripsLiterally()
    {
        var property = CssConstructionFunctions.ParseDeclaration("--b: var(--a, blue)");
        Assert.AreEqual("--b", property.Name);
        Assert.IsTrue(property.HasValue);
        StringAssert.Contains(property.Value, "var(--a, blue)");
    }

    [TestMethod]
    [Spec("css-variables-1", "#defining-variables")]
    public void CustomProperty_NameIsCaseSensitive()
    {
        var lower = CssConstructionFunctions.ParseDeclaration("--foo: 1px");
        var upper = CssConstructionFunctions.ParseDeclaration("--Foo: 2px");
        Assert.AreEqual("--foo", lower.Name);
        Assert.AreEqual("--Foo", upper.Name);
    }

    [TestMethod]
    [Spec("css-variables-1", "#defining-variables")]
    public void CustomProperty_MultiHyphenatedName_ParsesAsSingleIdentifier()
    {
        var property = CssConstructionFunctions.ParseDeclaration("--foo-bar-baz: 1px");
        Assert.AreEqual("--foo-bar-baz", property.Name);
    }

    [TestMethod]
    [Spec("css-variables-1", "#using-variables")]
    public void ColorProperty_WithVarFunction_DoesNotFailToParse()
    {
        var property = CssConstructionFunctions.ParseDeclaration("color: var(--main-color, red)");
        Assert.AreEqual("color", property.Name);
        Assert.IsTrue(property.HasValue);
        StringAssert.Contains(property.Value, "var(--main-color, red)");
    }

    [TestMethod]
    [Spec("css-variables-1", "#using-variables")]
    public void MarginProperty_WithMultipleVarFunctions_DoesNotFailToParse()
    {
        var property = CssConstructionFunctions.ParseDeclaration("margin: var(--a) var(--b)");
        Assert.AreEqual("margin", property.Name);
        Assert.IsTrue(property.HasValue);
    }

    [TestMethod]
    [Spec("css-variables-1", "#variables-in-shorthands")]
    public void MarginShorthandInStyleSheet_WithVar_KeepsShorthandWholeUntilCascadeTime()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("* { margin: var(--a) var(--b); }");
        var rule = sheet.StyleRules.Single();
        var names = rule.Style.Select(p => p.Name).ToArray();
        CollectionAssert.AreEqual(new[] { "margin" }, names);
    }

    [TestMethod]
    [Spec("css-variables-1", "#using-variables")]
    public void Property_WithNestedVarInsideOtherFunction_DoesNotFailToParse()
    {
        var property = CssConstructionFunctions.ParseDeclaration("background-image: linear-gradient(var(--c1), var(--c2))");
        Assert.AreEqual("background-image", property.Name);
        Assert.IsTrue(property.HasValue);
    }

    // ── Tokenizing -- as an identifier start ─────────────────────────────

    [TestMethod]
    public void HtmlCommentClose_StillTokenizesAsCdc_NotAffectedByDashFix()
    {
        var stylesheet = CssConstructionFunctions.ParseStyleSheet("<!-- .a { color: red; } -->");
        Assert.IsTrue(stylesheet.StyleRules.Any());
    }

    [TestMethod]
    public void VendorPrefixedProperty_StillParsesAsUnknownSingleHyphenIdent()
    {
        var property = CssConstructionFunctions.ParseDeclaration("-webkit-transform: none", includeUnknownDeclarations: true);
        Assert.AreEqual("-webkit-transform", property.Name);
    }

    [TestMethod]
    public void NegativeLength_StillParsesCorrectly()
    {
        var property = CssConstructionFunctions.ParseDeclaration("margin-top: -5px");
        Assert.AreEqual("margin-top", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("-5px", property.Value);
    }

    // css-variables-1 §2: the computed value of a custom property has its var() substituted, and that
    // is what inherits.
    [TestMethod]
    public void CustomProperty_IsInheritedWithVarSubstitutedOnTheParent()
    {
        var p = FindBoxByTag("<div style='--a: #ff0000; --b: var(--a)'><p style='--a: #0000ff; color: var(--b)'>a</p></div>", "p");
        Assert.AreEqual("rgb(255, 0, 0)", p.Color);
    }

    [TestMethod]
    public void CustomProperty_IsInheritedWithVarSubstitutedOnEachAncestor()
    {
        var p = FindBoxByTag(
            "<div style='--foo: 10px'><div style='--bar: calc(var(--foo) + 10px)'>" +
            "<p style='--foo: calc(var(--bar) + 10px); margin-left: var(--foo)'>a</p></div></div>", "p");
        Assert.AreEqual(30, p.ActualMarginLeft, 0.01);
    }

    [TestMethod]
    public void CyclicCustomProperty_IsInheritedAsGuaranteedInvalid()
    {
        var p = FindBoxByTag("<div style='--a: var(--b); --b: var(--a)'><p style='--b: #ff0000; color: var(--a, #008000)'>a</p></div>", "p");
        Assert.AreEqual("rgb(0, 128, 0)", p.Color);
    }

    [TestMethod]
    public void InheritedProperty_UsingVar_InheritsTheParentsResolvedValue()
    {
        var p = FindBoxByTag("<div style='--a: #ff0000; color: var(--a)'><p style='--a: #0000ff'>a</p></div>", "p");
        Assert.AreEqual("rgb(255, 0, 0)", p.Color);
    }

    private static CssBox FindBoxByTag(string body, string tag)
    {
        var (root, _) = LayoutHarness.Layout("<!DOCTYPE html><html><body>" + body + "</body></html>");
        var box = LayoutHarness.Descendants(root).FirstOrDefault(b => b.HtmlTag != null && b.HtmlTag.Name == tag);
        Assert.IsNotNull(box);
        return box!;
    }
}
