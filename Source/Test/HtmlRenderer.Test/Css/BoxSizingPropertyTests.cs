using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class BoxSizingPropertyTests
{
    [TestMethod]
    [Spec("css-sizing-3", "#box-sizing")]
    public void BoxSizingContentBoxLegal()
    {
        var snippet = "box-sizing: content-box";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("box-sizing", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxSizingProperty>(property);
        var concrete = (BoxSizingProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("content-box", concrete.Value);
    }

    [TestMethod]
    [Spec("css-sizing-3", "#box-sizing")]
    public void BoxSizingBorderBoxLegal()
    {
        var snippet = "box-sizing: border-box";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("box-sizing", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxSizingProperty>(property);
        var concrete = (BoxSizingProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("border-box", concrete.Value);
    }
}
