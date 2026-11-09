using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Layout;

[DoNotParallelize]
[TestClass]
public sealed class FloatLayoutIntegrationTests
{
    private const double Delta = 1.0;

    [TestMethod]
    [Spec("css21", "visuren.html#floats")]
    public void Float_PushesFollowingSiblingTextToTheRight()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:300px;'>"
            + "<div style='float:left; width:100px; height:50px;'></div>"
            + "<p id='text' style='margin:0;'>Hello world</p></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var text = LayoutHarness.FindById(root, "text")!;
        var firstWord = FindFirstWord(text);

        Assert.IsNotNull(firstWord);
        Assert.IsTrue(firstWord!.Left >= 90,
            $"first word should be pushed right past the 100px float, was at {firstWord.Left}");
    }

    [TestMethod]
    public void WithoutFloat_SiblingTextStartsAtContainerEdge()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:300px;'>"
            + "<div style='width:100px; height:50px;'></div>"
            + "<p id='text' style='margin:0;'>Hello world</p></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var text = LayoutHarness.FindById(root, "text")!;
        var firstWord = FindFirstWord(text);

        Assert.IsNotNull(firstWord);
        Assert.IsTrue(firstWord!.Left < 10,
            $"first word should start at the container's left edge without a float, was at {firstWord.Left}");
    }

    [TestMethod]
    [Spec("css21", "visuren.html#floats")]
    public void Float_NarrowsAvailableWidth_SoTextWrapsToMoreLines()
    {
        const string longText =
            "This is a fairly long sentence that should wrap across multiple lines once the available width is narrowed by a floated sibling element.";

        var withFloatHtml = LayoutHarness.Wrap(
            $"<div style='width:250px;'><div style='float:left; width:150px; height:40px;'></div>"
            + $"<p id='text' style='margin:0;'>{longText}</p></div>");
        var withoutFloatHtml = LayoutHarness.Wrap(
            $"<div style='width:250px;'><p id='text' style='margin:0;'>{longText}</p></div>");

        var (withFloatRoot, _) = LayoutHarness.Layout(withFloatHtml);
        var (withoutFloatRoot, _) = LayoutHarness.Layout(withoutFloatHtml);

        var withFloatText = LayoutHarness.FindById(withFloatRoot, "text")!;
        var withoutFloatText = LayoutHarness.FindById(withoutFloatRoot, "text")!;

        Assert.IsTrue(withFloatText.ActualBottom - withFloatText.Location.Y
                      > withoutFloatText.ActualBottom - withoutFloatText.Location.Y,
            "narrowing the line width with a float should force extra line wraps and a taller box "
            + $"(with float height: {withFloatText.ActualBottom - withFloatText.Location.Y}, "
            + $"without: {withoutFloatText.ActualBottom - withoutFloatText.Location.Y})");
    }

    [TestMethod]
    [Spec("css21", "visuren.html#float-position")]
    public void FloatLeft_WrapsBelowAFullWidthFloatRightSibling()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:200px;'>"
            + "<div id='r' style='float:right; width:200px; height:50px;'></div>"
            + "<div id='l' style='float:left; width:100px; height:30px;'></div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var r = LayoutHarness.FindById(root, "r")!;
        var l = LayoutHarness.FindById(root, "l")!;

        Assert.IsTrue(l.Location.Y >= r.ActualBottom,
            $"float:left box should wrap below the full-width float:right sibling it can't fit beside "
            + $"(l.Y={l.Location.Y}, r.ActualBottom={r.ActualBottom})");
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "visuren.html#float-position")]
    public void FloatRight_InNarrowerNestedBlock_StaysInsideItsContainingBlock()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:500px;'>"
            + "<div id='outerR' style='float:right; width:50px; height:80px;'></div>"
            + "<div id='cb' style='width:200px;'><div id='r' style='float:right; width:100px; height:30px;'></div></div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var cb = LayoutHarness.FindById(root, "cb")!;
        var r = LayoutHarness.FindById(root, "r")!;

        Assert.AreEqual(cb.ActualRight, r.ActualRight, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "visuren.html#float-position")]
    public void FloatRight_InNarrowerNestedBlock_WithMarginLeft_StaysInsideItsContainingBlock()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:500px;'>"
            + "<div id='outerR' style='float:right; width:280px; height:80px;'></div>"
            + "<div id='cb' style='width:200px;'>"
            + "<div id='r' style='float:right; width:100px; height:30px; margin-left:40px;'></div></div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var cb = LayoutHarness.FindById(root, "cb")!;
        var r = LayoutHarness.FindById(root, "r")!;

        Assert.AreEqual(cb.ActualRight, r.ActualRight, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#floats")]
    public void FloatRight_NarrowsLineWrapWidth_SoTextWrapsBeforeReachingIt()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:300px;'>"
            + "<div id='f' style='float:right; width:100px; height:50px;'></div>"
            + "<p id='text' style='margin:0;'>this line of text should wrap before it reaches the floated box on the right</p></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var floatBox = LayoutHarness.FindById(root, "f")!;
        var text = LayoutHarness.FindById(root, "text")!;
        var floatLeftEdge = floatBox.Location.X - floatBox.ActualMarginLeft;

        var wordsOverlappingFloat = WordsOverlappingVerticalSpan(text, floatBox.Location.Y, floatBox.ActualBottom);

        Assert.AreNotEqual(0, wordsOverlappingFloat.Count);

        foreach (var word in wordsOverlappingFloat)
        {
            Assert.IsTrue(word.Left + word.Width <= floatLeftEdge + 1,
                $"word '{word.Text}' at right={word.Left + word.Width} overlaps the float:right box, "
                + $"whose left edge (including margin) is at {floatLeftEdge}");
        }
    }

    [TestMethod]
    [Spec("css21", "visuren.html#float-position")]
    public void FloatRight_WithMarginLeft_StillReachesContainingBlockRightEdge()
    {
        var html = LayoutHarness.Wrap(
            "<dl style='width:200px; margin:0; padding:0; border:0;'>"
            + "<dd id='dd' style='float:right; width:80px; height:20px; margin:0 0 0 10px;'></dd></dl>");

        var (root, _) = LayoutHarness.Layout(html);
        var dl = LayoutHarness.FindById(root, "dd")!.ParentBox;
        var dd = LayoutHarness.FindById(root, "dd")!;

        Assert.AreEqual(dl.ClientRight, dd.ActualRight, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#floats")]
    public void FloatLeft_StillNarrowsLineWrapWidth_AfterTheRightFloatFix()
    {
        const string longText =
            "this line of text should wrap below and around the floated box on the left before it reaches the container edge";

        var withFloatHtml = LayoutHarness.Wrap(
            $"<div style='width:300px;'><div id='f' style='float:left; width:100px; height:50px;'></div>"
            + $"<p id='text' style='margin:0;'>{longText}</p></div>");
        var withoutFloatHtml = LayoutHarness.Wrap(
            $"<div style='width:300px;'><p id='text' style='margin:0;'>{longText}</p></div>");

        var (withFloatRoot, _) = LayoutHarness.Layout(withFloatHtml);
        var (withoutFloatRoot, _) = LayoutHarness.Layout(withoutFloatHtml);

        var floatBox = LayoutHarness.FindById(withFloatRoot, "f")!;
        var withFloatText = LayoutHarness.FindById(withFloatRoot, "text")!;
        var withoutFloatText = LayoutHarness.FindById(withoutFloatRoot, "text")!;
        var floatRightEdge = floatBox.ActualRight + floatBox.ActualMarginRight;

        var wordsOverlappingFloat =
            WordsOverlappingVerticalSpan(withFloatText, floatBox.Location.Y, floatBox.ActualBottom);

        Assert.AreNotEqual(0, wordsOverlappingFloat.Count);

        foreach (var word in wordsOverlappingFloat)
        {
            Assert.IsTrue(word.Left >= floatRightEdge - 1,
                $"word '{word.Text}' at left={word.Left} starts before the float:left box's right edge "
                + $"(including margin) at {floatRightEdge}");
        }

        Assert.IsTrue(withFloatText.ActualBottom - withFloatText.Location.Y
                      > withoutFloatText.ActualBottom - withoutFloatText.Location.Y,
            "narrowing the line width with a float:left should force extra line wraps and a taller box");
    }

    [TestMethod]
    [Spec("css21", "visuren.html#flow-control")]
    public void ClearLeft_IgnoresAPrecedingFloatRightSibling()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:200px;'>"
            + "<div id='r' style='float:right; width:50px; height:80px;'></div>"
            + "<div id='cleared' style='clear:left; margin:0;'>text</div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var cleared = LayoutHarness.FindById(root, "cleared")!;

        Assert.IsTrue(cleared.Location.Y < 80,
            $"clear:left must not clear past a float:right sibling, was pushed to Y={cleared.Location.Y}");
    }

    [TestMethod]
    [Spec("css21", "visuren.html#flow-control")]
    public void ClearRight_IgnoresAPrecedingFloatLeftSibling()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:200px;'>"
            + "<div id='l' style='float:left; width:50px; height:80px;'></div>"
            + "<div id='cleared' style='clear:right; margin:0;'>text</div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var cleared = LayoutHarness.FindById(root, "cleared")!;

        Assert.IsTrue(cleared.Location.Y < 80,
            $"clear:right must not clear past a float:left sibling, was pushed to Y={cleared.Location.Y}");
    }

    [TestMethod]
    [Spec("css21", "visuren.html#floats")]
    public void FollowingBlock_FlowsAsIfTheFloatDidNotExist()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:300px'>"
            + "<div id='f' style='float:left;width:100px;height:50px'></div>"
            + "<div id='b' style='height:10px'></div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var f = LayoutHarness.FindById(root, "f")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(f.Location.Y, b.Location.Y, Delta);
        Assert.AreEqual(0, b.Location.X, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#floats")]
    public void BlockFormattingContextRoot_DoesNotOverlapAFloat()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:300px'>"
            + "<div id='f' style='float:left;width:100px;height:50px'></div>"
            + "<div id='b' style='overflow:hidden;height:20px'></div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var f = LayoutHarness.FindById(root, "f")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.IsTrue(b.Location.X >= f.ActualRight - Delta || b.Location.Y >= f.ActualBottom - Delta,
            $"b at ({b.Location.X}, {b.Location.Y}) overlaps the float ending at ({f.ActualRight}, {f.ActualBottom})");
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#floats")]
    public void InlineFloat_IsPlacedAtTheTopOfTheCurrentLine_AndTheEarlierTextMovesPastIt()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:300px'>before<span id='f' style='float:left;width:50px;height:30px'></span> after</div>");

        var (root, _) = LayoutHarness.Layout(html);
        var f = LayoutHarness.FindById(root, "f")!;
        var before = FindWordByText(root, "before")!;

        Assert.AreEqual(0, f.Location.X, Delta);
        Assert.AreEqual(before.Top, f.Location.Y, Delta);
        Assert.IsGreaterThanOrEqualTo(50 - Delta, before.Left);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#floats")]
    public void Float_PaintsInFrontOfAnOverlappingInFlowBlock()
    {
        var html = PaintHarness.Wrap(
            "<div style='float:left;width:50px;height:50px;background-color:rgb(255,0,0)'></div>"
            + "<div style='height:50px;background-color:rgb(0,0,255)'></div>");

        var (_, container) = PaintHarness.Layout(html);
        var rects = PaintHarness.PaintPage(container).Log.OfType<RecordingGraphics.DrawRectCall>().ToList();

        var floatIndex = rects.FindIndex(c => c.Color.R == 255 && c.Color.B == 0);
        var blockIndex = rects.FindIndex(c => c.Color.B == 255 && c.Color.R == 0);

        Assert.IsGreaterThanOrEqualTo(0, floatIndex);
        Assert.IsGreaterThan(blockIndex, floatIndex);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#float-position")]
    public void LeftFloat_OuterEdgeTouchesTheContainingBlockEdge()
    {
        var html = LayoutHarness.Wrap(
            "<div style='padding-left:20px;width:300px'><div id='f' style='float:left;margin-left:10px;width:50px;height:20px'></div></div>");

        var (root, _) = LayoutHarness.Layout(html);

        Assert.AreEqual(30, LayoutHarness.FindById(root, "f")!.Location.X, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#float-position")]
    public void SuccessiveLeftFloats_StackSideBySide()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:300px'>"
            + "<div id='a' style='float:left;width:50px;height:20px;margin-right:5px'></div>"
            + "<div id='b' style='float:left;width:50px;height:20px;margin-left:7px'></div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(a.Location.Y, b.Location.Y, Delta);
        Assert.AreEqual(a.ActualRight + 5 + 7, b.Location.X, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#float-position")]
    public void SuccessiveRightFloats_StackSideBySide()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:300px'>"
            + "<div id='a' style='float:right;width:50px;height:20px'></div>"
            + "<div id='b' style='float:right;width:50px;height:20px'></div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(250, a.Location.X, Delta);
        Assert.AreEqual(a.Location.Y, b.Location.Y, Delta);
        Assert.AreEqual(a.Location.X, b.ActualRight, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#float-position")]
    public void LeftFloatThatDoesNotFit_MovesBelowTheEarlierLeftFloat()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:100px'>"
            + "<div id='a' style='float:left;width:60px;height:20px'></div>"
            + "<div id='b' style='float:left;width:60px;height:20px'></div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(0, b.Location.X, Delta);
        Assert.AreEqual(a.ActualBottom, b.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#float-position")]
    public void FloatThatDoesNotFit_IsPlacedAsHighAsPossible()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:120px'>"
            + "<div id='a' style='float:left;width:50px;height:100px'></div>"
            + "<div id='b' style='float:left;width:50px;height:20px'></div>"
            + "<div id='c' style='float:left;width:50px;height:20px'></div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var b = LayoutHarness.FindById(root, "b")!;
        var c = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(b.ActualBottom, c.Location.Y, Delta);
        Assert.AreEqual(50, c.Location.X, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#float-position")]
    public void Float_IsNotHigherThanAnEarlierBlock()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:300px'>"
            + "<div id='a' style='height:40px'></div>"
            + "<div id='f' style='float:left;width:50px;height:20px'></div></div>");

        var (root, _) = LayoutHarness.Layout(html);

        Assert.AreEqual(LayoutHarness.FindById(root, "a")!.ActualBottom, LayoutHarness.FindById(root, "f")!.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#float-position")]
    public void Float_IsNotHigherThanAnEarlierLineBox()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:300px'>"
            + "<p id='p' style='margin:0'>text</p>"
            + "<div id='f' style='float:left;width:50px;height:20px'></div></div>");

        var (root, _) = LayoutHarness.Layout(html);

        Assert.IsGreaterThanOrEqualTo(FindWordByText(root, "text")!.Bottom - Delta, LayoutHarness.FindById(root, "f")!.Location.Y);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#flow-control")]
    public void ClearBoth_MovesTheBoxBelowLeftAndRightFloats()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:300px'>"
            + "<div style='float:left;width:50px;height:40px'></div>"
            + "<div style='float:right;width:50px;height:70px'></div>"
            + "<div id='c' style='clear:both;height:10px'></div></div>");

        var (root, _) = LayoutHarness.Layout(html);

        Assert.AreEqual(70, LayoutHarness.FindById(root, "c")!.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#flow-control")]
    public void Clearance_PutsTheBorderEdgeAtTheFloatsBottomOuterEdge()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:300px'>"
            + "<div id='b1' style='height:10px;margin-bottom:10px'></div>"
            + "<div style='float:left;width:50px;height:50px;margin-bottom:5px'></div>"
            + "<div id='b2' style='clear:left;margin-top:20px;height:10px'></div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var b1 = LayoutHarness.FindById(root, "b1")!;

        Assert.AreEqual(b1.ActualBottom + 10 + 50 + 5, LayoutHarness.FindById(root, "b2")!.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "visuren.html#flow-control")]
    public void NegativeClearance_KeepsTheHypotheticalPositionWhenAlreadyPastTheFloat()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:300px'>"
            + "<div id='p1' style='height:10px;margin-bottom:40px'></div>"
            + "<div style='float:left;width:50px;height:20px;margin:0'></div>"
            + "<div id='p3' style='clear:left;margin-top:30px;height:10px'></div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var p1 = LayoutHarness.FindById(root, "p1")!;

        Assert.AreEqual(p1.ActualBottom + 40 + 20, LayoutHarness.FindById(root, "p3")!.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#flow-control")]
    public void ClearOnAFloat_MovesItBelowEarlierFloats()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:300px'>"
            + "<div id='a' style='float:left;width:50px;height:40px'></div>"
            + "<div id='b' style='float:left;clear:left;width:50px;height:20px'></div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(0, b.Location.X, Delta);
        Assert.AreEqual(a.ActualBottom, b.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#flow-control")]
    public void Clear_IgnoresFloatsInOtherBlockFormattingContexts()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:300px'>"
            + "<div id='bfc' style='overflow:hidden;height:10px'><div style='float:left;width:50px;height:80px'></div></div>"
            + "<div id='c' style='clear:both;height:10px'></div></div>");

        var (root, _) = LayoutHarness.Layout(html);

        Assert.AreEqual(10, LayoutHarness.FindById(root, "c")!.Location.Y, Delta);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static CssRect? FindWordByText(CssBox box, string text)
    {
        foreach (var word in box.Words)
        {
            if (word.Text == text)
                return word;
        }

        foreach (var child in box.Boxes)
        {
            var found = FindWordByText(child, text);
            if (found is not null)
                return found;
        }

        return null;
    }

    private static CssRect? FindFirstWord(CssBox box)
    {
        if (box.Words.Count > 0) return box.Words[0];
        foreach (var child in box.Boxes)
        {
            var found = FindFirstWord(child);
            if (found is not null) return found;
        }
        return null;
    }

    private static List<CssRect> WordsOverlappingVerticalSpan(CssBox box, double top, double bottom)
    {
        List<CssRect> words = [];
        CollectWordsOverlappingVerticalSpan(box, top, bottom, words);
        return words;
    }

    private static void CollectWordsOverlappingVerticalSpan(CssBox box, double top, double bottom, List<CssRect> words)
    {
        foreach (var word in box.Words)
        {
            if (word.Top < bottom && word.Top + word.Height > top)
            {
                words.Add(word);
            }
        }

        foreach (var child in box.Boxes)
        {
            CollectWordsOverlappingVerticalSpan(child, top, bottom, words);
        }
    }
}
