using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Content;

[TestClass]
public sealed class ListStyleTests
{
    [TestMethod]
    [Spec("css21", "generate.html#lists")]
    [Spec("css-lists-3", "#declaring-a-list-item")]
    [Spec("css-lists-3", "#marker-pseudo")]
    public void ListItem_GetsAMarkerBox()
    {
        var x = Box("<div id=x style='display:list-item'>a</div>");

        Assert.IsNotNull(x.ListItemBox);
        Assert.AreEqual("•", Marker("<div id=x style='display:list-item'>a</div>"));
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [DataRow("decimal", "3.")]
    [DataRow("decimal-leading-zero", "03.")]
    [DataRow("lower-roman", "iii.")]
    [DataRow("upper-roman", "III.")]
    [DataRow("lower-greek", "γ.")]
    [DataRow("lower-latin", "c.")]
    [DataRow("upper-latin", "C.")]
    [DataRow("lower-alpha", "c.")]
    [DataRow("upper-alpha", "C.")]
    [DataRow("armenian", "Գ.")]
    [DataRow("georgian", "გ.")]
    [Spec("css-lists-3", "#text-markers")]
    public void NumberedListStyleType_NumbersTheItem(string type, string expected)
    {
        Assert.AreEqual(expected, Marker($"<ol style='list-style-type:{type}'><li>a</li><li>b</li><li id=x>c</li></ol>"));
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#text-markers")]
    public void GlyphListStyleTypes_AreDistinct()
    {
        var disc = Marker("<ul style='list-style-type:disc'><li id=x>a</li></ul>");
        var circle = Marker("<ul style='list-style-type:circle'><li id=x>a</li></ul>");
        var square = Marker("<ul style='list-style-type:square'><li id=x>a</li></ul>");

        Assert.AreEqual(3, new[] { disc, circle, square }.Where(m => m.Length > 0).Distinct().Count());
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#text-markers")]
    public void ListStyleTypeNone_HasNoMarker()
    {
        Assert.IsNull(Box("<ul style='list-style-type:none'><li id=x>a</li></ul>").ListItemBox);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#list-style-property")]
    public void ListStyleShorthand_SetsTheType()
    {
        Assert.AreEqual("square", Box("<ul style='list-style:square inside'><li id=x>a</li></ul>").ListStyleType);
        Assert.IsNull(Box("<ul style='list-style:none'><li id=x>a</li></ul>").ListItemBox);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#list-style-position-property")]
    public void OutsideMarker_IsLeftOfTheContent()
    {
        var x = Box("<ul><li id=x>a</li></ul>");

        Assert.IsTrue(x.ListItemBox!.Words[0].Right <= x.Location.X);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#list-style-position-property")]
    public void InsideMarker_IsInsideThePrincipalBox()
    {
        var x = Box("<ul style='list-style-position:inside'><li id=x>a</li></ul>");

        Assert.IsTrue(x.ListItemBox!.Words[0].Left >= x.Location.X);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#list-style-position-property")]
    public void RtlMarker_IsRightOfTheContent()
    {
        var x = Box("<ul style='direction:rtl'><li id=x>a</li></ul>");

        Assert.IsTrue(x.ListItemBox!.Words[0].Left >= x.Location.X + x.Size.Width);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#image-markers")]
    [Spec("css-images-3", "#image-values")]
    public void ListStyleImage_ReplacesTheMarker()
    {
        var x = Box("<ul style='list-style-image:url(data:image/png;base64,iVBORw0KGgo=)'><li id=x>a</li></ul>");

        Assert.IsTrue(x.ListItemBox!.Words[0].IsImage);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "generate.html#list-style")]
    public void Georgian_Twenty_IsKan()
    {
        Assert.AreEqual("კ.", Marker("<ol style='list-style-type:georgian' start=20><li id=x>a</li></ol>"));
    }
}
