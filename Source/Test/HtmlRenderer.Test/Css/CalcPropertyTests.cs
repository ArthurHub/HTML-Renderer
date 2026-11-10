using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// calc(), min(), max() and clamp() at parse time: grammar, type checking and the folded text.
/// </summary>
[TestClass]
public sealed class CalcPropertyTests
{
    // ── basic arithmetic (fully numeric/absolute -> folds to a plain value) ─────────────

    [TestMethod]
    [Spec("css-values-4", "#calc-simplification")]
    public void Width_CalcAddition_FoldsToPixelLength()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(100px + 20px)");
        Assert.AreEqual("width", property.Name);
        Assert.IsInstanceOfType<WidthProperty>(property);
        var concrete = (WidthProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("120px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-simplification")]
    public void Width_CalcSubtraction_FoldsToPixelLength()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(200px - 60px)");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("140px", property.Value);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-simplification")]
    public void Width_CalcMultiplication_FoldsToPixelLength()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(20px * 4)");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("80px", property.Value);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-simplification")]
    public void Width_CalcMultiplicationNumberFirst_FoldsToPixelLength()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(4 * 20px)");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("80px", property.Value);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-simplification")]
    public void Width_CalcDivision_FoldsToPixelLength()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(100px / 4)");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("25px", property.Value);
    }

    [TestMethod]
    [Spec("css-values-4", "#compat")]
    public void Width_CalcMixedAbsoluteUnits_FoldsToPixelLength()
    {
        // 1in = 96px, 2cm = 2 * 96 / 2.54px
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(1in + 2cm)");
        Assert.IsTrue(property.HasValue);
        StringAssert.StartsWith(property.Value, "171.");
        StringAssert.EndsWith(property.Value, "px");
    }

    // ── mixed relative units (can't fold until layout - canonical calc() text preserved) ─

    [TestMethod]
    [Spec("css-values-4", "#calc-simplification")]
    public void Width_CalcMixedEmPx_PreservesCalcExpression()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(1em + 5px)");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("calc(1em + 5px)", property.Value);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-simplification")]
    public void Width_CalcPercentMinusPx_PreservesCalcExpression()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(100% - 20px)");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("calc(100% - 20px)", property.Value);
    }

    // ── nesting ───────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-values-4", "#calc-syntax")]
    public void Width_NestedCalc_FoldsToPixelLength()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(calc(10px + 10px) * 2)");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("40px", property.Value);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-syntax")]
    public void Width_ParenGrouping_FoldsToPixelLength()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc((10px + 10px) * 2)");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("40px", property.Value);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#calc-simplification")]
    public void Width_ParenGroupingMixedUnits_MultipliesIntoTheSum()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc((1em + 5px) * 2)");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("calc(2em + 10px)", property.Value);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-syntax")]
    public void Width_ParenGroupingMixedUnits_ResolvesToTheProductOfTheSum()
    {
        var (root, _) = LayoutHarness.Layout("<html><body style='margin:0'><div id=x style='font-size:20px; width:calc((1em + 5px) * 2)'>a</div></body></html>");

        Assert.AreEqual(50, LayoutHarness.FindById(root, "x")!.ActualWidth, 0.01);
    }

    // ── min() / max() / clamp() ──────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-values-4", "#comp-func")]
    public void Width_Min_FoldsToSmallerPixelLength()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: min(150px, 100px)");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("100px", property.Value);
    }

    [TestMethod]
    [Spec("css-values-4", "#comp-func")]
    public void Width_Max_FoldsToLargerPixelLength()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: max(150px, 100px)");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("150px", property.Value);
    }

    [TestMethod]
    [Spec("css-values-4", "#comp-func")]
    public void Width_MinMixedUnits_PreservesCanonicalText()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: min(10px, 1em)");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("min(10px, 1em)", property.Value);
    }

    [TestMethod]
    [Spec("css-values-4", "#comp-func")]
    public void Width_ClampWithPercent_PreservesCanonicalText()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: clamp(10px, 50%, 200px)");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("clamp(10px, 50%, 200px)", property.Value);
    }

    [TestMethod]
    [Spec("css-values-4", "#comp-func")]
    public void Width_ClampAllAbsolute_FoldsToPixelLength()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: clamp(10px, 300px, 150px)");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("150px", property.Value);
    }

    [TestMethod]
    [Spec("css-values-4", "#comp-func")]
    public void Width_ClampWrongArgCount_IsInvalid()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: clamp(10px, 20px)");
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [Spec("css-values-4", "#comp-func")]
    public void Width_MinNoArgs_IsInvalid()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: min()");
        Assert.IsFalse(property.HasValue);
    }

    // ── plain-number category (NumberConverter-based properties) ────────────────────

    [TestMethod]
    [Spec("css-values-4", "#calc-type-checking")]
    public void FlexGrow_CalcNumberArithmetic_FoldsToPlainNumber()
    {
        var property = CssConstructionFunctions.ParseDeclaration("flex-grow: calc(1 + 1)");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("2", property.Value);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-type-checking")]
    public void FlexGrow_CalcWithLength_IsInvalid()
    {
        var property = CssConstructionFunctions.ParseDeclaration("flex-grow: calc(1px + 2px)");
        Assert.IsFalse(property.HasValue);
    }

    // ── invalid / degenerate expressions ─────────────────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#calc-ieee")]
    public void Width_CalcDivideByZero_IsValid()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(10px / 0)");
        Assert.IsTrue(property.HasValue);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-type-checking")]
    public void Width_CalcAddNumberToLength_IsInvalid()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(10px + 5)");
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-type-checking")]
    public void Width_CalcMultiplyTwoLengths_IsInvalid()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(10px * 5px)");
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-type-checking")]
    public void Width_CalcDivideByLength_IsInvalid()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(10px / 5px)");
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-syntax")]
    public void Width_CalcNoWhitespaceAroundPlus_IsInvalid()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(10px+5px)");
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-syntax")]
    public void Width_CalcWhitespaceOnOneSideOfMinus_IsInvalid()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(10px -5px)");
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-syntax")]
    public void Width_CalcUnbalancedParens_IsInvalid()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(10px + (5px)");
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-type-checking")]
    public void Width_CalcAngleUnit_IsInvalid()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(10deg + 5deg)");
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-syntax")]
    public void Width_CalcEmpty_IsInvalid()
    {
        var property = CssConstructionFunctions.ParseDeclaration("width: calc()");
        Assert.IsFalse(property.HasValue);
    }

    // ── var() interaction ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void Width_CalcWithVar_StoredOpaquely()
    {
        // var() bypasses the strict per-property converter entirely (Property.TrySetValue routes
        // any var()-containing value to Converters.Any) - the raw text round-trips unresolved here;
        // DomParser resolves and re-validates it through the real converter at cascade time.
        var property = CssConstructionFunctions.ParseDeclaration("width: calc(var(--x) + 10px)");
        Assert.IsTrue(property.HasValue);
        StringAssert.Contains(property.Value, "var(");
        StringAssert.Contains(property.Value, "calc(");
    }

    // ── transform function arguments ─────────────────────────────────────────────────

    [TestMethod]
    public void Transform_TranslateXCalc_Legal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform: translateX(calc(10px + 5px))");
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("translateX(15px)", concrete.Value);
    }

    [TestMethod]
    public void Transform_ScaleCalc_Legal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform: scale(calc(1 + 1))");
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("scale(2)", concrete.Value);
    }

    [TestMethod]
    public void Transform_ScaleCalcArithmeticOperators_FoldsToPlainNumber()
    {
        // Exercises CalcTypeChecker.FoldBinaryNumber's -, *, and / arms (+ is already covered by
        // Transform_ScaleCalc_Legal above), plus FoldUnaryNumber's negation.
        Assert.AreEqual("scale(1)", ((TransformProperty)CssConstructionFunctions.ParseDeclaration("transform: scale(calc(3 - 2))")).Value);
        Assert.AreEqual("scale(6)", ((TransformProperty)CssConstructionFunctions.ParseDeclaration("transform: scale(calc(2 * 3))")).Value);
        Assert.AreEqual("scale(3)", ((TransformProperty)CssConstructionFunctions.ParseDeclaration("transform: scale(calc(6 / 2))")).Value);
        Assert.AreEqual("scale(-2)", ((TransformProperty)CssConstructionFunctions.ParseDeclaration("transform: scale(calc(-2))")).Value);
    }

    [TestMethod]
    public void Transform_ScaleCalcMinMaxClamp_FoldsToPlainNumber()
    {
        // Exercises CalcTypeChecker.FoldCallNumber: min()/max()/clamp() nested inside a Number-
        // category calc() expression, not just the plain-arithmetic case above.
        Assert.AreEqual("scale(2)", ((TransformProperty)CssConstructionFunctions.ParseDeclaration("transform: scale(calc(min(2, 3)))")).Value);
        Assert.AreEqual("scale(3)", ((TransformProperty)CssConstructionFunctions.ParseDeclaration("transform: scale(calc(max(2, 3)))")).Value);
        Assert.AreEqual("scale(2)", ((TransformProperty)CssConstructionFunctions.ParseDeclaration("transform: scale(calc(clamp(1, 2, 3)))")).Value);
    }

    // ── angle calc() (rotate/skew, gradient direction, hsl hue) ──────────────────────

    [TestMethod]
    public void Transform_RotateCalcSameUnit_FoldsToDegrees()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform: rotate(calc(45deg + 10deg))");
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rotate(55deg)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-values-4", "#compat")]
    public void Transform_SkewXCalcMixedAngleUnits_FoldsToDegrees()
    {
        // 1turn / 4 = 90deg - division by a plain number is legal for an angle numerator.
        var property = CssConstructionFunctions.ParseDeclaration("transform: skewX(calc(1turn / 4))");
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("skewX(90deg)", concrete.Value);
    }

    [TestMethod]
    public void BackgroundImage_LinearGradientAngleCalc_FoldsToDegrees()
    {
        var property = CssConstructionFunctions.ParseDeclaration("background-image: linear-gradient(calc(45deg + 45deg), red, blue)");
        Assert.IsTrue(property.HasValue);
        StringAssert.StartsWith(property.Value, "linear-gradient(90deg,");
    }

    [TestMethod]
    public void Transform_RotateCalcUnaryNegation_FoldsToDegrees()
    {
        // Exercises CalcSerializer.FoldUnaryAngle.
        var property = CssConstructionFunctions.ParseDeclaration("transform: rotate(calc(-(45deg)))");
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rotate(-45deg)", concrete.Value);
    }

    [TestMethod]
    public void Transform_RotateCalcNumberTimesAngle_FoldsToDegrees()
    {
        // Exercises CalcSerializer.FoldMultiplicativeAngle's right-hand-is-angle branch (the sibling
        // "angle * number" form goes through the left-hand branch, already covered by the plain
        // arithmetic scale/rotate tests above).
        var property = CssConstructionFunctions.ParseDeclaration("transform: rotate(calc(2 * 45deg))");
        Assert.IsInstanceOfType<TransformProperty>(property);
        var concrete = (TransformProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rotate(90deg)", concrete.Value);
    }

    [TestMethod]
    public void Transform_RotateCalcMinMaxClamp_FoldsToDegrees()
    {
        // Exercises CalcSerializer.FoldCallAngle: min()/max()/clamp() nested inside an Angle-
        // category calc() expression, not just the plain-arithmetic case above.
        Assert.AreEqual("rotate(45deg)", ((TransformProperty)CssConstructionFunctions.ParseDeclaration("transform: rotate(calc(min(45deg, 90deg)))")).Value);
        Assert.AreEqual("rotate(90deg)", ((TransformProperty)CssConstructionFunctions.ParseDeclaration("transform: rotate(calc(max(45deg, 90deg)))")).Value);
        Assert.AreEqual("rotate(45deg)", ((TransformProperty)CssConstructionFunctions.ParseDeclaration("transform: rotate(calc(clamp(0deg, 45deg, 90deg)))")).Value);
    }

    [TestMethod]
    public void Color_HslHueCalcPlainNumber_FoldsToPlainNumber()
    {
        // hue accepts either an angle or a bare number (implicit degrees) - calc() should too.
        var property = CssConstructionFunctions.ParseDeclaration("color: hsl(calc(100 + 20), 50%, 50%)");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("hsl(120, 50%, 50%)", property.Value);
    }

    [TestMethod]
    public void Transform_RotateCalcAngleAndLength_IsInvalid()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transform: rotate(calc(45deg + 10px))");
        Assert.IsFalse(property.HasValue);
    }

    // ── border-radius two-value form ─────────────────────────────────────────────────

    [TestMethod]
    public void BorderTopLeftRadius_CalcFirstValue_Legal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("border-top-left-radius: calc(10px + 2px) 5px");
        Assert.IsInstanceOfType<BorderTopLeftRadiusProperty>(property);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("12px 5px", property.Value);
    }

    // ── length-only (no-percentage) converters: border-width, border-spacing ────────

    [TestMethod]
    public void BorderTopWidth_Calc_Legal()
    {
        // border-top-width goes through Converters.LineWidthConverter, a separate field from
        // LengthOrPercentConverter (border-width doesn't accept percentages) that also needs calc().
        var property = CssConstructionFunctions.ParseDeclaration("border-top-width: calc(2px + 1px)");
        Assert.IsInstanceOfType<BorderTopWidthProperty>(property);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("3px", property.Value);
    }

    [TestMethod]
    public void BorderTopWidth_CalcPercent_IsInvalid()
    {
        // border-width doesn't accept percentages, calc() or not.
        var property = CssConstructionFunctions.ParseDeclaration("border-top-width: calc(50% - 2px)");
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    public void BorderSpacing_CalcTwoValueForm_Legal()
    {
        // border-spacing goes through Converters.LengthConverter (also percentage-free), a separate
        // field from LengthOrPercentConverter.
        var property = CssConstructionFunctions.ParseDeclaration("border-spacing: calc(5px + 5px) 20px");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("10px 20px", property.Value);
    }
}
