using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Content;

/// <summary>
/// Ported from PeachPDF.Tests/Html/Core/Dom/CssContentEngineTests.cs, exercised through the real box tree
/// (a <c>::before</c>/<c>::after</c> rule + <see cref="LayoutHarness"/>) rather than a hand-built box, since
/// this fork's pseudo-element boxes are created as a side effect of selector matching
/// (<c>CssData.DoesSelectorMatch</c>) rather than being independently constructible.
/// <para>
/// PeachPDF's <c>string()</c>/named-string and <c>target-counter()</c>/margin-box cases are not ported: this
/// fork has neither named strings nor CSS Paged Media margin boxes. <c>counter-reset</c>/
/// <c>counter-increment</c> threading through document order (<see cref="CssCounterEngine.ResolveCounters"/>)
/// is a new, deliberately scoped-down backport for this pass - see that class's remarks for exactly what's
/// covered (ordinary sequential/nested usage) and what isn't (<c>reversed()</c>, <c>counter-set</c>).
/// </para>
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class CssContentEngineIntegrationTests
{
    [TestMethod]
    public void ApplyContent_WithStringLiteral_SetsText()
    {
        var before = ResolveBeforeContent("\"Hello World\"");
        Assert.AreEqual("Hello World", before.Text);
    }

    [TestMethod]
    public void ApplyContent_WithMultipleStringLiterals_Concatenates()
    {
        var before = ResolveBeforeContent("\"Hello\" \" \" \"World\"");
        Assert.AreEqual("Hello World", before.Text);
    }

    [TestMethod]
    public void ApplyContent_WithNone_DoesNotCreateContent()
    {
        var html = LayoutHarness.Wrap("<p id='p'>text</p><style>#p::before { content: none; }</style>");
        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsFalse(p.Boxes.Any(b => b.IsBeforePseudoElement && b.Text != null));
    }

    [TestMethod]
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
    public void ApplyContent_WithOpenAndCloseQuote_UsesCurlyQuotes()
    {
        var before = ResolveBeforeContent("open-quote \"quoted\" close-quote");
        Assert.AreEqual("“quoted”", before.Text);
    }

    [TestMethod]
    public void ApplyContent_WithCounterNoStyle_UsesDecimal()
    {
        var html = LayoutHarness.Wrap(
            "<p id='p' style='counter-reset:item 7'>text</p><style>#p::before { content: counter(item); }</style>");
        var (root, _) = LayoutHarness.Layout(html);
        var before = LayoutHarness.FindById(root, "p")!.Boxes.First(b => b.IsBeforePseudoElement);

        Assert.AreEqual("7", before.Text);
    }

    [TestMethod]
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
        // CSS Counter Styles Level 3 §2: unknown style must render as decimal, not empty.
        var html = LayoutHarness.Wrap(
            "<p id='p' style='counter-reset:item 5'>text</p>"
            + "<style>#p::before { content: counter(item, bogus-style); }</style>");
        var (root, _) = LayoutHarness.Layout(html);
        var before = LayoutHarness.FindById(root, "p")!.Boxes.First(b => b.IsBeforePseudoElement);

        Assert.AreEqual("5", before.Text);
    }

    [TestMethod]
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
    public void CounterIncrement_ThreadsSequentiallyAcrossSiblings()
    {
        // CSS 2.1 §12.4: counter-increment accumulates through document order - three siblings each
        // incrementing "item" should see 1, 2, 3 in turn.
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
    public void CounterReset_OnNestedAncestor_IsVisibleToDescendant()
    {
        // A counter-reset on an ancestor is visible to descendants further down the tree, not just
        // direct children.
        var html = LayoutHarness.Wrap(
            "<div style='counter-reset:section 5'><div><p id='p'>text</p></div></div>"
            + "<style>#p::before { content: counter(section); }</style>");
        var (root, _) = LayoutHarness.Layout(html);
        var before = LayoutHarness.FindById(root, "p")!.Boxes.First(b => b.IsBeforePseudoElement);

        Assert.AreEqual("5", before.Text);
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
