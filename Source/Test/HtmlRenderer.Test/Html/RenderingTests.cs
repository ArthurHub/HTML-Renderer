using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class RenderingTests
{
    [TestMethod]
    [Spec("html", "rendering.html#hidden-elements")]
    [DataRow("<div><style id=x>p {}</style></div>")]
    [DataRow("<div><script id=x>a</script></div>")]
    public void MetadataAndScripts_AreNotDisplayed(string html)
    {
        Assert.AreEqual("none", Box(html).Display);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#hidden-elements")]
    [DataRow("<p id=x hidden>a</p>")]
    [DataRow("<template id=x>a</template>")]
    [DataRow("<datalist id=x>a</datalist>")]
    [DataRow("<noembed id=x>a</noembed>")]
    [DataRow("<rp id=x>a</rp>")]
    [DataRow("<dialog id=x>a</dialog>")]
    public void HiddenElements_AreNotDisplayed(string html)
    {
        Assert.AreEqual("none", Box(html).Display);
    }

    [TestMethod]
    [Spec("html", "rendering.html#flow-content-3")]
    [DataRow("address")]
    [DataRow("blockquote")]
    [DataRow("center")]
    [DataRow("div")]
    [DataRow("form")]
    [DataRow("hr")]
    [DataRow("p")]
    [DataRow("pre")]
    public void FlowContent_IsABlock(string tag)
    {
        Assert.AreEqual("block", Box($"<{tag} id=x>a</{tag}>").Display);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#flow-content-3")]
    [DataRow("article")]
    [DataRow("aside")]
    [DataRow("details")]
    [DataRow("figcaption")]
    [DataRow("figure")]
    [DataRow("footer")]
    [DataRow("header")]
    [DataRow("hgroup")]
    [DataRow("legend")]
    [DataRow("listing")]
    [DataRow("main")]
    [DataRow("nav")]
    [DataRow("plaintext")]
    [DataRow("search")]
    [DataRow("section")]
    [DataRow("summary")]
    [DataRow("xmp")]
    public void NewerFlowContent_IsABlock(string tag)
    {
        Assert.AreEqual("block", Box($"<{tag} id=x>a</{tag}>").Display);
    }

    [TestMethod]
    [Spec("html", "rendering.html#flow-content-3")]
    public void Blockquote_IsIndentedOnBothSides()
    {
        var box = Box("<blockquote id=x>a</blockquote>");

        Assert.AreEqual(40, box.ActualMarginLeft);
        Assert.AreEqual(40, box.ActualMarginRight);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#flow-content-3")]
    [DataRow("p")]
    [DataRow("blockquote")]
    [DataRow("pre")]
    [DataRow("figure")]
    public void Paragraphs_HaveOneEmVerticalMargins(string tag)
    {
        var box = Box($"<{tag} id=x style='font-size:16px'>a</{tag}>");

        Assert.AreEqual(16, box.ActualMarginTop, 0.01);
        Assert.AreEqual(16, box.ActualMarginBottom, 0.01);
    }

    [TestMethod]
    [Spec("html", "rendering.html#lists")]
    public void NestedUnorderedLists_SwitchToCircleThenSquare()
    {
        Assert.AreEqual("circle", Box("<ul><li><ul><li id=x>a</li></ul></li></ul>").ListStyleType);
        Assert.AreEqual("circle", Box("<ol><li><ul><li id=x>a</li></ul></li></ol>").ListStyleType);
        Assert.AreEqual("square", Box("<ul><li><ul><li><ul><li id=x>a</li></ul></li></ul></li></ul>").ListStyleType);
        Assert.AreEqual("decimal", Box("<ol><li><ol><li id=x>a</li></ol></li></ol>").ListStyleType);
    }

    [TestMethod]
    [Spec("html", "rendering.html#lists")]
    public void DescriptionDetails_AreIndented()
    {
        Assert.AreEqual(40, Box("<dl><dd id=x>a</dd></dl>").ActualMarginLeft);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#lists")]
    public void Lists_AreIndentedWithPadding()
    {
        var box = Box("<ul id=x><li>a</li></ul>");

        Assert.AreEqual(40, box.ActualPaddingLeft);
        Assert.AreEqual(0, box.ActualMarginLeft);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#lists")]
    public void TypeAttribute_SetsTheMarker()
    {
        Assert.AreEqual("b.", Marker("<ol type=a><li>a</li><li id=x>b</li></ol>"));
        Assert.AreEqual("II.", Marker("<ol type=I><li>a</li><li id=x>b</li></ol>"));
        Assert.AreEqual("B.", Marker("<ol><li>a</li><li id=x type=A>b</li></ol>"));
        Assert.AreEqual("square", Box("<ul type=square><li id=x>a</li></ul>").ListStyleType);
    }

    [TestMethod]
    [Spec("html", "rendering.html#the-hr-element-2")]
    public void Hr_HasAnInsetOnePixelBorder()
    {
        var box = Box("<hr id=x>");

        Assert.AreEqual("inset", box.BorderTopStyle);
        Assert.AreEqual(1, box.ActualBorderTopWidth);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-hr-element-2")]
    public void Hr_IsCenteredWithHalfAnEmVerticalMargin()
    {
        var box = Box("<div style='font-size:16px;width:200px'><hr id=x style='width:100px'></div>");

        Assert.AreEqual(8, box.ActualMarginTop, 0.01);
        Assert.AreEqual(50, box.ActualMarginLeft, 0.01);
        Assert.AreEqual("gray", box.Color);
    }
}
