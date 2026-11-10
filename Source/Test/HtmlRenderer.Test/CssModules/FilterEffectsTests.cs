using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.CssModules;

/// <summary>Which Filter Effects values the CSS engine accepts.</summary>
[TestClass]
public sealed class FilterEffectsTests
{
    [Ignore("Gap, see Spec/filter-effects-1.yml")]
    [TestMethod]
    [Spec("filter-effects-1", "#FilterProperty")]
    [Spec("filter-effects-1", "#supported-filter-functions")]
    [DataRow("none")]
    [DataRow("url(#f)")]
    [DataRow("blur(2px)")]
    [DataRow("brightness(0.5)")]
    [DataRow("contrast(200%)")]
    [DataRow("drop-shadow(2px 2px 4px rgb(0, 0, 0))")]
    [DataRow("grayscale(1)")]
    [DataRow("hue-rotate(90deg)")]
    [DataRow("invert(1)")]
    [DataRow("opacity(0.5)")]
    [DataRow("saturate(2)")]
    [DataRow("sepia(1) blur(1px)")]
    public void Filter_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("filter", value));
    }

    [Ignore("Gap, see Spec/filter-effects-1.yml")]
    [TestMethod]
    [Spec("filter-effects-1", "#FloodColorProperty")]
    [Spec("filter-effects-1", "#FloodOpacityProperty")]
    [Spec("filter-effects-1", "#ColorInterpolationFiltersProperty")]
    [Spec("filter-effects-1", "#LightingColorProperty")]
    [DataRow("flood-color", "rgb(255, 0, 0)")]
    [DataRow("flood-opacity", "0.5")]
    [DataRow("color-interpolation-filters", "linearRGB")]
    [DataRow("lighting-color", "rgb(255, 0, 0)")]
    public void FilterPrimitiveProperties_AreParsed(string property, string value)
    {
        Assert.AreEqual(value, Value(property, value));
    }

    [Ignore("Gap, see Spec/filter-effects-1.yml")]
    [TestMethod]
    [Spec("filter-effects-1", "#FilterCSSImageValue")]
    public void FilterImage_IsAcceptedAsBackgroundImage()
    {
        Assert.AreNotEqual("", Value("background-image", "filter(url(a.png), blur(2px))"));
    }

    private static string Value(string property, string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {property}: {value}; }}");
        return sheet.Rules.OfType<StyleRule>().Single().Style.GetPropertyValue(property);
    }
}
