using HtmlRenderer.Test.CssEngineSupport;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssShapesTests
{
    [Ignore("Gap, see Spec/css-shapes-1.yml")]
    [TestMethod]
    [DataRow("shape-outside", "circle(50%)")]
    [DataRow("shape-outside", "inset(10px round 5px) margin-box")]
    [DataRow("shape-outside", "url(shape.png)")]
    [DataRow("shape-margin", "10px")]
    [DataRow("shape-image-threshold", "0.5")]
    [Spec("css-shapes-1", "#shape-outside-property")]
    [Spec("css-shapes-1", "#shapes-from-box-values")]
    [Spec("css-shapes-1", "#shapes-from-image")]
    [Spec("css-shapes-1", "#shape-margin-property")]
    [Spec("css-shapes-1", "#shape-image-threshold-property")]
    public void ShapeProperties_AreAccepted(string property, string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations($"{property}: {value}");

        Assert.AreNotEqual("", style.GetPropertyValue(property));
    }
}
