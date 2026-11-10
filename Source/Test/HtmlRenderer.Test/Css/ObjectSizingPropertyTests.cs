using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class ObjectSizingPropertyTests
{
    [TestMethod]
    [Spec("css-images-3", "#the-object-fit")]
    public void CssObjectFitNoneLegal()
    {
        var property = ParseDeclaration("object-fit : none");
        Assert.AreEqual("object-fit", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ObjectFitProperty>(property);
        var concrete = (ObjectFitProperty)property;
        Assert.IsFalse(property.IsAnimatable);
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    [Spec("css-images-3", "#the-object-fit")]
    public void ObjectFitScaledownIllegal()
    {
        var property = ParseDeclaration("object-fit : scaledown");
        Assert.AreEqual("object-fit", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ObjectFitProperty>(property);
        var concrete = (ObjectFitProperty)property;
        Assert.IsFalse(property.IsAnimatable);
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-images-3", "#the-object-fit")]
    [Spec("css-images-4", "#the-object-fit")]
    public void ObjectFitScaleDownLegal()
    {
        var property = ParseDeclaration("object-fit : scale-DOWN");
        Assert.AreEqual("object-fit", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ObjectFitProperty>(property);
        var concrete = (ObjectFitProperty)property;
        Assert.IsFalse(property.IsAnimatable);
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("scale-down", concrete.Value);
    }

    [TestMethod]
    [Spec("css-images-3", "#the-object-fit")]
    [Spec("css-images-4", "#the-object-fit")]
    public void CssObjectFitCoverLegal()
    {
        var property = ParseDeclaration("object-fit : cover");
        Assert.AreEqual("object-fit", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ObjectFitProperty>(property);
        var concrete = (ObjectFitProperty)property;
        Assert.IsFalse(property.IsAnimatable);
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("cover", concrete.Value);
    }

    [TestMethod]
    [Spec("css-images-3", "#the-object-fit")]
    public void CssObjectFitContainLegal()
    {
        var property = ParseDeclaration("object-fit : contain");
        Assert.AreEqual("object-fit", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ObjectFitProperty>(property);
        var concrete = (ObjectFitProperty)property;
        Assert.IsFalse(property.IsAnimatable);
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("contain", concrete.Value);
    }

    [TestMethod]
    [Spec("css-images-3", "#the-object-position")]
    public void CssObjectPositionCenterLegal()
    {
        var property = ParseDeclaration("object-position : center");
        Assert.AreEqual("object-position", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ObjectPositionProperty>(property);
        var concrete = (ObjectPositionProperty)property;
        Assert.IsTrue(property.IsAnimatable);
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("center", concrete.Value);
    }

    [TestMethod]
    [Spec("css-images-3", "#the-object-position")]
    public void ObjectPositionTopLeftIllegal()
    {
        var property = ParseDeclaration("object-position : top-left");
        Assert.AreEqual("object-position", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ObjectPositionProperty>(property);
        var concrete = (ObjectPositionProperty)property;
        Assert.IsTrue(property.IsAnimatable);
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-images-3", "#the-object-position")]
    public void ObjectPositionTopLeftLegal()
    {
        var property = ParseDeclaration("object-position : top left");
        Assert.AreEqual("object-position", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ObjectPositionProperty>(property);
        var concrete = (ObjectPositionProperty)property;
        Assert.IsTrue(property.IsAnimatable);
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("top left", concrete.Value);
    }

    [TestMethod]
    [Spec("css-images-3", "#the-object-position")]
    public void CssObjectPosition5050Legal()
    {
        var property = ParseDeclaration("object-position : 50%   50% ");
        Assert.AreEqual("object-position", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ObjectPositionProperty>(property);
        var concrete = (ObjectPositionProperty)property;
        Assert.IsTrue(property.IsAnimatable);
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("50% 50%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-images-3", "#the-object-position")]
    public void CssObjectPositionLeft30Legal()
    {
        var property = ParseDeclaration("object-position : left  30px");
        Assert.AreEqual("object-position", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ObjectPositionProperty>(property);
        var concrete = (ObjectPositionProperty)property;
        Assert.IsTrue(property.IsAnimatable);
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("left 30px", concrete.Value);
    }
}
