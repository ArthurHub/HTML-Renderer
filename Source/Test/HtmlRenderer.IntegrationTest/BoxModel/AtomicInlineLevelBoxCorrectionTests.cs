using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.BoxModel;

/// <summary>
/// DomParser's "block inside inline" DOM-correction pass (CorrectBlockInsideInline) should never look through an
/// inline-level box that establishes its own independent formatting context (inline-block/inline-table both do)
/// when deciding whether an ANCESTOR box needs splitting - DomUtils.ContainsInlinesOnly is meant to make such a
/// box opaque for that purpose. A child whose own resolved display is "none" (not "inline") should not make the
/// correction pass misclassify the box's content as containing a "block", incorrectly splitting the child out of
/// its real parent entirely.
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class AtomicInlineLevelBoxCorrectionTests
{
    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#anonymous-block-level")]
    public void DisplayNoneChildOfInlineBlockBox_StaysNestedUnderItsRealParent()
    {
        var html = LayoutHarness.Wrap(
            "<select id='sel'>" +
            "<option id='opt1' style='display:none'>Red</option>" +
            "<option id='opt2' style='display:none'>Green</option>" +
            "</select>");

        var (root, _) = LayoutHarness.Layout(html);
        var select = LayoutHarness.FindById(root, "sel")!;
        var opt1 = LayoutHarness.FindById(root, "opt1")!;
        var opt2 = LayoutHarness.FindById(root, "opt2")!;

        Assert.IsTrue(IsDescendantOf(opt1, select), "opt1 (display:none) must stay nested under its real <select> parent.");
        Assert.IsTrue(IsDescendantOf(opt2, select), "opt2 must stay nested under its real <select> parent.");
    }

    [TestMethod]
    public void DisplayNoneChildOfInlineTableBox_StaysNestedUnderItsRealParent()
    {
        var html = LayoutHarness.Wrap(
            "<div id='wrap'>" +
            "<div id='it' style='display:inline-table'>" +
            "<div style='display:table-row'><div id='cell' style='display:table-cell'>cell</div></div>" +
            "<div id='hidden' style='display:none'>hidden</div>" +
            "</div>" +
            "</div>");

        var (root, _) = LayoutHarness.Layout(html);
        var inlineTable = LayoutHarness.FindById(root, "it")!;
        var hidden = LayoutHarness.FindById(root, "hidden")!;

        Assert.IsTrue(IsDescendantOf(hidden, inlineTable), "a display:none sibling inside an inline-table must stay nested under it.");
    }

    private static bool IsDescendantOf(TheArtOfDev.HtmlRenderer.Core.Dom.CssBox box, TheArtOfDev.HtmlRenderer.Core.Dom.CssBox ancestor)
    {
        var current = box.ParentBox;
        while (current != null)
        {
            if (ReferenceEquals(current, ancestor)) return true;
            current = current.ParentBox;
        }
        return false;
    }
}
