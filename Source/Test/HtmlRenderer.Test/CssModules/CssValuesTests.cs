using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssValuesTests
{
    private const string Green = "rgb(0, 128, 0)";
    private const double Delta = 0.01;

    [TestMethod]
    [Spec("css-values-4", "#common-keywords")]
    [DataRow("initial", "black", 0)]
    [DataRow("inherit", Green, 7)]
    [DataRow("unset", Green, 0)]
    public void CssWideKeyword_ResolvesTheValue(string keyword, string color, double marginLeft)
    {
        var box = Styled($"#x{{color:red;margin-left:5px;color:{keyword};margin-left:{keyword}}}",
            "<div style='color:green;margin-left:7px'><div id=x>a</div></div>");

        Assert.AreEqual(color, box.Color);
        Assert.AreEqual(marginLeft, box.ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css-values-4", "#custom-idents")]
    [Spec("css-values-5", "#textual-values")]
    public void CustomIdent_IsCaseSensitive()
    {
        Assert.AreEqual("2", Before("#x{counter-reset:foo 2} #x::before{content:counter(foo)}"));
        Assert.AreEqual("0", Before("#x{counter-reset:FOO 2} #x::before{content:counter(foo)}"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#custom-idents")]
    public void Default_IsNotACustomIdent()
    {
        Assert.AreEqual("2", Before("#x{counter-reset:default 2;counter-reset:default 5} #x::before{content:counter(default)}"));
    }

    [TestMethod]
    [Spec("css-values-4", "#dashed-idents")]
    public void DashedIdent_IsCaseSensitive()
    {
        Assert.AreEqual(4, Styled("#x{--a:4px;--A:8px;margin-left:var(--a)}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#urls")]
    public void SrcFunction_IsAUrl()
    {
        Assert.AreEqual("src(\"a.png\")", Styled("#x{background-image:src(\"a.png\")}").BackgroundImage);
    }

    [TestMethod]
    [Spec("css-values-4", "#url-empty")]
    [DataRow("url()")]
    [DataRow("url(\"\")")]
    public void EmptyUrl_ResolvesToNoResource(string value)
    {
        var requested = new List<string>();
        var container = new HtmlContainerInt(new MockAdapter()) { MaxSize = new RSize(800, 600) };
        container.ImageLoad += (_, e) => requested.Add(e.Src);
        container.SetHtml($"<html><head><style>#x{{width:10px;height:10px;background-image:{value}}}</style></head><body><div id=x></div></body></html>");
        using var graphics = new RecordingGraphics();
        container.PerformLayout(graphics);
        container.PerformPaint(graphics);

        Assert.AreEqual("url(\"\")", LayoutHarness.FindById(container.Root!, "x")!.BackgroundImage);
        CollectionAssert.AreEqual(new[] { "" }, requested);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#url-modifiers")]
    public void UnknownUrlModifier_IsInvalid()
    {
        Assert.AreEqual("none", Styled("#x{background-image:url(\"a.png\" foo)}").BackgroundImage);
    }

    [TestMethod]
    [Spec("css-values-4", "#numeric-ranges")]
    public void NegativeValue_IsAllowedWhereTheRangeAllowsIt()
    {
        var box = Styled("#x{margin-left:-5px;text-indent:-3px}");

        Assert.AreEqual(-5, box.ActualMarginLeft, Delta);
        Assert.AreEqual(-3, box.ActualTextIndent, Delta);
    }

    [TestMethod]
    [Spec("css-values-4", "#integers")]
    [DataRow("3", "3")]
    [DataRow("+3", "3")]
    [DataRow("-3", "-3")]
    [DataRow("2.5", "0")]
    public void Integer_TakesASignButNoFraction(string value, string expected)
    {
        Assert.AreEqual(expected, Before($"#x{{counter-reset:foo {value}}} #x::before{{content:counter(foo)}}"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#numbers")]
    [DataRow("1e1px")]
    [DataRow("1E1px")]
    [DataRow("+.1e2px")]
    [DataRow("100e-1px")]
    public void Number_TakesAnExponent(string value)
    {
        Assert.AreEqual(10, Styled($"#x{{margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css-values-4", "#dimensions")]
    public void Dimension_IsANumberImmediatelyFollowedByItsUnit()
    {
        Assert.AreEqual(5, Styled("#x{margin-left:5px;margin-left:10 px}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css-values-4", "#compat")]
    public void CompatibleUnits_AreCombined()
    {
        Assert.AreEqual(48 + 16 + 16 + 96 / 2.54 + 96 / 2.54,
            Styled("#x{margin-left:calc(0.5in + 12pt + 1pc + 1cm + 10mm)}").ActualMarginLeft, 0.01);
    }

    [TestMethod]
    [Spec("css-values-4", "#mixed-percentages")]
    public void PercentageAndLength_MixInCalc()
    {
        const string parent = "<div style='width:400px'><div id=x>a</div></div>";

        Assert.AreEqual(190, Styled("#x{width:calc(50% - 10px)}", parent).Size.Width, Delta);
        Assert.AreEqual(110, Styled("#x{font-size:10px;padding-left:calc(25% + 1em)}", parent).ActualPaddingLeft, Delta);
    }

    [TestMethod]
    [Spec("css-values-4", "#ratios")]
    [DataRow("4/3")]
    [DataRow("8 / 6")]
    public void Ratio_ComparesTheQuotient(string ratio)
    {
        Assert.AreEqual(Green, Styled($"@media (aspect-ratio: {ratio}) {{ #x {{ color: green }} }}").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#ratios")]
    public void Ratio_MayBeASingleNumber()
    {
        Assert.AreEqual(Green, Styled("@media (min-aspect-ratio: 1.3) { #x { color: green } }").Color);
    }

    [TestMethod]
    [Spec("css-values-4", "#font-relative-lengths")]
    public void Rem_IsTheRootFontSize()
    {
        var box = Styled("html{font-size:10px} #x{font-size:20px;margin-left:2rem;padding-left:calc(1rem)}");

        Assert.AreEqual(20, box.ActualMarginLeft, Delta);
        Assert.AreEqual(10, box.ActualPaddingLeft, Delta);
    }

    [TestMethod]
    [Spec("css-values-4", "#font-relative-lengths")]
    public void Rem_InTextIndent_IsTheRootFontSize()
    {
        Assert.AreEqual(20, Styled("html{font-size:10px} #x{font-size:20px;text-indent:2rem}").ActualTextIndent, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#font-relative-lengths")]
    public void Rem_InWordSpacing_IsTheRootFontSize()
    {
        double WordSpacing(string value) =>
            Styled($"html{{font-size:10px}} #x{{font-size:20px;word-spacing:{value}}}", "<div id=x>a b</div>").ActualWordSpacing;

        Assert.AreEqual(WordSpacing("20px"), WordSpacing("2rem"), Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#font-relative-lengths")]
    [DataRow("ch")]
    [DataRow("ic")]
    [DataRow("cap")]
    [DataRow("lh")]
    [DataRow("rex")]
    [DataRow("rch")]
    [DataRow("rcap")]
    [DataRow("ric")]
    [DataRow("rlh")]
    public void FontRelativeUnit_ResolvesToALength(string unit)
    {
        Assert.AreNotEqual(0, Styled($"#x{{font-size:20px;line-height:30px;margin-left:2{unit}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#viewport-relative-units")]
    [DataRow("10vw", 80)]
    [DataRow("10vh", 60)]
    [DataRow("10vmin", 60)]
    [DataRow("10vmax", 80)]
    [DataRow("10vi", 80)]
    [DataRow("10vb", 60)]
    [DataRow("calc(10vw)", 80)]
    public void ViewportUnit_IsAPercentageOfTheViewport(string value, double expected)
    {
        Assert.AreEqual(expected, Styled($"#x{{margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#viewport-variants")]
    [DataRow("10svw")]
    [DataRow("10lvw")]
    [DataRow("10dvw")]
    public void ViewportSizeVariant_IsAPercentageOfTheViewport(string value)
    {
        Assert.AreEqual(80, Styled($"#x{{margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#absolute-lengths")]
    [DataRow("40Q")]
    [DataRow("calc(40Q)")]
    public void QuarterMillimeter_IsAnAbsoluteUnit(string value)
    {
        Assert.AreEqual(96 / 2.54, Styled($"#x{{margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css-values-4", "#angles")]
    [DataRow("120deg")]
    [DataRow("133.33333grad")]
    [DataRow("2.0943951rad")]
    [DataRow("0.3333333turn")]
    [DataRow("-240deg")]
    [DataRow("480deg")]
    [DataRow("calc(60deg * 2)")]
    public void AngleUnits_AreConvertedToEachOther(string hue)
    {
        Assert.AreEqual(RColor.FromArgb(255, 0, 255, 0), Styled($"#x{{color:hsl({hue}, 100%, 50%)}}").ActualColor);
    }

    [TestMethod]
    [Spec("css-values-4", "#resolution")]
    [DataRow("96dpi")]
    [DataRow("1dppx")]
    [DataRow("1x")]
    [DataRow("37.795dpcm")]
    public void ResolutionUnits_AreConvertedToEachOther(string resolution)
    {
        Assert.AreEqual(Green, Styled($"@media (min-resolution: {resolution}) and (max-resolution: 96.01dpi) {{ #x {{ color: green }} }}").Color);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-func")]
    [DataRow("calc(10px + 5px)", 15)]
    [DataRow("calc(10px - 5px)", 5)]
    [DataRow("calc(10px*3)", 30)]
    [DataRow("calc(30px/3)", 10)]
    [DataRow("calc(2 * 3px + 1px)", 7)]
    [DataRow("calc(1px + 2px * 3)", 7)]
    [DataRow("calc(2px * (3 - 1))", 4)]
    [DataRow("calc(10px * -0.5)", -5)]
    [DataRow("calc(-1 * (10px))", -10)]
    [DataRow("calc(-(10px))", -10)]
    [DataRow("calc(10px - -5px)", 15)]
    [DataRow("calc(calc(10px) * 2)", 20)]
    [DataRow("calc(2em + 1px)", 41)]
    public void Calc_EvaluatesTheExpression(string value, double expected)
    {
        Assert.AreEqual(expected, Styled($"#x{{font-size:20px;margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css-values-4", "#comp-func")]
    [DataRow("min(10px, 20px)", 10)]
    [DataRow("min(10px)", 10)]
    [DataRow("max(10px, 2em, 15px)", 40)]
    [DataRow("clamp(10px, 5px, 20px)", 10)]
    [DataRow("clamp(10px, 15px, 20px)", 15)]
    [DataRow("clamp(10px, 50px, 20px)", 20)]
    [DataRow("calc(min(10px, 20px) + max(1px, 2px))", 12)]
    public void ComparisonFunction_PicksTheArgument(string value, double expected)
    {
        Assert.AreEqual(expected, Styled($"#x{{font-size:20px;margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#comp-func")]
    [DataRow("clamp(30px, 50px, 20px)", 30)]
    [DataRow("clamp(none, 50px, 20px)", 20)]
    [DataRow("clamp(10px, 5px, none)", 10)]
    public void Clamp_FavorsTheMinimumAndTakesNone(string value, double expected)
    {
        Assert.AreEqual(expected, Styled($"#x{{margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#round-func")]
    [DataRow("round(17px, 5px)", 15)]
    [DataRow("round(up, 17px, 5px)", 20)]
    [DataRow("mod(17px, 5px)", 2)]
    [DataRow("rem(-17px, 5px)", -2)]
    public void SteppedValueFunction_IsEvaluated(string value, double expected)
    {
        Assert.AreEqual(expected, Styled($"#x{{margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#trig-funcs")]
    [DataRow("calc(sin(90deg) * 10px)", 10)]
    [DataRow("calc(cos(0) * 10px)", 10)]
    [DataRow("calc(tan(45deg) * 10px)", 10)]
    [DataRow("calc(asin(1) / 1deg * 1px)", 90)]
    [DataRow("calc(atan2(1, 1) / 1deg * 1px)", 45)]
    public void TrigonometricFunction_IsEvaluated(string value, double expected)
    {
        Assert.AreEqual(expected, Styled($"#x{{margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#exponent-funcs")]
    [DataRow("calc(pow(2, 3) * 1px)", 8)]
    [DataRow("calc(sqrt(16) * 1px)", 4)]
    [DataRow("hypot(3px, 4px)", 5)]
    [DataRow("calc(log(1) * 1px + 5px)", 5)]
    [DataRow("calc(exp(0) * 5px)", 5)]
    public void ExponentialFunction_IsEvaluated(string value, double expected)
    {
        Assert.AreEqual(expected, Styled($"#x{{margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#sign-funcs")]
    [DataRow("abs(-5px)", 5)]
    [DataRow("calc(sign(-5px) * -5px)", 5)]
    public void SignFunction_IsEvaluated(string value, double expected)
    {
        Assert.AreEqual(expected, Styled($"#x{{margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#calc-constants")]
    [DataRow("calc(pi * 10px)", 31.42)]
    [DataRow("calc(e * 10px)", 27.18)]
    public void NumericConstant_IsEvaluated(string value, double expected)
    {
        Assert.AreEqual(expected, Styled($"#x{{margin-left:{value}}}").ActualMarginLeft, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#calc-error-constants")]
    public void Infinity_ClampsToTheLargestValue()
    {
        Assert.IsTrue(Styled("#x{margin-left:5px;margin-left:calc(infinity * 1px)}").ActualMarginLeft > 1e6);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#calc-syntax")]
    [DataRow("calc(2ch)")]
    [DataRow("calc(2vh)")]
    [DataRow("min(2vmin, 3px)")]
    public void Calc_TakesEveryLengthUnit(string value)
    {
        Assert.AreNotEqual(5, Styled($"#x{{margin-left:5px;margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-syntax")]
    [DataRow("calc(10px+5px)")]
    [DataRow("calc(10px -5px)")]
    [DataRow("calc(10px- 5px)")]
    [DataRow("calc()")]
    [DataRow("-calc(10px)")]
    public void MalformedCalc_IsIgnored(string value)
    {
        Assert.AreEqual(5, Styled($"#x{{margin-left:5px;margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-type-checking")]
    [DataRow("calc(10px + 1)")]
    [DataRow("calc(10px * 1px)")]
    [DataRow("calc(1deg)")]
    [DataRow("calc(1s)")]
    [DataRow("min(10px, 2deg)")]
    public void MistypedCalc_IsIgnored(string value)
    {
        Assert.AreEqual(5, Styled($"#x{{margin-left:5px;margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-type-checking")]
    public void NumberCalc_IsANumber()
    {
        Assert.AreEqual("30px", Styled("#x{font-size:20px;line-height:calc(1 + 0.5)}").LineHeight);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#calc-type-checking")]
    [DataRow("calc(3)", "3")]
    [DataRow("calc(2.6)", "3")]
    public void NumberCalc_IsRoundedWhereAnIntegerIsExpected(string value, string expected)
    {
        Assert.AreEqual(expected, Before($"#x{{counter-reset:foo {value}}} #x::before{{content:counter(foo)}}"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#calc-type-checking")]
    public void IntermediateLengthSquared_IsAllowed()
    {
        Assert.AreEqual(50, Styled("#x{margin-left:calc(100px * 2px / 4px)}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#calc-ieee")]
    public void DivisionByZero_IsInfinityClampedToTheRange()
    {
        Assert.AreEqual(0, Styled("#x{width:50px;width:calc(-10px / 0)}").ActualWidth, Delta);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-computed-value")]
    public void Calc_ResolvesPercentagesAgainstTheContainingBlock()
    {
        var box = Styled("#x{text-indent:calc(10% + 5px)}", "<div style='width:200px'><div id=x>a</div></div>");

        Assert.AreEqual(25, box.ActualTextIndent, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#calc-computed-value")]
    public void Children_InheritTheComputedLengthNotTheCalc()
    {
        var box = Styled("#p{font-size:10px;text-indent:calc(2em + 1px)} #x{font-size:20px}", "<div id=p><div id=x>a</div></div>");

        Assert.AreEqual(21, box.ActualTextIndent, Delta);
    }

    [TestMethod]
    [Spec("css-values-4", "#calc-range")]
    public void NegativeCalc_IsKeptWhereTheRangeAllowsIt()
    {
        Assert.AreEqual(-5, Styled("#x{margin-left:calc(5px - 10px)}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-4.yml")]
    [Spec("css-values-4", "#calc-range")]
    public void OutOfRangeCalc_IsClamped()
    {
        var box = Styled("#x{width:50px;width:calc(5px - 10px);padding-left:calc(5px - 10px)}");

        Assert.AreEqual(0, box.ActualWidth, Delta);
        Assert.AreEqual(0, box.ActualPaddingLeft, Delta);
    }

    private static CssBox Styled(string css, string body = "<div id=x>a</div>")
    {
        var (root, _) = LayoutHarness.Layout($"<html><head><style>{css}</style></head><body style='margin:0'>{body}</body></html>", 800, 600);
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box;
    }

    private static string Before(string css)
    {
        var before = Styled(css).Boxes.FirstOrDefault(b => b.IsBeforePseudoElement);
        Assert.IsNotNull(before, "No ::before box");
        return before.Text ?? string.Concat(LayoutHarness.Descendants(before).SelectMany(b => b.Words).Select(w => w.Text));
    }
}
