using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Content;

[TestClass]
public sealed class ListsAndCountersTests
{
    [TestMethod]
    [Ignore("Gap, see Spec/css-lists-3.yml")]
    [Spec("css-lists-3", "#marker-pseudo")]
    public void MarkerRule_StylesTheMarker()
    {
        var x = Box("<ul><li id=x>a</li></ul><style>li::marker { color: red }</style>");

        Assert.AreEqual(255, x.ListItemBox!.ActualColor.R);
        Assert.AreEqual(0, x.ActualColor.R);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-lists-3.yml")]
    [Spec("css-lists-3", "#content-property")]
    public void MarkerContent_ReplacesTheMarkerString()
    {
        Assert.AreEqual("X", Marker("<ul><li id=x>a</li></ul><style>li::marker { content: 'X' }</style>"));
    }

    [TestMethod]
    [Spec("css-lists-3", "#content-property")]
    public void ListItemWithoutMarkerString_HasNoMarkerBox()
    {
        Assert.IsNull(Box("<ol style='list-style-type:none'><li id=x>a</li></ol>").ListItemBox);
        Assert.IsNull(Box("<div id=x style='display:list-item;list-style:none'>a</div>").ListItemBox);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-lists-3.yml")]
    [Spec("css-lists-3", "#text-markers")]
    public void StringListStyleType_IsTheMarkerString()
    {
        Assert.AreEqual("*", Marker("<ul style='list-style-type:\"*\"'><li id=x>a</li></ul>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-lists-3.yml")]
    [Spec("css-lists-3", "#text-markers")]
    public void UnknownCounterStyle_FallsBackToDecimal()
    {
        Assert.AreEqual("1.", Marker("<ul style='list-style-type:no-such-style'><li id=x>a</li></ul>"));
    }

    [TestMethod]
    [Spec("css-lists-3", "#list-style-property")]
    public void ListStyleShorthand_NoneGoesToTheLonghandNotOtherwiseSet()
    {
        var noneImage = Box("<ul style='list-style:none url(x.png)'><li id=x>a</li></ul>");
        Assert.AreEqual("none", noneImage.ListStyleType);
        Assert.AreEqual("url(\"x.png\")", noneImage.ListStyleImage);

        var noneDisc = Box("<ol style='list-style:none disc'><li id=x>a</li></ol>");
        Assert.AreEqual("disc", noneDisc.ListStyleType);
        Assert.AreEqual("none", noneDisc.ListStyleImage);

        Assert.AreEqual("none", Box("<ol style='list-style:none'><li id=x>a</li></ol>").ListStyleType);
        Assert.AreEqual("decimal", Box("<ol style='list-style:none disc url(x.png)'><li id=x>a</li></ol>").ListStyleType);
    }

    [TestMethod]
    [Spec("css-lists-3", "#list-style-property")]
    public void ListStyleShorthand_ResetsTheOmittedLonghands()
    {
        Assert.AreEqual("disc", Box("<ol style='list-style:inside'><li id=x>a</li></ol>").ListStyleType);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-lists-3.yml")]
    [Spec("css-lists-3", "#auto-numbering")]
    public void None_IsNotACounterName()
    {
        Assert.IsFalse(ParseDeclaration("counter-reset: none 3").HasValue);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-lists-3.yml")]
    [Spec("css-lists-3", "#auto-numbering")]
    public void CounterValue_IsClampedToTheSupportedRange()
    {
        Assert.AreEqual("2147483647", Before("<p id=x style='counter-reset:c 2147483647;counter-increment:c'>a</p><style>#x::before { content: counter(c) }</style>").Text);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-lists-3.yml")]
    [Spec("css-lists-3", "#increment-set")]
    public void CounterSet_SetsTheValueAfterTheIncrement()
    {
        Assert.AreEqual("2", Before("<p id=x style='counter-reset:c 5;counter-increment:c;counter-set:c 2'>a</p><style>#x::before { content: counter(c) }</style>").Text);
    }

    [TestMethod]
    [Spec("css-lists-3", "#inheriting-counters")]
    public void CounterValue_ComesFromThePrecedingElementInTreeOrder()
    {
        var html = "<ul style='counter-reset:example 0'>"
                   + "<li id=foo style='counter-increment:example'>foo<div id=bar style='counter-increment:example'>bar</div></li>"
                   + "<li id=x>baz</li></ul>"
                   + "<style>#foo::before, #bar::before, #x::before { content: counter(example) }</style>";

        Assert.AreEqual("1", Before(html, "foo").Text);
        Assert.AreEqual("2", Before(html, "bar").Text);
        Assert.AreEqual("2", Before(html).Text);
    }

    [TestMethod]
    [Spec("css-lists-3", "#instantiating-counters")]
    public void ResetOnALaterSibling_ReplacesTheSiblingsCounter()
    {
        var html = "<div><p style='counter-reset:c 3'>a</p><p style='counter-reset:c 7'>b</p><p id=x>c</p></div>"
                   + "<div style='counter-reset:c 1'><p id=y style='counter-reset:c 2'>d</p></div>"
                   + "<style>#x::before, #y::before { content: counter(c) }</style>";

        Assert.AreEqual("7", Before(html).Text);
        Assert.AreEqual("2", Before(html, "y").Text);
    }

    [TestMethod]
    [Spec("css-lists-3", "#declaring-a-list-item")]
    [Spec("css-lists-3", "#list-item-counter")]
    public void ListItems_IncrementTheListItemCounter()
    {
        var html = "<div style='counter-reset:list-item'><p id=a style='display:list-item'>a</p><p>b</p><p id=c style='display:list-item'>c</p></div>"
                   + "<style>p::before { content: counter(list-item) }</style>";

        Assert.AreEqual("1", Before(html, "a").Text);
        Assert.AreEqual("2", Before(html, "c").Text);
    }

    [TestMethod]
    [Spec("css-lists-3", "#list-item-counter")]
    public void ExplicitListItemIncrement_ReplacesTheImplicitOne()
    {
        var html = "<ol><li>a</li><li id=x>b</li></ol><style>li { counter-increment: list-item 2 } li::before { content: counter(list-item) }</style>";
        Assert.AreEqual("4", Before(html).Text);

        html = "<ol><li>a</li><li id=x>b</li></ol><style>li { counter-increment: list-item 0 } li::before { content: counter(list-item) }</style>";
        Assert.AreEqual("0", Before(html).Text);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-lists-3.yml")]
    [Spec("css-lists-3", "#list-item-counter")]
    public void Marker_ShowsTheListItemCounter()
    {
        Assert.AreEqual("4.", Marker("<ol><li>a</li><li id=x>b</li></ol><style>li { counter-increment: list-item 2 }</style>"));
        Assert.AreEqual("5.", Marker("<ol style='counter-reset:list-item 4'><li id=x>a</li></ol>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-lists-3.yml")]
    [Spec("css-lists-3", "#list-item-counter")]
    public void NestedList_StartsANewListItemCounter()
    {
        Assert.AreEqual("1", Before("<ol><li>a<ol><li id=x>b</li></ol></li></ol><style>li::before { content: counter(list-item) }</style>").Text);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-lists-3.yml")]
    [Spec("css-lists-3", "#list-item-counter")]
    public void StartAndValue_SetTheListItemCounter()
    {
        Assert.AreEqual("5", Before("<ol start=5><li id=x>a</li></ol><style>li::before { content: counter(list-item) }</style>").Text);
        Assert.AreEqual("7", Before("<ol><li id=x value=7>a</li></ol><style>li::before { content: counter(list-item) }</style>").Text);
    }

    private static CssBox Before(string body, string id = "x")
    {
        // First in tree order, it can sit in an anonymous block
        var before = LayoutHarness.Descendants(Box(body, id)).FirstOrDefault(b => b.IsBeforePseudoElement);
        Assert.IsNotNull(before, $"No ::before on '{id}'");
        return before;
    }
}
