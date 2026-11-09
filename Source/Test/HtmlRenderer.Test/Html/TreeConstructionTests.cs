using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class TreeConstructionTests
{
    [TestMethod]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void BlockStartTag_ClosesAnOpenParagraph()
    {
        Assert.AreEqual("body", ParentTag("<p>a<div id=x>b</div>"));
        Assert.AreEqual("body", ParentTag("<p>a<ul id=x><li>b</li></ul>"));
        Assert.AreEqual("body", ParentTag("<p>a<p id=x>b"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void BlockStartTag_ClosesAnOpenParagraph_WithAnInlineElementStillOpen()
    {
        Assert.AreEqual("body", ParentTag("<p><b>a<div id=x>b</div>"));
        Assert.AreEqual("body", ParentTag("<p><b>a<p id=x>b"));
    }

    [TestMethod]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void Pre_DropsTheNewlineRightAfterTheStartTag()
    {
        Assert.AreEqual("a", Box("<pre id=x>\na</pre>").Boxes[0].Text);
    }

    [TestMethod]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void Textarea_ContentIsText()
    {
        Assert.AreEqual(0, Box("<textarea id=x><b>a</b></textarea>").Boxes.Count(b => b.HtmlTag?.Name == "b"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void ListItem_ClosesThePreviousListItem()
    {
        Assert.AreEqual("ul", ParentTag("<ul><li>a<li id=x>b</ul>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void DescriptionTerm_AndDetails_CloseEachOther()
    {
        Assert.AreEqual("dl", ParentTag("<dl><dt>a<dd id=x>b</dl>"));
        Assert.AreEqual("dl", ParentTag("<dl><dd>a<dt id=x>b</dl>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void Option_ClosesThePreviousOption()
    {
        Assert.AreEqual("select", ParentTag("<select><option>a<option id=x>b</select>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void UnmatchedEndTag_IsIgnored()
    {
        Assert.AreEqual("span", ParentTag("<div><span>a</i><b id=x>b</b></span></div>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void UnmatchedParagraphEndTag_InsertsAnEmptyParagraph()
    {
        Assert.IsTrue(Box("<div id=x></p></div>").Boxes.Any(b => b.HtmlTag?.Name == "p"));
    }

    [TestMethod]
    [Spec("html", "parsing.html#parsing-main-intable")]
    public void Caption_IsAChildOfTheTable()
    {
        Assert.AreEqual("table", ParentTag("<table><caption id=x>a</caption><tr><td>b</table>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-intable")]
    public void TableRow_OutsideASection_GetsAnImpliedTbody()
    {
        Assert.AreEqual("tbody", ParentTag("<table><tr id=x><td>a</table>"));
    }

    [TestMethod]
    [Spec("html", "parsing.html#parsing-main-intbody")]
    [Spec("html", "parsing.html#parsing-main-intr")]
    public void TableRow_ClosesThePreviousRow()
    {
        Assert.AreEqual("tbody", ParentTag("<table><tbody><tr><td>a<tr id=x><td>b</table>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-intbody")]
    public void TableSection_ClosesThePreviousSection()
    {
        Assert.AreEqual("table", ParentTag("<table><thead><tr><td>a<tbody id=x><tr><td>b</table>"));
    }

    [TestMethod]
    [Spec("html", "parsing.html#parsing-main-intd")]
    public void DataCell_ClosesThePreviousCell()
    {
        Assert.AreEqual("tr", ParentTag("<table><tr><td>a<td id=x>b</table>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-intd")]
    public void HeaderCell_ClosesThePreviousCell()
    {
        Assert.AreEqual("tr", ParentTag("<table><tr><th>a<th id=x>b</table>"));
    }

    [TestMethod]
    [Spec("html", "parsing.html#the-stack-of-open-elements")]
    public void EndTag_ClosesTheElementsOpenedInsideIt()
    {
        Assert.AreEqual("body", ParentTag("<div><span><b>a</div><p id=x>b</p>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#the-stack-of-open-elements")]
    public void EndTag_DoesNotCloseElementsOutsideATableCell()
    {
        Assert.AreEqual("td", ParentTag("<b><table><tr><td></b><span id=x>a</span></table>"));
    }

    [TestMethod]
    [Spec("html", "parsing.html#other-parsing-state-flags")]
    public void Noscript_ContentIsParsedAsMarkup()
    {
        Assert.AreEqual("noscript", ParentTag("<noscript><p id=x>a</p></noscript>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#other-parsing-state-flags")]
    public void Frameset_AfterBodyContent_IsIgnored()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p>a</p><frameset id=x><frame></frameset>"));
        Assert.IsNull(LayoutHarness.FindById(root, "x"));
    }

    [TestMethod]
    [Spec("html", "parsing.html#creating-and-inserting-nodes")]
    public void StartTag_AttributesAreCopiedToTheElement()
    {
        var tag = Box("<p id=x title='a b' lang=c>d</p>").HtmlTag!;
        Assert.AreEqual("a b", tag.TryGetAttribute("title"));
        Assert.AreEqual("c", tag.TryGetAttribute("lang"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#creating-and-inserting-nodes")]
    public void MisplacedContentInATable_IsFosterParentedBeforeTheTable()
    {
        Assert.AreEqual("body", ParentTag("<table><tr><td>a</td></tr><span id=x>b</span></table>"));
        Assert.AreEqual("b", Box("<div id=x><table><tr><td>a</td></tr>b</table></div>").Boxes[0].Text);
    }

    [TestMethod]
    [Spec("html", "parsing.html#creating-and-inserting-nodes")]
    public void ConsecutiveCharacters_AreInsertedIntoOneTextNode()
    {
        Assert.AreEqual(1, Box("<style id=x>a { } b < c { }</style>").Boxes.Count);
    }

    [TestMethod]
    [Spec("html", "parsing.html#parsing-elements-that-contain-only-text")]
    [Spec("html", "parsing.html#parsing-main-incdata")]
    public void TextOnlyElements_KeepMarkupAsText()
    {
        Assert.AreEqual("<b>a</b> &", Text(Box("<textarea id=x><b>a</b> &amp;</textarea>")));
        Assert.AreEqual("<b>a</b> &amp;", Text(Box("<xmp id=x><b>a</b> &amp;</xmp>")));
        Assert.AreEqual("body", ParentTag("<title>a <b></TITLE ><p id=x>b</p>"));
    }

    [TestMethod]
    [Spec("html", "parsing.html#closing-elements-that-have-implied-end-tags")]
    public void EndTag_ClosesAnOpenListItem()
    {
        Assert.AreEqual("body", ParentTag("<div><ul><li>a</div><p id=x>b</p>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#closing-elements-that-have-implied-end-tags")]
    public void RubyText_ClosesThePreviousRubyText()
    {
        Assert.AreEqual("ruby", ParentTag("<ruby>a<rt>b<rt id=x>c</ruby>"));
        Assert.AreEqual("ruby", ParentTag("<ruby>a<rt>b<rp id=x>c</ruby>"));
    }

    [TestMethod]
    [Spec("html", "parsing.html#the-initial-insertion-mode")]
    public void Doctype_DoesNotCreateABox()
    {
        var p = Document("<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.01//EN\"><html><body><p id=x>a</p></body></html>");
        Assert.AreEqual("a", Text(p.ParentBox!.ParentBox!.ParentBox!));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#the-initial-insertion-mode")]
    public void DocumentWithoutDoctype_IsInQuirksMode_WhereATableDoesNotCloseAParagraph()
    {
        Assert.AreEqual("p", ParentTag("<p>a<table id=x><tr><td>b</table>"));
    }

    [TestMethod]
    [Spec("html", "parsing.html#the-before-html-insertion-mode")]
    public void HtmlStartTag_CreatesTheRootElement()
    {
        Assert.AreEqual("html", Document("<html id=x><body>a</body></html>").HtmlTag!.Name);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#the-before-html-insertion-mode")]
    public void MissingHtmlStartTag_IsImplied()
    {
        Assert.AreEqual("html", Document("<head></head><body id=x>a</body>").ParentBox!.HtmlTag?.Name);
    }

    [TestMethod]
    [Spec("html", "parsing.html#the-before-head-insertion-mode")]
    public void HeadStartTag_CreatesTheHeadElement()
    {
        Assert.AreEqual("html", Document("<html><head id=x></head><body>a</body></html>").ParentBox!.HtmlTag?.Name);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#the-before-head-insertion-mode")]
    public void MissingHeadStartTag_IsImplied()
    {
        Assert.AreEqual("head", Document("<html><title id=x>t</title><body>a</body></html>").ParentBox!.HtmlTag?.Name);
    }

    [TestMethod]
    [Spec("html", "parsing.html#parsing-main-inhead")]
    public void StyleInHead_Applies()
    {
        Assert.AreEqual("rgb(255, 0, 0)", Document("<html><head><style>#x { color: red }</style></head><body><p id=x>a</p></body></html>").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inhead")]
    public void BodyContentInHead_ClosesTheHead()
    {
        Assert.AreEqual("body", Document("<html><head><p id=x>a</p></head><body></body></html>").ParentBox!.HtmlTag?.Name);
    }

    [TestMethod]
    [Spec("html", "parsing.html#parsing-main-inheadnoscript")]
    public void StyleInNoscriptInHead_Applies()
    {
        Assert.AreEqual("rgb(255, 0, 0)", Document("<html><head><noscript><style>#x { color: red }</style></noscript></head><body><p id=x>a</p></body></html>").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inheadnoscript")]
    public void BodyContentInNoscriptInHead_ClosesTheHead()
    {
        Assert.AreEqual("body", Document("<html><head><noscript><p id=x>a</p></noscript></head><body></body></html>").ParentBox!.HtmlTag?.Name);
    }

    [TestMethod]
    [Spec("html", "parsing.html#the-after-head-insertion-mode")]
    public void BodyStartTag_AfterTheHead_CreatesTheBody()
    {
        Assert.AreEqual("html", Document("<html><head></head><body id=x>a</body></html>").ParentBox!.HtmlTag?.Name);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#the-after-head-insertion-mode")]
    public void MissingBodyStartTag_IsImplied()
    {
        Assert.AreEqual("body", Document("<html><head></head><p id=x>a</p></html>").ParentBox!.HtmlTag?.Name);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void SecondBodyStartTag_AddsItsAttributesToTheBody()
    {
        var body = Document("<html><head></head><body id=x><body class=c>a</body></html>");
        Assert.AreEqual("c", body.HtmlTag!.TryGetAttribute("class"));
        Assert.IsFalse(body.Boxes.Any(b => b.HtmlTag?.Name == "body"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void SelfClosingSlash_OnANonVoidElement_IsIgnored()
    {
        Assert.AreEqual("a", Text(Box("<div id=\"x\"/>a</div>")));
    }

    [TestMethod]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    [DataRow("area")]
    [DataRow("img")]
    [DataRow("input")]
    [DataRow("hr")]
    [DataRow("param")]
    [DataRow("link")]
    [DataRow("meta")]
    [DataRow("basefont")]
    public void VoidElement_HasNoContent(string tag)
    {
        Assert.AreEqual(0, Box($"<{tag} id=x>a</{tag}>").Boxes.Count);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void ColStartTag_OutsideATable_IsIgnored()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<col id=x>a"));
        Assert.IsNull(LayoutHarness.FindById(root, "x"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    [DataRow("embed")]
    [DataRow("keygen")]
    [DataRow("source")]
    [DataRow("track")]
    [DataRow("wbr")]
    [DataRow("bgsound")]
    public void VoidElement_HasNoContent_Missing(string tag)
    {
        Assert.AreEqual(0, Box($"<p><{tag} id=x>a</p>").Boxes.Count);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void Isindex_IsAnOrdinaryElement()
    {
        Assert.AreEqual("a", Text(Box("<isindex id=x>a</isindex>")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void BrEndTag_InsertsABr()
    {
        Assert.IsTrue(Box("<p id=x>a</br>b</p>").Boxes.Any(b => b.HtmlTag?.Name == "br"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void ImageStartTag_BecomesImg()
    {
        Assert.AreEqual("img", Box("<image id=x src=a>").HtmlTag!.Name);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void NullCharacter_InText_IsDropped()
    {
        Assert.AreEqual("ab", Text(Box("<p id=x>a\0b</p>")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    [DataRow("center")]
    [DataRow("dir")]
    [DataRow("summary")]
    [DataRow("listing")]
    [DataRow("xmp")]
    [DataRow("plaintext")]
    [DataRow("li")]
    [DataRow("dd")]
    [DataRow("dt")]
    public void BlockStartTag_ClosesAnOpenParagraph_Missing(string tag)
    {
        Assert.AreEqual("body", ParentTag($"<p>a<{tag} id=x>b"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void HeadingStartTag_ClosesAnOpenHeading()
    {
        Assert.AreEqual("body", ParentTag("<h1>a<h2 id=x>b</h2>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void ButtonStartTag_ClosesAnOpenButton()
    {
        Assert.AreEqual("body", ParentTag("<button>a<button id=x>b</button>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void FormStartTag_InsideAForm_IsIgnored()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<form><form id=x>a</form>"));
        Assert.IsNull(LayoutHarness.FindById(root, "x"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void AnchorStartTag_ClosesAnOpenAnchor()
    {
        Assert.AreEqual("body", ParentTag("<a href=a>1<a id=x href=b>2</a>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void MisnestedFormattingEndTag_RunsTheAdoptionAgency()
    {
        Assert.AreEqual("body", ParentTag("<b>1<p id=x>2</b>3</p>"));
        Assert.AreEqual("23", Text(Box("<b>1<p id=x>2</b>3</p>")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-inbody")]
    public void UnclosedFormattingElement_IsReopenedInTheNextBlock()
    {
        Assert.AreEqual("b", Box("<p><b>a</p><p id=x>b</p>").Boxes[0].HtmlTag?.Name);
        Assert.AreEqual("i", ParentTag("<b>1<i>2</b><span id=x>3</span></i>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-intable")]
    public void Cell_OutsideARow_GetsAnImpliedTr()
    {
        Assert.AreEqual("tr", ParentTag("<table><td id=x>a</table>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-intable")]
    public void Col_OutsideAColgroup_GetsAnImpliedColgroup()
    {
        Assert.AreEqual("colgroup", ParentTag("<table><col id=x><tr><td>a</table>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-intable")]
    public void TableStartTag_InATable_ClosesTheTable()
    {
        Assert.AreEqual("body", ParentTag("<table><tr><td>a</td></tr><table id=x><tr><td>b</table>"));
    }

    [TestMethod]
    [Spec("html", "parsing.html#parsing-main-incaption")]
    public void CaptionEndTag_ClosesTheCaption()
    {
        Assert.AreEqual("tr", ParentTag("<table><caption>a</caption><tr><td id=x>b</table>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-incaption")]
    public void TableStructure_ClosesAnOpenCaption()
    {
        Assert.AreEqual("table", ParentTag("<table><caption>a<tbody id=x><tr><td>b</table>"));
    }

    [TestMethod]
    [Spec("html", "parsing.html#parsing-main-incolgroup")]
    public void Col_IsAChildOfTheColgroup()
    {
        Assert.AreEqual("colgroup", ParentTag("<table><colgroup><col id=x></colgroup><tr><td>a</table>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-incolgroup")]
    public void ContentOtherThanCol_ClosesAnOpenColgroup()
    {
        Assert.AreEqual("table", ParentTag("<table><colgroup><col><tbody id=x><tr><td>a</table>"));
    }

    [TestMethod]
    [Spec("html", "parsing.html#parsing-main-intr")]
    public void RowEndTag_ClosesTheRow()
    {
        Assert.AreEqual("tbody", ParentTag("<table><tbody><tr><td>a</td></tr><tr id=x><td>b</table>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-intr")]
    public void TableSection_ClosesAnOpenRow()
    {
        Assert.AreEqual("table", ParentTag("<table><tbody><tr><td>a</td><tbody id=x><tr><td>b</table>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-intd")]
    public void TableSection_ClosesAnOpenCell()
    {
        Assert.AreEqual("table", ParentTag("<table><tbody><tr><td>a<tbody id=x><tr><td>b</table>"));
    }

    [TestMethod]
    [Spec("html", "parsing.html#parsing-main-afterbody")]
    public void TextAfterTheBody_IsKept()
    {
        Assert.AreEqual("ab", Text(Document("<html id=x><head></head><body>a</body>b</html>")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#parsing-main-afterbody")]
    public void ContentAfterTheBody_GoesIntoTheBody()
    {
        Assert.AreEqual("body", Document("<html><head></head><body></body><p id=x>a</p></html>").ParentBox!.HtmlTag?.Name);
    }

    [TestMethod]
    [Spec("html", "parsing.html#the-after-after-body-insertion-mode")]
    public void TextAfterTheHtmlElement_IsKept()
    {
        var (root, _) = LayoutHarness.Layout("<html><head></head><body>a</body></html>b");
        Assert.AreEqual("ab", Text(root));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#the-after-after-body-insertion-mode")]
    public void ContentAfterTheHtmlElement_GoesIntoTheBody()
    {
        Assert.AreEqual("body", Document("<html><head></head><body></body></html><p id=x>a</p>").ParentBox!.HtmlTag?.Name);
    }

    /// <summary>Lays out a whole document, without the <see cref="LayoutHarness.Wrap"/> the other helpers add.</summary>
    private static CssBox Document(string html, string id = "x")
    {
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box, $"No box with id '{id}'");
        return box;
    }

    private static string Text(CssBox box) =>
        string.Concat(LayoutHarness.Descendants(box).Where(b => b.HtmlTag == null).Select(b => b.Text));
}
