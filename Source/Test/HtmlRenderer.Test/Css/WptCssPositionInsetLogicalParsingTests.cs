using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// The flow-relative inset longhands are stored under the physical property they map to, e.g. inset-block-start as
/// top, so looking one up by its own name finds nothing and the Longhand_Accepts* tests stay ignored.
/// </summary>
[TestClass]
public sealed class WptCssPositionInsetLogicalParsingTests
{
    private static string DeclaredValue(string property, string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {property}: {value}; }}");
        var rule = sheet.Rules.OfType<StyleRule>().Single();
        return rule.Style.GetPropertyValue(property);
    }

    // ─── Longhands: 'auto | <length-percentage>' ──────────────────────────────

    [TestMethod]
    [Spec("css-position-3", "#insets")]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_AcceptsZero(string property)
    {
        Assert.AreEqual("0", DeclaredValue(property, "0"));
    }

    [TestMethod]
    [Spec("css-position-3", "#insets")]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_AcceptsAuto(string property)
    {
        Assert.AreEqual("auto", DeclaredValue(property, "auto"));
    }

    [TestMethod]
    [Spec("css-position-3", "#insets")]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_AcceptsPercentage(string property)
    {
        Assert.AreEqual("10%", DeclaredValue(property, "10%"));
    }

    [TestMethod]
    [Spec("css-position-3", "#insets")]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_AcceptsRemLength(string property)
    {
        Assert.AreEqual("1rem", DeclaredValue(property, "1rem"));
    }

    [TestMethod]
    [Spec("css-position-3", "#insets")]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_AcceptsNegativeLength(string property)
    {
        Assert.AreEqual("-10px", DeclaredValue(property, "-10px"));
    }

    [TestMethod]
    [Spec("css-position-3", "#insets")]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_AcceptsNegativePercentage(string property)
    {
        Assert.AreEqual("-20%", DeclaredValue(property, "-20%"));
    }

    [TestMethod]
    [Spec("css-position-3", "#insets")]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_AcceptsCalcExpression(string property)
    {
        Assert.IsFalse(string.IsNullOrEmpty(DeclaredValue(property, "calc(2em + 3ex)")));
    }

    [TestMethod]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_RejectsAngle(string property)
    {
        Assert.AreEqual(string.Empty, DeclaredValue(property, "0deg"));
    }

    [TestMethod]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_RejectsCalcAngle(string property)
    {
        Assert.AreEqual(string.Empty, DeclaredValue(property, "calc(20deg)"));
    }

    [TestMethod]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_RejectsUnitlessNumber(string property)
    {
        Assert.AreEqual(string.Empty, DeclaredValue(property, "10"));
    }

    // ─── Shorthands: same single-value grammar, plus a 1-or-2-value periodic form ──

    [TestMethod]
    [DataRow("inset-block")]
    [DataRow("inset-inline")]
    [Spec("css-position-3", "#inset-shorthands")]
    public void Shorthand_SingleValue_AcceptsAuto(string property)
    {
        Assert.AreEqual("auto", DeclaredValue(property, "auto"));
    }

    [TestMethod]
    [DataRow("inset-block")]
    [DataRow("inset-inline")]
    [Spec("css-position-3", "#inset-shorthands")]
    public void Shorthand_SingleValue_AcceptsCalcExpression(string property)
    {
        Assert.IsFalse(string.IsNullOrEmpty(DeclaredValue(property, "calc(2em + 3ex)")));
    }

    [TestMethod]
    [DataRow("inset-block")]
    [DataRow("inset-inline")]
    [Spec("css-position-3", "#inset-shorthands")]
    public void Shorthand_TwoEqualValues_CollapseToOne(string property)
    {
        Assert.AreEqual("auto", DeclaredValue(property, "auto auto"));
        Assert.AreEqual("100px", DeclaredValue(property, "100px 100px"));
    }

    [TestMethod]
    [DataRow("inset-block")]
    [DataRow("inset-inline")]
    [Spec("css-position-3", "#inset-shorthands")]
    public void Shorthand_TwoUnequalValues_StayTwoValues(string property)
    {
        Assert.AreEqual("10% -5px", DeclaredValue(property, "10% -5px"));
    }

    [TestMethod]
    [DataRow("inset-block")]
    [DataRow("inset-inline")]
    [Spec("css-position-3", "#inset-shorthands")]
    public void Shorthand_RejectsAngle(string property)
    {
        Assert.AreEqual(string.Empty, DeclaredValue(property, "0deg"));
    }

    [TestMethod]
    [DataRow("inset-block")]
    [DataRow("inset-inline")]
    [Spec("css-position-3", "#inset-shorthands")]
    public void Shorthand_RejectsUnitlessNumber(string property)
    {
        Assert.AreEqual(string.Empty, DeclaredValue(property, "10"));
    }

    [TestMethod]
    [DataRow("inset-block")]
    [DataRow("inset-inline")]
    [Spec("css-position-3", "#inset-shorthands")]
    public void Shorthand_RejectsMixedGlobalAndOrdinaryValue(string property)
    {
        // A global keyword (inherit/initial/unset/revert) can only appear alone - CSS Cascade 4 §3.1 -
        // never combined with an ordinary value in the same multi-value shorthand.
        Assert.AreEqual(string.Empty, DeclaredValue(property, "inherit auto"));
        Assert.AreEqual(string.Empty, DeclaredValue(property, "inherit inherit"));
    }

    // ─── z-index: calc() folds to a plain rounded integer (css-position/parsing/z-index-positioned-computed.html) ──

    [TestMethod]
    [Spec("css-values-4", "#calc-type-checking")]
    [DataRow("calc(3 - 2)", "1")]
    [DataRow("calc(1.5 + 1.5)", "3")]
    [DataRow("calc(2 * -3)", "-6")]
    [DataRow("calc(0.5)", "1")]
    [DataRow("calc(0.4)", "0")]
    public void ZIndex_CalcExpression_FoldsToNearestInteger(string value, string expected)
    {
        Assert.AreEqual(expected, DeclaredValue("z-index", value));
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-type-checking")]
    public void ZIndex_RejectsLengthCalcExpression()
    {
        Assert.AreEqual(string.Empty, DeclaredValue("z-index", "calc(1px + 1px)"));
    }
}
