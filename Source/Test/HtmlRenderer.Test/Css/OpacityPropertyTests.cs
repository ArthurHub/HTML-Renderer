using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class OpacityPropertyTests
{
    [TestMethod]
    public void OpacityPercentLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("opacity: 50%");
        Assert.AreEqual("opacity", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OpacityProperty>(property);
        var result = (OpacityProperty)property;
        Assert.IsFalse(result.IsInherited);
        Assert.IsTrue(result.HasValue);
        Assert.AreEqual("50%", result.Value);
    }

    [TestMethod]
    public void OpacityVarianceTests()
    {
        var property = CssConstructionFunctions.ParseDeclaration("opacity: 50%");
        var result = (OpacityProperty)property;
        Assert.AreEqual("50%", result.Value);

        property = CssConstructionFunctions.ParseDeclaration("opacity: 0.4");
        result = (OpacityProperty)property;
        Assert.AreEqual("0.4", result.Value);

        property = CssConstructionFunctions.ParseDeclaration("opacity: .50");
        result = (OpacityProperty)property;
        Assert.AreEqual("0.5", result.Value);

        property = CssConstructionFunctions.ParseDeclaration("opacity: 1");
        result = (OpacityProperty)property;
        Assert.AreEqual("1", result.Value);

        property = CssConstructionFunctions.ParseDeclaration("opacity: 0");
        result = (OpacityProperty)property;
        Assert.AreEqual("0", result.Value);
    }
}
