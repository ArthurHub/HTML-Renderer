using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// A shorthand sets every longhand it manages, a longhand its value doesn't cover reads "initial".
/// </summary>
[TestClass]
public sealed class BorderPropertyTests
{
    private static string GetProperty(string declaration, string propertyName)
    {
        var rule = new CssParser(new MockAdapter()).ParseInlineStyle(declaration);
        Assert.IsNotNull(rule);
        return rule.Style[propertyName];
    }

    private static string GetPriority(string declaration, string propertyName)
    {
        var rule = new CssParser(new MockAdapter()).ParseInlineStyle(declaration);
        Assert.IsNotNull(rule);
        return rule.Style.GetPropertyPriority(propertyName);
    }

    // ── border-spacing ───────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    public void BorderSpacingLengthLegal()
    {
        Assert.AreEqual("20px", GetProperty("border-spacing: 20px", "border-spacing"));
    }

    [TestMethod]
    public void BorderSpacingZeroLegal()
    {
        Assert.AreEqual("0", GetProperty("border-spacing: 0", "border-spacing"));
    }

    [TestMethod]
    public void BorderSpacingLengthLengthLegal()
    {
        Assert.AreEqual("15px 3em", GetProperty("border-spacing: 15px 3em", "border-spacing"));
    }

    [TestMethod]
    public void BorderSpacingLengthZeroLegal()
    {
        Assert.AreEqual("15px 0", GetProperty("border-spacing: 15px 0", "border-spacing"));
    }

    [TestMethod]
    public void BorderSpacingPercentIllegal()
    {
        // A percentage is not a legal border-spacing value (only <length> is allowed) - the real engine
        // now actually validates this (the old parser stored it verbatim, unfiltered).
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty("border-spacing: 15%", "border-spacing")));
    }

    // ── longhand border-*-color ──────────────────────────────────────────────────────────────────────────

    [TestMethod]
    public void BorderBottomColorRedLegal()
    {
        Assert.AreEqual("rgb(255, 0, 0)", GetProperty("border-bottom-color: red", "border-bottom-color"));
    }

    [TestMethod]
    public void BorderTopColorHexLegal()
    {
        Assert.AreEqual("rgb(0, 255, 0)", GetProperty("border-top-color: #0F0", "border-top-color"));
    }

    [TestMethod]
    public void BorderRightColorRgbaLegal()
    {
        Assert.AreEqual("rgba(1, 1, 1, 0)", GetProperty("border-right-color: rgba(1, 1, 1, 0)", "border-right-color"));
    }

    [TestMethod]
    public void BorderLeftColorRgbLegal()
    {
        const string declaration = "border-left-color: rgb(1, 255, 100)  !important";
        Assert.AreEqual("rgb(1, 255, 100)", GetProperty(declaration, "border-left-color"));
        Assert.AreEqual("important", GetPriority(declaration, "border-left-color"));
    }

    // ── border-color (all-sides shorthand) ───────────────────────────────────────────────────────────────

    [TestMethod]
    public void BorderColorTransparentLegal()
    {
        const string declaration = "border-color: transparent";

        Assert.AreEqual("rgba(0, 0, 0, 0)", GetProperty(declaration, "border-top-color"));
        Assert.AreEqual("rgba(0, 0, 0, 0)", GetProperty(declaration, "border-right-color"));
        Assert.AreEqual("rgba(0, 0, 0, 0)", GetProperty(declaration, "border-bottom-color"));
        Assert.AreEqual("rgba(0, 0, 0, 0)", GetProperty(declaration, "border-left-color"));
    }

    [TestMethod]
    public void BorderColorRedGreenLegal()
    {
        const string declaration = "border-color: red   green";

        Assert.AreEqual("rgb(255, 0, 0)", GetProperty(declaration, "border-top-color"));
        Assert.AreEqual("rgb(255, 0, 0)", GetProperty(declaration, "border-bottom-color"));
        Assert.AreEqual("rgb(0, 128, 0)", GetProperty(declaration, "border-left-color"));
        Assert.AreEqual("rgb(0, 128, 0)", GetProperty(declaration, "border-right-color"));
    }

    [TestMethod]
    public void BorderColorRedRgbLegal()
    {
        const string declaration = "border-color: red   rgb(0,0,0)";

        Assert.AreEqual("rgb(255, 0, 0)", GetProperty(declaration, "border-top-color"));
        Assert.AreEqual("rgb(255, 0, 0)", GetProperty(declaration, "border-bottom-color"));
        Assert.AreEqual("rgb(0, 0, 0)", GetProperty(declaration, "border-left-color"));
        Assert.AreEqual("rgb(0, 0, 0)", GetProperty(declaration, "border-right-color"));
    }

    [TestMethod]
    public void BorderColorRedBlueGreenLegal()
    {
        const string declaration = "border-color: red blue green";

        Assert.AreEqual("rgb(255, 0, 0)", GetProperty(declaration, "border-top-color"));
        Assert.AreEqual("rgb(0, 0, 255)", GetProperty(declaration, "border-left-color"));
        Assert.AreEqual("rgb(0, 0, 255)", GetProperty(declaration, "border-right-color"));
        Assert.AreEqual("rgb(0, 128, 0)", GetProperty(declaration, "border-bottom-color"));
    }

    [TestMethod]
    public void BorderColorRedBlueGreenBlackLegal()
    {
        const string declaration = "border-color: red blue green   BLACK";

        Assert.AreEqual("rgb(255, 0, 0)", GetProperty(declaration, "border-top-color"));
        Assert.AreEqual("rgb(0, 0, 255)", GetProperty(declaration, "border-right-color"));
        Assert.AreEqual("rgb(0, 128, 0)", GetProperty(declaration, "border-bottom-color"));
        Assert.AreEqual("rgb(0, 0, 0)", GetProperty(declaration, "border-left-color"));
    }

    [TestMethod]
    public void BorderColorRedBlueGreenBlackTransparentIllegal()
    {
        // A 5-value list is invalid for a 1/2/3/4-value periodic shorthand, so none of the
        // border-*-color longhands get set at all.
        const string declaration = "border-color: red blue green black transparent";

        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "border-top-color")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "border-right-color")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "border-bottom-color")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "border-left-color")));
    }

    // ── border-style (longhand + all-sides shorthand) ────────────────────────────────────────────────────

    [TestMethod]
    public void BorderStyleDottedLegal()
    {
        const string declaration = "border-style: dotted";

        Assert.AreEqual("dotted", GetProperty(declaration, "border-top-style"));
        Assert.AreEqual("dotted", GetProperty(declaration, "border-right-style"));
        Assert.AreEqual("dotted", GetProperty(declaration, "border-bottom-style"));
        Assert.AreEqual("dotted", GetProperty(declaration, "border-left-style"));
    }

    [TestMethod]
    public void BorderStyleInsetOutsetUpperLegal()
    {
        const string declaration = "border-style: INSET   OUTset";

        Assert.AreEqual("inset", GetProperty(declaration, "border-top-style"));
        Assert.AreEqual("inset", GetProperty(declaration, "border-bottom-style"));
        Assert.AreEqual("outset", GetProperty(declaration, "border-left-style"));
        Assert.AreEqual("outset", GetProperty(declaration, "border-right-style"));
    }

    [TestMethod]
    public void BorderStyleDoubleGrooveLegal()
    {
        const string declaration = "border-style: double   groove";

        Assert.AreEqual("double", GetProperty(declaration, "border-top-style"));
        Assert.AreEqual("double", GetProperty(declaration, "border-bottom-style"));
        Assert.AreEqual("groove", GetProperty(declaration, "border-left-style"));
        Assert.AreEqual("groove", GetProperty(declaration, "border-right-style"));
    }

    [TestMethod]
    public void BorderStyleRidgeSolidDashedLegal()
    {
        const string declaration = "border-style: ridge solid dashed";

        Assert.AreEqual("ridge", GetProperty(declaration, "border-top-style"));
        Assert.AreEqual("solid", GetProperty(declaration, "border-left-style"));
        Assert.AreEqual("solid", GetProperty(declaration, "border-right-style"));
        Assert.AreEqual("dashed", GetProperty(declaration, "border-bottom-style"));
    }

    [TestMethod]
    public void BorderStyleHiddenDottedNoneNoneLegal()
    {
        const string declaration = "border-style   :   hidden  dotted  NONE   nONe";

        Assert.AreEqual("hidden", GetProperty(declaration, "border-top-style"));
        Assert.AreEqual("dotted", GetProperty(declaration, "border-right-style"));
        Assert.AreEqual("none", GetProperty(declaration, "border-bottom-style"));
        Assert.AreEqual("none", GetProperty(declaration, "border-left-style"));
    }

    [TestMethod]
    public void BorderStyleWavyIllegal()
    {
        // An invalid border-style keyword is rejected by the real engine, so none of the
        // border-*-style longhands get set (the old parser had no keyword validation at all and let
        // "wavy" through unfiltered onto all four sides).
        const string declaration = "border-style: wavy";

        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "border-top-style")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "border-right-style")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "border-bottom-style")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "border-left-style")));
    }

    [TestMethod]
    public void BorderBottomStyleGrooveLegal()
    {
        Assert.AreEqual("groove", GetProperty("border-bottom-style: GROOVE", "border-bottom-style"));
    }

    [TestMethod]
    public void BorderTopStyleNoneLegal()
    {
        Assert.AreEqual("none", GetProperty("border-top-style:none", "border-top-style"));
    }

    [TestMethod]
    public void BorderRightStyleDoubleLegal()
    {
        Assert.AreEqual("double", GetProperty("border-right-style:double", "border-right-style"));
    }

    [TestMethod]
    public void BorderLeftStyleHiddenLegal()
    {
        const string declaration = "border-left-style: hidden  !important";
        Assert.AreEqual("hidden", GetProperty(declaration, "border-left-style"));
        Assert.AreEqual("important", GetPriority(declaration, "border-left-style"));
    }

    // ── border-width (longhand + all-sides shorthand) ────────────────────────────────────────────────────
    // thin, medium and thick are resolved to 1px, 3px and 5px when parsed, so the stored longhand is a length.

    [TestMethod]
    public void BorderBottomWidthThinLegal()
    {
        Assert.AreEqual("1px", GetProperty("border-bottom-width: THIN", "border-bottom-width"));
    }

    [TestMethod]
    public void BorderTopWidthZeroLegal()
    {
        Assert.AreEqual("0", GetProperty("border-top-width: 0", "border-top-width"));
    }

    [TestMethod]
    public void BorderRightWidthEmLegal()
    {
        Assert.AreEqual("3em", GetProperty("border-right-width: 3em", "border-right-width"));
    }

    [TestMethod]
    public void BorderLeftWidthThickLegal()
    {
        const string declaration = "border-left-width: thick !important";
        Assert.AreEqual("5px", GetProperty(declaration, "border-left-width"));
        Assert.AreEqual("important", GetPriority(declaration, "border-left-width"));
    }

    [TestMethod]
    public void BorderWidthMediumLegal()
    {
        const string declaration = "border-width: medium";

        Assert.AreEqual("3px", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("3px", GetProperty(declaration, "border-right-width"));
        Assert.AreEqual("3px", GetProperty(declaration, "border-bottom-width"));
        Assert.AreEqual("3px", GetProperty(declaration, "border-left-width"));
    }

    [TestMethod]
    public void BorderWidthLengthZeroLegal()
    {
        const string declaration = "border-width: 3px   0";

        Assert.AreEqual("3px", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("3px", GetProperty(declaration, "border-bottom-width"));
        Assert.AreEqual("0", GetProperty(declaration, "border-left-width"));
        Assert.AreEqual("0", GetProperty(declaration, "border-right-width"));
    }

    [TestMethod]
    public void BorderWidthThinLengthLegal()
    {
        const string declaration = "border-width: THIN   1px";

        Assert.AreEqual("1px", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("1px", GetProperty(declaration, "border-bottom-width"));
        Assert.AreEqual("1px", GetProperty(declaration, "border-left-width"));
        Assert.AreEqual("1px", GetProperty(declaration, "border-right-width"));
    }

    [TestMethod]
    public void BorderWidthMediumThinThickLegal()
    {
        const string declaration = "border-width: medium thin thick";

        Assert.AreEqual("3px", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("1px", GetProperty(declaration, "border-left-width"));
        Assert.AreEqual("1px", GetProperty(declaration, "border-right-width"));
        Assert.AreEqual("5px", GetProperty(declaration, "border-bottom-width"));
    }

    [TestMethod]
    public void BorderWidthLengthLengthLengthLengthLegal()
    {
        const string declaration = "border-width:  1px  2px   3px  4px  !important ";

        Assert.AreEqual("1px", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("2px", GetProperty(declaration, "border-right-width"));
        Assert.AreEqual("3px", GetProperty(declaration, "border-bottom-width"));
        Assert.AreEqual("4px", GetProperty(declaration, "border-left-width"));
    }

    [TestMethod]
    public void BorderWidthLengthInEmZeroLegal()
    {
        const string declaration = "border-width:  0.3em 0 ";

        Assert.AreEqual("0.3em", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("0.3em", GetProperty(declaration, "border-bottom-width"));
        Assert.AreEqual("0", GetProperty(declaration, "border-left-width"));
        Assert.AreEqual("0", GetProperty(declaration, "border-right-width"));
    }

    [TestMethod]
    public void BorderWidthMediumZeroLengthThickLegal()
    {
        const string declaration = "border-width:   medium 0 1px thick ";

        Assert.AreEqual("3px", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("0", GetProperty(declaration, "border-right-width"));
        Assert.AreEqual("1px", GetProperty(declaration, "border-bottom-width"));
        Assert.AreEqual("5px", GetProperty(declaration, "border-left-width"));
    }

    [TestMethod]
    public void BorderWidthZerosIllegal()
    {
        // A 5-value list is invalid for a 1/2/3/4-value periodic shorthand, so none of the
        // border-*-width longhands get set at all.
        const string declaration = "border-width: 0 0 0 0 0";

        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "border-top-width")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "border-right-width")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "border-bottom-width")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "border-left-width")));
    }

    // ── border (single-side shorthand: "border", and equally "border-left/top/right/bottom", which share
    //    the exact same value grammar) - a longhand this shorthand's value didn't cover resolves to the
    //    literal string "initial" per CSS Cascading (a shorthand always sets every longhand it manages,
    //    explicitly or to its initial value), rather than being left unset the way the old positional
    //    whitespace-tokenizer parser left it. ───────────────────────────────────────────────────────────

    [TestMethod]
    public void BorderZeroLegal()
    {
        // A bare "0" is a legal border width. The old ParseBorderWidth only recognized a bare number as
        // a width when at least 3 characters long (a number plus a 2-character unit) or one of the
        // thin/medium/thick keywords, so a bare unitless "0" wasn't recognized at all - restored here
        // now that the real engine's LineWidthConverter handles it correctly.
        const string declaration = "border: 0";

        Assert.AreEqual("0", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("initial", GetProperty(declaration, "border-top-style"));
        Assert.AreEqual("initial", GetProperty(declaration, "border-top-color"));
    }

    [TestMethod]
    public void BorderLineStyleLegal()
    {
        const string declaration = "border: dotted";

        Assert.AreEqual("initial", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("dotted", GetProperty(declaration, "border-top-style"));
        Assert.AreEqual("initial", GetProperty(declaration, "border-top-color"));
    }

    [TestMethod]
    public void BorderLengthRedLegal()
    {
        const string declaration = "border :  2px red ";

        Assert.AreEqual("2px", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("initial", GetProperty(declaration, "border-top-style"));
        Assert.AreEqual("rgb(255, 0, 0)", GetProperty(declaration, "border-top-color"));
    }

    [TestMethod]
    public void BorderRgbLegal()
    {
        // "rgb(255, 100, 0)" is recognized as the border color. The old ParseBorder's whitespace-only
        // tokenizer (CommonUtils.GetNextSubString has no notion of parentheses) split a color function
        // containing internal spaces into unrecognizable fragments ("rgb(255,", "100,", "0)") that none
        // individually resolved to a width/style/color - restored here now that the real engine's
        // tokenizer correctly treats the whole function() as one token.
        const string declaration = "border :  rgb(255, 100, 0) ";

        Assert.AreEqual("initial", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("initial", GetProperty(declaration, "border-top-style"));
        Assert.AreEqual("rgb(255, 100, 0)", GetProperty(declaration, "border-top-color"));
    }

    [TestMethod]
    public void BorderGrooveRgbLegal()
    {
        const string declaration = "border :  GROOVE rgb(255, 100, 0) ";

        Assert.AreEqual("initial", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("groove", GetProperty(declaration, "border-top-style"));
        Assert.AreEqual("rgb(255, 100, 0)", GetProperty(declaration, "border-top-color"));
    }

    [TestMethod]
    public void BorderInsetGreenLengthLegal()
    {
        const string declaration = "border :  inset  green 3em ";

        Assert.AreEqual("3em", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("inset", GetProperty(declaration, "border-top-style"));
        Assert.AreEqual("rgb(0, 128, 0)", GetProperty(declaration, "border-top-color"));
    }

    [TestMethod]
    public void BorderRedSolidLengthLegal()
    {
        const string declaration = "border :  red  SOLID 1px ";

        Assert.AreEqual("1px", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("solid", GetProperty(declaration, "border-top-style"));
        Assert.AreEqual("rgb(255, 0, 0)", GetProperty(declaration, "border-top-color"));
    }

    [TestMethod]
    public void BorderLengthBlackDoubleLegal()
    {
        const string declaration = "border :  0.5px black double ";

        Assert.AreEqual("0.5px", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("double", GetProperty(declaration, "border-top-style"));
        Assert.AreEqual("rgb(0, 0, 0)", GetProperty(declaration, "border-top-color"));
    }

    [TestMethod]
    public void BorderOutSetCurrentColor()
    {
        // "currentColor" is now a legal color keyword, recognized as the border color - the old parser
        // had no special handling for it (looked it up like any other named color, which the adapter
        // doesn't know, so it was rejected).
        const string declaration = "border: 1px outset currentColor";

        Assert.AreEqual("1px", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("outset", GetProperty(declaration, "border-top-style"));
        Assert.AreEqual("currentColor", GetProperty(declaration, "border-top-color"));
    }

    [TestMethod]
    public void BorderOutSetWithNoColor()
    {
        const string declaration = "border: 1px outset";

        Assert.AreEqual("1px", GetProperty(declaration, "border-top-width"));
        Assert.AreEqual("outset", GetProperty(declaration, "border-top-style"));
        Assert.AreEqual("initial", GetProperty(declaration, "border-top-color"));
    }
}
