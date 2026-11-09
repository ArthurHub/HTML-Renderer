using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using static HtmlRenderer.Test.Html.EmbeddedTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class ImageMapsTests
{
    [TestMethod]
    [Spec("html", "image-maps.html#the-map-element")]
    public void Map_RepresentsItsChildren()
    {
        StringAssert.Contains(Text(Loaded("<p id=x>a <map name=m>b<area href='a.html'></map> c</p>")), "a b c");
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "image-maps.html#the-area-element")]
    [Spec("html", "image-maps.html#image-map-processing-model")]
    public void AreaOfAnImageMap_IsAHyperlinkOverTheImage()
    {
        var (root, container) = LayoutWithImages(
            $"<img id=x src='{Png}' usemap='#m'><map name=m><area shape=rect coords='0,0,10,10' href='a.html'></map>");
        var image = LayoutHarness.FindById(root, "x")!.Rectangles.Values.Single();

        Assert.AreEqual("a.html", container.GetLinkAt(new RPoint(image.X + 5, image.Y + 5)));
        Assert.IsNull(container.GetLinkAt(new RPoint(image.X + 30, image.Y + 20)));
    }
}
