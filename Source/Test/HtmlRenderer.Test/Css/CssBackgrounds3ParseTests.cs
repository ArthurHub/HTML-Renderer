using HtmlRenderer.Test.CssEngineSupport;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class CssBackgrounds3ParseTests
{
    [TestMethod]
    [DataRow("right 10px top")]
    [DataRow("right 10px bottom 20px")]
    [DataRow("center bottom 10%")]
    [Spec("css-backgrounds-3", "#background-position")]
    public void BackgroundPosition_EdgeOffsetsAreAccepted(string position)
    {
        var property = CssConstructionFunctions.ParseDeclaration($"background-position: {position}");

        Assert.IsTrue(property.HasValue);
    }

    [TestMethod]
    [DataRow("left right")]
    [DataRow("left 10px 20px")]
    [Spec("css-backgrounds-3", "#background-position")]
    public void BackgroundPosition_InvalidCombinationsAreRejected(string position)
    {
        var property = CssConstructionFunctions.ParseDeclaration($"background-position: {position}");

        Assert.IsFalse(property.HasValue);
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [DataRow("10px left")]
    [DataRow("top 10px")]
    [Spec("css-backgrounds-3", "#background-position")]
    public void BackgroundPosition_AnOffsetFirstMustBeHorizontal(string position)
    {
        var property = CssConstructionFunctions.ParseDeclaration($"background-position: {position}");

        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background")]
    public void BackgroundShorthand_ColorIsOnlyAllowedInTheFinalLayer()
    {
        var style = CssConstructionFunctions.ParseDeclarations("background: red url(a.png), url(b.png)");

        Assert.AreEqual("", style.BackgroundImage);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background")]
    public void BackgroundShorthand_TwoBoxValuesSetOriginThenClip()
    {
        var style = CssConstructionFunctions.ParseDeclarations("background: url(a.png) padding-box content-box");

        Assert.AreEqual("padding-box", style.BackgroundOrigin);
        Assert.AreEqual("content-box", style.BackgroundClip);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#background")]
    public void BackgroundShorthand_OneBoxValueSetsOriginAndClip()
    {
        var style = CssConstructionFunctions.ParseDeclarations("background: url(a.png) padding-box");

        Assert.AreEqual("padding-box", style.BackgroundOrigin);
        Assert.AreEqual("padding-box", style.BackgroundClip);
    }

    [Ignore("Gap, see Spec/css-backgrounds-3.yml")]
    [TestMethod]
    [DataRow("border-radius: -5px")]
    [DataRow("border-top-left-radius: 5px -5px")]
    [Spec("css-backgrounds-3", "#border-radius")]
    public void BorderRadius_NegativeValuesAreRejected(string declaration)
    {
        var property = CssConstructionFunctions.ParseDeclaration(declaration);

        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [DataRow("box-shadow: 1px 2px 3px -1px red inset")]
    [DataRow("box-shadow: inset 0 0 red, 1px 1px blue")]
    [Spec("css-backgrounds-3", "#box-shadow")]
    public void BoxShadow_IsAccepted(string declaration)
    {
        var property = CssConstructionFunctions.ParseDeclaration(declaration);

        Assert.IsTrue(property.HasValue);
    }

    [TestMethod]
    [DataRow("box-shadow: 1px 2px -3px red")]
    [DataRow("box-shadow: 1px")]
    [DataRow("box-shadow: none, 1px 1px red")]
    [Spec("css-backgrounds-3", "#box-shadow")]
    public void BoxShadow_InvalidValuesAreRejected(string declaration)
    {
        var property = CssConstructionFunctions.ParseDeclaration(declaration);

        Assert.IsFalse(property.HasValue);
    }
}
