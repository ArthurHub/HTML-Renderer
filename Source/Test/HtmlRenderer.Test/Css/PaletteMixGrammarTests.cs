using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class PaletteMixGrammarTests
{
    private static ParsedPaletteMix? Parse(string value) =>
        PaletteMixGrammar.TryParse(CssValueParser.GetCssTokens(value));

    [TestMethod]
    public void Parses_SpaceAndTwoOperands()
    {
        var mix = Parse("palette-mix(in oklab, normal, --brand)");
        Assert.IsNotNull(mix);
        Assert.AreEqual("oklab", mix!.ColorSpace);
        Assert.IsNull(mix.HueMethod);
        Assert.AreEqual("normal", mix.First.Palette);
        Assert.IsNull(mix.First.Percentage);
        Assert.AreEqual("--brand", mix.Second.Palette);
    }

    [TestMethod]
    public void Parses_MixedCaseSpaceAndKeywords()
    {
        // CSS keywords are case-insensitive; the space/hue names must still parse (and later map).
        var mix = Parse("palette-mix(in OKLCH LONGER HUE, --a, --b)");
        Assert.IsNotNull(mix);
        Assert.AreEqual("OKLCH", mix!.ColorSpace);
        Assert.AreEqual("LONGER", mix.HueMethod);
    }

    [TestMethod]
    public void Parses_PercentagesAndPolarHueMethod()
    {
        var mix = Parse("palette-mix(in lch longer hue, --a 25%, --b 75%)");
        Assert.IsNotNull(mix);
        Assert.AreEqual("lch", mix!.ColorSpace);
        Assert.AreEqual("longer", mix.HueMethod);
        Assert.AreEqual(25, mix.First.Percentage);
        Assert.AreEqual(75, mix.Second.Percentage);
    }

    [TestMethod]
    [DataRow("palette-mix(in oklab, --a)")]                 // one operand
    [DataRow("palette-mix(in oklab, --a, --b, --c)")]       // three operands
    [DataRow("palette-mix(--a, --b)")]                      // no interpolation method
    [DataRow("palette-mix(in bogus, --a, --b)")]            // unknown space
    [DataRow("palette-mix(in srgb longer hue, --a, --b)")]  // hue method on rectangular space
    [DataRow("palette-mix(in lch longer, --a, --b)")]       // hue method missing trailing 'hue'
    [DataRow("palette-mix(in oklab, 5px, --b)")]            // non-palette operand
    [DataRow("palette-mix(in oklab, normal 0%, --b 0%)")]   // both percentages zero
    [DataRow("not-a-mix(in oklab, --a, --b)")]              // wrong function name
    public void Rejects_Malformed(string value)
    {
        Assert.IsNull(Parse(value));
    }
}
