using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class FontFamilyPropertyTests
{
    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    public void CssFontFamilyMultipleWithIdentifiersLegal()
    {
        var property = ParseDeclaration("font-family: Gill Sans Extrabold, sans-serif ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("Gill Sans Extrabold, sans-serif", concrete.Value);
    }

    [TestMethod]
    public void CssFontFamilyInitialLegal()
    {
        var property = ParseDeclaration("font-family: initial ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("initial", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    public void CssFontFamilyMultipleDiverseLegal()
    {
        var property = ParseDeclaration("font-family: Courier, \"Lucida Console\", monospace ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("Courier, \"Lucida Console\", monospace", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    public void CssFontFamilyMultipleStringLegal()
    {
        var property = ParseDeclaration("font-family: \"Goudy Bookletter 1911\", sans-serif ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("\"Goudy Bookletter 1911\", sans-serif", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    public void CssFontFamilyMultipleNumberIllegal()
    {
        // "1911" is a number, an unquoted family name may only contain identifiers.
        var property = ParseDeclaration("font-family: Goudy Bookletter 1911, sans-serif  ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    public void CssFontFamilyMultipleFractionIllegal()
    {
        var property = ParseDeclaration("font-family: Red/Black, sans-serif  ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    public void CssFontFamilyMultipleStringMixedWithIdentifierIllegal()
    {
        var property = ParseDeclaration("font-family: \"Lucida\" Grande, sans-serif ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    public void CssFontFamilyMultipleExclamationMarkIllegal()
    {
        var property = ParseDeclaration("font-family: Ahem!, sans-serif ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    public void CssFontFamilyMultipleAtIllegal()
    {
        var property = ParseDeclaration("font-family: test@foo, sans-serif ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    public void CssFontFamilyHashIllegal()
    {
        var property = ParseDeclaration("font-family: #POUND ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    public void CssFontFamilyDashIllegal()
    {
        var property = ParseDeclaration("font-family: Hawaii 5-0 ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }
}
