using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class CssImages4ParseTests
{
    [TestMethod]
    [DataRow("image-set(url(a.png) 1x, url(b.png) 2x)")]
    [DataRow("image-set(\"a.png\" 1x type(\"image/png\"), \"b.png\" 192dpi)")]
    [DataRow("image-set(linear-gradient(red, blue) 1x, url(a.png) 3x)")]
    [Spec("css-images-4", "#image-set-notation")]
    public void ImageSet_IsAccepted(string image)
    {
        Assert.IsTrue(Parse($"background-image: {image}").HasValue);
    }

    [TestMethod]
    [DataRow("image-set()")]
    [DataRow("image-set(1x)")]
    [Spec("css-images-4", "#image-set-notation")]
    public void ImageSet_InvalidValuesAreRejected(string image)
    {
        Assert.IsFalse(Parse($"background-image: {image}").HasValue);
    }

    [Ignore("Gap, see Spec/css-images-4.yml")]
    [TestMethod]
    [DataRow("image-set(url(a.png) 1x 2x)")]
    [DataRow("image-set(url(a.png) type(\"image/png\") type(\"image/gif\"))")]
    [Spec("css-images-4", "#image-set-notation")]
    public void ImageSet_AnOptionTakesOneResolutionAndOneType(string image)
    {
        Assert.IsFalse(Parse($"background-image: {image}").HasValue);
    }

    [Ignore("Gap, see Spec/css-images-4.yml")]
    [TestMethod]
    [DataRow("image(\"a.png\")")]
    [DataRow("image(\"a.png\", red)")]
    [DataRow("image(rgba(0, 0, 255, .5))")]
    [DataRow("image(ltr \"arrow.png\")")]
    [Spec("css-images-4", "#image-notation")]
    [Spec("css-images-4", "#image-fallbacks")]
    [Spec("css-images-4", "#color-images")]
    [Spec("css-images-4", "#bidi-images")]
    public void ImageFunction_IsAccepted(string image)
    {
        Assert.IsTrue(Parse($"background-image: {image}").HasValue);
    }

    [TestMethod]
    [DataRow("cross-fade(url(a.png) 30%, red)")]
    [DataRow("cross-fade(url(a.png), url(b.png))")]
    [DataRow("cross-fade(url(a.png))")]
    [DataRow("cross-fade(50% linear-gradient(red, blue), url(a.png) 50%)")]
    [Spec("css-images-4", "#cross-fade-function")]
    public void CrossFade_IsAccepted(string image)
    {
        Assert.IsTrue(Parse($"background-image: {image}").HasValue);
    }

    [Ignore("Gap, see Spec/css-images-4.yml")]
    [TestMethod]
    [DataRow("cross-fade(url(a.png) 150%, red)")]
    [DataRow("cross-fade(url(a.png) -10%, red)")]
    [DataRow("cross-fade(url(a.png) 10% 20%)")]
    [Spec("css-images-4", "#cross-fade-function")]
    public void CrossFade_InvalidPercentagesAreRejected(string image)
    {
        Assert.IsFalse(Parse($"background-image: {image}").HasValue);
    }

    [TestMethod]
    [DataRow("element(#src)", true)]
    [DataRow("element(src)", false)]
    [DataRow("element(#a, #b)", false)]
    [Spec("css-images-4", "#element-notation")]
    public void Element_TakesAnIdSelector(string image, bool valid)
    {
        Assert.AreEqual(valid, Parse($"background-image: {image}").HasValue);
    }

    [TestMethod]
    [DataRow("linear-gradient(0, red, blue)")]
    [DataRow("linear-gradient(in oklab 45deg, red, blue)")]
    [Spec("css-images-4", "#linear-gradients")]
    public void LinearGradient_AcceptsZeroAndAnInterpolationMethod(string image)
    {
        Assert.IsTrue(Parse($"background-image: {image}").HasValue);
    }

    [TestMethod]
    [Spec("css-images-4", "#conic-gradient-syntax")]
    [Spec("css-images-4", "#conic-color-stops")]
    public void ConicGradient_AcceptsZeroAnglesAndPercentages()
    {
        Assert.IsTrue(Parse("background-image: conic-gradient(red 0, blue 0.5turn, green 100%)").HasValue);
    }

    [Ignore("Gap, see Spec/css-images-4.yml")]
    [TestMethod]
    [Spec("css-images-4", "#conic-gradient-syntax")]
    public void ConicGradient_AcceptsAZeroRotation()
    {
        Assert.IsTrue(Parse("background-image: conic-gradient(from 0, red, blue)").HasValue);
    }

    [TestMethod]
    [DataRow("radial-gradient(in lab farthest-side at left bottom, red, blue)")]
    [DataRow("repeating-radial-gradient(in oklch longer hue, red, blue 10px)")]
    [Spec("css-images-4", "#radial-color-interpolation")]
    public void RadialGradient_AcceptsAnInterpolationMethod(string image)
    {
        Assert.IsTrue(Parse($"background-image: {image}").HasValue);
    }

    [Ignore("Gap, see Spec/css-images-4.yml")]
    [TestMethod]
    [DataRow("radial-gradient(circle 50%, red, blue)")]
    [DataRow("radial-gradient(closest-side farthest-side, red, blue)")]
    [Spec("css-images-4", "#radial-size")]
    public void RadialGradient_AcceptsTheExpandedSizes(string image)
    {
        Assert.IsTrue(Parse($"background-image: {image}").HasValue);
    }

    [TestMethod]
    [Spec("css-images-4", "#radial-size")]
    public void RadialGradient_RejectsAnExtentMixedWithALength()
    {
        Assert.IsFalse(Parse("background-image: radial-gradient(ellipse closest-corner 20px, red, blue)").HasValue);
    }

    [TestMethod]
    [DataRow("linear-gradient(red)")]
    [DataRow("linear-gradient(to right, red 10px)")]
    [DataRow("conic-gradient(red)")]
    [Spec("css-images-4", "#color-stop-syntax")]
    public void Gradient_ASingleColorStopIsAccepted(string image)
    {
        Assert.IsTrue(Parse($"background-image: {image}").HasValue);
    }

    [TestMethod]
    [DataRow("linear-gradient(in a98-rgb, red, blue)")]
    [DataRow("linear-gradient(in prophoto-rgb, red, blue)")]
    [DataRow("linear-gradient(in rec2020, red, blue)")]
    [Spec("css-images-4", "#linear-gradients")]
    public void Gradient_AcceptsEveryPredefinedRgbInterpolationSpace(string image)
    {
        Assert.IsTrue(Parse($"background-image: {image}").HasValue);
    }

    [Ignore("Gap, see Spec/css-images-4.yml")]
    [TestMethod]
    [DataRow("object-fit: contain scale-down")]
    [DataRow("object-fit: scale-down cover")]
    [Spec("css-images-4", "#the-object-fit")]
    public void ObjectFit_CombinesScaleDownWithContainOrCover(string declaration)
    {
        Assert.IsTrue(CssConstructionFunctions.ParseDeclaration(declaration).HasValue);
    }

    [TestMethod]
    [DataRow("image-resolution: 300dpi")]
    [DataRow("image-resolution: from-image 2dppx snap")]
    [Spec("css-images-4", "#the-image-resolution")]
    public void ImageResolution_IsAccepted(string declaration)
    {
        Assert.IsTrue(CssConstructionFunctions.ParseDeclaration(declaration).HasValue);
    }

    private static Property Parse(string declaration) => CssConstructionFunctions.ParseDeclaration(declaration);
}
