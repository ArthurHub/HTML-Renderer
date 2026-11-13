using HtmlRenderer.Test.CssEngineSupport;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssColorAdjustTests
{
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

    [TestMethod]
    [DataRow("forced-color-adjust", "preserve-parent-color")]
    [Spec("css-color-adjust-1", "#forced-color-adjust-prop")]
    public void ForcedColorAdjust_IsAccepted(string property, string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations($"{property}: {value}");

        Assert.AreEqual(value, style.GetPropertyValue(property));
    }

    [TestMethod]
    [Spec("css-color-adjust-1", "#print-color-adjust")]
    [DataRow("print-color-adjust: exact", "print-color-adjust", "exact")]
    public void PrintColorAdjust_IsAccepted(string declaration, string property, string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations(declaration);

        Assert.AreEqual(value, style.GetPropertyValue(property));
    }

    [Ignore("Gap, see Spec/css-color-adjust-1.yml")]
    [TestMethod]
    [Spec("css-color-adjust-1", "#color-adjust")]
    [DataRow("color-adjust: exact", "print-color-adjust", "exact")]
    public void ColorAdjust_IsAnAliasOfPrintColorAdjust(string declaration, string property, string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations(declaration);

        Assert.AreEqual(value, style.GetPropertyValue(property));
    }
}
