using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class StrokePropertyTests
{
    [TestMethod]
    public void StrokeColorRedLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke: red");
        Assert.AreEqual("stroke", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeProperty>(property);
        var concrete = (StrokeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(255, 0, 0)", concrete.Value);
    }

    [TestMethod]
    public void StrokeColorHexLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke: #0F0");
        Assert.AreEqual("stroke", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeProperty>(property);
        var concrete = (StrokeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(0, 255, 0)", concrete.Value);
    }

    [TestMethod]
    public void StrokeColorRgbaLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke: rgba(1, 1, 1, 0)");
        Assert.AreEqual("stroke", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeProperty>(property);
        var concrete = (StrokeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgba(1, 1, 1, 0)", concrete.Value);
    }

    [TestMethod]
    public void StrokeColorRgbLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke: rgb(1, 255, 100)");
        Assert.AreEqual("stroke", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeProperty>(property);
        var concrete = (StrokeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(1, 255, 100)", concrete.Value);
    }

    [TestMethod]
    public void StrokeNoneLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke: none");
        Assert.AreEqual("stroke", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeProperty>(property);
        var concrete = (StrokeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    public void StrokeColorRedRedIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke: red red");
        Assert.AreEqual("stroke", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeProperty>(property);
        var concrete = (StrokeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void StrokeUrlLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke: url(#linear)");
        Assert.AreEqual("stroke", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeProperty>(property);
        var concrete = (StrokeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"#linear\")", concrete.Value);
    }

    [TestMethod]
    public void StrokeDasharrayNumberNumberLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-dasharray: 5 5");
        Assert.AreEqual("stroke-dasharray", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeDasharrayProperty>(property);
        var concrete = (StrokeDasharrayProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("5 5", concrete.Value);
    }

    [TestMethod]
    public void StrokeDasharrayLengthLengthLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-dasharray: 5px 5em");
        Assert.AreEqual("stroke-dasharray", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeDasharrayProperty>(property);
        var concrete = (StrokeDasharrayProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("5px 5em", concrete.Value);
    }

    [TestMethod]
    public void StrokeDasharrayManyLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-dasharray: 1px 2em 3vh 4vw 5 6");
        Assert.AreEqual("stroke-dasharray", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeDasharrayProperty>(property);
        var concrete = (StrokeDasharrayProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("1px 2em 3vh 4vw 5 6", concrete.Value);
    }

    [TestMethod]
    public void StrokeDasharrayNoneLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-dasharray: none");
        Assert.AreEqual("stroke-dasharray", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeDasharrayProperty>(property);
        var concrete = (StrokeDasharrayProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    public void StrokeDashoffsetLengthLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-dashoffset: 5px");
        Assert.AreEqual("stroke-dashoffset", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeDashoffsetProperty>(property);
        var concrete = (StrokeDashoffsetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("5px", concrete.Value);
    }

    [TestMethod]
    public void StrokeDashoffsetLengthLengthIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-dashoffset: 5px 5px");
        Assert.AreEqual("stroke-dashoffset", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeDashoffsetProperty>(property);
        var concrete = (StrokeDashoffsetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void StrokeDashoffsetPercentLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-dashoffset: 50%");
        Assert.AreEqual("stroke-dashoffset", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeDashoffsetProperty>(property);
        var concrete = (StrokeDashoffsetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("50%", concrete.Value);
    }

    [TestMethod]
    public void StrokeDashoffsetPercentPercentIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-dashoffset: 50% 25%");
        Assert.AreEqual("stroke-dashoffset", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeDashoffsetProperty>(property);
        var concrete = (StrokeDashoffsetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void StrokeLinecapButtLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-linecap: butt");
        Assert.AreEqual("stroke-linecap", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeLinecapProperty>(property);
        var concrete = (StrokeLinecapProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("butt", concrete.Value);
    }

    [TestMethod]
    public void StrokeLinecapRoundLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-linecap: round");
        Assert.AreEqual("stroke-linecap", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeLinecapProperty>(property);
        var concrete = (StrokeLinecapProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("round", concrete.Value);
    }

    [TestMethod]
    public void StrokeLinecapSquareLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-linecap: square");
        Assert.AreEqual("stroke-linecap", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeLinecapProperty>(property);
        var concrete = (StrokeLinecapProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("square", concrete.Value);
    }

    [TestMethod]
    public void StrokeLinecapNoneIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-linecap: none");
        Assert.AreEqual("stroke-linecap", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeLinecapProperty>(property);
        var concrete = (StrokeLinecapProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void StrokeLinejoinMiterLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-linejoin: miter");
        Assert.AreEqual("stroke-linejoin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeLinejoinProperty>(property);
        var concrete = (StrokeLinejoinProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("miter", concrete.Value);
    }

    [TestMethod]
    public void StrokeLinejoinRoundLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-linejoin: round");
        Assert.AreEqual("stroke-linejoin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeLinejoinProperty>(property);
        var concrete = (StrokeLinejoinProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("round", concrete.Value);
    }

    [TestMethod]
    public void StrokeLinejoinBevelLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-linejoin: bevel");
        Assert.AreEqual("stroke-linejoin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeLinejoinProperty>(property);
        var concrete = (StrokeLinejoinProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("bevel", concrete.Value);
    }

    [TestMethod]
    public void StrokeLinejoinNoneIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-linejoin: none");
        Assert.AreEqual("stroke-linejoin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeLinejoinProperty>(property);
        var concrete = (StrokeLinejoinProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void StrokeMiterlimitNumberLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-miterlimit: 2");
        Assert.AreEqual("stroke-miterlimit", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeMiterlimitProperty>(property);
        var concrete = (StrokeMiterlimitProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("2", concrete.Value);
    }

    [TestMethod]
    public void StrokeMiterlimitNumberIlegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-miterlimit: 0.5");
        Assert.AreEqual("stroke-miterlimit", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeMiterlimitProperty>(property);
        var concrete = (StrokeMiterlimitProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void StrokeMiterlimitNumberNumberIlegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-miterlimit: 2 0.5");
        Assert.AreEqual("stroke-miterlimit", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeMiterlimitProperty>(property);
        var concrete = (StrokeMiterlimitProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void StrokeOpacitytNumberLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-opacity: 0.5");
        Assert.AreEqual("stroke-opacity", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeOpacityProperty>(property);
        var concrete = (StrokeOpacityProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0.5", concrete.Value);
    }

    [TestMethod]
    public void StrokeOpacityNumberNumberIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-opacity: 0.5 0.5");
        Assert.AreEqual("stroke-opacity", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeOpacityProperty>(property);
        var concrete = (StrokeOpacityProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void StrokeWidthLengthLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-width: 5px");
        Assert.AreEqual("stroke-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeWidthProperty>(property);
        var concrete = (StrokeWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("5px", concrete.Value);
    }

    [TestMethod]
    public void StrokeWidthPercentLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-width: 5%");
        Assert.AreEqual("stroke-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeWidthProperty>(property);
        var concrete = (StrokeWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("5%", concrete.Value);
    }

    [TestMethod]
    public void StrokeWidthNoneIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("stroke-width: none");
        Assert.AreEqual("stroke-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StrokeWidthProperty>(property);
        var concrete = (StrokeWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }
}
