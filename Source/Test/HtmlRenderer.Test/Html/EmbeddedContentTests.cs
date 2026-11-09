using static HtmlRenderer.Test.Html.EmbeddedTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class EmbeddedContentTests
{
    [TestMethod]
    [Spec("html", "embedded-content.html#the-img-element")]
    public void AvailableImage_IsRendered()
    {
        var img = Loaded($"<img id=x src='{Png}'>");

        Assert.IsNotNull(ImageOf(img));
        Assert.AreEqual(40, img.Words[0].Width, 0.5);
        Assert.AreEqual(30, img.Words[0].Height, 0.5);
    }

    [TestMethod]
    [Spec("html", "embedded-content.html#the-img-element")]
    public void BrokenImage_WithoutAlt_ShowsAnIndicator()
    {
        var img = Loaded("<img id=x src='missing.png'>");

        Assert.IsNull(ImageOf(img));
        Assert.IsTrue(img.Words[0].Width > 0);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "embedded-content.html#the-img-element")]
    [DataRow("<img id=x src='missing.png' alt='Hello'>")]
    [DataRow("<img id=x alt='Hello'>")]
    public void ImageWithAlt_NotAvailable_RepresentsTheAltText(string body)
    {
        StringAssert.Contains(Text(Loaded(body)), "Hello");
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "embedded-content.html#the-img-element")]
    [DataRow("<img src='missing.png' alt=''>")]
    [DataRow("<img>")]
    [DataRow("<img alt=''>")]
    [DataRow("<img src=''>")]
    public void ImageRepresentingNothing_TakesNoSpace(string img)
    {
        var next = Loaded($"<p>{img}<span id=x>a</span></p>");

        Assert.AreEqual(0, next.Rectangles.Values.Single().X, 0.5);
    }

    [TestMethod]
    [Spec("html", "embedded-content.html#the-picture-element")]
    public void Picture_RepresentsItsImg()
    {
        var img = Loaded($"<picture><img id=x src='{Png}'></picture>");

        Assert.IsNotNull(ImageOf(img));
        Assert.AreEqual(40, img.Words[0].Width, 0.5);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "embedded-content.html#the-picture-element")]
    [Spec("html", "embedded-content.html#the-source-element")]
    public void Picture_SelectsTheSourceSrcset()
    {
        var img = Loaded($"<picture><source srcset='{Png}'><img id=x src='missing.png'></picture>");

        Assert.IsNotNull(ImageOf(img));
    }
}
