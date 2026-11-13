using HtmlRenderer.Test.CssEngineSupport;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class CssBackgrounds4ParseTests
{
    [Ignore("Gap, see Spec/css-backgrounds-4.yml")]
    [TestMethod]
    [DataRow("background-repeat-x")]
    [DataRow("background-repeat-y")]
    [DataRow("background-repeat-inline")]
    [DataRow("background-repeat-block")]
    [Spec("css-backgrounds-4", "#background-repeat-longhands")]
    public void BackgroundRepeatLonghands_AreAccepted(string property)
    {
        var style = CssConstructionFunctions.ParseDeclarations($"{property}: no-repeat");

        Assert.AreEqual("no-repeat", style.GetPropertyValue(property));
    }

    [TestMethod]
    [DataRow("repeat-x")]
    [DataRow("space round")]
    [DataRow("no-repeat, repeat-y")]
    [Spec("css-backgrounds-4", "#background-repeat")]
    public void BackgroundRepeat_IsAccepted(string repeat)
    {
        var property = CssConstructionFunctions.ParseDeclaration($"background-repeat: {repeat}");

        Assert.IsTrue(property.HasValue);
    }

    [Ignore("Gap, see Spec/css-backgrounds-4.yml")]
    [TestMethod]
    [DataRow("repeat-block")]
    [DataRow("repeat-inline")]
    [Spec("css-backgrounds-4", "#background-repeat")]
    public void BackgroundRepeat_LogicalKeywordsAreAccepted(string repeat)
    {
        var property = CssConstructionFunctions.ParseDeclaration($"background-repeat: {repeat}");

        Assert.IsTrue(property.HasValue);
    }

    [Ignore("Gap, see Spec/css-backgrounds-4.yml")]
    [TestMethod]
    [DataRow("background-position-x", "right 10px")]
    [DataRow("background-position-y", "bottom")]
    [DataRow("background-position-inline", "end")]
    [DataRow("background-position-block", "center")]
    [Spec("css-backgrounds-4", "#background-position-longhands")]
    public void BackgroundPositionLonghands_AreAccepted(string property, string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations($"{property}: {value}");

        Assert.AreEqual(value, style.GetPropertyValue(property));
    }

    [TestMethod]
    [DataRow("center bottom 10px")]
    [DataRow("right 10px top")]
    [DataRow("left 10px top 15px")]
    [Spec("css-backgrounds-4", "#background-position")]
    public void BackgroundPosition_ThreeAndFourValueFormsAreAccepted(string position)
    {
        var property = CssConstructionFunctions.ParseDeclaration($"background-position: {position}");

        Assert.IsTrue(property.HasValue);
    }

    [TestMethod]
    [Spec("css-backgrounds-4", "#background-position")]
    [Spec("css-backgrounds-3", "#background-position")]
    [Spec("css-values-4", "#position")]
    public void BackgroundPosition_FourValueFormWithVerticalEdgeFirstIsAccepted()
    {
        var property = CssConstructionFunctions.ParseDeclaration("background-position: bottom 10px right 20px");

        Assert.IsTrue(property.HasValue);
    }

    [TestMethod]
    [DataRow("top 10px left")]
    [DataRow("bottom right 5px")]
    [Spec("css-backgrounds-4", "#background-position")]
    [Spec("css-backgrounds-3", "#background-position")]
    public void BackgroundPosition_ThreeValueFormWithVerticalEdgeFirstIsAccepted(string position)
    {
        var property = CssConstructionFunctions.ParseDeclaration($"background-position: {position}");

        Assert.IsTrue(property.HasValue);
    }

    [Ignore("Gap, see Spec/css-backgrounds-4.yml")]
    [TestMethod]
    [DataRow("left center 10px")]
    [DataRow("center 10px top")]
    [Spec("css-backgrounds-4", "#background-position")]
    [Spec("css-backgrounds-3", "#background-position")]
    public void BackgroundPosition_CenterWithAnOffsetIsRejected(string position)
    {
        var property = CssConstructionFunctions.ParseDeclaration($"background-position: {position}");

        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [DataRow("background-clip: padding-box, content-box")]
    [DataRow("background-origin: content-box")]
    [DataRow("background-size: cover, 10px auto")]
    [Spec("css-backgrounds-4", "#background-clip")]
    [Spec("css-backgrounds-4", "#background-origin")]
    [Spec("css-backgrounds-4", "#background-size")]
    public void BoxAndSizeValues_AreAccepted(string declaration)
    {
        var property = CssConstructionFunctions.ParseDeclaration(declaration);

        Assert.IsTrue(property.HasValue);
    }

    [TestMethod]
    [DataRow("text")]
    [DataRow("border-area")]
    [Spec("css-backgrounds-4", "#background-clip")]
    public void BackgroundClip_TextAndBorderAreaAreAccepted(string clip)
    {
        var property = CssConstructionFunctions.ParseDeclaration($"background-clip: {clip}");

        Assert.IsTrue(property.HasValue);
    }

    [Ignore("Gap, see Spec/css-backgrounds-4.yml")]
    [TestMethod]
    [DataRow("border-area text")]
    [Spec("css-backgrounds-4", "#background-clip")]
    public void BackgroundClip_BorderAreaWithTextIsAccepted(string clip)
    {
        var property = CssConstructionFunctions.ParseDeclaration($"background-clip: {clip}");

        Assert.IsTrue(property.HasValue);
    }

    [Ignore("Gap, see Spec/css-backgrounds-4.yml")]
    [TestMethod]
    [Spec("css-backgrounds-4", "#background")]
    public void BackgroundShorthand_AClipOnlyValueSetsOriginToBorderBox()
    {
        var style = CssConstructionFunctions.ParseDeclarations("background: url(a.png) text");

        Assert.AreEqual("text", style.BackgroundClip);
        Assert.AreEqual("border-box", style.BackgroundOrigin);
    }
}
