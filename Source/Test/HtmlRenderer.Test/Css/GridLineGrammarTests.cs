using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class GridLineGrammarTests
{
    private static GridLine? Parse(string value) =>
        GridLineGrammar.TryParse(CssValueParser.GetCssTokens(value));

    [TestMethod]
    [Spec("css-grid-2", "#line-placement")]
    public void Auto_Parses()
    {
        var line = Parse("auto");
        Assert.IsNotNull(line);
        Assert.IsTrue(line!.IsAuto);
    }

    [TestMethod]
    [Spec("css-grid-2", "#line-placement")]
    [DataRow("1", 1)]
    [DataRow("3", 3)]
    [DataRow("-1", -1)]
    public void IntegerLine_Parses(string value, int expected)
    {
        var line = Parse(value);
        Assert.IsNotNull(line);
        Assert.IsFalse(line!.IsAuto);
        Assert.IsFalse(line.IsSpan);
        Assert.AreEqual(expected, line.Value);
    }

    [TestMethod]
    [Spec("css-grid-2", "#line-placement")]
    [DataRow("span 1", 1)]
    [DataRow("span 3", 3)]
    public void Span_Parses(string value, int expected)
    {
        var line = Parse(value);
        Assert.IsNotNull(line);
        Assert.IsTrue(line!.IsSpan);
        Assert.AreEqual(expected, line.Value);
    }

    [TestMethod]
    [Spec("css-grid-2", "#line-placement")]
    [DataRow("sidebar")]
    [DataRow("main-start")]
    public void NamedLine_Parses(string value)
    {
        var line = Parse(value);
        Assert.IsNotNull(line);
        Assert.IsFalse(line!.IsAuto);
        Assert.IsFalse(line.IsSpan);
        Assert.AreEqual(value, line.Name);
        Assert.AreEqual(1, line.Value);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-grid-2.yml")]
    [Spec("css-grid-2", "#line-placement")]
    public void NoneAsALineName_Parses() => NamedLine_Parses("none");

    [TestMethod]
    [Spec("css-grid-2", "#line-placement")]
    [DataRow("col 2", "col", 2)]
    [DataRow("2 col", "col", 2)]
    [DataRow("col -1", "col", -1)]
    public void NamedNthLine_Parses(string value, string name, int nth)
    {
        var line = Parse(value);
        Assert.IsNotNull(line);
        Assert.AreEqual(name, line!.Name);
        Assert.AreEqual(nth, line.Value);
    }

    [TestMethod]
    [Spec("css-grid-2", "#line-placement")]
    [DataRow("")]
    [DataRow("0")]           // line 0 is invalid
    [DataRow("span 0")]      // span must be >= 1
    [DataRow("span")]        // span needs a count
    [DataRow("span auto")]
    [DataRow("1.5")]         // not an integer
    [DataRow("[name]")]      // bracketed line names belong in a track list, not a <grid-line>
    [DataRow("initial")]     // a CSS-wide keyword is not a custom-ident line name
    public void Invalid_ReturnsNull(string value)
    {
        Assert.IsNull(Parse(value));
    }
}
