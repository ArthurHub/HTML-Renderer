using HtmlRenderer.Test.CssEngineSupport;

namespace HtmlRenderer.Test.Css;

/// <summary>Which palette-mix() values the CSS engine accepts, checked through font-palette.</summary>
[TestClass]
public sealed class PaletteMixTests
{
    private static string Parse(string value) =>
        CssConstructionFunctions.ParseDeclarations($"font-palette: {value}").GetPropertyValue("font-palette");

    [TestMethod]
    [Spec("css-fonts-4", "#font-palette-prop")]
    [DataRow("palette-mix(in oklab, normal, --brand)")]
    [DataRow("palette-mix(in OKLCH LONGER HUE, --a, --b)")]
    [DataRow("palette-mix(in lch longer hue, --a 25%, --b 75%)")]
    [DataRow("palette-mix(in oklab, --a)")]
    [DataRow("palette-mix(in oklab, --a, --b, --c)")]
    [DataRow("palette-mix(in oklab, 30% --a, --b)")]
    [DataRow("palette-mix(in oklab, palette-mix(in srgb, light, dark) 40%, --b)")]
    public void ValidMix_IsAccepted(string value)
    {
        Assert.AreNotEqual(string.Empty, Parse(value));
    }

    [TestMethod]
    [DataRow("palette-mix(in oklab)")]
    [DataRow("palette-mix(--a, --b)")]
    [DataRow("palette-mix(in bogus, --a, --b)")]
    [DataRow("palette-mix(in srgb longer hue, --a, --b)")]
    [DataRow("palette-mix(in lch longer, --a, --b)")]
    [DataRow("palette-mix(in oklab, 5px, --b)")]
    [DataRow("palette-mix(in oklab, normal 0%, --b 0%)")]
    [DataRow("palette-mix(in oklab, --a 30% 40%, --b)")]
    [DataRow("palette-mix(in oklab, palette-mix(--a, --b), --c)")]
    [DataRow("not-a-mix(in oklab, --a, --b)")]
    public void MalformedMix_IsDropped(string value)
    {
        Assert.AreEqual(string.Empty, Parse(value));
    }
}
