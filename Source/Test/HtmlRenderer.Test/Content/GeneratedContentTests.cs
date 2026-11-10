using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Content;

[TestClass]
public sealed class GeneratedContentTests
{
    private const double Delta = 0.5;

    [TestMethod]
    [Spec("css21", "generate.html#before-after-content")]
    [Spec("css-pseudo-4", "#generated-content")]
    public void BeforeAndAfter_AreTheFirstAndLastChildOfTheElement()
    {
        var x = Box("<p id=x><span>a</span></p><style>#x::before { content: 'B' } #x::after { content: 'A' }</style>");

        Assert.IsTrue(x.Boxes[0].IsBeforePseudoElement);
        Assert.AreEqual("B", x.Boxes[0].Text);
        Assert.IsTrue(x.Boxes[^1].IsAfterPseudoElement);
        Assert.AreEqual("A", x.Boxes[^1].Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#before-after-content")]
    [Spec("css-pseudo-4", "#treelike")]
    public void PseudoElement_InheritsInheritedPropertiesOnly()
    {
        var before = Before("<p id=x style='color:red;border:2px solid black'>a</p><style>#x::before { content: 'B' }</style>");

        Assert.AreEqual(255, before.ActualColor.R);
        Assert.AreEqual(0, before.ActualColor.G);
        Assert.AreEqual(0, before.ActualBorderLeftWidth);
        Assert.AreEqual("inline", before.Display);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "generate.html#before-after-content")]
    [Spec("css-pseudo-4", "#generated-content")]
    public void BlockPseudoElement_IsLaidOutAsABlockInsideTheElement()
    {
        var inline = Box("<div id=x>Text</div><style>#x::before { content: 'Some' }</style>");
        var block = Box("<div id=x>Text</div><style>#x::before { content: 'Some'; display: block }</style>");

        Assert.AreEqual(2 * inline.Size.Height, block.Size.Height, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "generate.html#before-after-content")]
    [DataRow("none")]
    [DataRow("normal")]
    [Spec("css-pseudo-4", "#generated-content")]
    public void PseudoElementWithoutContent_IsNotGenerated(string content)
    {
        var plain = Box("<div id=x>Text</div>");
        var x = Box($"<div id=x>Text</div><style>#x::before {{ content: {content}; display: block; height: 50px }}</style>");

        Assert.AreEqual(plain.Size.Height, x.Size.Height, Delta);
    }

    [TestMethod]
    [Spec("css21", "generate.html#content")]
    public void Content_ConcatenatesStringsAndAttributes()
    {
        var before = Before("<p id=x data-n='7'>a</p><style>#x::before { content: 'No. ' attr(data-n) attr(data-missing) '!' }</style>");

        Assert.AreEqual("No. 7!", before.Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#content")]
    public void Content_OnAnElement_IsIgnored()
    {
        var x = Box("<p id=x style=\"content:'X'\">a</p>");

        Assert.IsFalse(LayoutHarness.Descendants(x).Any(b => b.Text == "X"));
    }

    [TestMethod]
    [Spec("css21", "generate.html#content")]
    public void NewlineEscape_BreaksTheLineWhenWhiteSpaceIsPreserved()
    {
        var oneLine = Box("<div id=x></div><style>#x::before { content: 'a b' }</style>");
        var pre = Box("<div id=x></div><style>#x::before { content: 'a\\A b'; white-space: pre }</style>");

        Assert.AreEqual(2 * oneLine.Size.Height, pre.Size.Height, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "generate.html#content")]
    public void NewlineEscape_CollapsesToASpaceWithNormalWhiteSpace()
    {
        var oneLine = Box("<div id=x></div><style>#x::before { content: 'a b' }</style>");
        var collapsed = Box("<div id=x></div><style>#x::before { content: 'a\\A b' }</style>");

        Assert.AreEqual(oneLine.Size.Height, collapsed.Size.Height, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "generate.html#content")]
    public void CountersFunction_JoinsAllCountersInScope()
    {
        var html = "<div style='counter-reset:c 1'><div style='counter-reset:c 2'><p id=x>a</p></div></div>"
                   + "<style>#x::before { content: counters(c, '.') }</style>";

        Assert.AreEqual("1.2", Before(html).Text);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "generate.html#content")]
    public void Uri_InsertsTheImage()
    {
        var before = Before("<p id=x>a</p><style>#x::before { content: url(data:image/png;base64,iVBORw0KGgo=) }</style>");

        Assert.IsTrue(LayoutHarness.Descendants(before).Any(b => b.Words.Any(w => w.IsImage)));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "generate.html#quotes-specify")]
    public void Quotes_SuppliesTheQuoteMarks()
    {
        Assert.AreEqual("<a>", Before("<p id=x style=\"quotes:'<' '>'\">a</p><style>#x::before { content: open-quote 'a' close-quote }</style>").Text);
        Assert.AreEqual("a", Before("<p id=x style='quotes:none'>a</p><style>#x::before { content: open-quote 'a' close-quote }</style>").Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#quotes-insert")]
    public void OpenAndCloseQuote_InsertQuoteMarks()
    {
        var html = "<p><q id=x>a</q></p><style>q::before { content: open-quote } q::after { content: close-quote }</style>";
        var x = Box(html);

        Assert.AreEqual("“", x.Boxes[0].Text);
        Assert.AreEqual("”", x.Boxes[^1].Text);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "generate.html#quotes-insert")]
    public void NestedQuote_UsesTheNextPair()
    {
        var html = "<p style=\"quotes:'1' '1' '2' '2'\"><q>a <q id=x>b</q></q></p>"
                   + "<style>q::before { content: open-quote }</style>";

        Assert.AreEqual("2", Before(html).Text);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "generate.html#quotes-insert")]
    public void CloseQuoteAtDepthZero_InsertsNothing()
    {
        Assert.AreEqual("a", Before("<p id=x>a</p><style>#x::before { content: close-quote 'a' }</style>").Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counters")]
    public void CounterResetAndIncrement_OnTheSameElement_ResetFirst()
    {
        Assert.AreEqual("6", Before("<p id=x style='counter-reset:c 5;counter-increment:c'>a</p><style>#x::before { content: counter(c) }</style>").Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counters")]
    public void RepeatedCounterName_IsProcessedInOrder()
    {
        Assert.AreEqual("0", Before("<p id=x style='counter-reset:c 2 c'>a</p><style>#x::before { content: counter(c) }</style>").Text);
        Assert.AreEqual("3", Before("<p id=x style='counter-reset:c;counter-increment:c c 2'>a</p><style>#x::before { content: counter(c) }</style>").Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counters")]
    public void ZeroAndNegativeIncrements_AreAllowed()
    {
        Assert.AreEqual("5", Before("<p id=x style='counter-reset:c 5;counter-increment:c 0'>a</p><style>#x::before { content: counter(c) }</style>").Text);
        Assert.AreEqual("3", Before("<p id=x style='counter-reset:c 5;counter-increment:c -2'>a</p><style>#x::before { content: counter(c) }</style>").Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counters")]
    public void CounterReset_FollowsTheCascade()
    {
        var html = "<p id=x>a</p><style>#x { counter-reset: a 5 } #x { counter-reset: b 2 } #x::before { content: counter(a) counter(b) }</style>";

        Assert.AreEqual("02", Before(html).Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counters")]
    [Spec("css21", "generate.html#scope")]
    public void ChaptersAndSections_AreNumbered()
    {
        var html = "<div style='counter-reset:chapter'>"
                   + "<h1 id=c1>About CSS</h1><h2 id=s1>CSS 2</h2><h2 id=s2>CSS 2.1</h2><h1 id=c2>Style</h1><h2 id=s3>Selectors</h2>"
                   + "</div><style>"
                   + "h1::before { content: 'Chapter ' counter(chapter) '. '; counter-increment: chapter }"
                   + "h1 { counter-reset: section }"
                   + "h2::before { content: counter(chapter) '.' counter(section) ' '; counter-increment: section }"
                   + "</style>";

        Assert.AreEqual("Chapter 1. ", Before(html, "c1").Text);
        Assert.AreEqual("1.1 ", Before(html, "s1").Text);
        Assert.AreEqual("1.2 ", Before(html, "s2").Text);
        Assert.AreEqual("Chapter 2. ", Before(html, "c2").Text);
        Assert.AreEqual("2.1 ", Before(html, "s3").Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#scope")]
    public void NestedLists_GetANewCounterInstancePerList()
    {
        var html = "<ol><li id=a>i</li><li id=b>i"
                   + "<ol><li id=c>i</li><li id=d>i</li><li id=e>i"
                   + "<ol><li id=f>i</li></ol><ol><li id=g>i</li></ol>"
                   + "</li><li id=h>i</li></ol>"
                   + "</li><li id=i>i</li><li id=j>i</li></ol>"
                   + "<ol><li id=k>i</li><li id=l>i</li></ol>"
                   + "<style>ol { counter-reset: item } li { display: block } li::before { content: counter(item); counter-increment: item }</style>";

        var expected = new[] { ("a", "1"), ("b", "2"), ("c", "1"), ("d", "2"), ("e", "3"), ("f", "1"), ("g", "1"), ("h", "4"), ("i", "3"), ("j", "4"), ("k", "1"), ("l", "2") };
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html));
        foreach (var (id, number) in expected)
            Assert.AreEqual(number, LayoutHarness.Descendants(LayoutHarness.FindById(root, id)!).First(b => b.IsBeforePseudoElement).Text, id);
    }

    [TestMethod]
    [Spec("css21", "generate.html#scope")]
    public void CounterIncrement_OutOfScope_ResetsTheCounterOnThatElement()
    {
        var html = "<div><p style='counter-increment:c'>a</p><p id=x>b</p></div><style>#x::before { content: counter(c) }</style>";

        Assert.AreEqual("1", Before(html).Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counter-styles")]
    [DataRow("upper-latin", "C")]
    [DataRow("lower-latin", "c")]
    [DataRow("lower-greek", "γ")]
    [DataRow("lower-roman", "iii")]
    [DataRow("armenian", "Գ")]
    [DataRow("georgian", "გ")]
    [DataRow("decimal-leading-zero", "03")]
    public void Counter_IsFormattedInTheGivenStyle(string style, string expected)
    {
        Assert.AreEqual(expected, Before($"<p id=x style='counter-reset:c 3'>a</p><style>#x::before {{ content: counter(c, {style}) }}</style>").Text);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "generate.html#counter-styles")]
    public void Counter_WithGlyphOrNoneStyle_UsesTheGlyphOrNothing()
    {
        Assert.AreEqual("[]", Before("<p id=x style='counter-reset:c 3'>a</p><style>#x::before { content: '[' counter(c, none) ']' }</style>").Text);
        Assert.AreEqual(Marker("<ul style='list-style-type:square'><li id=x>a</li></ul>"),
            Before("<p id=x style='counter-reset:c 3'>a</p><style>#x::before { content: counter(c, square) }</style>").Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#undisplayed-counters")]
    public void HiddenElement_StillIncrements()
    {
        var html = "<div><p style='counter-reset:c'>a</p><p style='counter-increment:c;visibility:hidden'>b</p><p id=x>c</p></div>"
                   + "<style>#x::before { content: counter(c) }</style>";

        Assert.AreEqual("1", Before(html).Text);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "generate.html#undisplayed-counters")]
    public void PseudoElementThatIsNotGenerated_DoesNotIncrement()
    {
        var html = "<div style='counter-reset:c'><p id=a>a</p><p id=x>b</p></div>"
                   + "<style>#a::before { content: normal; counter-increment: c } #x::before { content: counter(c) }</style>";

        Assert.AreEqual("0", Before(html).Text);
    }

    private static CssBox Before(string body, string id = "x")
    {
        var before = Box(body, id).Boxes.FirstOrDefault(b => b.IsBeforePseudoElement);
        Assert.IsNotNull(before, $"No ::before on '{id}'");
        return before;
    }
}
