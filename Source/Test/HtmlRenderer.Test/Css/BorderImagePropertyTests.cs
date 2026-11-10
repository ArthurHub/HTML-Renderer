using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class BorderImagePropertyTests
{
    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-source")]
    public void BorderImageSourceNoneLegal()
    {
        var snippet = "border-image-source: none    ";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-source", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageSourceProperty>(property);
        var concrete = (BorderImageSourceProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-source")]
    [Spec("css-borders-4", "#border-image-source")]
    public void BorderImageSourceUrlLegal()
    {
        var snippet = "border-image-source: url(image.jpg)";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-source", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageSourceProperty>(property);
        var concrete = (BorderImageSourceProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"image.jpg\")", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-source")]
    public void BorderImageSourceLinearGradientLegal()
    {
        var snippet = "border-image-source: linear-gradient(to top, red, yellow)";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-source", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageSourceProperty>(property);
        var concrete = (BorderImageSourceProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("linear-gradient(to top, rgb(255, 0, 0), rgb(255, 255, 0))", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-outset")]
    [Spec("css-borders-4", "#border-image-outset")]
    public void BorderImageOutsetZeroLegal()
    {
        var snippet = "border-image-outset: 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-outset", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageOutsetProperty>(property);
        var concrete = (BorderImageOutsetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-outset")]
    public void BorderImageOutsetLengthPercentLegal()
    {
        var snippet = "border-image-outset: 10px   25%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-outset", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageOutsetProperty>(property);
        var concrete = (BorderImageOutsetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("10px 25%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-outset")]
    public void BorderImageOutsetLengthPercentZeroLegal()
    {
        var snippet = "border-image-outset: 10px   25% 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-outset", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageOutsetProperty>(property);
        var concrete = (BorderImageOutsetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("10px 25% 0", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-outset")]
    public void BorderImageOutsetLengthPercentZeroPercentLegal()
    {
        var snippet = "border-image-outset: 10px   25% 0 10%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-outset", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageOutsetProperty>(property);
        var concrete = (BorderImageOutsetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("10px 25% 0 10%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-outset")]
    public void BorderImageOutsetZerosIllegal()
    {
        var snippet = "border-image-outset: 0 0 0 0 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-outset", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageOutsetProperty>(property);
        var concrete = (BorderImageOutsetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-width")]
    public void BorderImageWidthZeroLegal()
    {
        var snippet = "border-image-width: 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageWidthProperty>(property);
        var concrete = (BorderImageWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-width")]
    [Spec("css-borders-4", "#border-image-width")]
    public void BorderImageWidthAutoLegal()
    {
        var snippet = "border-image-width: auto";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageWidthProperty>(property);
        var concrete = (BorderImageWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("auto", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-width")]
    public void BorderImageWidthMultipleLegal()
    {
        var snippet = "border-image-width: 5";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageWidthProperty>(property);
        var concrete = (BorderImageWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("5", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-width")]
    public void BorderImageWidthLengthPercentLegal()
    {
        var snippet = "border-image-width: 10px   25%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageWidthProperty>(property);
        var concrete = (BorderImageWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("10px 25%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-width")]
    public void BorderImageWidthLengthPercentZeroLegal()
    {
        var snippet = "border-image-width: 10px   25% 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageWidthProperty>(property);
        var concrete = (BorderImageWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("10px 25% 0", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-width")]
    public void BorderImageWidthLengthPercentAutoPercentLegal()
    {
        var snippet = "border-image-width: 10px   25% auto 10%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageWidthProperty>(property);
        var concrete = (BorderImageWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("10px 25% auto 10%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-width")]
    public void BorderImageWidthZerosIllegal()
    {
        var snippet = "border-image-width: 0 0 0 0 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageWidthProperty>(property);
        var concrete = (BorderImageWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-repeat")]
    public void BorderImageRepeatStretchUppercaseLegal()
    {
        var snippet = "border-image-repeat:   StRETCH";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-repeat", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageRepeatProperty>(property);
        var concrete = (BorderImageRepeatProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("stretch", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-repeat")]
    public void BorderImageRepeatRepeatLegal()
    {
        var snippet = "border-image-repeat:   repeat";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-repeat", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageRepeatProperty>(property);
        var concrete = (BorderImageRepeatProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("repeat", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-repeat")]
    [Spec("css-borders-4", "#border-image-repeat")]
    public void BorderImageRepeatRoundLegal()
    {
        var snippet = "border-image-repeat:   round";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-repeat", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageRepeatProperty>(property);
        var concrete = (BorderImageRepeatProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("round", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-repeat")]
    public void BorderImageRepeatStretchRoundLegal()
    {
        var snippet = "border-image-repeat: stretch round";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-repeat", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageRepeatProperty>(property);
        var concrete = (BorderImageRepeatProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("stretch round", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-repeat")]
    public void BorderImageRepeatNoRepeatIllegal()
    {
        var snippet = "border-image-repeat: no-repeat";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-repeat", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageRepeatProperty>(property);
        var concrete = (BorderImageRepeatProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-slice")]
    public void BorderImageSlicePixelsLegal()
    {
        var snippet = "border-image-slice: 3";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-slice", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageSliceProperty>(property);
        var concrete = (BorderImageSliceProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("3", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-slice")]
    public void BorderImageSlicePercentLegal()
    {
        var snippet = "border-image-slice: 10%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-slice", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageSliceProperty>(property);
        var concrete = (BorderImageSliceProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("10%", concrete.Value);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [Spec("css-backgrounds-3", "#border-image-slice")]
    [Spec("css-borders-4", "#border-image-slice")]
    public void BorderImageSliceFillIllegal()
    {
        var snippet = "border-image-slice: fill";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-slice", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageSliceProperty>(property);
        var concrete = (BorderImageSliceProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-slice")]
    public void BorderImageSlicePercentFillLegal()
    {
        var snippet = "border-image-slice: 10% fill";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-slice", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageSliceProperty>(property);
        var concrete = (BorderImageSliceProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("10% fill", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-slice")]
    public void BorderImageSlicePercentPixelsFillLegal()
    {
        var snippet = "border-image-slice: 10% 30 fill";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-slice", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageSliceProperty>(property);
        var concrete = (BorderImageSliceProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("10% 30 fill", concrete.Value);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [Spec("css-backgrounds-3", "#border-image-slice")]
    public void BorderImageSliceFillBetweenNumbersIllegal()
    {
        var snippet = "border-image-slice: 10% 30 fill 0 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-slice", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageSliceProperty>(property);
        var concrete = (BorderImageSliceProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-slice")]
    public void BorderImageSlicePercentPixelsFillZerosIllegal()
    {
        var snippet = "border-image-slice: 10% 30 fill 0 0 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-slice", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageSliceProperty>(property);
        var concrete = (BorderImageSliceProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image-slice")]
    public void BorderImageSlicePercentPixelsZerosFillIllegal()
    {
        var snippet = "border-image-slice: 10% 30  0 0 0 fill";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image-slice", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageSliceProperty>(property);
        var concrete = (BorderImageSliceProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image")]
    public void BorderImageNoneLegal()
    {
        var snippet = "border-image: none    ";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageProperty>(property);
        var concrete = (BorderImageProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image")]
    [Spec("css-borders-4", "#border-image")]
    public void BorderImageUrlOffsetLegal()
    {
        var snippet = "border-image: url(image.png) 50 50";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageProperty>(property);
        var concrete = (BorderImageProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"image.png\") 50 50", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image")]
    public void BorderImageUrlOffsetRepeatLegal()
    {
        var snippet = "border-image: url(image.png) 30 30 repeat";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageProperty>(property);
        var concrete = (BorderImageProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"image.png\") 30 30 repeat", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image")]
    public void BorderImageUrlStretchUppercaseLegal()
    {
        var snippet = "border-image: url(image.png) STRETCH";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageProperty>(property);
        var concrete = (BorderImageProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"image.png\") stretch", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image")]
    public void BorderImageUrlOffsetWidthTwoLegal()
    {
        var snippet = "border-image: url(image.png) 30 30 / 15px 15px";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageProperty>(property);
        var concrete = (BorderImageProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"image.png\") 30 30 / 15px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image")]
    public void BorderImageUrlOffsetWidthFourLegal()
    {
        var snippet = "border-image: url(image.png) 30 30 0 10 / 15px 0 15px 2em";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageProperty>(property);
        var concrete = (BorderImageProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"image.png\") 30 30 0 10 / 15px 0 15px 2em", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#border-image")]
    public void BorderImageUrlOffsetWidthOutsetLegal()
    {
        var snippet = "border-image: url(image.png) 30 30 / 15px 15px / 5% 2% 0 10%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("border-image", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BorderImageProperty>(property);
        var concrete = (BorderImageProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"image.png\") 30 30 / 15px / 5% 2% 0 10%", concrete.Value);
    }
}
