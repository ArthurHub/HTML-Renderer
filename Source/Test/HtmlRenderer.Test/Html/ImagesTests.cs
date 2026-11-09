using HtmlRenderer.Test.TestSupport;
using static HtmlRenderer.Test.Html.EmbeddedTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class ImagesTests
{
    [TestMethod]
    [Spec("html", "images.html#images-processing-model")]
    [Spec("html", "images.html#when-to-obtain-images")]
    public void Image_IsSizedByItsNaturalDimensions()
    {
        var img = Loaded($"<img id=x src='{Png}'>");

        Assert.AreEqual(40, img.Words[0].Width, 0.5);
        Assert.AreEqual(30, img.Words[0].Height, 0.5);
    }

    [TestMethod]
    [Spec("html", "images.html#updating-the-image-data")]
    public void Src_IsResolvedAgainstTheDocumentBaseUrl()
    {
        var loader = new RecordingLoader("http://example.com/dir/page.html");

        Loaded("<img id=x src='a.png'>", adapter: new MockAdapter { NetworkLoader = loader });
        LayoutHarness.Layout(
            "<html><head><base href='http://example.org/base/'></head><body><img src='b.png'></body></html>",
            adapter: new MockAdapter { NetworkLoader = loader },
            prepare: root => root.HtmlContainer!.AvoidAsyncImagesLoading = true);

        CollectionAssert.AreEqual(new[] { "http://example.com/dir/a.png", "http://example.org/base/b.png" }, loader.Requests);
    }

    [TestMethod]
    [Spec("html", "images.html#updating-the-image-data")]
    public void EmptySrc_IsNotFetched()
    {
        var loader = new RecordingLoader("http://example.com/");

        var img = Loaded("<img id=x src=''>", adapter: new MockAdapter { NetworkLoader = loader });

        Assert.AreEqual(0, loader.Requests.Count);
        Assert.IsNull(ImageOf(img));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "images.html#selecting-an-image-source")]
    public void Srcset_WithoutSrc_IsLoaded()
    {
        Assert.IsNotNull(ImageOf(Loaded($"<img id=x srcset='{Png} 1x'>")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "images.html#normalizing-the-source-densities")]
    public void DensityDescriptor_DividesTheNaturalSize()
    {
        var img = Loaded($"<img id=x srcset='{Png} 2x'>");

        Assert.AreEqual(20, img.Words[0].Width, 0.5);
        Assert.AreEqual(15, img.Words[0].Height, 0.5);
    }
}
