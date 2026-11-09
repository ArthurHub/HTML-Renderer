using static HtmlRenderer.Test.Html.EmbeddedTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class IframeEmbedObjectTests
{
    [TestMethod]
    [Spec("html", "iframe-embed-object.html#the-object-element")]
    [DataRow("<object id=x data='a.swf' type='application/x-shockwave-flash'>fallback</object>")]
    [DataRow("<object id=x>fallback</object>")]
    public void ObjectWithoutDisplayableData_RepresentsItsFallbackContent(string body)
    {
        StringAssert.Contains(Text(Loaded(body)), "fallback");
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "iframe-embed-object.html#the-object-element")]
    public void ObjectWithImageData_DisplaysTheImage()
    {
        var obj = Loaded($"<object id=x data='{Png}' type='image/png'>fallback</object>");

        Assert.IsFalse(Text(obj).Contains("fallback"));
        Assert.AreEqual(40, obj.Rectangles.Values.Single().Width, 0.5);
    }
}
