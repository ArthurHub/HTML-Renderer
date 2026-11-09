using static HtmlRenderer.Test.Html.EmbeddedTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class EmbeddedContentOtherTests
{
    [TestMethod]
    [Spec("html", "embedded-content-other.html#dimension-attributes")]
    public void WidthAndHeight_SetTheImageSize()
    {
        var img = Loaded($"<img id=x src='{Png}' width=80 height=10>");

        Assert.AreEqual(80, img.Words[0].Width, 0.5);
        Assert.AreEqual(10, img.Words[0].Height, 0.5);
    }

    [TestMethod]
    [Spec("html", "embedded-content-other.html#dimension-attributes")]
    [Spec("html", "rendering.html#attributes-for-embedded-content-and-images")]
    [DataRow("80", 80, 60)]
    [DataRow("50%", 100, 75)]
    public void Width_ScalesTheHeightByTheNaturalAspectRatio(string width, double expectedWidth, double expectedHeight)
    {
        var img = Loaded($"<div style='width:200px'><img id=x src='{Png}' width='{width}'></div>");

        Assert.AreEqual(expectedWidth, img.Words[0].Width, 0.5);
        Assert.AreEqual(expectedHeight, img.Words[0].Height, 0.5);
    }

    [TestMethod]
    [Spec("html", "embedded-content-other.html#dimension-attributes")]
    [Spec("html", "rendering.html#attributes-for-embedded-content-and-images")]
    public void Height_ScalesTheWidthByTheNaturalAspectRatio()
    {
        var img = Loaded($"<img id=x src='{Png}' height=60>");

        Assert.AreEqual(80, img.Words[0].Width, 0.5);
        Assert.AreEqual(60, img.Words[0].Height, 0.5);
    }

    [TestMethod]
    [Spec("html", "embedded-content-other.html#dimension-attributes")]
    public void WidthAndHeight_SetTheIframeSize()
    {
        var iframe = Loaded("<iframe id=x width=100 height=50></iframe>");

        Assert.AreEqual(100, iframe.Words[0].Width, 0.5);
        Assert.AreEqual(50, iframe.Words[0].Height - iframe.ActualBorderTopWidth - iframe.ActualBorderBottomWidth, 0.5);
    }
}
