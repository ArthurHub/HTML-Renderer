using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class FillPropertyTests
{
    [TestMethod]
    public void FillColorRedLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("fill: red");
        Assert.AreEqual("fill", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FillProperty>(property);
        var concrete = (FillProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(255, 0, 0)", concrete.Value);
    }

    [TestMethod]
    public void FillColorHexLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("fill: #0F0");
        Assert.AreEqual("fill", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FillProperty>(property);
        var concrete = (FillProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(0, 255, 0)", concrete.Value);
    }

    [TestMethod]
    public void FillColorRgbaLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("fill: rgba(1, 1, 1, 0)");
        Assert.AreEqual("fill", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FillProperty>(property);
        var concrete = (FillProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgba(1, 1, 1, 0)", concrete.Value);
    }

    [TestMethod]
    public void FillColorRgbLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("fill: rgb(1, 255, 100)");
        Assert.AreEqual("fill", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FillProperty>(property);
        var concrete = (FillProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(1, 255, 100)", concrete.Value);
    }

    [TestMethod]
    public void FillNoneLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("fill: none");
        Assert.AreEqual("fill", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FillProperty>(property);
        var concrete = (FillProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    public void FillColorRedRedIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("fill: red red");
        Assert.AreEqual("fill", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FillProperty>(property);
        var concrete = (FillProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void FillUrlLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("fill: url(#linear)");
        Assert.AreEqual("fill", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FillProperty>(property);
        var concrete = (FillProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"#linear\")", concrete.Value);
    }

    [TestMethod]
    public void FillOpacitytNumberLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("fill-opacity: 0.5");
        Assert.AreEqual("fill-opacity", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FillOpacityProperty>(property);
        var concrete = (FillOpacityProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0.5", concrete.Value);
    }

    [TestMethod]
    public void FillOpacityNumberNumberIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("fill-opacity: 0.5 0.5");
        Assert.AreEqual("fill-opacity", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FillOpacityProperty>(property);
        var concrete = (FillOpacityProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void FillRuleNonzeroLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("fill-rule: nonzero");
        Assert.AreEqual("fill-rule", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FillRuleProperty>(property);
        var concrete = (FillRuleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("nonzero", concrete.Value);
    }

    [TestMethod]
    public void FillRuleEvenoddLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("fill-rule: evenodd");
        Assert.AreEqual("fill-rule", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FillRuleProperty>(property);
        var concrete = (FillRuleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("evenodd", concrete.Value);
    }

    [TestMethod]
    public void FillRuleNoneIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("fill-rule: none");
        Assert.AreEqual("fill-rule", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FillRuleProperty>(property);
        var concrete = (FillRuleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }
}
