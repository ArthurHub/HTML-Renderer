using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class CommonMicrosyntaxesTests
{
    [TestMethod]
    [Spec("html", "common-microsyntaxes.html#boolean-attributes")]
    [DataRow("nowrap")]
    [DataRow("nowrap=''")]
    [DataRow("nowrap=nowrap")]
    [DataRow("nowrap=NoWrap")]
    public void BooleanAttribute_IsTrueWhenPresent(string attribute)
    {
        Assert.AreEqual("nowrap", Box($"<table><tr><td id=x {attribute}>a</td></tr></table>").WhiteSpace);
    }

    [TestMethod]
    [Spec("html", "common-microsyntaxes.html#keywords-and-enumerated-attributes")]
    public void Keyword_MatchesCaseInsensitively()
    {
        Assert.AreEqual("top", Box("<table><tr><td id=x valign=TOP>a</td></tr></table>").VerticalAlign);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "common-microsyntaxes.html#keywords-and-enumerated-attributes")]
    public void AlignKeyword_MatchesCaseInsensitively()
    {
        Assert.AreEqual("center", Box("<p id=x align=CENTER>a</p>").TextAlign);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "common-microsyntaxes.html#keywords-and-enumerated-attributes")]
    public void InvalidKeyword_IsIgnored()
    {
        Assert.AreEqual("rtl", Box("<div dir=rtl><p id=x dir=sideways>a</p></div>").Direction);
    }

    [TestMethod]
    [Spec("html", "common-microsyntaxes.html#signed-integers")]
    [DataRow("5", "6.")]
    [DataRow(" 5", "6.")]
    [DataRow("+5", "6.")]
    [DataRow("-5", "-4.")]
    public void Integer_IsParsed(string start, string marker)
    {
        Assert.AreEqual(marker, Marker($"<ol start='{start}'><li>a</li><li id=x>b</li></ol>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "common-microsyntaxes.html#signed-integers")]
    [DataRow("5th", "6.")]
    [DataRow("5.9", "6.")]
    public void Integer_IgnoresTrailingCharacters(string start, string marker)
    {
        Assert.AreEqual(marker, Marker($"<ol start='{start}'><li>a</li><li id=x>b</li></ol>"));
    }

    [TestMethod]
    [Spec("html", "common-microsyntaxes.html#non-negative-integers")]
    [DataRow("2")]
    [DataRow(" 2")]
    public void NonNegativeInteger_IsParsed(string colspan)
    {
        Assert.AreEqual(200, SpannedCellWidth(colspan), 1);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "common-microsyntaxes.html#non-negative-integers")]
    public void NonNegativeInteger_IgnoresTrailingCharacters()
    {
        Assert.AreEqual(200, SpannedCellWidth("2cols"), 1);
    }

    [TestMethod]
    [Spec("html", "common-microsyntaxes.html#percentages-and-dimensions")]
    [DataRow("100", 100)]
    [DataRow(" 100", 100)]
    [DataRow("100px", 100)]
    [DataRow("100.5", 100.5)]
    [DataRow("50%", 500)]
    [DataRow("50.%", 500)]
    public void Dimension_IsParsed(string width, double expected)
    {
        Assert.AreEqual(expected, RuleWidth(width), 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "common-microsyntaxes.html#percentages-and-dimensions")]
    [DataRow("100abc", 100)]
    [DataRow("10em", 10)]
    [DataRow("-5", 1000)]
    [DataRow("abc", 1000)]
    public void Dimension_IgnoresTrailingCharactersAndInvalidValues(string width, double expected)
    {
        Assert.AreEqual(expected, RuleWidth(width), 0.01);
    }

    [TestMethod]
    [Spec("html", "common-microsyntaxes.html#nonzero-percentages-and-lengths")]
    public void NonzeroDimension_IsParsed()
    {
        Assert.AreEqual(100, Box("<table id=x width=100 style='border-spacing:0'><tr><td style='padding:0'>a</td></tr></table>").Size.Width, 0.01);
    }

    [TestMethod]
    [Spec("html", "common-microsyntaxes.html#nonzero-percentages-and-lengths")]
    public void ZeroDimension_IsIgnored()
    {
        const string table = "<table id=x{0} style='border-spacing:0'><tr><td style='padding:0'>aaaa bbbb cccc</td></tr></table>";

        Assert.AreEqual(Box(string.Format(table, "")).Size.Width, Box(string.Format(table, " width=0")).Size.Width, 0.01);
    }

    [TestMethod]
    [Spec("html", "common-microsyntaxes.html#colours")]
    [DataRow("red", 255, 0, 0)]
    [DataRow("RED", 255, 0, 0)]
    [DataRow(" red ", 255, 0, 0)]
    [DataRow("#f00", 255, 0, 0)]
    [DataRow("#00ff00", 0, 255, 0)]
    public void LegacyColor_IsParsed(string bgcolor, int r, int g, int b)
    {
        AssertColor(r, g, b, Box($"<div id=x bgcolor='{bgcolor}'>a</div>").ActualBackgroundColor);
        AssertColor(r, g, b, Box($"<font id=x color='{bgcolor}'>a</font>").ActualColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "common-microsyntaxes.html#colours")]
    [DataRow("ff0000", 255, 0, 0)]
    [DataRow("#ff000080", 255, 0, 128)]
    [DataRow("chucknorris", 192, 0, 0)]
    [DataRow("rgb(255,0,0)", 0, 37, 0)]
    public void LegacyColor_ParsesAnyStringAsHexDigits(string bgcolor, int r, int g, int b)
    {
        AssertColor(r, g, b, Box($"<div id=x bgcolor='{bgcolor}'>a</div>").ActualBackgroundColor);
    }

    [TestMethod]
    [Spec("html", "common-microsyntaxes.html#space-separated-tokens")]
    [DataRow("a b")]
    [DataRow("  a   b  ")]
    public void ClassTokens_AreSeparatedBySpaces(string classes)
    {
        AssertClassesMatch(classes);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "common-microsyntaxes.html#space-separated-tokens")]
    [DataRow("a\tb")]
    [DataRow("a\nb")]
    [DataRow("a\fb")]
    [DataRow("a\r\nb")]
    public void ClassTokens_AreSeparatedByAnyAsciiWhitespace(string classes)
    {
        AssertClassesMatch(classes);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "common-microsyntaxes.html#mq")]
    public void StyleElement_OnlyAppliesWhenItsMediaMatches()
    {
        AssertColor(0, 0, 0, Box("<style media=print>#x { color: #ff0000 }</style><p id=x>a</p>").ActualColor);
    }

    private static double RuleWidth(string width) =>
        Box($"<hr id=x width='{width}' style='border:0;margin:0'>").Size.Width;

    private static double SpannedCellWidth(string colspan) =>
        Box($"<table style='border-spacing:0'><tr><td id=x colspan='{colspan}' style='padding:0'></td></tr>" +
            "<tr><td style='width:100px;padding:0'></td><td style='width:100px;padding:0'></td></tr></table>").Size.Width;

    private static void AssertClassesMatch(string classes)
    {
        var box = Box($"<style>.a {{ color: #00ff00 }} .b {{ background-color: #0000ff }}</style><p id=x class='{classes}'>a</p>");

        AssertColor(0, 255, 0, box.ActualColor);
        AssertColor(0, 0, 255, box.ActualBackgroundColor);
    }

    private static void AssertColor(int r, int g, int b, RColor actual)
    {
        Assert.AreEqual((r, g, b), (actual.R, actual.G, actual.B), $"Actual {actual}");
    }
}
