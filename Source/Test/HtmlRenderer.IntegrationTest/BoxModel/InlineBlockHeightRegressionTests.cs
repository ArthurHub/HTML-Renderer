using System;
using System.Collections.Generic;
using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.BoxModel;

/// <summary>
/// Verifies block height around a padded <c>inline-block</c> (e.g. a themeable button).
/// </summary>
/// <remarks>
/// An inline-block goes through the same inline flow as an inline (<c>CssBox.IsInline</c>), it isn't measured
/// as one atomic box first.
/// </remarks>
[DoNotParallelize]
[TestClass]
public sealed class InlineBlockHeightRegressionTests
{
    [Ignore("Gap, see Spec/html.yml")]
    [TestMethod]
    [Spec("html", "rendering.html#button-layout")]
    public void PaddedInlineBlock_SoleContentOfBlock_BlockHeightCoversPadding()
    {
        const string html = "<div style='height:300px'>filler</div>" +
                             "<div class='wrapper'><button class='btn' style='padding:100px 14px'>Go</button></div>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html));

        var wrapper = FindBoxByClass(root, "wrapper");
        Assert.IsNotNull(wrapper);

        var height = wrapper!.ActualBottom - wrapper.Location.Y;
        Assert.IsTrue(height > 0,
            $"Block wrapping only a padded inline-block must not collapse to a negative height (top={wrapper.Location.Y}, bottom={wrapper.ActualBottom})");
        Assert.IsTrue(height >= 200,
            $"Block height ({height}) must cover the inline-block's own 200px of vertical padding");
    }

    // A tighter companion to the test above: asserts the block's height against the button's own measured
    // insets and word height, not just a loose lower bound. Delta of 1px is a guess at cross-engine
    // rounding/line-metric slack - flagging for the follow-up verification pass in case it needs widening.
    [Ignore("Gap, see Spec/html.yml")]
    [TestMethod]
    [Spec("html", "rendering.html#button-layout")]
    public void PaddedInlineBlock_SoleContentOfBlock_BlockHeightMatchesInsetsExactly()
    {
        const string html = "<div style='height:300px'>filler</div>" +
                             "<div class='wrapper'><button class='btn' style='padding:100px 14px'>Go</button></div>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html));

        var wrapper = FindBoxByClass(root, "wrapper");
        var button = FindBoxByClass(root, "btn");
        Assert.IsNotNull(wrapper);
        Assert.IsNotNull(button);

        var word = FindFirstWord(button!);
        Assert.IsNotNull(word);

        var expectedHeight = button!.ActualBorderTopWidth + button.ActualPaddingTop
            + word!.Height
            + button.ActualBorderBottomWidth + button.ActualPaddingBottom;
        var actualHeight = wrapper!.ActualBottom - wrapper.Location.Y;

        Assert.AreEqual(expectedHeight, actualHeight, 1,
            $"Block height ({actualHeight}) must equal the button's own top inset + word height + bottom inset ({expectedHeight}) exactly, not merely cover it");
    }

    [TestMethod]
    public void PaddedInlineBlock_TallerContentLine_DoesNotShrinkBlock()
    {
        const string html = "<div style='height:300px'>filler</div>" +
                             "<div class='wrapper'><button style='padding:1px 14px'>Go</button></div>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html));

        var wrapper = FindBoxByClass(root, "wrapper");
        Assert.IsNotNull(wrapper);
        Assert.IsTrue(wrapper!.ActualBottom > wrapper.Location.Y,
            $"Block height must stay positive (top={wrapper.Location.Y}, bottom={wrapper.ActualBottom})");
    }

    // CSS2.1 §10.8.1: the vertical padding/border of a non-replaced `display: inline` box must NOT
    // influence line box height - a plain span's 200px of vertical padding must not grow its block.
    [TestMethod]
    public void PaddedPlainInline_DoesNotGrowBlockHeight()
    {
        const string html = "<div style='height:300px'>filler</div>" +
                             "<div class='wrapper'><span style='padding:100px 0'>text</span></div>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html));

        var wrapper = FindBoxByClass(root, "wrapper");
        Assert.IsNotNull(wrapper);

        var height = wrapper!.ActualBottom - wrapper.Location.Y;
        Assert.IsTrue(height > 0,
            $"Block height must stay positive (top={wrapper.Location.Y}, bottom={wrapper.ActualBottom})");
        Assert.IsTrue(height < 200,
            $"Plain inline vertical padding must not grow the block (CSS2.1 section 10.8.1) - got height {height}");
    }

    // CSS2.1 §8.1: an atomic inline-level box's content is laid out inside its padding box, so the label of
    // a padded button must start border+padding-top BELOW the box's top edge (and end padding-bottom above
    // its bottom edge).
    [TestMethod]
    public void PaddedInlineBlock_WordsSitInsidePaddingBox()
    {
        const string html = "<button class='btn' style='padding:6px 14px'>Go</button>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html));

        var button = FindBoxByClass(root, "btn");
        Assert.IsNotNull(button);

        var rect = button!.Rectangles.Values.Single();
        var word = FindFirstWord(button);
        Assert.IsNotNull(word);

        Assert.IsTrue(word!.Top >= rect.Top + 6 - 0.5,
            $"Button label (top={word.Top}) must sit at least padding-top (6) below the box top ({rect.Top})");
        Assert.IsTrue(word.Bottom <= rect.Bottom - 6 + 0.5,
            $"Button label (bottom={word.Bottom}) must end at least padding-bottom (6) above the box bottom ({rect.Bottom})");
    }

    // With only padding-bottom set, the rect must extend below the words by that amount while the top edge
    // stays at the word band - regression guard against padding-top/padding-bottom being swapped.
    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#block-root-margin")]
    public void AsymmetricPaddingBottom_ExpandsRectBottomByPaddingBottom()
    {
        const string html = "<button class='btn' style='padding:0 0 30px 0'>Go</button>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html));

        var button = FindBoxByClass(root, "btn");
        Assert.IsNotNull(button);

        var rect = button!.Rectangles.Values.Single();
        var word = FindFirstWord(button);
        Assert.IsNotNull(word);

        Assert.IsTrue(rect.Bottom >= word!.Bottom + 30 - 0.5,
            $"Box rect bottom ({rect.Bottom}) must extend padding-bottom (30) below the words (bottom={word.Bottom})");
        Assert.IsTrue(Math.Abs(rect.Top - word.Top) < 0.5,
            $"With no padding-top the rect top ({rect.Top}) must coincide with the word band top ({word.Top})");
    }

    [TestMethod]
    public void PaddedInlineBlockPseudoElement_WordsSitInsidePaddingBox()
    {
        const string html = "<style>.host::before { content: 'NEW'; display: inline-block; padding: 6px 14px; }</style>" +
                             "<div class='host'>after marker</div>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html));

        var host = FindBoxByClass(root, "host");
        Assert.IsNotNull(host);
        var pseudo = FindFirst(host!, b => b.Display == "inline-block");
        Assert.IsNotNull(pseudo);

        var rect = pseudo!.Rectangles.Values.Single();
        var word = FindFirstWord(pseudo);
        Assert.IsNotNull(word);

        Assert.IsTrue(word!.Top >= rect.Top + 6 - 0.1,
            $"Pseudo-element label (top={word.Top}) must sit at least padding-top (6) below the box top ({rect.Top})");
    }

    [TestMethod]
    public void InsetShiftNearPageBoundary_WordsDoNotStraddleThePage()
    {
        const string html = "<div style='height:760px'>filler</div>" +
                             "<div><button style='padding:100px 14px'>Go</button></div>";

        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(html), maxWidth: 595, maxHeight: 842);
        var pageHeight = container.PageSize.Height;

        var button = FindFirst(root, b => b.HtmlTag?.Name == "button");
        Assert.IsNotNull(button);
        var word = FindFirstWord(button!);
        Assert.IsNotNull(word);

        var topPage = Math.Floor(word!.Top / pageHeight);
        var bottomPage = Math.Floor((word.Bottom - 0.01) / pageHeight);
        Assert.AreEqual(topPage, bottomPage,
            $"Inset-shifted word should not straddle a page boundary (top={word.Top:F1}, bottom={word.Bottom:F1}, pageHeight={pageHeight})");
    }

    [Ignore("Gap, see Spec/css-multicol-1.yml")]
    [TestMethod]
    [Spec("css-multicol-1", "#the-multi-column-model")]
    public void InsetShiftNearColumnBoundary_WholeLineMovesToNextColumn()
    {
        const string html = "<div id='mc' style='columns:2;column-gap:0;column-fill:auto;width:200px'>" +
                             "<div style='height:220px'>filler</div>" +
                             "<button id='btn' style='padding-top:60px'>Go</button></div>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html), maxWidth: 200, maxHeight: 300);

        var mc = LayoutHarness.FindById(root, "mc")!;
        var button = LayoutHarness.FindById(root, "btn")!;

        var word = FindFirstWord(button);
        Assert.IsNotNull(word);

        var columnWidth = (mc.ClientRight - mc.ClientLeft) / 2;
        Assert.IsTrue(word!.Left >= mc.ClientLeft + columnWidth - 0.1,
            $"expected the inset-shifted line to move whole to column 2 (columnWidth={columnWidth}), word is at x={word.Left}");
        Assert.IsTrue(Math.Abs(word.Top - 60) < 5,
            $"expected padding-top (60) to apply once, at column 2's own content top, word top={word.Top}");
    }

    [Ignore("Gap, see Spec/css-multicol-1.yml")]
    [TestMethod]
    [Spec("css-multicol-1", "#the-multi-column-model")]
    public void InsetShiftedInlineBlock_SecondInternalLine_MovesAloneToNextColumn()
    {
        const string html = "<div id='mc' style='columns:2;column-gap:0;column-fill:auto;width:200px;orphans:1;widows:1'>" +
                             "<div style='height:150px'>filler</div>" +
                             "<span id='btn' style='display:inline-block;padding-top:60px'>First<br>Second</span></div>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html), maxWidth: 200, maxHeight: 300);

        var mc = LayoutHarness.FindById(root, "mc")!;
        var button = LayoutHarness.FindById(root, "btn")!;

        var firstWord = AllWords(button).Single(w => w.Text == "First");
        var secondWord = AllWords(button).Single(w => w.Text == "Second");

        var columnWidth = (mc.ClientRight - mc.ClientLeft) / 2;

        Assert.IsTrue(firstWord.Left < mc.ClientLeft + columnWidth - 0.1,
            $"expected the button's first internal line to stay in column 1, word is at x={firstWord.Left}");
        Assert.IsTrue(secondWord.Left >= mc.ClientLeft + columnWidth - 0.1,
            $"expected the button's second internal line to move alone to column 2, word is at x={secondWord.Left}");
        Assert.IsTrue(secondWord.Top < 15,
            $"expected the resumed (continuation) line to start at column 2's raw content top with no re-applied inset, word top={secondWord.Top}");
    }

    #region Helpers

    private static CssBox? FindFirst(CssBox box, Func<CssBox, bool> predicate)
    {
        if (predicate(box)) return box;
        foreach (var child in box.Boxes)
        {
            var found = FindFirst(child, predicate);
            if (found != null) return found;
        }
        return null;
    }

    private static CssRect? FindFirstWord(CssBox box)
    {
        if (box.Words.Count > 0)
            return box.Words[0];

        foreach (var child in box.Boxes)
        {
            var word = FindFirstWord(child);
            if (word != null)
                return word;
        }

        return null;
    }

    private static List<CssRect> AllWords(CssBox box)
    {
        var words = new List<CssRect>(box.Words);

        foreach (var child in box.Boxes)
        {
            words.AddRange(AllWords(child));
        }

        return words;
    }

    private static CssBox? FindBoxByClass(CssBox root, string className)
    {
        var classAttr = root.HtmlTag?.TryGetAttribute("class", "");
        if (!string.IsNullOrEmpty(classAttr))
        {
            foreach (var cls in classAttr.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (cls == className)
                    return root;
            }
        }

        foreach (var child in root.Boxes)
        {
            var result = FindBoxByClass(child, className);
            if (result != null)
                return result;
        }

        return null;
    }

    #endregion
}
