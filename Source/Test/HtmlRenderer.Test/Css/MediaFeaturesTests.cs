using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class MediaFeaturesTests
{
    [TestMethod]
    [Spec("mediaqueries-4", "#mq-features")]
    public void CssMediaFeatureFactory()
    {
        var aspectRatio = MediaFeatureFactory.Instance.Create(FeatureNames.AspectRatio);
        Assert.IsNotNull(aspectRatio);
        Assert.IsInstanceOfType<AspectRatioMediaFeature>(aspectRatio);
        Assert.IsFalse(aspectRatio.IsMaximum);
        Assert.IsFalse(aspectRatio.IsMinimum);

        var colorIndex = MediaFeatureFactory.Instance.Create(FeatureNames.ColorIndex);
        Assert.IsNotNull(colorIndex);
        Assert.IsInstanceOfType<ColorIndexMediaFeature>(colorIndex);
        Assert.IsFalse(colorIndex.IsMaximum);
        Assert.IsFalse(colorIndex.IsMinimum);

        var deviceWidth = MediaFeatureFactory.Instance.Create(FeatureNames.DeviceWidth);
        Assert.IsNotNull(deviceWidth);
        Assert.IsInstanceOfType<DeviceWidthMediaFeature>(deviceWidth);
        Assert.IsFalse(deviceWidth.IsMaximum);
        Assert.IsFalse(deviceWidth.IsMinimum);

        var monochrome = MediaFeatureFactory.Instance.Create(FeatureNames.MaxMonochrome);
        Assert.IsNotNull(monochrome);
        Assert.IsInstanceOfType<MonochromeMediaFeature>(monochrome);
        Assert.IsTrue(monochrome.IsMaximum);
        Assert.IsFalse(monochrome.IsMinimum);

        var illegal = MediaFeatureFactory.Instance.Create("illegal");
        Assert.IsNull(illegal);
    }
}
