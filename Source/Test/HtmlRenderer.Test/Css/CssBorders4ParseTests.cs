using HtmlRenderer.Test.CssEngineSupport;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class CssBorders4ParseTests
{
    [TestMethod]
    [DataRow("border-block-start-color: red", "border-top-color", "red")]
    [DataRow("border-inline-end-color: red", "border-right-color", "red")]
    [DataRow("border-block-color: red blue", "border-bottom-color", "blue")]
    [Spec("css-borders-4", "#border-color")]
    public void LogicalBorderColor_SetsThePhysicalSide(string declaration, string physical, string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations(declaration);
        var expected = CssConstructionFunctions.ParseDeclarations($"{physical}: {value}");

        Assert.AreEqual(expected.GetPropertyValue(physical), style.GetPropertyValue(physical));
        Assert.AreNotEqual("", style.GetPropertyValue(physical));
    }

    [TestMethod]
    [DataRow("border-inline-start-style: dotted", "border-left-style", "dotted")]
    [DataRow("border-block-style: solid dashed", "border-bottom-style", "dashed")]
    [Spec("css-borders-4", "#border-style")]
    public void LogicalBorderStyle_SetsThePhysicalSide(string declaration, string physical, string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations(declaration);

        Assert.AreEqual(value, style.GetPropertyValue(physical));
    }

    [TestMethod]
    [DataRow("border-block-end-width: 5px", "border-bottom-width", "5px")]
    [DataRow("border-inline-width: 1px 2px", "border-right-width", "2px")]
    [Spec("css-borders-4", "#border-width")]
    public void LogicalBorderWidth_SetsThePhysicalSide(string declaration, string physical, string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations(declaration);

        Assert.AreEqual(value, style.GetPropertyValue(physical));
    }

    [TestMethod]
    [DataRow("border-inline: 2px solid red", "border-right-width", "2px")]
    [DataRow("border-block-start: 3px dotted", "border-top-style", "dotted")]
    [Spec("css-borders-4", "#border-shorthands")]
    public void LogicalBorderShorthands_SetThePhysicalSides(string declaration, string physical, string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations(declaration);

        Assert.AreEqual(value, style.GetPropertyValue(physical));
    }

    [Ignore("Gap, see Spec/css-borders-4.yml")]
    [TestMethod]
    [Spec("css-borders-4", "#border-color")]
    [Spec("css-images-4", "#stripes")]
    public void BorderColor_StripesAreAccepted()
    {
        var property = CssConstructionFunctions.ParseDeclaration("border-top-color: stripes(red, blue)");

        Assert.IsTrue(property.HasValue);
    }

    [Ignore("Gap, see Spec/css-borders-4.yml")]
    [TestMethod]
    [Spec("css-borders-4", "#border-shorthands")]
    [Spec("css-backgrounds-3", "#border-shorthands")]
    public void BorderShorthand_ResetsTheBorderImage()
    {
        var style = CssConstructionFunctions.ParseDeclarations("border-image-source: url(a.png); border: solid red");

        Assert.IsFalse(style.GetPropertyValue("border-image-source").Contains("a.png"));
    }

    [Ignore("Gap, see Spec/css-borders-4.yml")]
    [TestMethod]
    [Spec("css-borders-4", "#border-radius")]
    public void BorderRadiusLonghand_AcceptsTheSlashSyntax()
    {
        var property = CssConstructionFunctions.ParseDeclaration("border-top-left-radius: 10px / 20px");

        Assert.IsTrue(property.HasValue);
    }

    [Ignore("Gap, see Spec/css-borders-4.yml")]
    [TestMethod]
    [DataRow("border-start-start-radius", "border-top-left-radius")]
    [DataRow("border-start-end-radius", "border-top-right-radius")]
    [DataRow("border-end-end-radius", "border-bottom-right-radius")]
    [Spec("css-borders-4", "#border-radius")]
    public void LogicalBorderRadius_SetsThePhysicalCorner(string logical, string physical)
    {
        var style = CssConstructionFunctions.ParseDeclarations($"{logical}: 5px");

        Assert.AreEqual("5px", style.GetPropertyValue(physical));
    }

    [Ignore("Gap, see Spec/css-borders-4.yml")]
    [TestMethod]
    [DataRow("border-top-radius: 5px 10px", "border-top-right-radius", "10px")]
    [DataRow("border-left-radius: 5px 10px", "border-bottom-left-radius", "10px")]
    [Spec("css-borders-4", "#corner-sizing-side-shorthands")]
    public void SideRadiusShorthands_SetBothCorners(string declaration, string physical, string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations(declaration);

        Assert.AreEqual(value, style.GetPropertyValue(physical));
    }

    [Ignore("Gap, see Spec/css-borders-4.yml")]
    [TestMethod]
    [DataRow("corner-top-left-shape: bevel")]
    [DataRow("corner-top-left-shape: superellipse(2)")]
    [DataRow("corner-shape: scoop notch")]
    [DataRow("corner: 10px squircle")]
    [Spec("css-borders-4", "#corner-shaping")]
    [Spec("css-borders-4", "#corner-shaping-shorthand")]
    [Spec("css-borders-4", "#corner-shorthand")]
    public void CornerShape_IsAccepted(string declaration)
    {
        var property = CssConstructionFunctions.ParseDeclaration(declaration);

        Assert.IsTrue(property.HasValue);
    }

    [Ignore("Gap, see Spec/css-borders-4.yml")]
    [TestMethod]
    [DataRow("box-shadow-color: red")]
    [DataRow("box-shadow-offset: 5px 5px")]
    [DataRow("box-shadow-blur: 2px")]
    [DataRow("box-shadow-spread: 1px")]
    [DataRow("box-shadow-position: inset")]
    [Spec("css-borders-4", "#box-shadow-color")]
    [Spec("css-borders-4", "#box-shadow-offset")]
    [Spec("css-borders-4", "#box-shadow-blur")]
    [Spec("css-borders-4", "#box-shadow-spread")]
    [Spec("css-borders-4", "#box-shadow-position")]
    public void BoxShadowLonghands_AreAccepted(string declaration)
    {
        var property = CssConstructionFunctions.ParseDeclaration(declaration);

        Assert.IsTrue(property.HasValue);
    }

    [Ignore("Gap, see Spec/css-borders-4.yml")]
    [TestMethod]
    [Spec("css-borders-4", "#border-shape-func")]
    public void BorderShape_IsAccepted()
    {
        var property = CssConstructionFunctions.ParseDeclaration("border-shape: circle(50%)");

        Assert.IsTrue(property.HasValue);
    }
}
