using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Parsing of the font longhands. <c>font-variant</c> only knows the CSS 2.1 keywords normal and small-caps.
/// </summary>
[TestClass]
public sealed class FontKeywordPropertyTests
{
    // ── font-variant (base keywords only) ────────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css21", "fonts.html#small-caps")]
    [Spec("css-fonts-4", "#font-variant-prop")]
    public void CssFontVariantNormalUppercaseLegal()
    {
        var property = ParseDeclaration("font-variant : NORMAL");
        Assert.AreEqual("font-variant", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontVariantProperty>(property);
        var concrete = (FontVariantProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("normal", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#small-caps")]
    [Spec("css-fonts-4", "#font-variant-prop")]
    public void CssFontVariantSmallCapsLegal()
    {
        var property = ParseDeclaration("font-variant : small-caps ");
        Assert.AreEqual("font-variant", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontVariantProperty>(property);
        var concrete = (FontVariantProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("small-caps", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#small-caps")]
    [Spec("css-fonts-4", "#font-variant-prop")]
    public void CssFontVariantSmallCapsIllegal()
    {
        var property = ParseDeclaration("font-variant : smallCaps ");
        Assert.AreEqual("font-variant", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontVariantProperty>(property);
        var concrete = (FontVariantProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    // ── font-style ────────────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css21", "fonts.html#font-styling")]
    [Spec("css-fonts-4", "#font-style-prop")]
    public void CssFontStyleItalicLegal()
    {
        var property = ParseDeclaration("font-style : italic");
        Assert.AreEqual("font-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontStyleProperty>(property);
        var concrete = (FontStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("italic", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-styling")]
    [Spec("css-fonts-4", "#font-style-prop")]
    public void CssFontStyleObliqueLegal()
    {
        var property = ParseDeclaration("font-style : oblique ");
        Assert.AreEqual("font-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontStyleProperty>(property);
        var concrete = (FontStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("oblique", concrete.Value);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#font-style-prop")]
    public void CssFontStyleObliqueWithAngleLegal()
    {
        var property = ParseDeclaration("font-style : oblique 10deg");
        Assert.AreEqual("font-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontStyleProperty>(property);
        var concrete = (FontStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("oblique 10deg", concrete.Value);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#font-style-prop")]
    public void CssFontStyleObliqueWithNegativeAngleLegal()
    {
        var property = ParseDeclaration("font-style : oblique -10deg");
        Assert.AreEqual("font-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontStyleProperty>(property);
        var concrete = (FontStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("oblique -10deg", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-styling")]
    [Spec("css-fonts-4", "#font-style-prop")]
    public void CssFontStyleNormalImportantLegal()
    {
        var property = ParseDeclaration("font-style : normal !important");
        Assert.AreEqual("font-style", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<FontStyleProperty>(property);
        var concrete = (FontStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("normal", concrete.Value);
    }

    // ── font-size ─────────────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void CssFontSizeAbsoluteImportantXxSmallLegal()
    {
        var property = ParseDeclaration("font-size : xx-small !important");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("xx-small", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void CssFontSizeAbsoluteMediumUppercaseLegal()
    {
        var property = ParseDeclaration("font-size : medium");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("medium", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void CssFontSizeAbsoluteLargeImportantLegal()
    {
        var property = ParseDeclaration("font-size : large !important");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("large", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void CssFontSizeRelativeLargerLegal()
    {
        var property = ParseDeclaration("font-size : larger ");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("larger", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void CssFontSizeRelativeLargestIllegal()
    {
        var property = ParseDeclaration("font-size : largest ");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void CssFontSizePercentLegal()
    {
        var property = ParseDeclaration("font-size : 120% ");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("120%", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void CssFontSizeZeroLegal()
    {
        var property = ParseDeclaration("font-size : 0 ");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void CssFontSizeLengthLegal()
    {
        var property = ParseDeclaration("font-size : 3.5em ");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("3.5em", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-size-props")]
    [Spec("css-fonts-4", "#font-size-prop")]
    public void CssFontSizeNumberIllegal()
    {
        var property = ParseDeclaration("font-size : 120.3 ");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    // ── font-weight ───────────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css21", "fonts.html#font-boldness")]
    [Spec("css-fonts-4", "#font-weight-prop")]
    public void CssFontWeightPercentllegal()
    {
        var property = ParseDeclaration("font-weight : 100% ");
        Assert.AreEqual("font-weight", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontWeightProperty>(property);
        var concrete = (FontWeightProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-boldness")]
    [Spec("css-fonts-4", "#font-weight-prop")]
    public void CssFontWeightBolderLegalImportant()
    {
        var property = ParseDeclaration("font-weight : bolder !important");
        Assert.AreEqual("font-weight", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<FontWeightProperty>(property);
        var concrete = (FontWeightProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("bolder", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-boldness")]
    [Spec("css-fonts-4", "#font-weight-prop")]
    public void CssFontWeightBoldLegal()
    {
        var property = ParseDeclaration("font-weight : bold");
        Assert.AreEqual("font-weight", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontWeightProperty>(property);
        var concrete = (FontWeightProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("bold", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-boldness")]
    [Spec("css-fonts-4", "#font-weight-prop")]
    public void CssFontWeight400Legal()
    {
        var property = ParseDeclaration("font-weight : 400 ");
        Assert.AreEqual("font-weight", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontWeightProperty>(property);
        var concrete = (FontWeightProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("400", concrete.Value);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [Spec("css-fonts-4", "#font-weight-prop")]
    public void CssFontWeight550Legal()
    {
        var property = ParseDeclaration("font-weight : 550 ");
        Assert.AreEqual("font-weight", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontWeightProperty>(property);
        var concrete = (FontWeightProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("550", concrete.Value);
    }

    [TestMethod]
    public void CssFontWeight550IsCurrentlyIllegal()
    {
        var property = ParseDeclaration("font-weight : 550 ");
        Assert.AreEqual("font-weight", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontWeightProperty>(property);
        var concrete = (FontWeightProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-boldness")]
    [Spec("css-fonts-4", "#font-weight-prop")]
    public void CssFontWeightOutOfRangeIllegal()
    {
        var property = ParseDeclaration("font-weight : 1001 ");
        Assert.AreEqual("font-weight", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontWeightProperty>(property);
        var concrete = (FontWeightProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-boldness")]
    [Spec("css-fonts-4", "#font-weight-prop")]
    public void CssFontWeightZeroIllegal()
    {
        var property = ParseDeclaration("font-weight : 0 ");
        Assert.AreEqual("font-weight", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontWeightProperty>(property);
        var concrete = (FontWeightProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    // ── font-stretch ──────────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-fonts-4", "#font-stretch-prop")]
    public void CssFontStretchNormalUppercaseImportantLegal()
    {
        var property = ParseDeclaration("font-stretch : NORMAL !important");
        Assert.AreEqual("font-stretch", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<FontStretchProperty>(property);
        var concrete = (FontStretchProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("normal", concrete.Value);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#font-stretch-prop")]
    public void CssFontStretchExtraCondensedLegal()
    {
        var property = ParseDeclaration("font-stretch : extra-condensed ");
        Assert.AreEqual("font-stretch", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontStretchProperty>(property);
        var concrete = (FontStretchProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("extra-condensed", concrete.Value);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#font-stretch-prop")]
    public void CssFontStretchSemiExpandedSpaceBetweenIllegal()
    {
        var property = ParseDeclaration("font-stretch : semi expanded ");
        Assert.AreEqual("font-stretch", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontStretchProperty>(property);
        var concrete = (FontStretchProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    // ── letter-spacing ────────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-text-3", "#letter-spacing-property")]
    public void CssLetterSpacingLengthPxLegal()
    {
        var property = ParseDeclaration("letter-spacing: 3px ");
        Assert.AreEqual("letter-spacing", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<LetterSpacingProperty>(property);
        var concrete = (LetterSpacingProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("3px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#letter-spacing-property")]
    public void CssLetterSpacingLengthFloatPxLegal()
    {
        var property = ParseDeclaration("letter-spacing: .3px ");
        Assert.AreEqual("letter-spacing", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<LetterSpacingProperty>(property);
        var concrete = (LetterSpacingProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0.3px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#letter-spacing-property")]
    public void CssLetterSpacingLengthFloatEmLegal()
    {
        var property = ParseDeclaration("letter-spacing: 0.3em ");
        Assert.AreEqual("letter-spacing", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<LetterSpacingProperty>(property);
        var concrete = (LetterSpacingProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0.3em", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#letter-spacing-property")]
    public void CssLetterSpacingNormalLegal()
    {
        var property = ParseDeclaration("letter-spacing: normal ");
        Assert.AreEqual("letter-spacing", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<LetterSpacingProperty>(property);
        var concrete = (LetterSpacingProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("normal", concrete.Value);
    }

    // ── font-size-adjust ──────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-fonts-4", "#font-size-adjust-prop")]
    public void CssFontSizeAdjustNoneLegal()
    {
        var property = ParseDeclaration("font-size-adjust : NONE");
        Assert.AreEqual("font-size-adjust", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeAdjustProperty>(property);
        var concrete = (FontSizeAdjustProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#font-size-adjust-prop")]
    public void CssFontSizeAdjustNumberLegal()
    {
        var property = ParseDeclaration("font-size-adjust : 0.5");
        Assert.AreEqual("font-size-adjust", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeAdjustProperty>(property);
        var concrete = (FontSizeAdjustProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0.5", concrete.Value);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#font-size-adjust-prop")]
    public void CssFontSizeAdjustLengthIllegal()
    {
        var property = ParseDeclaration("font-size-adjust : 1.1em ");
        Assert.AreEqual("font-size-adjust", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeAdjustProperty>(property);
        var concrete = (FontSizeAdjustProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }
}
