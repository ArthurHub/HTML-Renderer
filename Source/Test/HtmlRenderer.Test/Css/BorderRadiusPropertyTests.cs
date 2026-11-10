using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class BorderRadiusPropertyTests
{
    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderBottomLeftRadiusPxPxLegal()
    {
        var snippet = "border-bottom-left-radius: 40px  40px";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-bottom-left-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderBottomLeftRadiusProperty>(property);
        var concrete = (BorderBottomLeftRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("40px 40px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderBottomLeftRadiusPxEmLegal()
    {
        var snippet = "border-bottom-left-radius  : 40px 20em";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-bottom-left-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderBottomLeftRadiusProperty>(property);
        var concrete = (BorderBottomLeftRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("40px 20em", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderBottomLeftRadiusPxPercentLegal()
    {
        var snippet = "border-bottom-left-radius: 10px 5%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-bottom-left-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderBottomLeftRadiusProperty>(property);
        var concrete = (BorderBottomLeftRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("10px 5%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderBottomLeftRadiusPercentLegal()
    {
        var snippet = "border-bottom-left-radius: 10%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-bottom-left-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderBottomLeftRadiusProperty>(property);
        var concrete = (BorderBottomLeftRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("10%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderBottomRightRadiusZeroLegal()
    {
        var snippet = "border-bottom-right-radius: 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-bottom-right-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderBottomRightRadiusProperty>(property);
        var concrete = (BorderBottomRightRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderBottomRightRadiusPxLegal()
    {
        var snippet = "border-bottom-right-radius: 20px";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-bottom-right-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderBottomRightRadiusProperty>(property);
        var concrete = (BorderBottomRightRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("20px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderTopLeftRadiusCmLegal()
    {
        var snippet = "border-top-left-radius: 3.5cm";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-top-left-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderTopLeftRadiusProperty>(property);
        var concrete = (BorderTopLeftRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("3.5cm", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderTopRightRadiusPercentPercentLegal()
    {
        var snippet = "border-top-right-radius: 15% 3.5%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-top-right-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderTopRightRadiusProperty>(property);
        var concrete = (BorderTopRightRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("15% 3.5%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadiusPercentPercentLegal()
    {
        var snippet = "border-radius: 15% 3.5%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderRadiusProperty>(property);
        var concrete = (BorderRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("15% 3.5%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadiusZeroLegal()
    {
        var snippet = "border-radius: 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderRadiusProperty>(property);
        var concrete = (BorderRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadiusThreeLengthsLegal()
    {
        var snippet = "border-radius: 2px 4px 3px";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderRadiusProperty>(property);
        var concrete = (BorderRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("2px 4px 3px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadiusFourLengthsLegal()
    {
        var snippet = "border-radius: 2px 4px 3px 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderRadiusProperty>(property);
        var concrete = (BorderRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("2px 4px 3px 0", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadiusFiveLengthsIllegal()
    {
        var snippet = "border-radius: 2px 4px 3px 0 1px";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderRadiusProperty>(property);
        var concrete = (BorderRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadiusLengthFractionLegal()
    {
        var snippet = "border-radius: 1em/5em";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderRadiusProperty>(property);
        var concrete = (BorderRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("1em / 5em", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadiusLengthFractionInbalancedLegal()
    {
        var snippet = "border-radius: 4px 3px 6px / 2px 4px";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderRadiusProperty>(property);
        var concrete = (BorderRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("4px 3px 6px / 2px 4px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadiusFullFractionLegal()
    {
        var snippet = "border-radius: 4px 3px 6px 1em / 2px 4px 0 20%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderRadiusProperty>(property);
        var concrete = (BorderRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("4px 3px 6px 1em / 2px 4px 0 20%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadiusFiveTailFractionIllegal()
    {
        var snippet = "border-radius: 4px 3px 6px 1em / 2px 4px 0 20% 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderRadiusProperty>(property);
        var concrete = (BorderRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadiusFiveHeadFractionIllegal()
    {
        var snippet = "border-radius: 4px 3px 6px 1em 0 / 2px 4px 0 20%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-radius", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderRadiusProperty>(property);
        var concrete = (BorderRadiusProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadiusCircleShouldBeExpandedAndRecombinedCorrectly()
    {
        var snippet = ".centered { border-radius: 5px; }";
        var expected = ".centered { border-radius: 5px }";
        var result = CssConstructionFunctions.ParseRule(snippet);
        var actual = result.Text;
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadiusEllipseShouldBeExpandedAndRecombinedCorrectly()
    {
        var snippet = ".centered { border-radius: 5px/3px; }";
        var expected = ".centered { border-radius: 5px / 3px }";
        var result = CssConstructionFunctions.ParseRule(snippet);
        var actual = result.Text;
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadiusSimplificationShouldWork()
    {
        var snippet = ".centered { border-top-left-radius: 0 1px; border-bottom-left-radius: 1px 2px; border-top-right-radius: 0 3px; border-bottom-right-radius: 1px 4px; }";
        var expected = ".centered { border-radius: 0 0 1px 1px / 1px 3px 4px 2px }";
        var result = CssConstructionFunctions.ParseRule(snippet);
        var actual = result.Text;
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadiusRecombinationAndReductionCheck()
    {
        var snippet = ".centered { border-top-left-radius: 0 1px; border-bottom-left-radius: 0 1px; border-top-right-radius: 1px 1px; border-bottom-right-radius: 0 1px; }";
        var expected = ".centered { border-radius: 0 1px 0 0 / 1px }";
        var result = CssConstructionFunctions.ParseRule(snippet);
        var actual = result.Text;
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadiusPureCircleRecombination()
    {
        var snippet = ".test { border-top-left-radius:15px;border-bottom-left-radius:15px;border-bottom-right-radius:0;border-top-right-radius:0;}";
        var expected = ".test { border-radius: 15px 0 0 15px }";
        var result = CssConstructionFunctions.ParseRule(snippet);
        var actual = result.Text;
        Assert.AreEqual(expected, actual);
    }
}
