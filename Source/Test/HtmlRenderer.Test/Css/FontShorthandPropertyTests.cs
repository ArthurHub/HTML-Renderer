using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class FontShorthandPropertyTests
{
    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    public void CssFontShorthandWithFractionLegal()
    {
        var property = ParseDeclaration("font : 12px/14px sans-serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("12px / 14px sans-serif", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    public void CssFontShorthandPercentLegal()
    {
        var property = ParseDeclaration("font : 80% sans-serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("80% sans-serif", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    public void CssFontShorthandBoldItalicLargeLegal()
    {
        // Serialized in canonical order: style, variant, weight, stretch.
        var property = ParseDeclaration("font : bold italic large serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("italic bold large serif", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    public void CssFontShorthandPredefinedLegal()
    {
        var property = ParseDeclaration("font : status-bar ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("status-bar", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    public void CssFontShorthandSizeAndFontListLegal()
    {
        var property = ParseDeclaration("font : 15px arial,sans-serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("15px arial, sans-serif", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    public void CssFontShorthandStyleWeightSizeLineHeightAndFontListLegal()
    {
        var property = ParseDeclaration("font : italic bold 12px/30px Georgia, serif");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("italic bold 12px / 30px Georgia, serif", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    public void CssFontSizeHeightFamilyLegal()
    {
        var property = ParseDeclaration("font: 12pt/14pt sans-serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("12pt / 14pt sans-serif", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    public void CssFontSizeHeightMultipleFamiliesLegal()
    {
        var property = ParseDeclaration("font: x-large/110% 'New Century Schoolbook', serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("x-large / 110% \"New Century Schoolbook\", serif", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    public void CssFontWeightVariantSizeFamiliesLegal()
    {
        var property = ParseDeclaration("font: bold italic large Palatino, serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("italic bold large Palatino, serif", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    public void CssFontStyleVariantSizeHeightFamilyLegal()
    {
        var property = ParseDeclaration("font: normal small-caps 120%/120% Fantasy ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("normal small-caps 120% / 120% fantasy", concrete.Value);
    }

    [TestMethod]
    public void CssFontStyleVariantSizeFamiliesLegal()
    {
        // font-stretch in the shorthand is CSS Fonts 3.
        var property = ParseDeclaration("font: condensed oblique 12pt \"Helvetica Neue\", serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("oblique condensed 12pt \"Helvetica Neue\", serif", concrete.Value);
    }

    [TestMethod]
    public void CssFontInitialAll()
    {
        var property = ParseDeclaration("font: initial ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("initial", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-shorthand")]
    public void CssFontInheritAll()
    {
        var property = ParseDeclaration("font: inherit ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("inherit", concrete.Value);
    }

    [TestMethod]
    public void CssFontUnsetAll()
    {
        var property = ParseDeclaration("font: unset ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("unset", concrete.Value);
    }
}
