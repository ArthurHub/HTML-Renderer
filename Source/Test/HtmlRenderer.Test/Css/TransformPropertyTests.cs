using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Which transform, transform-origin, transform-style, perspective and perspective-origin values the CSS engine accepts.
/// </summary>
[TestClass]
public sealed class TransformPropertyTests
{
    [TestMethod]
    [Spec("css-transforms-2", "#perspective-property")]
    public void CssPerspectiveNoneUppercaseLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("perspective:  NONE ");
        Assert.AreEqual("perspective", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PerspectiveProperty>(property);
        var concrete = (PerspectiveProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#perspective-property")]
    public void CssPerspectiveLengthPixelLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("perspective:  20px  ");
        Assert.AreEqual("perspective", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PerspectiveProperty>(property);
        var concrete = (PerspectiveProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("20px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#perspective-property")]
    public void CssPerspectiveLengthEmLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("perspective:  3.5em  ");
        Assert.AreEqual("perspective", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PerspectiveProperty>(property);
        var concrete = (PerspectiveProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("3.5em", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#perspective-property")]
    public void CssPerspectiveZeroLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("perspective:  0  ");
        Assert.AreEqual("perspective", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PerspectiveProperty>(property);
        var concrete = (PerspectiveProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#perspective-property")]
    public void CssPerspectivePercentIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("perspective:  10%  ");
        Assert.AreEqual("perspective", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PerspectiveProperty>(property);
        var concrete = (PerspectiveProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#perspective-origin-property")]
    public void CssPerspectiveOriginZeroLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("perspective-origin:  0  ");
        Assert.AreEqual("perspective-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PerspectiveOriginProperty>(property);
        var concrete = (PerspectiveOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#perspective-origin-property")]
    public void CssPerspectiveOriginLengthLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("perspective-origin:  20px  ");
        Assert.AreEqual("perspective-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PerspectiveOriginProperty>(property);
        var concrete = (PerspectiveOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("20px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#perspective-origin-property")]
    public void CssPerspectiveOriginLeftLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("perspective-origin:  left  ");
        Assert.AreEqual("perspective-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PerspectiveOriginProperty>(property);
        var concrete = (PerspectiveOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("left", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#perspective-origin-property")]
    public void CssPerspectiveOriginPercentLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("perspective-origin:  15%  ");
        Assert.AreEqual("perspective-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PerspectiveOriginProperty>(property);
        var concrete = (PerspectiveOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("15%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#perspective-origin-property")]
    public void CssPerspectiveOriginPercentPercentLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("perspective-origin:  15% 25% ");
        Assert.AreEqual("perspective-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PerspectiveOriginProperty>(property);
        var concrete = (PerspectiveOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("15% 25%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#perspective-origin-property")]
    public void CssPerspectiveOriginLeftCenterLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("perspective-origin:  left center ");
        Assert.AreEqual("perspective-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PerspectiveOriginProperty>(property);
        var concrete = (PerspectiveOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("left center", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#perspective-origin-property")]
    public void CssPerspectiveOriginRightBottomLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("perspective-origin:  right BOTTOM ");
        Assert.AreEqual("perspective-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PerspectiveOriginProperty>(property);
        var concrete = (PerspectiveOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("right bottom", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#perspective-origin-property")]
    public void CssPerspectiveOriginTopCenterLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("perspective-origin:  top center ");
        Assert.AreEqual("perspective-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PerspectiveOriginProperty>(property);
        var concrete = (PerspectiveOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("center top", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#perspective-origin-property")]
    public void CssPerspectiveOriginVerticalKeywordThenLengthIllegal()
    {
        // A length forces horizontal-then-vertical order, only keyword pairs may be swapped
        var property = CssConstructionFunctions.ParseDeclaration("perspective-origin:  top 2px ");
        Assert.AreEqual("perspective-origin", property.Name);
        Assert.IsInstanceOfType<PerspectiveOriginProperty>(property);
        var concrete = (PerspectiveOriginProperty)property;
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#perspective-origin-property")]
    public void CssPerspectiveOriginLengthThenWrongAxisKeywordIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("perspective-origin:  2px left ");
        Assert.AreEqual("perspective-origin", property.Name);
        Assert.IsInstanceOfType<PerspectiveOriginProperty>(property);
        var concrete = (PerspectiveOriginProperty)property;
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#transform-style-property")]
    public void CssTransformStylePreserve3DLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform-style:  preserve-3d ");
        Assert.AreEqual("transform-style", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformStyleProperty>(property);
        var concrete = (TransformStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("preserve-3d", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#transform-style-property")]
    public void CssTransformStyleNoneIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform-style:  none ");
        Assert.AreEqual("transform-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformStyleProperty>(property);
        var concrete = (TransformStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#transform-origin-property")]
    public void CssTransformOriginXOffsetLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform-origin:  2px ");
        Assert.AreEqual("transform-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformOriginProperty>(property);
        var concrete = (TransformOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("2px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#transform-origin-property")]
    public void CssTransformOriginXOffsetKeywordLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform-origin:  bottom ");
        Assert.AreEqual("transform-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformOriginProperty>(property);
        var concrete = (TransformOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("bottom", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#transform-origin-property")]
    public void CssTransformOriginYOffsetLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform-origin:  3cm 2px");
        Assert.AreEqual("transform-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformOriginProperty>(property);
        var concrete = (TransformOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("3cm 2px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#transform-origin-property")]
    public void CssTransformOriginLengthThenWrongAxisKeywordIllegal()
    {
        // A length forces horizontal-then-vertical order, only keyword pairs may be swapped
        var property = CssConstructionFunctions.ParseDeclaration("transform-origin:  2px left");
        Assert.AreEqual("transform-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformOriginProperty>(property);
        var concrete = (TransformOriginProperty)property;
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#transform-origin-property")]
    public void CssTransformOriginVerticalKeywordThenLengthIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform-origin:  top 2px ");
        Assert.AreEqual("transform-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformOriginProperty>(property);
        var concrete = (TransformOriginProperty)property;
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#transform-origin-property")]
    public void CssTransformOriginVerticalKeywordThenLengthWithZIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform-origin:  top 2px 10px ");
        Assert.AreEqual("transform-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformOriginProperty>(property);
        var concrete = (TransformOriginProperty)property;
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#transform-origin-property")]
    public void CssTransformOriginXKeywordYOffsetLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform-origin:  left 2px ");
        Assert.AreEqual("transform-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformOriginProperty>(property);
        var concrete = (TransformOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("left 2px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#transform-origin-property")]
    public void CssTransformOriginXKeywordYKeywordLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform-origin:  right top ");
        Assert.AreEqual("transform-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformOriginProperty>(property);
        var concrete = (TransformOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("right top", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#transform-origin-property")]
    public void CssTransformOriginYKeywordXKeywordLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform-origin:  top  right ");
        Assert.AreEqual("transform-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformOriginProperty>(property);
        var concrete = (TransformOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("right top", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#transform-origin-property")]
    public void CssTransformOriginXYZLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform-origin:  2px 30% 10px ");
        Assert.AreEqual("transform-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformOriginProperty>(property);
        var concrete = (TransformOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("2px 30% 10px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#transform-origin-property")]
    public void CssTransformOriginLengthThenWrongAxisKeywordWithZIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform-origin:  2px left 10px ");
        Assert.AreEqual("transform-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformOriginProperty>(property);
        var concrete = (TransformOriginProperty)property;
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#transform-origin-property")]
    public void CssTransformOriginXKeywordYZLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform-origin:  left 5px -3px ");
        Assert.AreEqual("transform-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformOriginProperty>(property);
        var concrete = (TransformOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("left 5px -3px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#transform-origin-property")]
    public void CssTransformOriginXKeywordYKeywordZLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform-origin:  right bottom 2cm ");
        Assert.AreEqual("transform-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformOriginProperty>(property);
        var concrete = (TransformOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("right bottom 2cm", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#transform-origin-property")]
    public void CssTransformOriginYKeywordXKeywordZLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform-origin:  bottom  right  2cm ");
        Assert.AreEqual("transform-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformOriginProperty>(property);
        var concrete = (TransformOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("right bottom 2cm", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#transform-property")]
    public void CssTransformNoneLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  none ");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#two-d-transform-functions")]
    public void CssTransformMatrixLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  matrix(1.0, 2.0, 3.0, 4.0, 5.0, 6.0) ");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("matrix(1, 2, 3, 4, 5, 6)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#two-d-transform-functions")]
    public void CssTransformTranslateLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  translate(12px, 50%) ");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("translate(12px, 50%)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#two-d-transform-functions")]
    public void CssTransformTranslateXLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  translateX(2em) ");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("translateX(2em)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#two-d-transform-functions")]
    public void CssTransformTranslateYLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  translateY(3in) ");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("translateY(3in)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#two-d-transform-functions")]
    public void CssTransformScaleLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  scale(2, 0.5) ");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("scale(2, 0.5)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#two-d-transform-functions")]
    public void CssTransformScaleXLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  scaleX(0.1) ");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("scaleX(0.1)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#two-d-transform-functions")]
    public void CssTransformScaleYLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  scaleY(1.5) ");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("scaleY(1.5)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#two-d-transform-functions")]
    public void CssTransformRotateLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  rotate(0.5turn) ");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rotate(0.5turn)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#two-d-transform-functions")]
    public void CssTransformSkewXLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  skewX(  30deg  ) ");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("skewX(30deg)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#two-d-transform-functions")]
    public void CssTransformSkewYLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  skewY(  1.07rad  ) ");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("skewY(1.07rad)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-1", "#transform-property")]
    public void CssTransformMultipleLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  translate(50%, 50%) rotate(45deg) scale(1.5)");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("translate(50%, 50%) rotate(45deg) scale(1.5)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#three-d-transform-functions")]
    public void CssTransformMatrix3dLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration(
            "transform:  matrix3d(1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0)");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#three-d-transform-functions")]
    public void CssTransformTranslate3dLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  translate3d(12px, 50%, 3em)");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#three-d-transform-functions")]
    public void CssTransformTranslateZLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  translateZ(2px)");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#three-d-transform-functions")]
    public void CssTransformScale3dLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  scale3d(2.5, 1.2, 0.3)");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#three-d-transform-functions")]
    public void CssTransformScaleZLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  scaleZ(0.3)");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#three-d-transform-functions")]
    public void CssTransformRotate3dLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  rotate3d(1, 2.0, 3.0, 10deg)");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#three-d-transform-functions")]
    public void CssTransformRotateXLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  rotateX(10deg)");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#three-d-transform-functions")]
    public void CssTransformRotateYLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform:  rotateY(10deg)");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#three-d-transform-functions")]
    public void CssTransformRotateZLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform: rotateZ(10deg)");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-transforms-2", "#three-d-transform-functions")]
    public void CssTransformPerspectiveLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform: perspective(17px)");
        Assert.AreEqual("transform", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
    }
}
