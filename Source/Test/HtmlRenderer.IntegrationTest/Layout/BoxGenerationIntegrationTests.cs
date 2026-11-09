using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Layout;

[DoNotParallelize]
[TestClass]
public sealed class BoxGenerationIntegrationTests
{
    private const double Delta = 1.0;

    [TestMethod]
    [Spec("css21", "visuren.html#viewport")]
    public void AutoWidthBlock_FollowsTheViewportWidth()
    {
        var html = LayoutHarness.Wrap("<div id='d'>text</div>");

        var (narrowRoot, _) = LayoutHarness.Layout(html, maxWidth: 300);
        var (wideRoot, _) = LayoutHarness.Layout(html, maxWidth: 600);

        Assert.AreEqual(300, LayoutHarness.FindById(narrowRoot, "d")!.Size.Width, Delta);
        Assert.AreEqual(600, LayoutHarness.FindById(wideRoot, "d")!.Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#containing-block")]
    public void ABoxWiderThanItsContainingBlock_OverflowsIt()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='cb' style='width:100px'><div id='c' style='width:250px;height:10px'></div></div>"));

        var cb = LayoutHarness.FindById(root, "cb")!;
        var c = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(cb.Location.X, c.Location.X, Delta);
        Assert.AreEqual(250, c.Size.Width, Delta);
    }

    [TestMethod]
    [DataRow("block")]
    [DataRow("list-item")]
    [DataRow("table")]
    [Spec("css21", "visuren.html#block-boxes")]
    public void BlockLevelDisplayValues_StackVertically(string display)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            $"<span id='a' style='display:{display};height:20px'>a</span><span id='b' style='display:{display};height:20px'>b</span>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(a.Location.X, b.Location.X, Delta);
        Assert.IsGreaterThanOrEqualTo(a.ActualBottom - Delta, b.Location.Y);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#anonymous-block-level")]
    public void TextBeforeABlock_IsWrappedInAnAnonymousBlock()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='div'>Some text<p id='p' style='margin:0'>More text</p></div>"));

        var div = LayoutHarness.FindById(root, "div")!;
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.HasCount(2, div.Boxes);
        var anonymous = div.Boxes[0];
        Assert.IsNull(anonymous.HtmlTag);
        Assert.AreEqual("block", anonymous.Display);
        Assert.AreEqual(anonymous.ActualBottom, p.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#anonymous-block-level")]
    public void AnonymousBlock_InheritsInheritedPropertiesAndResetsTheOthers()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='div' style='color:rgb(0,128,0);margin:10px;border:2px solid red'>Some text<p style='margin:0'>More text</p></div>"));

        var anonymous = LayoutHarness.FindById(root, "div")!.Boxes[0];

        Assert.AreEqual(RColor.FromArgb(0, 128, 0), anonymous.ActualColor);
        Assert.AreEqual(0, anonymous.ActualMarginLeft);
        Assert.AreEqual(0, anonymous.ActualBorderLeftWidth);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#anonymous-block-level")]
    public void BlockInsideInline_StacksTheInlineContentAroundIt()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<p style='margin:0'><span id='s'>before<span id='block' style='display:block'>inside</span>after</span></p>"));

        var block = LayoutHarness.FindById(root, "block")!;
        var before = FindWord(root, "before")!;
        var after = FindWord(root, "after")!;

        Assert.IsLessThanOrEqualTo(block.Location.Y + Delta, before.Bottom);
        Assert.IsGreaterThanOrEqualTo(block.ActualBottom - Delta, after.Top);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#anonymous-block-level")]
    public void BlockInsideInline_IsNotInsetByTheInlinesBorder()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<p style='margin:0'><span style='border-left:10px solid red'>before<span id='block' style='display:block'>inside</span>after</span></p>"));

        Assert.AreEqual(0, LayoutHarness.FindById(root, "block")!.Location.X, Delta);
    }

    [TestMethod]
    [DataRow("inline")]
    [DataRow("inline-block")]
    [Spec("css21", "visuren.html#inline-boxes")]
    public void InlineLevelDisplayValues_SitOnTheSameLine(string display)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            $"<div>first <span style='display:{display}'>second</span></div>"));

        var first = FindWord(root, "first")!;
        var second = FindWord(root, "second")!;

        Assert.AreEqual(first.Top, second.Top, 5);
        Assert.IsGreaterThan(first.Right, second.Left + Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#inline-boxes")]
    public void InlineTable_SitsOnTheSameLine()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div>first <span style='display:inline-table'>second</span></div>"));

        var first = FindWord(root, "first")!;
        var second = FindWord(root, "second")!;

        Assert.AreEqual(first.Top, second.Top, 5);
        Assert.IsGreaterThan(first.Right, second.Left + Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#anonymous")]
    public void AnonymousInlineText_InheritsTheColorButNotTheBackground()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<p id='p' style='color:rgb(0,0,255);background-color:rgb(255,255,0)'>Some <em>emphasized</em> text</p>"));

        var p = LayoutHarness.FindById(root, "p")!;
        var anonymous = p.Boxes[0];

        Assert.IsNull(anonymous.HtmlTag);
        Assert.AreEqual(RColor.FromArgb(0, 0, 255), anonymous.ActualColor);
        Assert.AreEqual(0, anonymous.ActualBackgroundColor.A);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#anonymous")]
    public void CollapsibleWhiteSpaceBetweenBlocks_GeneratesNoBox()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='d'>\n  <div>a</div>\n  <div>b</div>\n</div>"));

        Assert.HasCount(2, LayoutHarness.FindById(root, "d")!.Boxes);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#display-prop")]
    public void DisplayNone_GeneratesNoBoxAndTakesNoSpace()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:20px'></div><div style='display:none;height:50px'><div style='display:block;height:50px'></div></div><div id='b' style='height:20px'></div>"));

        Assert.AreEqual(LayoutHarness.FindById(root, "a")!.ActualBottom, LayoutHarness.FindById(root, "b")!.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#display-prop")]
    public void UnknownElement_IsInlineByDefault()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div><x-a>first</x-a> <x-b>second</x-b></div>"));

        Assert.AreEqual(FindWord(root, "first")!.Top, FindWord(root, "second")!.Top, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#display-prop")]
    public void InlineBlock_TakesItsWidthAndHeightOnTheLine()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div>first <span id='ib' style='display:inline-block;width:80px;height:40px'></span> second</div>"));

        var ib = LayoutHarness.FindById(root, "ib")!;
        var first = FindWord(root, "first")!;
        var second = FindWord(root, "second")!;

        Assert.IsGreaterThanOrEqualTo(80 - Delta, second.Left - first.Right);
        Assert.AreEqual(80, ib.ActualWidth, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#dis-pos-flo")]
    public void DisplayNone_IgnoresPositionAndFloat()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:none;float:left;width:100px;height:100px'></div><p id='p' style='margin:0'>text</p>"));

        Assert.IsLessThan(10, FindWord(root, "text")!.Left);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#dis-pos-flo")]
    public void FloatedInline_IsBlockified()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div><span id='f' style='float:left;width:50px;height:30px'></span>text</div>"));

        var f = LayoutHarness.FindById(root, "f")!;

        Assert.AreEqual("block", f.Display);
        Assert.AreEqual(30, f.ActualBottom - f.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#dis-pos-flo")]
    public void AbsolutelyPositionedInline_IsBlockified()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='position:relative'><span id='a' style='position:absolute;top:10px;left:10px;width:50px;height:30px'></span>text</div>"));

        var a = LayoutHarness.FindById(root, "a")!;

        Assert.AreEqual("block", a.Display);
        Assert.AreEqual(30, a.ActualBottom - a.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#dis-pos-flo")]
    public void AbsolutePosition_ComputesFloatToNone()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='position:relative'><div style='position:absolute;float:left;top:0;left:0;width:100px;height:100px'></div><p style='margin:0'>text</p></div>"));

        Assert.IsLessThan(10, FindWord(root, "text")!.Left);
    }

    private static CssRect? FindWord(CssBox box, string text)
    {
        foreach (var word in box.Words)
        {
            if (word.Text == text)
                return word;
        }

        foreach (var child in box.Boxes)
        {
            var found = FindWord(child, text);
            if (found is not null)
                return found;
        }

        return null;
    }
}
