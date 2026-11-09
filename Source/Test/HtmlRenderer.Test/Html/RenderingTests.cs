using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class RenderingTests
{
    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);

    /// <summary>Lays out a whole document, without the zero body margin <see cref="Box"/> adds.</summary>
    private static CssBox Body(string html) =>
        LayoutHarness.Descendants(LayoutHarness.Layout(html).Root).First(b => b.HtmlTag?.Name == "body");

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-css-user-agent-style-sheet-and-presentational-hints")]
    public void Attributes_AreOnlyMappedOnTheElementsTheyBelongTo()
    {
        Assert.AreEqual("auto", Box("<div id=x width=50>a</div>").Width);
        Assert.AreEqual("normal", Box("<span id=x nowrap>a</span>").WhiteSpace);
        Assert.AreEqual("transparent", Box("<span id=x bgcolor=red>a</span>").BackgroundColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-css-user-agent-style-sheet-and-presentational-hints")]
    public void LegacyColors_WithoutAHash_AreParsed()
    {
        Assert.AreEqual(Red, Box("<table id=x bgcolor=ff0000><tr><td>a</td></tr></table>").ActualBackgroundColor);
        Assert.AreEqual(RColor.FromArgb(192, 0, 0), Box("<table id=x bgcolor=chucknorris><tr><td>a</td></tr></table>").ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-css-user-agent-style-sheet-and-presentational-hints")]
    public void DimensionValues_IgnoreTrailingGarbage()
    {
        Assert.AreEqual(100, Box("<table id=x width=100abc><tr><td>a</td></tr></table>").Size.Width, 0.01);
    }

    [TestMethod]
    [Spec("html", "rendering.html#hidden-elements")]
    [DataRow("<div><style id=x>p {}</style></div>")]
    [DataRow("<div><script id=x>a</script></div>")]
    [DataRow("<div><area id=x></div>")]
    [DataRow("<div><param id=x></div>")]
    public void MetadataAndScripts_AreNotDisplayed(string html)
    {
        Assert.AreEqual("none", Box(html).Display);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#hidden-elements")]
    [DataRow("<p id=x hidden>a</p>")]
    [DataRow("<basefont id=x>")]
    [DataRow("<template id=x>a</template>")]
    [DataRow("<datalist id=x>a</datalist>")]
    [DataRow("<noembed id=x>a</noembed>")]
    [DataRow("<noframes id=x>a</noframes>")]
    [DataRow("<rp id=x>a</rp>")]
    [DataRow("<dialog id=x>a</dialog>")]
    [DataRow("<input id=x type=hidden>")]
    public void HiddenElements_AreNotDisplayed(string html)
    {
        Assert.AreEqual("none", Box(html).Display);
    }

    [TestMethod]
    [Spec("html", "rendering.html#the-page")]
    public void Body_IsABlockWithAnEightPixelMargin()
    {
        var body = Body("<html><body>a</body></html>");

        Assert.AreEqual("block", body.Display);
        Assert.AreEqual(8, body.ActualMarginTop);
        Assert.AreEqual(8, body.ActualMarginLeft);
    }

    [TestMethod]
    [Spec("html", "rendering.html#the-page")]
    public void BodyBgcolor_SetsItsBackgroundColor()
    {
        Assert.AreEqual(Red, Body("<html><body bgcolor=red>a</body></html>").ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-page")]
    public void BodyBackground_SetsItsBackgroundImage()
    {
        var image = Body("<html><body background=Img/A.png>a</body></html>").ActualBackgroundImage;

        Assert.AreEqual("Img/A.png", (image as CssImage.Url)?.Href);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-page")]
    [DataRow("marginwidth=20 marginheight=10")]
    [DataRow("leftmargin=20 topmargin=10")]
    public void BodyMarginAttributes_SetItsMargins(string attributes)
    {
        var body = Body($"<html><body {attributes}>a</body></html>");

        Assert.AreEqual(10, body.ActualMarginTop);
        Assert.AreEqual(20, body.ActualMarginLeft);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-page")]
    public void BodyTextAndLink_SetTheTextAndLinkColors()
    {
        var (root, _) = LayoutHarness.Layout("<html><body text=red link=lime><p id=p>a</p><a id=a href=#>b</a></body></html>");

        Assert.AreEqual(Red, LayoutHarness.FindById(root, "p")!.ActualColor);
        Assert.AreEqual(RColor.FromArgb(0, 255, 0), LayoutHarness.FindById(root, "a")!.ActualColor);
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
    [DataRow("figcaption")]
    [DataRow("figure")]
    [DataRow("footer")]
    [DataRow("header")]
    [DataRow("legend")]
    [DataRow("listing")]
    [DataRow("main")]
    [DataRow("plaintext")]
    [DataRow("search")]
    [DataRow("xmp")]
    public void NewerFlowContent_IsABlock(string tag)
    {
        Assert.AreEqual("block", Box($"<{tag} id=x>a</{tag}>").Display);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#flow-content-3")]
    [DataRow("listing")]
    [DataRow("plaintext")]
    [DataRow("xmp")]
    public void PreLikeElements_PreserveWhiteSpace(string tag)
    {
        Assert.AreEqual("pre", Box($"<{tag} id=x>a</{tag}>").WhiteSpace);
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
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#flow-content-3")]
    public void Form_HasNoVerticalMargins()
    {
        var box = Box("<form id=x>a</form>");

        Assert.AreEqual(0, box.ActualMarginTop);
        Assert.AreEqual(0, box.ActualMarginBottom);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#flow-content-3")]
    public void OpenDialog_IsAnAbsolutelyPositionedBorderedBlock()
    {
        var box = Box("<dialog id=x open>a</dialog>");

        Assert.AreEqual("block", box.Display);
        Assert.AreEqual("absolute", box.Position);
        Assert.AreEqual("solid", box.BorderTopStyle);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#flow-content-3")]
    public void Popover_IsNotDisplayedUntilOpened()
    {
        Assert.AreEqual("none", Box("<div id=x popover>a</div>").Display);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#flow-content-3")]
    public void PreWrapAttribute_WrapsTheText()
    {
        Assert.AreEqual("pre-wrap", Box("<pre id=x wrap>a</pre>").WhiteSpace);
    }

    [TestMethod]
    [Spec("html", "rendering.html#flow-content-3")]
    [DataRow("<center id=x>a</center>", "center")]
    [DataRow("<div id=x align=center>a</div>", "center")]
    [DataRow("<div id=x align=left>a</div>", "left")]
    [DataRow("<div id=x align=right>a</div>", "right")]
    [DataRow("<div id=x align=justify>a</div>", "justify")]
    public void CenterAndDivAlign_AlignTheText(string html, string textAlign)
    {
        Assert.AreEqual(textAlign, Box(html).TextAlign);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#flow-content-3")]
    public void DivAlignMiddle_CentersTheText()
    {
        Assert.AreEqual("center", Box("<div id=x align=middle>a</div>").TextAlign);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#flow-content-3")]
    [DataRow("<center><div id=x style='width:100px'>a</div></center>")]
    [DataRow("<div align=center><div id=x style='width:100px'>a</div></div>")]
    public void CenterAndDivAlign_CenterDescendantBlocks(string html)
    {
        Assert.AreEqual(450, Box(html).Location.X, 0.01);
    }

    [TestMethod]
    [Spec("html", "rendering.html#sections-and-headings")]
    [DataRow("h1", 32)]
    [DataRow("h2", 24)]
    [DataRow("h3", 18.72)]
    [DataRow("h4", 16)]
    [DataRow("h5", 13.28)]
    public void Headings_HaveDecreasingFontSizes(string tag, double px)
    {
        var box = Box($"<div style='font-size:16px'><{tag} id=x>a</{tag}></div>");

        Assert.AreEqual("block", box.Display);
        Assert.AreEqual(px * 0.75, box.ActualFont.Size, 0.05);
        Assert.IsTrue(box.ActualFontWeight >= 700);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#sections-and-headings")]
    public void H6_IsTwoThirdsOfAnEm()
    {
        Assert.AreEqual(16 * 0.67 * 0.75, Box("<div style='font-size:16px'><h6 id=x>a</h6></div>").ActualFont.Size, 0.05);
    }

    [TestMethod]
    [Spec("html", "rendering.html#sections-and-headings")]
    public void H1_HasTwoThirdsOfAnEmVerticalMargins()
    {
        var box = Box("<h1 id=x style='font-size:16px'>a</h1>");

        Assert.AreEqual(16 * 0.67, box.ActualMarginTop, 0.01);
        Assert.AreEqual(16 * 0.67, box.ActualMarginBottom, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#sections-and-headings")]
    [DataRow("h2", 0.83)]
    [DataRow("h3", 1.00)]
    [DataRow("h4", 1.33)]
    [DataRow("h5", 1.67)]
    [DataRow("h6", 2.33)]
    public void Headings_HaveTheirVerticalMargins(string tag, double em)
    {
        var box = Box($"<{tag} id=x style='font-size:16px'>a</{tag}>");

        Assert.AreEqual(16 * em, box.ActualMarginTop, 0.01);
        Assert.AreEqual(16 * em, box.ActualMarginBottom, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#sections-and-headings")]
    [DataRow("article")]
    [DataRow("aside")]
    [DataRow("hgroup")]
    [DataRow("nav")]
    [DataRow("section")]
    public void Sections_AreBlocks(string tag)
    {
        Assert.AreEqual("block", Box($"<{tag} id=x>a</{tag}>").Display);
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
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#lists")]
    public void NestedMenusAndDirs_SwitchToCircle()
    {
        Assert.AreEqual("circle", Box("<menu><li><menu><li id=x>a</li></menu></li></menu>").ListStyleType);
        Assert.AreEqual("circle", Box("<ul><li><dir><li id=x>a</li></dir></li></ul>").ListStyleType);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#lists")]
    [DataRow("dl")]
    [DataRow("dir")]
    [DataRow("menu")]
    public void ListsNestedInLists_HaveNoVerticalMargins(string tag)
    {
        var box = Box($"<ul><li><{tag} id=x><li>a</li></{tag}></li></ul>");

        Assert.AreEqual(0, box.ActualMarginTop);
        Assert.AreEqual(0, box.ActualMarginBottom);
    }

    [TestMethod]
    [Spec("html", "rendering.html#lists")]
    public void DescriptionDetails_AreIndented()
    {
        Assert.AreEqual(40, Box("<dl><dd id=x>a</dd></dl>").ActualMarginLeft);
    }

    [TestMethod]
    [Spec("html", "rendering.html#lists")]
    public void StartAndValue_SetTheOrdinal()
    {
        Assert.AreEqual("5.", Marker("<ol start=5><li id=x>a</li></ol>"));
        Assert.AreEqual("8.", Marker("<ol><li value=7>a</li><li id=x>b</li></ol>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#lists")]
    public void Reversed_CountsDown()
    {
        Assert.AreEqual("1.", Marker("<ol reversed><li>a</li><li id=x>b</li></ol>"));
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
    [Spec("html", "rendering.html#the-hr-element-2")]
    public void HrWidth_SetsItsWidth()
    {
        Assert.AreEqual(50, Box("<hr id=x width=50>").Size.Width, 0.01);
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

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-hr-element-2")]
    [DataRow("center", 50)]
    [DataRow("right", 100)]
    public void HrAlign_SetsItsMargins(string align, double x)
    {
        Assert.AreEqual(x, Box($"<div style='width:200px'><hr id=x align={align} width=100></div>").Location.X, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-hr-element-2")]
    public void HrSize_SetsTheHeightWithoutTheBorders()
    {
        Assert.AreEqual("8px", Box("<hr id=x size=10>").Height);
        Assert.AreEqual(0, Box("<hr id=x size=1>").ActualBorderBottomWidth);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-hr-element-2")]
    public void HrNoshadeAndColor_DrawASolidBorder()
    {
        var noshade = Box("<hr id=x noshade size=10>");
        Assert.AreEqual("solid", noshade.BorderTopStyle);
        Assert.AreEqual(5, noshade.ActualBorderTopWidth);

        var colored = Box("<hr id=x color=red>");
        Assert.AreEqual("solid", colored.BorderTopStyle);
        Assert.AreEqual(Red, colored.ActualBorderTopColor);
    }
}
