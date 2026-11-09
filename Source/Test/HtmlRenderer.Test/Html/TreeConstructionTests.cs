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
}
