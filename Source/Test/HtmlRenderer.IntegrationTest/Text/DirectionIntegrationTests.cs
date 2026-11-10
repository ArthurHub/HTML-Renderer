using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Text;

[DoNotParallelize]
[TestClass]
public sealed class DirectionIntegrationTests
{
    [TestMethod]
    [Spec("css21", "visuren.html#direction")]
    [Spec("css-writing-modes-4", "#direction")]
    public void Direction_IsInherited()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div style='direction:rtl'><p id='p'>text</p></div>"));

        Assert.AreEqual("rtl", LayoutHarness.FindById(root, "p")!.Direction);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#direction")]
    [Spec("css-writing-modes-4", "#bidi-para-direction")]
    [Spec("css-writing-modes-4", "#logical-direction-layout")]
    [Spec("css-text-3", "#bidi-linebox")]
    [Spec("css-text-4", "#bidi-linebox")]
    public void RightToLeftWords_InAnRtlBlock_RunFromRightToLeft()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='direction:rtl;width:300px'>אבג דהו</div>"));

        var first = FindWord(root, "אבג")!;
        var second = FindWord(root, "דהו")!;

        Assert.IsGreaterThan(second.Left, first.Left);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#direction")]
    [Spec("css-writing-modes-4", "#bidi-algo")]
    public void LeftToRightWords_InAnRtlBlock_KeepTheirOrder()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='direction:rtl;width:300px'>one two</div>"));

        Assert.IsLessThan(FindWord(root, "two")!.Left, FindWord(root, "one")!.Left);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#direction")]
    [Spec("css-writing-modes-4", "#bidi-control-codes")]
    public void DirectionOnAnInline_WithoutUnicodeBidi_DoesNotReorder()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:300px'>one <span style='direction:rtl'>two three</span> four</div>"));

        Assert.IsLessThan(FindWord(root, "three")!.Left, FindWord(root, "two")!.Left);
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
