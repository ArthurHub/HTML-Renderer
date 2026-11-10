using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssCounterStylesTests
{
    // ── 2 Counter Styles ─────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-counter-styles-3", "#counter-styles")]
    [DataRow(4000, "lower-roman", "4000")]
    [DataRow(4000, "upper-roman", "4000")]
    public void ValueOutsideTheRange_UsesTheFallbackStyle(int number, string style, string expected)
    {
        Assert.AreEqual(expected, CssCounterEngine.FormatCounterValue(number, style));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-counter-styles-3.yml")]
    [Spec("css-counter-styles-3", "#counter-styles")]
    public void ValueOutsideTheHebrewRange_UsesTheFallbackStyle()
    {
        Assert.AreEqual("11000", CssCounterEngine.FormatCounterValue(11000, "hebrew"));
    }

    [TestMethod]
    [Spec("css-counter-styles-3", "#counter-styles")]
    public void NegativeValue_InARomanStyle_FallsBackToDecimal()
    {
        Assert.AreEqual("-3.", Marker("<ol start=-3 style='list-style-type:lower-roman'><li id=x>a</li></ol>"));
    }

    // ── 3 @counter-style, 4 symbols() ────────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/css-counter-styles-3.yml")]
    [Spec("css-counter-styles-3", "#the-counter-style-rule")]
    [Spec("css-counter-styles-3", "#cyclic-system")]
    public void CounterStyleRule_DefinesAStyleForListStyleType()
    {
        var marker = Marker("<style>@counter-style stars { system: cyclic; symbols: '*'; suffix: ' ' }</style>" +
                            "<ul style='list-style-type:stars'><li id=x>a</li></ul>");

        Assert.AreEqual("*", marker.Trim());
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-counter-styles-3.yml")]
    [Spec("css-counter-styles-3", "#symbols-function")]
    public void SymbolsFunction_DefinesAnAnonymousStyle()
    {
        Assert.AreEqual("*", Marker("<ul style=\"list-style-type:symbols(cyclic '*')\"><li id=x>a</li></ul>").Trim());
    }

    // ── 6 Simple Predefined Counter Styles ───────────────────────────────

    [TestMethod]
    [Spec("css-counter-styles-3", "#simple-alphabetic")]
    [DataRow(27, "lower-alpha", "aa")]
    [DataRow(28, "upper-latin", "AB")]
    [DataRow(25, "lower-greek", "αα")]
    public void AlphabeticStyle_IsBijective(int number, string style, string expected)
    {
        Assert.AreEqual(expected, CssCounterEngine.FormatCounterValue(number, style));
    }

    [TestMethod]
    [Spec("css-counter-styles-3", "#simple-numeric")]
    [DataRow(3999, "upper-roman", "MMMCMXCIX")]
    [DataRow(15, "hebrew", "טו")]
    [DataRow(1, "armenian", "Ա")]
    [DataRow(10000, "armenian", "10000")]
    public void AdditiveStyle_FormatsWithinItsRange(int number, string style, string expected)
    {
        Assert.AreEqual(expected, CssCounterEngine.FormatCounterValue(number, style));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-counter-styles-3.yml")]
    [Spec("css-counter-styles-3", "#simple-numeric")]
    public void DecimalLeadingZero_CountsTheNegativeSignTowardsThePadding()
    {
        Assert.AreEqual("-1", CssCounterEngine.FormatCounterValue(-1, "decimal-leading-zero"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-counter-styles-3.yml")]
    [Spec("css-counter-styles-3", "#simple-numeric")]
    [Spec("css-counter-styles-3", "#simple-alphabetic")]
    [Spec("css-counter-styles-3", "#simple-fixed")]
    [DataRow("upper-armenian", "Գ.")]
    [DataRow("lower-armenian", "գ.")]
    [DataRow("arabic-indic", "٣.")]
    [DataRow("hiragana", "う、")]
    [DataRow("cjk-earthly-branch", "寅、")]
    public void OtherPredefinedStyles_NumberTheItem(string type, string expected)
    {
        Assert.AreEqual(expected, Marker($"<ol style='list-style-type:{type}'><li>a</li><li>b</li><li id=x>c</li></ol>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-counter-styles-3.yml")]
    [Spec("css-counter-styles-3", "#simple-symbolic")]
    [Spec("css-counter-styles-3", "#complex-cjk")]
    [Spec("css-counter-styles-3", "#ethiopic-numeric-counter-style")]
    [DataRow("disclosure-open")]
    [DataRow("disclosure-closed")]
    [DataRow("japanese-informal")]
    [DataRow("simp-chinese-formal")]
    [DataRow("korean-hangul-formal")]
    [DataRow("ethiopic-numeric")]
    public void OtherPredefinedStyles_AreAccepted(string type)
    {
        Assert.AreEqual(type, CssConstructionFunctions.ParseDeclaration("list-style-type: " + type)!.Value);
    }
}
