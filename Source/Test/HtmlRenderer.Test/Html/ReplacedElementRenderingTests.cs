using HtmlRenderer.Test.TestSupport;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class ReplacedElementRenderingTests
{
    private static string Text(string html) =>
        string.Concat(LayoutHarness.Descendants(Box(html)).Prepend(Box(html)).SelectMany(b => b.Words).Where(w => !w.IsImage).Select(w => w.Text));

    [TestMethod]
    [Spec("html", "rendering.html#embedded-content-rendering-rules")]
    [Spec("html", "rendering.html#attributes-for-embedded-content-and-images")]
    public void Iframe_IsSizedByItsAttributes()
    {
        var box = Box("<div><iframe id=x width=50 height=60 style='display:block'></iframe></div>");

        Assert.AreEqual(50 + 4, box.Size.Width, 0.01);
        Assert.AreEqual(60 + 4, box.Size.Height, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#embedded-content-rendering-rules")]
    public void Iframe_HasAnInsetBorderAndTheDefaultObjectSize()
    {
        var box = Box("<div><iframe id=x style='display:block'></iframe></div>");

        Assert.AreEqual("inset", box.BorderTopStyle);
        Assert.AreEqual(300 + 4, box.Size.Width, 0.01);
        Assert.AreEqual(150 + 4, box.Size.Height, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#embedded-content-rendering-rules")]
    [DataRow("<video id=x width=50 height=60>fallback</video>")]
    [DataRow("<object id=x data=a.png width=50 height=60>fallback</object>")]
    public void VideoAndObject_AreReplacedElements(string html)
    {
        Assert.AreEqual("", Text(html));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#embedded-content-rendering-rules")]
    public void AudioWithoutControls_IsNotDisplayed()
    {
        Assert.AreEqual("none", Box("<audio id=x></audio>").Display);
    }

    [TestMethod]
    [Spec("html", "rendering.html#images-3")]
    [Spec("html", "rendering.html#attributes-for-embedded-content-and-images")]
    public void Img_IsAReplacedElementSizedByItsAttributes()
    {
        var box = Box("<img id=x width=50 height=60>");

        Assert.IsInstanceOfType<TheArtOfDev.HtmlRenderer.Core.Dom.CssBoxImage>(box);
        Assert.AreEqual("50px", box.Width);
        Assert.AreEqual("60px", box.Height);
        Assert.AreEqual("50%", Box("<img id=x width=50%>").Width);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#images-3")]
    public void BrokenImage_ShowsItsAltText()
    {
        Assert.AreEqual("hello", Text("<img id=x src=missing.png alt=hello>"));
    }

    [TestMethod]
    [Spec("html", "rendering.html#attributes-for-embedded-content-and-images")]
    public void HspaceAndVspace_SetTheMargins()
    {
        var box = Box("<img id=x hspace=5 vspace=6 width=5 height=5>");

        Assert.AreEqual(5, box.ActualMarginLeft);
        Assert.AreEqual(5, box.ActualMarginRight);
        Assert.AreEqual(6, box.ActualMarginTop);
        Assert.AreEqual(6, box.ActualMarginBottom);
    }

    [TestMethod]
    [Spec("html", "rendering.html#attributes-for-embedded-content-and-images")]
    public void ImgBorder_IsASolidBorder()
    {
        var box = Box("<img id=x border=2 width=5 height=5>");

        Assert.AreEqual("solid", box.BorderTopStyle);
        Assert.AreEqual(2, box.ActualBorderTopWidth);
    }

    [TestMethod]
    [Spec("html", "rendering.html#attributes-for-embedded-content-and-images")]
    [DataRow("top", "top")]
    [DataRow("middle", "middle")]
    [DataRow("bottom", "bottom")]
    [DataRow("baseline", "baseline")]
    public void ImgAlign_SetsTheVerticalAlignment(string align, string verticalAlign)
    {
        Assert.AreEqual(verticalAlign, Box($"<img id=x align={align} width=5 height=5>").VerticalAlign);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#attributes-for-embedded-content-and-images")]
    [DataRow("texttop", "text-top")]
    [DataRow("absmiddle", "middle")]
    [DataRow("abscenter", "middle")]
    public void ImgAlignLegacyValues_SetTheVerticalAlignment(string align, string verticalAlign)
    {
        Assert.AreEqual(verticalAlign, Box($"<img id=x align={align} width=5 height=5>").VerticalAlign);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#attributes-for-embedded-content-and-images")]
    [DataRow("left")]
    [DataRow("right")]
    public void ImgAlignLeftAndRight_FloatTheImage(string align)
    {
        Assert.AreEqual(align, Box($"<img id=x align={align} width=5 height=5>").Float);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#attributes-for-embedded-content-and-images")]
    public void IframeFrameborderZero_RemovesTheBorder()
    {
        Assert.AreEqual(0, Box("<iframe id=x frameborder=0></iframe>").ActualBorderTopWidth);
    }
}
