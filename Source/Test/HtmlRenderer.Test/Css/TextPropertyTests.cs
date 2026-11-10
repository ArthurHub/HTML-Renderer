using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>Parsing of the CSS Text and CSS Text Decoration properties.</summary>
[TestClass]
public sealed class TextPropertyTests
{
    [TestMethod]
    [Spec("css-text-3", "#word-spacing-property")]
    [Spec("css-text-4", "#word-spacing-property")]
    public void WordSpacingZeroLengthLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("word-spacing: 0");
        Assert.AreEqual("word-spacing", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<WordSpacingProperty>(property);
        var concrete = (WordSpacingProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#word-spacing-property")]
    [Spec("css-text-4", "#word-spacing-property")]
    public void WordSpacingLengthFloatRemLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("word-spacing: .3rem ");
        Assert.AreEqual("word-spacing", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<WordSpacingProperty>(property);
        var concrete = (WordSpacingProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0.3rem", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#word-spacing-property")]
    [Spec("css-text-4", "#word-spacing-property")]
    public void WordSpacingLengthFloatEmLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("word-spacing: 0.3em ");
        Assert.AreEqual("word-spacing", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<WordSpacingProperty>(property);
        var concrete = (WordSpacingProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0.3em", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#word-spacing-property")]
    [Spec("css-text-4", "#word-spacing-property")]
    public void WordSpacingNormalLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("word-spacing: normal ");
        Assert.AreEqual("word-spacing", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<WordSpacingProperty>(property);
        var concrete = (WordSpacingProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("normal", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#text-shadow-property")]
    public void TextShadowLegalInsetAtLast()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-shadow: 0 0 2px black inset");
        Assert.AreEqual("text-shadow", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextShadowProperty>(property);
        var concrete = (TextShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("inset 0 0 2px rgb(0, 0, 0)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#text-shadow-property")]
    public void TextShadowLegalColorInFront()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-shadow: rgba(255,255,255,0.5) 0px 3px 3px");
        Assert.AreEqual("text-shadow", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextShadowProperty>(property);
        var concrete = (TextShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("0 3px 3px rgba(255, 255, 255, 0.5)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#text-shadow-property")]
    public void TextShadowLegalMultipleMultilines()
    {
        const string snippet = @"text-shadow: 0px 3px 0px #b2a98f,
             0px 14px 10px rgba(0,0,0,0.15),
             0px 24px 2px rgba(0,0,0,0.1),
             0px 34px 30px rgba(0,0,0,0.1)";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("text-shadow", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextShadowProperty>(property);
        var concrete = (TextShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual(
            "0 3px 0 rgb(178, 169, 143), 0 14px 10px rgba(0, 0, 0, 0.15), 0 24px 2px rgba(0, 0, 0, 0.1), 0 34px 30px rgba(0, 0, 0, 0.1)",
            concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#text-shadow-property")]
    public void TextShadowLegalMultipleInline()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-shadow: 4px 3px 0px #fff, 9px 8px 0px rgba(0,0,0,0.15)");
        Assert.AreEqual("text-shadow", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextShadowProperty>(property);
        var concrete = (TextShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("4px 3px 0 rgb(255, 255, 255), 9px 8px 0 rgba(0, 0, 0, 0.15)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#text-shadow-property")]
    public void TextShadowLegalColorRgbaLast()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-shadow: 2px 4px 3px rgba(0,0,0,0.3)");
        Assert.AreEqual("text-shadow", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextShadowProperty>(property);
        var concrete = (TextShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("2px 4px 3px rgba(0, 0, 0, 0.3)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-align-property")]
    [Spec("css-text-4", "#text-align-property")]
    public void TextAlignLegalJustify()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-align:justify");
        Assert.AreEqual("text-align", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextAlignProperty>(property);
        var concrete = (TextAlignProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("justify", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void TextIndentLegalLength()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-indent:3em");
        Assert.AreEqual("text-indent", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextIndentProperty>(property);
        var concrete = (TextIndentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("3em", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void TextIndentLegalZero()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-indent:0");
        Assert.AreEqual("text-indent", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextIndentProperty>(property);
        var concrete = (TextIndentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("0", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void TextIndentLegalPercent()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-indent:10%");
        Assert.AreEqual("text-indent", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextIndentProperty>(property);
        var concrete = (TextIndentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("10%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void TextIndentIllegalNone()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-indent:none");
        Assert.AreEqual("text-indent", property.Name);
        Assert.IsFalse(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextIndentProperty>(property);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-text-3.yml")]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void TextIndentLegalHanging()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-indent:40pt hanging");
        Assert.AreEqual("text-indent", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextIndentProperty>(property);
        var concrete = (TextIndentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("40pt hanging", concrete.Value);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-text-3.yml")]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void TextIndentLegalEachLine()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-indent:40pt each-line");
        Assert.AreEqual("text-indent", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextIndentProperty>(property);
        var concrete = (TextIndentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("40pt each-line", concrete.Value);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-text-3.yml")]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void TextIndentLegalHangingEachLine()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-indent:40pt hanging each-line");
        Assert.AreEqual("text-indent", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextIndentProperty>(property);
        var concrete = (TextIndentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("40pt hanging each-line", concrete.Value);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-text-3.yml")]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void TextIndentLegalReorderedKeywordsSerializeInCanonicalOrder()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-indent:each-line hanging 40pt");
        Assert.AreEqual("text-indent", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextIndentProperty>(property);
        var concrete = (TextIndentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("each-line hanging 40pt", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void TextIndentIllegalHangingAloneMissingLength()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-indent:hanging");
        Assert.AreEqual("text-indent", property.Name);
        Assert.IsFalse(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextIndentProperty>(property);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-indent-property")]
    [Spec("css-text-4", "#text-indent-property")]
    public void TextIndentIllegalDuplicateKeyword()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-indent:40pt hanging hanging");
        Assert.AreEqual("text-indent", property.Name);
        Assert.IsFalse(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextIndentProperty>(property);
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#text-decoration-property")]
    public void TextDecorationIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-decoration: line-pass");
        Assert.AreEqual("text-decoration", property.Name);
        Assert.IsFalse(property.HasValue);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextDecorationProperty>(property);
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#text-decoration-property")]
    public void TextDecorationLegalLineThrough()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-decoration: line-Through");
        Assert.AreEqual("text-decoration", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextDecorationProperty>(property);
        var concrete = (TextDecorationProperty)property;
        Assert.AreEqual("line-through", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#text-decoration-property")]
    public void TextDecorationLegalUnderlineOverline()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-decoration:  underline  overline");
        Assert.AreEqual("text-decoration", property.Name);
        Assert.IsInstanceOfType<TextDecorationProperty>(property);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        var concrete = (TextDecorationProperty)property;
        Assert.AreEqual("underline overline", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#text-decoration-color-property")]
    public void TextDecorationColorLegalHex()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-decoration-color: #F00");
        Assert.AreEqual("text-decoration-color", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextDecorationColorProperty>(property);
        var concrete = (TextDecorationColorProperty)property;
        Assert.AreEqual("rgb(255, 0, 0)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#text-decoration-color-property")]
    public void TextDecorationColorLegalRed()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-decoration-color: red");
        Assert.AreEqual("text-decoration-color", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextDecorationColorProperty>(property);
        var concrete = (TextDecorationColorProperty)property;
        Assert.AreEqual("rgb(255, 0, 0)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#text-decoration-line-property")]
    public void TextDecorationLineIllegalInteger()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-decoration-line: 5");
        Assert.AreEqual("text-decoration-line", property.Name);
        Assert.IsFalse(property.HasValue);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextDecorationLineProperty>(property);
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#text-decoration-line-property")]
    public void TextDecorationLineLegalNone()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-decoration-line: none");
        Assert.AreEqual("text-decoration-line", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextDecorationLineProperty>(property);
        var concrete = (TextDecorationLineProperty)property;
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#text-decoration-line-property")]
    public void TextDecorationLineLegalOverlineUnderlineLineThrough()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-decoration-line: overline    underline line-through  ");
        Assert.AreEqual("text-decoration-line", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextDecorationLineProperty>(property);
        var concrete = (TextDecorationLineProperty)property;
        Assert.AreEqual("overline underline line-through", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#text-decoration-style-property")]
    public void TextDecorationStyleLegalWavyUppercase()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-decoration-style: WAVY ");
        Assert.AreEqual("text-decoration-style", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextDecorationStyleProperty>(property);
        var concrete = (TextDecorationStyleProperty)property;
        Assert.AreEqual("wavy", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#text-decoration-style-property")]
    public void TextDecorationStyleIllegalMultiple()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-decoration-style: wavy dotted");
        Assert.AreEqual("text-decoration-style", property.Name);
        Assert.IsFalse(property.HasValue);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextDecorationStyleProperty>(property);
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#text-decoration-property")]
    public void TextDecorationExpansionAndRecombination()
    {
        const string snippet = ".centered {text-decoration:underline;}";
        const string expected = ".centered { text-decoration: underline }";
        var result = CssConstructionFunctions.ParseRule(snippet);
        Assert.AreEqual(expected, result.Text);
    }

    [TestMethod]
    [Spec("css-text-3", "#word-break-property")]
    [Spec("css-text-4", "#word-break-property")]
    public void WordBreakNormalLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("word-break : normal");
        Assert.AreEqual("word-break", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<WordBreakProperty>(property);
        var concrete = (WordBreakProperty)property;
        Assert.AreEqual("normal", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#word-break-property")]
    [Spec("css-text-4", "#word-break-property")]
    public void WordBreakBreakAllLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("word-break : break-all");
        Assert.AreEqual("word-break", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<WordBreakProperty>(property);
        var concrete = (WordBreakProperty)property;
        Assert.AreEqual("break-all", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#word-break-property")]
    [Spec("css-text-4", "#word-break-property")]
    public void WordBreakKeepAllLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("word-break : keep-all");
        Assert.AreEqual("word-break", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<WordBreakProperty>(property);
        var concrete = (WordBreakProperty)property;
        Assert.AreEqual("keep-all", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#word-break-property")]
    [Spec("css-text-4", "#word-break-property")]
    public void WordBreakNoneIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("word-break : none");
        Assert.AreEqual("word-break", property.Name);
        Assert.IsFalse(property.HasValue);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<WordBreakProperty>(property);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-align-last-property")]
    [Spec("css-text-4", "#text-align-last-property")]
    public void TextAlignLastAutoLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-align-last: auto");
        Assert.AreEqual("text-align-last", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextAlignLastProperty>(property);
        var concrete = (TextAlignLastProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("auto", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-align-last-property")]
    [Spec("css-text-4", "#text-align-last-property")]
    public void TextAlignLastStartLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-align-last: start");
        Assert.AreEqual("text-align-last", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextAlignLastProperty>(property);
        var concrete = (TextAlignLastProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("start", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-align-last-property")]
    [Spec("css-text-4", "#text-align-last-property")]
    public void TextAlignLastEndLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-align-last: end");
        Assert.AreEqual("text-align-last", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextAlignLastProperty>(property);
        var concrete = (TextAlignLastProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("end", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-align-last-property")]
    [Spec("css-text-4", "#text-align-last-property")]
    public void TextAlignLastRightLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-align-last: right");
        Assert.AreEqual("text-align-last", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextAlignLastProperty>(property);
        var concrete = (TextAlignLastProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("right", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-align-last-property")]
    [Spec("css-text-4", "#text-align-last-property")]
    public void TextAlignLastLeftLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-align-last: left");
        Assert.AreEqual("text-align-last", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextAlignLastProperty>(property);
        var concrete = (TextAlignLastProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("left", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-align-last-property")]
    [Spec("css-text-4", "#text-align-last-property")]
    public void TextAlignLastCenterLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-align-last: center");
        Assert.AreEqual("text-align-last", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextAlignLastProperty>(property);
        var concrete = (TextAlignLastProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("center", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-align-last-property")]
    [Spec("css-text-4", "#text-align-last-property")]
    public void TextAlignLastJustifyLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-align-last: justify");
        Assert.AreEqual("text-align-last", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextAlignLastProperty>(property);
        var concrete = (TextAlignLastProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("justify", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-align-last-property")]
    [Spec("css-text-4", "#text-align-last-property")]
    public void TextAlignLastNoneIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-align-last: none");
        Assert.AreEqual("text-align-last", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextAlignLastProperty>(property);
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    public void TextAnchorStartLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-anchor: start");
        Assert.AreEqual("text-anchor", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextAnchorProperty>(property);
        var concrete = (TextAnchorProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("start", concrete.Value);
    }

    [TestMethod]
    public void TextAnchorMiddleLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-anchor: middle");
        Assert.AreEqual("text-anchor", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextAnchorProperty>(property);
        var concrete = (TextAnchorProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("middle", concrete.Value);
    }

    [TestMethod]
    public void TextAnchorEndLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-anchor: end");
        Assert.AreEqual("text-anchor", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextAnchorProperty>(property);
        var concrete = (TextAnchorProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("end", concrete.Value);
    }

    [TestMethod]
    public void TextAnchorNoneIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-anchor: none");
        Assert.AreEqual("text-anchor", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextAnchorProperty>(property);
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-justify-property")]
    [Spec("css-text-4", "#text-justify-property")]
    public void TextJustifyAutoLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-justify: auto");
        Assert.AreEqual("text-justify", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextJustifyProperty>(property);
        var concrete = (TextJustifyProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("auto", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-justify-property")]
    [Spec("css-text-4", "#text-justify-property")]
    public void TextJustifyDistributeLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-justify: distribute");
        Assert.AreEqual("text-justify", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextJustifyProperty>(property);
        var concrete = (TextJustifyProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("distribute", concrete.Value);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-text-3.yml")]
    [Spec("css-text-3", "#text-justify-property")]
    [Spec("css-text-4", "#text-justify-property")]
    public void TextJustifyDistributeAllLinesIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-justify: distribute-all-lines");
        Assert.AreEqual("text-justify", property.Name);
        Assert.IsInstanceOfType<TextJustifyProperty>(property);
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-text-3.yml")]
    [Spec("css-text-3", "#text-justify-property")]
    [Spec("css-text-4", "#text-justify-property")]
    public void TextJustifyDistributeCenterLastIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-justify: distribute-center-last");
        Assert.AreEqual("text-justify", property.Name);
        Assert.IsInstanceOfType<TextJustifyProperty>(property);
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    public void TextJustifyInterClusterLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-justify: inter-cluster");
        Assert.AreEqual("text-justify", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextJustifyProperty>(property);
        var concrete = (TextJustifyProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("inter-cluster", concrete.Value);
    }

    [TestMethod]
    public void TextJustifyInterIdeographLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-justify: inter-ideograph");
        Assert.AreEqual("text-justify", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextJustifyProperty>(property);
        var concrete = (TextJustifyProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("inter-ideograph", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-justify-property")]
    [Spec("css-text-4", "#text-justify-property")]
    public void TextJustifyInterWordLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-justify: inter-word");
        Assert.AreEqual("text-justify", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextJustifyProperty>(property);
        var concrete = (TextJustifyProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("inter-word", concrete.Value);
    }

    [TestMethod]
    public void TextJustifyKashidaLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-justify: kashida");
        Assert.AreEqual("text-justify", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextJustifyProperty>(property);
        var concrete = (TextJustifyProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("kashida", concrete.Value);
    }

    [TestMethod]
    public void TextJustifyNewspaperLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-justify: newspaper");
        Assert.AreEqual("text-justify", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextJustifyProperty>(property);
        var concrete = (TextJustifyProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("newspaper", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#text-justify-property")]
    public void TextJustifyNoneIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("text-justify: none");
        Assert.AreEqual("text-justify", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TextJustifyProperty>(property);
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [Spec("css-text-3", "#overflow-wrap-property")]
    [Spec("css-text-4", "#overflow-wrap-property")]
    public void OverflowWrapNormalLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("overflow-wrap: normal");
        Assert.AreEqual("overflow-wrap", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OverflowWrapProperty>(property);
        var concrete = (OverflowWrapProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("normal", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#overflow-wrap-property")]
    [Spec("css-text-4", "#overflow-wrap-property")]
    public void OverflowWrapAlternateNameNormalLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("word-wrap: normal");
        Assert.AreEqual("overflow-wrap", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OverflowWrapProperty>(property);
        var concrete = (OverflowWrapProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("normal", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#overflow-wrap-property")]
    [Spec("css-text-4", "#overflow-wrap-property")]
    public void OverflowWrapBreakWordLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("overflow-wrap: break-word");
        Assert.AreEqual("overflow-wrap", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OverflowWrapProperty>(property);
        var concrete = (OverflowWrapProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("break-word", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#overflow-wrap-property")]
    [Spec("css-text-4", "#overflow-wrap-property")]
    public void OverflowWrapAlternateNameBreakWordLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("word-wrap: break-word");
        Assert.AreEqual("overflow-wrap", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OverflowWrapProperty>(property);
        var concrete = (OverflowWrapProperty)property;
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("break-word", concrete.Value);
    }

    [TestMethod]
    [Spec("css-text-3", "#overflow-wrap-property")]
    [Spec("css-text-4", "#overflow-wrap-property")]
    public void OverflowWrapNoneIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("overflow-wrap: none");
        Assert.AreEqual("overflow-wrap", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OverflowWrapProperty>(property);
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [Spec("css-text-3", "#overflow-wrap-property")]
    [Spec("css-text-4", "#overflow-wrap-property")]
    public void OverflowWrapAlternateNameNoneIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("word-wrap: none");
        Assert.AreEqual("overflow-wrap", property.Name);
        Assert.IsFalse(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OverflowWrapProperty>(property);
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [DataRow("text-indent")]
    [DataRow("word-spacing")]
    public void RemLengthDoesNotFailTheDocument(string property)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<p id=x style='{property}: 2rem'>a b</p>"));

        Assert.IsNotNull(LayoutHarness.FindById(root, "x"));
    }
}
