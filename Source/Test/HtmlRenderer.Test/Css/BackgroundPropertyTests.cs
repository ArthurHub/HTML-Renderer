using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class BackgroundPropertyTests
{
    [TestMethod]
    [Spec("css-backgrounds-3", "#background-attachment")]
    public void BackgroundAttachmentScrollLegal()
    {
        var snippet = "background-attachment : scroll";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-attachment", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundAttachmentProperty>(property);
        var concrete = (BackgroundAttachmentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("scroll", concrete.Value);
    }

    [TestMethod]
    public void BackgroundAttachmentInitialLegal()
    {
        var snippet = "background-attachment : initial";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-attachment", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundAttachmentProperty>(property);
        var concrete = (BackgroundAttachmentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("initial", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-attachment")]
    public void BackgroundAttachmentFixedUppercaseLegal()
    {
        var snippet = "background-attachment : Fixed ";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-attachment", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundAttachmentProperty>(property);
        var concrete = (BackgroundAttachmentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("fixed", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-attachment")]
    public void BackgroundAttachmentFixedLocalLegal()
    {
        var snippet = "background-attachment : fixed  ,  local ";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-attachment", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundAttachmentProperty>(property);
        var concrete = (BackgroundAttachmentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("fixed, local", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-attachment")]
    public void BackgroundAttachmentFixedLocalScrollScrollLegal()
    {
        var snippet = "background-attachment : fixed  ,  local,scroll,scroll ";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-attachment", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundAttachmentProperty>(property);
        var concrete = (BackgroundAttachmentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("fixed, local, scroll, scroll", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-attachment")]
    public void BackgroundAttachmentNoneIllegal()
    {
        var snippet = "background-attachment : none ";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-attachment", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundAttachmentProperty>(property);
        var concrete = (BackgroundAttachmentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-clip")]
    public void BackgroundClipPaddingBoxUppercaseLegal()
    {
        var snippet = "background-clip : Padding-Box ";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-clip", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundClipProperty>(property);
        var concrete = (BackgroundClipProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("padding-box", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-clip")]
    public void BackgroundClipPaddingBoxBorderBoxLegal()
    {
        var snippet = "background-clip : Padding-Box, border-box ";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-clip", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundClipProperty>(property);
        var concrete = (BackgroundClipProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("padding-box, border-box", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-clip")]
    public void BackgroundClipContentBoxLegal()
    {
        var snippet = "background-clip : content-box";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-clip", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundClipProperty>(property);
        var concrete = (BackgroundClipProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("content-box", concrete.Value);
    }

    [TestMethod]
    public void BackgroundColorTealLegal()
    {
        var snippet = "background-color : teal";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundColorProperty>(property);
        var concrete = (BackgroundColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(0, 128, 128)", concrete.Value);
    }

    [TestMethod]
    [DataRow("background-color: rgb(255, 255, 128)", "rgb(255, 255, 128)")]
    [DataRow("background-color: hsla(50, 33%, 25%, 0.75)", "hsla(50deg, 33%, 25%, 0.75)")]
    [DataRow("background-color : rgb(255  ,  255  ,  128)", "rgb(255, 255, 128)")]
    [DataRow("background-color: Transparent", "rgba(0, 0, 0, 0)")]
    [DataRow("background-color: #F09", "rgb(255, 0, 153)")]
    [DataRow("background-color: #F09F", "rgb(255, 0, 153)")]
    [DataRow("background-color: #AABBCC", "rgb(170, 187, 204)")]
    [DataRow("background-color: #AABBCC11", "rgba(170, 187, 204, 0.07)")]
    public void BackgroundColorRgbLegal(string snippet, string expectedValue)
    {
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundColorProperty>(property);
        var concrete = (BackgroundColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual(expectedValue, concrete.Value);
    }

    [TestMethod]
    public void BackgroundColorMultipleIllegal()
    {
        var snippet = "background-color : #bbff00, transparent, red, #ff00ff";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundColorProperty>(property);
        var concrete = (BackgroundColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void BackgroundImageNoneLegal()
    {
        var snippet = "background-image: NONE";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-image", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var concrete = (BackgroundImageProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    public void BackgroundImageUrlAndNoneLegal()
    {
        var snippet = "background-image: url(\"img/sprites.svg?v=1bc768be1b3c\"),none";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-image", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var concrete = (BackgroundImageProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"img/sprites.svg?v=1bc768be1b3c\"), none", concrete.Value);
    }

    [TestMethod]
    public void BackgroundImageUrlLegal()
    {
        var snippet = "background-image: url(image.png)";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-image", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var concrete = (BackgroundImageProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"image.png\")", concrete.Value);
    }

    [TestMethod]
    [DataRow("background-image: image-set(\"a.png\" 1x, \"b.png\" 2x)")]
    [DataRow("background-image: cross-fade(url(a.png), url(b.png), 50%)")]
    [DataRow("background-image: element(#hero)")]
    public void BackgroundImageExtendedFunctionLegal(string snippet)
    {
        // Valid <image> values (CSS Images 4), parsed even though nothing is rendered for them.
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-image", property.Name);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var concrete = (BackgroundImageProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.IsFalse(string.IsNullOrEmpty(concrete.Value)); // serializes back through ImageFunctionValue.CssText
    }

    [TestMethod]
    [DataRow("background-image: image-set(banana)")]
    [DataRow("background-image: element(.klass)")]
    [DataRow("background-image: cross-fade(5px)")]
    public void BackgroundImageMalformedExtendedFunctionIllegal(string snippet)
    {
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.IsFalse(((BackgroundImageProperty)property).HasValue);
    }

    [TestMethod]
    public void BackgroundImageUrlAbsoluteLegal()
    {
        var snippet = "background-image: url(http://www.example.com/images/bck.png)";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-image", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var concrete = (BackgroundImageProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"http://www.example.com/images/bck.png\")", concrete.Value);
    }

    [TestMethod]
    public void BackgroundImageUrlsLegal()
    {
        var snippet = "background-image: url(image.png),url('bla.png')";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-image", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var concrete = (BackgroundImageProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"image.png\"), url(\"bla.png\")", concrete.Value);
    }

    [TestMethod]
    public void BackgroundImageUrlNoneUrlLegal()
    {
        var snippet = "background-image: url(image.png),none, url(foo.gif)";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-image", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var concrete = (BackgroundImageProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"image.png\"), none, url(\"foo.gif\")", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-origin")]
    public void BackgroundOriginContentBoxLegal()
    {
        var snippet = "background-origin: CONTENT-BOX";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundOriginProperty>(property);
        var concrete = (BackgroundOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("content-box", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-origin")]
    public void BackgroundOriginContentBoxPaddingBoxLegal()
    {
        var snippet = "background-origin: CONTENT-BOX, Padding-Box";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundOriginProperty>(property);
        var concrete = (BackgroundOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("content-box, padding-box", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-origin")]
    public void BackgroundOriginBorderBoxLegal()
    {
        var snippet = "background-origin: border-box";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-origin", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundOriginProperty>(property);
        var concrete = (BackgroundOriginProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("border-box", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-position")]
    public void BackgroundPositionTopLegal()
    {
        var snippet = "background-position: top";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-position", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundPositionProperty>(property);
        var concrete = (BackgroundPositionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("top", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-position")]
    public void BackgroundPositionPercentPercentLegal()
    {
        var snippet = "background-position: 25% 75%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-position", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundPositionProperty>(property);
        var concrete = (BackgroundPositionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("25% 75%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-position")]
    public void BackgroundPositionCenterPercentLegal()
    {
        var snippet = "background-position: center 75%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-position", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundPositionProperty>(property);
        var concrete = (BackgroundPositionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("center 75%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-position")]
    public void BackgroundPositionRightLengthBottomLengthLegal()
    {
        var snippet = "background-position: right 20px bottom 20px";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-position", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundPositionProperty>(property);
        var concrete = (BackgroundPositionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("right 20px bottom 20px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-position")]
    public void BackgroundPositionLengthLengthCenterMultipleLegal()
    {
        var snippet = "background-position: 10px 20px, center";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-position", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundPositionProperty>(property);
        var concrete = (BackgroundPositionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("10px 20px, center", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-position")]
    public void BackgroundPositionZeroMultipleLegal()
    {
        var snippet = "background-position: 0 0, 0 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-position", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundPositionProperty>(property);
        var concrete = (BackgroundPositionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0 0, 0 0", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-repeat")]
    public void BackgroundRepeatRepeatXLegal()
    {
        var snippet = "background-repeat: repeat-x";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-repeat", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundRepeatProperty>(property);
        var concrete = (BackgroundRepeatProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("repeat-x", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-repeat")]
    public void BackgroundRepeatRepeatYLegal()
    {
        var snippet = "background-repeat: repeat-y";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-repeat", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundRepeatProperty>(property);
        var concrete = (BackgroundRepeatProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("repeat-y", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-repeat")]
    public void BackgroundRepeatRepeatLegal()
    {
        var snippet = "background-repeat: REPEAT";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-repeat", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundRepeatProperty>(property);
        var concrete = (BackgroundRepeatProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("repeat", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-repeat")]
    public void BackgroundRepeatRoundLegal()
    {
        var snippet = "background-repeat: rounD";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-repeat", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundRepeatProperty>(property);
        var concrete = (BackgroundRepeatProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("round", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-repeat")]
    public void BackgroundRepeatRepeatSpaceLegal()
    {
        var snippet = "background-repeat: repeat space";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-repeat", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundRepeatProperty>(property);
        var concrete = (BackgroundRepeatProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("repeat space", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-repeat")]
    public void BackgroundRepeatRepeatXSpaceIllegal()
    {
        var snippet = "background-repeat: repeat-x space";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-repeat", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundRepeatProperty>(property);
        var concrete = (BackgroundRepeatProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-repeat")]
    public void BackgroundRepeatRepeatXRepeatYMultipleLegal()
    {
        var snippet = "background-repeat: repeat-X, repeat-Y";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-repeat", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundRepeatProperty>(property);
        var concrete = (BackgroundRepeatProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("repeat-x, repeat-y", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-repeat")]
    public void BackgroundRepeatSpaceRoundLegal()
    {
        var snippet = "background-repeat: space round";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-repeat", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundRepeatProperty>(property);
        var concrete = (BackgroundRepeatProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("space round", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-repeat")]
    public void BackgroundRepeatNoRepeatRepeatXIllegal()
    {
        var snippet = "background-repeat: no-repeat repeat-x";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-repeat", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundRepeatProperty>(property);
        var concrete = (BackgroundRepeatProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-repeat")]
    public void BackgroundRepeatRepeatRepeatNoRepeatRepeatLegal()
    {
        var snippet = "background-repeat: repeat repeat, no-repeat repeat";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-repeat", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundRepeatProperty>(property);
        var concrete = (BackgroundRepeatProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("repeat repeat, no-repeat repeat", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-size")]
    public void BackgroundSizeLengthLegal()
    {
        var snippet = "background-size: 2em";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundSizeProperty>(property);
        var concrete = (BackgroundSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("2em", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-size")]
    public void BackgroundSizePercentLegal()
    {
        var snippet = "background-size: 20%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundSizeProperty>(property);
        var concrete = (BackgroundSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("20%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-size")]
    public void BackgroundSizeAutoAutoLegal()
    {
        var snippet = "background-size: auto auto";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundSizeProperty>(property);
        var concrete = (BackgroundSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("auto auto", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-size")]
    public void BackgroundSizeAutoLengthLegal()
    {
        var snippet = "background-size: auto 50px";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundSizeProperty>(property);
        var concrete = (BackgroundSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("auto 50px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-size")]
    public void BackgroundSizeLengthLengthLegal()
    {
        var snippet = "background-size: 25px 50px";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundSizeProperty>(property);
        var concrete = (BackgroundSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("25px 50px", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-size")]
    public void BackgroundSizePercentPercentLegal()
    {
        var snippet = "background-size: 50% 50%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundSizeProperty>(property);
        var concrete = (BackgroundSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("50% 50%", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-size")]
    public void BackgroundSizeAutoUppercaseLegal()
    {
        var snippet = "background-size: AUTO";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundSizeProperty>(property);
        var concrete = (BackgroundSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("auto", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-size")]
    public void BackgroundSizeCoverLegal()
    {
        var snippet = "background-size: cover";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundSizeProperty>(property);
        var concrete = (BackgroundSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("cover", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-size")]
    public void BackgroundSizeContainCoverMultipleLegal()
    {
        var snippet = "background-size: contain,cover";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundSizeProperty>(property);
        var concrete = (BackgroundSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("contain, cover", concrete.Value);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background-size")]
    public void BackgroundSizeContainLengthAutoPercentLegal()
    {
        var snippet = "background-size: contain,100px,auto,20%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundSizeProperty>(property);
        var concrete = (BackgroundSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("contain, 100px, auto, 20%", concrete.Value);
    }

    [TestMethod]
    public void BackgroundRedLegal()
    {
        var snippet = "background: red";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundProperty>(property);
        var concrete = (BackgroundProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(255, 0, 0)", concrete.Value);
    }

    [TestMethod]
    public void BackgroundWhiteImageLegal()
    {
        var snippet = "background: white url(\"pendant.png\");";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundProperty>(property);
        var concrete = (BackgroundProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"pendant.png\") rgb(255, 255, 255)", concrete.Value);
    }

    [TestMethod]
    public void BackgroundImageLegal()
    {
        var snippet = "background: url(\"topbanner.png\") #00d repeat-y fixed";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundProperty>(property);
        var concrete = (BackgroundProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"topbanner.png\") repeat-y fixed rgb(0, 0, 221)", concrete.Value);
    }

    [TestMethod]
    public void BackgroundWithoutColorLegal()
    {
        var snippet = "background: url(\"img_tree.png\") no-repeat right top";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundProperty>(property);
        var concrete = (BackgroundProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"img_tree.png\") right top no-repeat", concrete.Value);
    }

    [TestMethod]
    public void BackgroundImageDataUrlLegal()
    {
        var url = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEcAAAAcCAMAAAAEJ1IZAAAABGdBTUEAALGPC/xhBQAAVAI/VAI/VAI/VAI/VAI/VAI/VAAAA////AI/VRZ0U8AAAAFJ0Uk5TYNV4S2UbgT/Gk6uQt585w2wGXS0zJO2lhGttJK6j4YqZSobH1AAAAAElFTkSuQmCC";
        var snippet = "background-image: url('" + url + "')";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background-image", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var concrete = (BackgroundImageProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"" + url + "\")", concrete.Value);
    }

    [TestMethod]
    public void BackgroundTransparentLegal()
    {
        var snippet = "background: transparent";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background", property.Name);
        Assert.IsInstanceOfType<BackgroundProperty>(property);
        var concrete = (BackgroundProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgba(0, 0, 0, 0)", concrete.Value);
    }

    [TestMethod]
    public void BackgroundNoneLegal()
    {
        var snippet = "background: none";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background", property.Name);
        Assert.IsInstanceOfType<BackgroundProperty>(property);
        var concrete = (BackgroundProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    public void BackgroundWithPositionLegal()
    {
        var snippet = "background: url(\"img.png\") center center no-repeat";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background", property.Name);
        Assert.IsInstanceOfType<BackgroundProperty>(property);
        var concrete = (BackgroundProperty)property;
        Assert.IsTrue(concrete.HasValue);
    }

    [TestMethod]
    public void BackgroundWithPositionAndSizeLegal()
    {
        var snippet = "background: url(\"img.png\") center / cover no-repeat";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background", property.Name);
        Assert.IsInstanceOfType<BackgroundProperty>(property);
        var concrete = (BackgroundProperty)property;
        Assert.IsTrue(concrete.HasValue);
    }

    [TestMethod]
    public void BackgroundLinearGradientLegal()
    {
        var snippet = "background: linear-gradient(to right, red, blue)";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background", property.Name);
        Assert.IsInstanceOfType<BackgroundProperty>(property);
        var concrete = (BackgroundProperty)property;
        Assert.IsTrue(concrete.HasValue);
        StringAssert.Contains(concrete.Value, "linear-gradient");
    }

    [TestMethod]
    public void BackgroundHexColorLegal()
    {
        var snippet = "background: #ff0000";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background", property.Name);
        Assert.IsInstanceOfType<BackgroundProperty>(property);
        var concrete = (BackgroundProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(255, 0, 0)", concrete.Value);
    }

    [TestMethod]
    public void BackgroundRgbaColorLegal()
    {
        var snippet = "background: rgba(0, 128, 255, 0.5)";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background", property.Name);
        Assert.IsInstanceOfType<BackgroundProperty>(property);
        var concrete = (BackgroundProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgba(0, 128, 255, 0.5)", concrete.Value);
    }

    [TestMethod]
    public void BackgroundWithRepeatXLegal()
    {
        var snippet = "background: url(\"tile.png\") repeat-x top";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background", property.Name);
        Assert.IsInstanceOfType<BackgroundProperty>(property);
        var concrete = (BackgroundProperty)property;
        Assert.IsTrue(concrete.HasValue);
    }

    [TestMethod]
    [DataRow("background: red", "rgb(255, 0, 0)")]
    [DataRow("background: white url(\"pendant.png\")", "url(\"pendant.png\") rgb(255, 255, 255)")]
    [DataRow("background: url(\"topbanner.png\") #00d repeat-y fixed", "url(\"topbanner.png\") repeat-y fixed rgb(0, 0, 221)")]
    [DataRow("background: url(\"img_tree.png\") no-repeat right top", "url(\"img_tree.png\") right top no-repeat")]
    public void BackgroundShorthandValues(string snippet, string expectedValue)
    {
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("background", property.Name);
        Assert.IsInstanceOfType<BackgroundProperty>(property);
        var concrete = (BackgroundProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual(expectedValue, concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "colors.html#background-properties")]
    public void BackgroundShorthand_Color_SetsBackgroundColor()
    {
        var style = CssConstructionFunctions.ParseDeclarations("background: red");
        Assert.AreEqual("rgb(255, 0, 0)", style.BackgroundColor);
        Assert.AreEqual("initial", style.BackgroundImage);
        Assert.AreEqual("initial", style.BackgroundRepeat);
        Assert.AreEqual("initial", style.BackgroundPosition);
        Assert.AreEqual("initial", style.BackgroundSize);
        Assert.AreEqual("initial", style.BackgroundAttachment);
    }

    [TestMethod]
    public void BackgroundShorthand_Transparent_SetsBackgroundColorToTransparent()
    {
        var style = CssConstructionFunctions.ParseDeclarations("background: transparent");
        Assert.AreEqual("rgba(0, 0, 0, 0)", style.BackgroundColor);
        Assert.AreEqual("initial", style.BackgroundImage);
    }

    [TestMethod]
    [Spec("css21", "colors.html#background-properties")]
    public void BackgroundShorthand_ImageAndColor_SetsBothLonghands()
    {
        var style = CssConstructionFunctions.ParseDeclarations("background: white url(\"pendant.png\")");
        Assert.AreEqual("rgb(255, 255, 255)", style.BackgroundColor);
        Assert.AreEqual("url(\"pendant.png\")", style.BackgroundImage);
        Assert.AreEqual("initial", style.BackgroundRepeat);
        Assert.AreEqual("initial", style.BackgroundPosition);
    }

    [TestMethod]
    [Spec("css21", "colors.html#background-properties")]
    public void BackgroundShorthand_ImageRepeatAttachmentColor_SetsAllFourLonghands()
    {
        var style = CssConstructionFunctions.ParseDeclarations("background: url(\"topbanner.png\") #00d repeat-y fixed");
        Assert.AreEqual("rgb(0, 0, 221)", style.BackgroundColor);
        Assert.AreEqual("url(\"topbanner.png\")", style.BackgroundImage);
        Assert.AreEqual("repeat-y", style.BackgroundRepeat);
        Assert.AreEqual("fixed", style.BackgroundAttachment);
        Assert.AreEqual("initial", style.BackgroundPosition);
    }

    [TestMethod]
    [Spec("css21", "colors.html#background-properties")]
    public void BackgroundShorthand_ImageNoRepeatPosition_SetsImageRepeatPosition()
    {
        var style = CssConstructionFunctions.ParseDeclarations("background: url(\"img_tree.png\") no-repeat right top");
        Assert.AreEqual("url(\"img_tree.png\")", style.BackgroundImage);
        Assert.AreEqual("no-repeat", style.BackgroundRepeat);
        Assert.AreEqual("right top", style.BackgroundPosition);
        Assert.AreEqual("initial", style.BackgroundColor);
    }

    [TestMethod]
    public void BackgroundShorthand_LinearGradient_SetsBackgroundImage()
    {
        var style = CssConstructionFunctions.ParseDeclarations("background: linear-gradient(to right, red, blue)");
        StringAssert.Contains(style.BackgroundImage, "linear-gradient");
        Assert.AreEqual("initial", style.BackgroundColor);
    }

    [TestMethod]
    public void BackgroundShorthand_ImagePositionSize_SetsPositionAndSize()
    {
        var style = CssConstructionFunctions.ParseDeclarations("background: url(\"img.png\") center / cover no-repeat");
        Assert.AreEqual("url(\"img.png\")", style.BackgroundImage);
        Assert.AreEqual("center", style.BackgroundPosition);
        Assert.AreEqual("cover", style.BackgroundSize);
        Assert.AreEqual("no-repeat", style.BackgroundRepeat);
    }

    [TestMethod]
    public void BackgroundShorthand_MultipleLayers_ExtractsCommaJoinedNotSpaceJoinedLonghands()
    {
        // Layers are joined with a comma, "no-repeat repeat-x" would read as one two-axis value
        var style = CssConstructionFunctions.ParseDeclarations(
            "background: url(\"a.png\") top no-repeat, url(\"b.png\") bottom repeat-x");

        Assert.AreEqual("url(\"a.png\"), url(\"b.png\")", style.BackgroundImage);
        Assert.AreEqual("top, bottom", style.BackgroundPosition);
        Assert.AreEqual("no-repeat, repeat-x", style.BackgroundRepeat);
        Assert.AreNotEqual("no-repeat repeat-x", style.BackgroundRepeat);
    }

    [TestMethod]
    public void BackgroundShorthand_MultipleLayers_OriginClipCommaJoined()
    {
        // Both box keywords per layer, a single one only sets background-origin (see Spec/css-backgrounds-3.yml)
        var style = CssConstructionFunctions.ParseDeclarations(
            "background: url(\"a.png\") padding-box content-box, url(\"b.png\") border-box padding-box");

        Assert.AreEqual("url(\"a.png\"), url(\"b.png\")", style.BackgroundImage);
        Assert.AreEqual("padding-box, border-box", style.BackgroundOrigin);
        Assert.AreEqual("content-box, padding-box", style.BackgroundClip);
    }

    [TestMethod]
    public void BackgroundShorthand_HexColor_SetsBackgroundColor()
    {
        var style = CssConstructionFunctions.ParseDeclarations("background: #336699");
        Assert.AreEqual("rgb(51, 102, 153)", style.BackgroundColor);
        Assert.AreEqual("initial", style.BackgroundImage);
    }

    [TestMethod]
    public void BackgroundShorthand_None_SetsBackgroundImageToNone()
    {
        var style = CssConstructionFunctions.ParseDeclarations("background: none");
        Assert.AreEqual("none", style.BackgroundImage);
        Assert.AreEqual("initial", style.BackgroundColor);
    }
}
