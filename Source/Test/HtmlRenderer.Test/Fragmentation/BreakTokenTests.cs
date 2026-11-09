using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragmentation;

namespace HtmlRenderer.Test.Fragmentation;

/// <summary>
/// The resumption record says where layout stopped, the box tree holds the geometry, so these pin its
/// shape rather than any placement.
/// </summary>
[TestClass]
public sealed class BreakTokenTests
{
    [TestMethod]
    public void BreakBefore_CarriesNoChildToken()
    {
        var box = new CssBox(null, null);

        var token = new BlockBreakToken(box, ResumeSlotIndex: 1, ResumeChildIndex: 3, ChildToken: null, IsBreakBefore: true, ResumeTopOverride: null);

        // A break *before* a child means the child was never entered, so there is nothing inside it
        // to resume - this is what makes "no fragment in the earlier fragmentainer" structural.
        Assert.IsTrue(token.IsBreakBefore);
        Assert.IsNull(token.ChildToken);
        Assert.AreEqual(3, token.ResumeChildIndex);
        Assert.AreSame(box, token.Box);
    }

    [TestMethod]
    public void BreakInside_CarriesTheChildsOwnToken()
    {
        var parent = new CssBox(null, null);
        var child = new CssBox(null, null);

        var childToken = new BlockBreakToken(child, ResumeSlotIndex: 1, ResumeChildIndex: 1, ChildToken: null, IsBreakBefore: true, ResumeTopOverride: null);
        var token = new BlockBreakToken(parent, ResumeSlotIndex: 1, ResumeChildIndex: 0, ChildToken: childToken, IsBreakBefore: false, ResumeTopOverride: null);

        Assert.IsFalse(token.IsBreakBefore);
        Assert.AreSame(childToken, token.ChildToken);
    }

    [TestMethod]
    public void Chain_NestsOneLinkPerAncestorOnThePathToTheContextRoot()
    {
        var root = new CssBox(null, null);
        var middle = new CssBox(null, null);
        var leaf = new CssBox(null, null);

        var leafToken = new BlockBreakToken(leaf, ResumeSlotIndex: 1, ResumeChildIndex: 2, ChildToken: null, IsBreakBefore: true, ResumeTopOverride: null);
        var middleToken = new BlockBreakToken(middle, ResumeSlotIndex: 1, ResumeChildIndex: 1, ChildToken: leafToken, IsBreakBefore: false, ResumeTopOverride: null);
        var rootToken = new BlockBreakToken(root, ResumeSlotIndex: 1, ResumeChildIndex: 2, ChildToken: middleToken, IsBreakBefore: false, ResumeTopOverride: null);

        // Walking the chain down from the root is exactly how a resumed pass re-enters each ancestor
        // mid-flight while leaving boxes off the path alone.
        var boxes = new List<CssBox>();
        for (BlockBreakToken? t = rootToken; t is not null; t = t.ChildToken as BlockBreakToken)
            boxes.Add(t.Box);

        CollectionAssert.AreEqual(new[] { root, middle, leaf }, boxes);
    }

    [TestMethod]
    public void ResumeTopOverride_IsCarriedForTheAdjustedTargetPathsThatComputeIt()
    {
        var box = new CssBox(null, null);

        var token = new BlockBreakToken(box, ResumeSlotIndex: 1, ResumeChildIndex: 0, ChildToken: null, IsBreakBefore: true, ResumeTopOverride: 1234.5);

        // Margin truncation and the keep-with-next pull have already worked out where the box goes;
        // the resumed pass must use that value rather than re-deriving it.
        Assert.AreEqual(1234.5, token.ResumeTopOverride);
    }

    [TestMethod]
    public void BlockBreakToken_IsARecord_WithStructuralEquality()
    {
        var box = new CssBox(null, null);

        // A resumed pass compares tokens to detect landing on the same resumption point again.
        var first = new BlockBreakToken(box, ResumeSlotIndex: 2, ResumeChildIndex: 4, ChildToken: null, IsBreakBefore: true, ResumeTopOverride: null);
        var second = new BlockBreakToken(box, ResumeSlotIndex: 2, ResumeChildIndex: 4, ChildToken: null, IsBreakBefore: true, ResumeTopOverride: null);

        Assert.AreEqual(first, second);
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
    }

    [TestMethod]
    public void BlockBreakToken_WithADifferentResumeChildIndex_ComparesUnequal()
    {
        var box = new CssBox(null, null);

        var first = new BlockBreakToken(box, ResumeSlotIndex: 2, ResumeChildIndex: 4, ChildToken: null, IsBreakBefore: true, ResumeTopOverride: null);
        var second = new BlockBreakToken(box, ResumeSlotIndex: 2, ResumeChildIndex: 5, ChildToken: null, IsBreakBefore: true, ResumeTopOverride: null);

        Assert.AreNotEqual(first, second);
    }
}
