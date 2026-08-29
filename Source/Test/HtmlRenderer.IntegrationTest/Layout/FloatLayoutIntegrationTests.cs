using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Layout;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/FloatLayoutRegressionTests.cs. CSS 2.1 §9.5: a floated box is taken
/// out of normal flow and shifted to the left/right edge of its containing block; subsequent inline content
/// flows around it, and other floats stack against it rather than overlapping. §9.5.2 defines <c>clear</c> as
/// only clearing floats of the matching (or "both") side.
/// <para>
/// The four perf-regression-guard cases from the source file (<c>FloatScanCalls</c>/<c>FloatScanBoxVisits</c>
/// counters on <c>HtmlContainerInt</c>, guarding an O(document size) vs O(1) float-scan short-circuit) are not
/// ported: this fork's <c>HtmlContainerInt</c> has the same <c>HasFloatedBoxes</c> short-circuit and float-scan
/// helpers (confirmed in <c>DomUtils.GetFirstIntersectingFloatBox</c> et al.), but no <c>FloatScanCalls</c>/
/// <c>FloatScanBoxVisits</c> instrumentation to observe it through - that's an internal perf counter, not CSS
/// 2.1 behavior, so it's out of scope for this pass rather than something to add.
/// </para>
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class FloatLayoutIntegrationTests
{
    private const double Delta = 1.0;

    [TestMethod]
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
    public void FloatLeft_WrapsBelowAFullWidthFloatRightSibling()
    {
        // A float:left box that would overlap a previously-placed, full-width float:right sibling must wrap
        // below it rather than overlapping.
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
    public void FloatRight_InNarrowerNestedBlock_AvoidsAWiderAncestorFloatRightSibling()
    {
        // A float:right box placed inside a narrower, non-floated nested block still avoids an ancestor
        // float:right sibling that sits past the nested block's own right edge.
        var html = LayoutHarness.Wrap(
            "<div style='width:500px;'>"
            + "<div id='outerR' style='float:right; width:50px; height:80px;'></div>"
            + "<div style='width:200px;'><div id='r' style='float:right; width:100px; height:30px;'></div></div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var outerR = LayoutHarness.FindById(root, "outerR")!;
        var r = LayoutHarness.FindById(root, "r")!;

        Assert.AreEqual(outerR.Location.X - outerR.ActualMarginLeft, r.ActualRight, Delta);
    }

    [TestMethod]
    public void FloatRight_InNarrowerNestedBlock_WithMarginLeft_StillAvoidsAWiderAncestorFloatRightSibling()
    {
        var html = LayoutHarness.Wrap(
            "<div style='width:500px;'>"
            + "<div id='outerR' style='float:right; width:280px; height:80px;'></div>"
            + "<div style='width:200px;'>"
            + "<div id='r' style='float:right; width:100px; height:30px; margin-left:40px;'></div></div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var outerR = LayoutHarness.FindById(root, "outerR")!;
        var r = LayoutHarness.FindById(root, "r")!;

        Assert.AreEqual(outerR.Location.X - outerR.ActualMarginLeft, r.ActualRight, Delta);
    }

    [TestMethod]
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
    public void ClearLeft_IgnoresAPrecedingFloatRightSibling()
    {
        // clear:left only clears past float:left siblings - a float:right sibling must not push it down.
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
    public void ClearRight_IgnoresAPrecedingFloatLeftSibling()
    {
        // Symmetric case: clear:right ignoring a float:left sibling.
        var html = LayoutHarness.Wrap(
            "<div style='width:200px;'>"
            + "<div id='l' style='float:left; width:50px; height:80px;'></div>"
            + "<div id='cleared' style='clear:right; margin:0;'>text</div></div>");

        var (root, _) = LayoutHarness.Layout(html);
        var cleared = LayoutHarness.FindById(root, "cleared")!;

        Assert.IsTrue(cleared.Location.Y < 80,
            $"clear:right must not clear past a float:left sibling, was pushed to Y={cleared.Location.Y}");
    }

    // ── Helpers ────────────────────────────────────────────────────────────

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
