using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class SectionsTests
{
    [TestMethod]
    [Spec("html", "sections.html#the-body-element")]
    public void Body_HoldsTheContent()
    {
        var body = Box("<p id=x>a</p>").ParentBox!;

        Assert.AreEqual("body", body.HtmlTag?.Name);
        Assert.AreEqual("block", body.Display);
    }

    [TestMethod]
    [Spec("html", "sections.html#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements")]
    [Spec("html", "sections.html#the-address-element")]
    [DataRow("h1")]
    [DataRow("h2")]
    [DataRow("h3")]
    [DataRow("h4")]
    [DataRow("h5")]
    [DataRow("h6")]
    [DataRow("address")]
    public void Element_IsABlock(string tag)
    {
        Assert.AreEqual("block", Box($"<{tag} id=x>a</{tag}>").Display);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "sections.html#the-article-element")]
    [Spec("html", "sections.html#the-section-element")]
    [Spec("html", "sections.html#the-nav-element")]
    [Spec("html", "sections.html#the-aside-element")]
    [Spec("html", "sections.html#the-hgroup-element")]
    [Spec("html", "sections.html#the-header-element")]
    [Spec("html", "sections.html#the-footer-element")]
    [DataRow("article")]
    [DataRow("section")]
    [DataRow("nav")]
    [DataRow("aside")]
    [DataRow("hgroup")]
    [DataRow("header")]
    [DataRow("footer")]
    public void SectioningElement_IsABlock(string tag)
    {
        Assert.AreEqual("block", Box($"<{tag} id=x>a</{tag}>").Display);
    }

    [TestMethod]
    [Spec("html", "sections.html#heading-levels-&-offsets")]
    public void HigherHeadingLevel_IsSmaller()
    {
        var sizes = Enumerable.Range(1, 6).Select(level => Box($"<h{level} id=x>a</h{level}>").ActualFont.Size).ToArray();

        CollectionAssert.AreEqual(sizes.OrderByDescending(s => s).ToArray(), sizes);
        Assert.AreEqual(6, sizes.Distinct().Count());
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "sections.html#heading-levels-&-offsets")]
    public void HeadingOffset_RaisesTheHeadingLevel()
    {
        var h2 = Box("<h2 id=x>a</h2>").ActualFont.Size;

        Assert.AreEqual(h2, Box("<div headingoffset=1><h1 id=x>a</h1></div>").ActualFont.Size);
        Assert.AreEqual(h2, Box("<div headingoffset=3><div headingreset headingoffset=1><h1 id=x>a</h1></div></div>").ActualFont.Size);
    }
}
