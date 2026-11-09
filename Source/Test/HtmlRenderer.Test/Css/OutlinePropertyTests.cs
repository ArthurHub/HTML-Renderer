using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class OutlinePropertyTests
{
    [TestMethod]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void CssOutlineStyleDottedLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline-style   :  dotTED");
        Assert.AreEqual("outline-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OutlineStyleProperty>(property);
        var concrete = (OutlineStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("dotted", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void CssOutlineStyleSolidLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline-style   :  solid");
        Assert.AreEqual("outline-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OutlineStyleProperty>(property);
        var concrete = (OutlineStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("solid", concrete.Value);
    }

    [TestMethod]
    public void CssOutlineStyleNoIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline-style   :  no");
        Assert.AreEqual("outline-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OutlineStyleProperty>(property);
        var concrete = (OutlineStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void CssOutlineStyleHiddenIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline-style : hidden");
        Assert.IsInstanceOfType<OutlineStyleProperty>(property);
        Assert.IsFalse(((OutlineStyleProperty)property).HasValue);
    }

    [TestMethod]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void CssOutlineColorInvertLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline-color :  invert ");
        Assert.AreEqual("outline-color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OutlineColorProperty>(property);
        var concrete = (OutlineColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("invert", concrete.Value);
    }

    [TestMethod]
    public void CssOutlineColorHslLegal()
    {
        // hsl(320, 80%, 50%) is equivalent to rgba(229, 26, 161, 1)
        var property = CssConstructionFunctions.ParseDeclaration("outline-color :  hsl(320, 80%, 50%) ");
        Assert.AreEqual("outline-color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OutlineColorProperty>(property);
        var concrete = (OutlineColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("hsl(320deg, 80%, 50%)", concrete.Value);
    }

    [TestMethod]
    public void CssOutlineColorHexLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline-color :  #0000FF ");
        Assert.AreEqual("outline-color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OutlineColorProperty>(property);
        var concrete = (OutlineColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(0, 0, 255)", concrete.Value);
    }

    [TestMethod]
    public void CssOutlineColorRedLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline-color :  red ");
        Assert.AreEqual("outline-color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OutlineColorProperty>(property);
        var concrete = (OutlineColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(255, 0, 0)", concrete.Value);
    }

    [TestMethod]
    public void CssOutlineColorIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline-color :  blau ");
        Assert.AreEqual("outline-color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OutlineColorProperty>(property);
        var concrete = (OutlineColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssOutlineWidthThinImportantLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline-width :  thin !important");
        Assert.AreEqual("outline-width", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<OutlineWidthProperty>(property);
        var concrete = (OutlineWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("1px", concrete.Value);
    }

    [TestMethod]
    public void CssOutlineWidthNumberIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline-width :  3");
        Assert.AreEqual("outline-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OutlineWidthProperty>(property);
        var concrete = (OutlineWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssOutlineWidthLengthLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline-width :  0.1em");
        Assert.AreEqual("outline-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OutlineWidthProperty>(property);
        var concrete = (OutlineWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0.1em", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void CssOutlineSingleLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline :  thin");
        Assert.AreEqual("outline", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OutlineProperty>(property);
        var concrete = (OutlineProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("1px", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void CssOutlineDualLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline :  thin   invert");
        Assert.AreEqual("outline", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OutlineProperty>(property);
        var concrete = (OutlineProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("1px invert", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void CssOutlineAllDottedLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline :  dotted 0.3em rgb(255, 255, 255)");
        Assert.AreEqual("outline", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OutlineProperty>(property);
        var concrete = (OutlineProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0.3em dotted rgb(255, 255, 255)", concrete.Value);
    }

    [TestMethod]
    public void CssOutlineDoubleColorIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline :  dotted #123456 rgb(255, 255, 255)");
        Assert.AreEqual("outline", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OutlineProperty>(property);
        var concrete = (OutlineProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void CssOutlineAllSolidLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline :  1px solid #000");
        Assert.AreEqual("outline", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OutlineProperty>(property);
        var concrete = (OutlineProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("1px solid rgb(0, 0, 0)", concrete.Value);
    }

    [TestMethod]
    public void CssOutlineAllColorNamedLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline :  solid black 1px");
        Assert.AreEqual("outline", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OutlineProperty>(property);
        var concrete = (OutlineProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("1px solid rgb(0, 0, 0)", concrete.Value);
    }
}
