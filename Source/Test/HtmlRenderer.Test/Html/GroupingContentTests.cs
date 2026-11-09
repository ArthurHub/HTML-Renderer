using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class GroupingContentTests
{
    [TestMethod]
    [Spec("html", "grouping-content.html#the-p-element")]
    [Spec("html", "grouping-content.html#the-hr-element")]
    [Spec("html", "grouping-content.html#the-pre-element")]
    [Spec("html", "grouping-content.html#the-blockquote-element")]
    [Spec("html", "grouping-content.html#the-dl-element")]
    [Spec("html", "grouping-content.html#the-dt-element")]
    [Spec("html", "grouping-content.html#the-dd-element")]
    [Spec("html", "grouping-content.html#the-div-element")]
    [DataRow("p")]
    [DataRow("hr")]
    [DataRow("pre")]
    [DataRow("blockquote")]
    [DataRow("dl")]
    [DataRow("dt")]
    [DataRow("dd")]
    [DataRow("div")]
    public void Element_IsABlock(string tag)
    {
        Assert.AreEqual("block", Box($"<{tag} id=x>a</{tag}>").Display);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "grouping-content.html#the-figure-element")]
    [Spec("html", "grouping-content.html#the-figcaption-element")]
    [Spec("html", "grouping-content.html#the-main-element")]
    [Spec("html", "grouping-content.html#the-search-element")]
    [DataRow("figure")]
    [DataRow("figcaption")]
    [DataRow("main")]
    [DataRow("search")]
    public void NewerElement_IsABlock(string tag)
    {
        Assert.AreEqual("block", Box($"<{tag} id=x>a</{tag}>").Display);
    }

    [TestMethod]
    [Spec("html", "grouping-content.html#the-ul-element")]
    [Spec("html", "grouping-content.html#the-menu-element")]
    [Spec("html", "grouping-content.html#the-li-element")]
    public void UnorderedListItems_HaveADiscMarker()
    {
        Assert.AreEqual("disc", Box("<ul><li id=x>a</li></ul>").ListStyleType);
        Assert.AreEqual("disc", Box("<menu><li id=x>a</li></menu>").ListStyleType);
    }

    [TestMethod]
    [Spec("html", "grouping-content.html#the-ol-element")]
    [Spec("html", "grouping-content.html#the-li-element")]
    public void OrderedListItems_AreNumberedFromOne()
    {
        Assert.AreEqual("2.", Marker("<ol><li>a</li><li id=x>b</li></ol>"));
    }

    [TestMethod]
    [Spec("html", "grouping-content.html#the-ol-element")]
    public void Start_SetsTheFirstNumber()
    {
        Assert.AreEqual("6.", Marker("<ol start=5><li>a</li><li id=x>b</li></ol>"));
        Assert.AreEqual("-1.", Marker("<ol start=-2><li>a</li><li id=x>b</li></ol>"));
    }

    [TestMethod]
    [Spec("html", "grouping-content.html#the-ol-element")]
    public void Reversed_CountsDownFromTheNumberOfItems()
    {
        Assert.AreEqual("1.", Marker("<ol reversed=reversed><li>a</li><li id=x>b</li></ol>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "grouping-content.html#the-ol-element")]
    public void Reversed_WithoutAValue_CountsDown()
    {
        Assert.AreEqual("1.", Marker("<ol reversed><li>a</li><li id=x>b</li></ol>"));
    }

    [TestMethod]
    [Spec("html", "grouping-content.html#the-ol-element")]
    public void Reversed_WithStart_CountsDownFromStart()
    {
        Assert.AreEqual("4.", Marker("<ol reversed=reversed start=5><li>a</li><li id=x>b</li></ol>"));
    }

    [TestMethod]
    [Spec("html", "grouping-content.html#the-li-element")]
    public void Value_SetsThisAndTheFollowingNumbers()
    {
        Assert.AreEqual("7.", Marker("<ol><li>a</li><li id=x value=7>b</li></ol>"));
        Assert.AreEqual("8.", Marker("<ol><li value=7>a</li><li id=x>b</li></ol>"));
    }
}
