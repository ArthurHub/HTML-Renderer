using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class ObsoleteFeaturesTests
{
    [TestMethod]
    [Spec("html", "obsolete.html#other-elements,-attributes-and-apis")]
    public void Dir_IsRenderedLikeUl()
    {
        var dir = Box("<dir id=x><li>a</li></dir>");
        var ul = Box("<ul id=x><li>a</li></ul>");

        Assert.AreEqual(ul.Display, dir.Display);
        Assert.AreEqual(ul.ActualMarginLeft, dir.ActualMarginLeft);
        Assert.AreEqual(ul.ActualMarginTop, dir.ActualMarginTop);
        Assert.AreEqual(Marker("<ul><li id=x>a</li></ul>"), Marker("<dir><li id=x>a</li></dir>"));
    }

    [TestMethod]
    [Spec("html", "obsolete.html#other-elements,-attributes-and-apis")]
    public void Acronym_IsRenderedLikeAbbr()
    {
        var acronym = Box("<acronym id=x title=t>a</acronym>");
        var abbr = Box("<abbr id=x title=t>a</abbr>");

        Assert.AreEqual(abbr.Display, acronym.Display);
        Assert.AreEqual(abbr.Boxes[0].TextDecoration, acronym.Boxes[0].TextDecoration);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "obsolete.html#other-elements,-attributes-and-apis")]
    [DataRow("listing")]
    [DataRow("plaintext")]
    [DataRow("xmp")]
    public void PreLikeElements_AreRenderedLikePre(string tag)
    {
        var box = Box($"<{tag} id=x>a</{tag}>");
        var pre = Box("<pre id=x>a</pre>");

        Assert.AreEqual(pre.Display, box.Display);
        Assert.AreEqual(pre.WhiteSpace, box.WhiteSpace);
        Assert.AreEqual(pre.ActualFont.Size, box.ActualFont.Size);
    }
}
