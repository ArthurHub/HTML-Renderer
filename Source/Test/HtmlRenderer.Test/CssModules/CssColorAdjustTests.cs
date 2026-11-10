using HtmlRenderer.Test.CssEngineSupport;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssColorAdjustTests
{
    [Ignore("Gap, see Spec/css-color-adjust-1.yml")]
    [TestMethod]
    [DataRow("normal")]
    [DataRow("light dark")]
    [DataRow("only light")]
    [DataRow("dark future-scheme")]
    [Spec("css-color-adjust-1", "#color-scheme-prop")]
    public void ColorScheme_IsAccepted(string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations($"color-scheme: {value}");

        Assert.AreEqual(value, style.GetPropertyValue("color-scheme"));
    }

    [Ignore("Gap, see Spec/css-color-adjust-1.yml")]
    [TestMethod]
    [DataRow("forced-color-adjust", "preserve-parent-color")]
    [Spec("css-color-adjust-1", "#forced-color-adjust-prop")]
    public void ForcedColorAdjust_IsAccepted(string property, string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations($"{property}: {value}");

        Assert.AreEqual(value, style.GetPropertyValue(property));
    }

    [Ignore("Gap, see Spec/css-color-adjust-1.yml")]
    [TestMethod]
    [DataRow("print-color-adjust: exact", "print-color-adjust", "exact")]
    [DataRow("color-adjust: exact", "print-color-adjust", "exact")]
    [Spec("css-color-adjust-1", "#print-color-adjust")]
    [Spec("css-color-adjust-1", "#color-adjust")]
    public void PrintColorAdjust_IsAccepted(string declaration, string property, string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations(declaration);

        Assert.AreEqual(value, style.GetPropertyValue(property));
    }
}
