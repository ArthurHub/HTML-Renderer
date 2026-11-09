using static HtmlRenderer.Test.Html.EmbeddedTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class MediaTests
{
    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "media.html#the-video-element")]
    [Spec("html", "media.html#the-audio-element")]
    [DataRow("video")]
    [DataRow("audio")]
    public void FallbackContent_IsNotShown(string tag)
    {
        Assert.AreEqual("", Text(Loaded($"<{tag} id=x controls>fallback</{tag}>")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "media.html#the-video-element")]
    public void Video_RepresentsItsPosterFrame()
    {
        var video = Loaded($"<video id=x poster='{Png}'></video>");

        Assert.AreEqual(40, video.Rectangles.Values.Single().Width, 0.5);
    }
}
