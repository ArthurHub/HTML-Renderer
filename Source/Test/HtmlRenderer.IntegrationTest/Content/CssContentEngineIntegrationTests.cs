using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Content;

[DoNotParallelize]
[TestClass]
public sealed class CssContentEngineIntegrationTests
{
    [TestMethod]
    [Spec("css21", "generate.html#content")]
    [Spec("css-content-3", "#strings")]
    public void ApplyContent_WithStringLiteral_SetsText()
    {
        var before = ResolveBeforeContent("\"Hello World\"");
        Assert.AreEqual("Hello World", before.Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#content")]
    [Spec("css-content-3", "#strings")]
    public void ApplyContent_WithMultipleStringLiterals_Concatenates()
    {
        var before = ResolveBeforeContent("\"Hello\" \" \" \"World\"");
        Assert.AreEqual("Hello World", before.Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#content")]
    [Spec("css-content-3", "#content-property")]
    public void ApplyContent_WithNone_DoesNotCreateContent()
    {
        var html = LayoutHarness.Wrap("<p id='p'>text</p><style>#p::before { content: none; }</style>");
        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsFalse(p.Boxes.Any(b => b.IsBeforePseudoElement && b.Text != null));
    }

    [TestMethod]
    [Spec("css21", "generate.html#content")]
    [Spec("css-content-3", "#strings")]
    public void ApplyContent_WithAttrFunction_RetrievesAttribute()
    {
        var html = LayoutHarness.Wrap(
            "<p id='p' data-label='Chapter One'>text</p><style>#p::before { content: attr(data-label); }</style>");
        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;
        var before = p.Boxes.First(b => b.IsBeforePseudoElement);

        Assert.AreEqual("Chapter One", before.Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#quotes-insert")]
    [Spec("css-content-3", "#quote-values")]
    public void ApplyContent_WithOpenAndCloseQuote_UsesCurlyQuotes()
    {
        var before = ResolveBeforeContent("open-quote \"quoted\" close-quote");
        Assert.AreEqual("“quoted”", before.Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counter-styles")]
    [Spec("css-lists-3", "#counter-functions")]
    public void ApplyContent_WithCounterNoStyle_UsesDecimal()
    {
        var html = LayoutHarness.Wrap(
            "<p id='p' style='counter-reset:item 7'>text</p><style>#p::before { content: counter(item); }</style>");
        var (root, _) = LayoutHarness.Layout(html);
        var before = LayoutHarness.FindById(root, "p")!.Boxes.First(b => b.IsBeforePseudoElement);

        Assert.AreEqual("7", before.Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counter-styles")]
    public void ApplyContent_WithCounterDecimalLeadingZero_PadsToTwoDigits()
    {
        var html = LayoutHarness.Wrap(
            "<p id='p' style='counter-reset:item 1'>text</p>"
            + "<style>#p::before { content: counter(item, decimal-leading-zero); }</style>");
        var (root, _) = LayoutHarness.Layout(html);
        var before = LayoutHarness.FindById(root, "p")!.Boxes.First(b => b.IsBeforePseudoElement);

        Assert.AreEqual("01", before.Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counter-styles")]
    public void ApplyContent_WithCounterAlphabeticStyle_FormatsWithStyle()
    {
        var html = LayoutHarness.Wrap(
            "<p id='p' style='counter-reset:item 4'>text</p>"
            + "<style>#p::before { content: counter(item, upper-roman); }</style>");
        var (root, _) = LayoutHarness.Layout(html);
        var before = LayoutHarness.FindById(root, "p")!.Boxes.First(b => b.IsBeforePseudoElement);

        Assert.AreEqual("IV", before.Text);
    }

    [TestMethod]
    public void ApplyContent_WithCounterUnknownStyle_FallsBackToDecimal()
    {
        // CSS Counter Styles 3 §2
        var html = LayoutHarness.Wrap(
            "<p id='p' style='counter-reset:item 5'>text</p>"
            + "<style>#p::before { content: counter(item, bogus-style); }</style>");
        var (root, _) = LayoutHarness.Layout(html);
        var before = LayoutHarness.FindById(root, "p")!.Boxes.First(b => b.IsBeforePseudoElement);

        Assert.AreEqual("5", before.Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#content")]
    [Spec("css-content-3", "#content-values")]
    public void ApplyContent_WithCounterAndStyleAndLiteral_Concatenates()
    {
        var html = LayoutHarness.Wrap(
            "<p id='p' style='counter-reset:item 3'>text</p>"
            + "<style>#p::before { content: counter(item, decimal-leading-zero) \" Item\"; }</style>");
        var (root, _) = LayoutHarness.Layout(html);
        var before = LayoutHarness.FindById(root, "p")!.Boxes.First(b => b.IsBeforePseudoElement);

        Assert.AreEqual("03 Item", before.Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counters")]
    [Spec("css-lists-3", "#increment-set")]
    public void CounterIncrement_ThreadsSequentiallyAcrossSiblings()
    {
        var html = LayoutHarness.Wrap(
            "<div style='counter-reset:item 0'>"
            + "<p id='a' style='counter-increment:item'>text</p>"
            + "<p id='b' style='counter-increment:item'>text</p>"
            + "<p id='c' style='counter-increment:item'>text</p>"
            + "</div>"
            + "<style>p::before { content: counter(item); }</style>");
        var (root, _) = LayoutHarness.Layout(html);

        var a = LayoutHarness.FindById(root, "a")!.Boxes.First(b => b.IsBeforePseudoElement);
        var b = LayoutHarness.FindById(root, "b")!.Boxes.First(b => b.IsBeforePseudoElement);
        var c = LayoutHarness.FindById(root, "c")!.Boxes.First(b => b.IsBeforePseudoElement);

        Assert.AreEqual("1", a.Text);
        Assert.AreEqual("2", b.Text);
        Assert.AreEqual("3", c.Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#scope")]
    [Spec("css-lists-3", "#inheriting-counters")]
    public void CounterReset_OnNestedAncestor_IsVisibleToDescendant()
    {
        var html = LayoutHarness.Wrap(
            "<div style='counter-reset:section 5'><div><p id='p'>text</p></div></div>"
            + "<style>#p::before { content: counter(section); }</style>");
        var (root, _) = LayoutHarness.Layout(html);
        var before = LayoutHarness.FindById(root, "p")!.Boxes.First(b => b.IsBeforePseudoElement);

        Assert.AreEqual("5", before.Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#scope")]
    [Spec("css-lists-3", "#nested-counters")]
    public void CounterReset_InANestedList_DoesNotLeakIntoTheOuterList()
    {
        var html = LayoutHarness.Wrap(
            "<div class='list'>"
            + "<div class='item'><span id='a'>A</span>"
            + "<div class='list'><p class='item'>x</p><p class='item'>y</p><p class='item'>z</p></div>"
            + "</div>"
            + "<div class='item'><span id='b'>B</span></div>"
            + "</div>"
            + "<style>.list { counter-reset: item; } .item { counter-increment: item; }"
            + " #a::before, #b::before { content: counter(item); }</style>");
        var (root, _) = LayoutHarness.Layout(html);

        var a = LayoutHarness.FindById(root, "a")!.Boxes.First(b => b.IsBeforePseudoElement);
        var b = LayoutHarness.FindById(root, "b")!.Boxes.First(b => b.IsBeforePseudoElement);

        Assert.AreEqual("1", a.Text);
        Assert.AreEqual("2", b.Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#undisplayed-counters")]
    [Spec("css-lists-3", "#counters-without-boxes")]
    public void CounterIncrement_OnADisplayNoneElement_IsIgnored()
    {
        var html = LayoutHarness.Wrap(
            "<div style='counter-reset:item'>"
            + "<p style='counter-increment:item'>a</p>"
            + "<p style='counter-increment:item; display:none'>hidden</p>"
            + "<p id='c' style='counter-increment:item'>c</p>"
            + "</div>"
            + "<style>#c::before { content: counter(item); }</style>");
        var (root, _) = LayoutHarness.Layout(html);
        var before = LayoutHarness.FindById(root, "c")!.Boxes.First(b => b.IsBeforePseudoElement);

        Assert.AreEqual("2", before.Text);
    }

    [TestMethod]
    [Spec("css21", "generate.html#scope")]
    [Spec("css-lists-3", "#counter-functions")]
    public void Counter_NotInScope_ReadsAsZero()
    {
        Assert.AreEqual("0", ResolveBeforeContent("counter(nothing)").Text);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static CssBox ResolveBeforeContent(string contentValue)
    {
        var html = LayoutHarness.Wrap(
            $"<p id='p'>text</p><style>#p::before {{ content: {contentValue}; }}</style>");
        var (root, _) = LayoutHarness.Layout(html);
        return LayoutHarness.FindById(root, "p")!.Boxes.First(b => b.IsBeforePseudoElement);
    }
}
