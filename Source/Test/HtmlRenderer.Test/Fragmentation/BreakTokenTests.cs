using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragmentation;

namespace HtmlRenderer.Test.Fragmentation;

/// <summary>
/// Ported from PeachPDF.Tests/Html/Core/Fragmentation/BreakTokenTests.cs (BreakTokenTests).
/// </summary>
/// <remarks>
/// The resumption record: says <i>where</i> layout stopped and nothing about geometry - the box tree still
/// holds the coordinates - so these pin its shape rather than any placement. PeachPDF's own file also covers
/// <c>InlineBreakToken</c>/<c>FlexBreakToken</c>/<c>GridBreakToken</c>/<c>FlexColumnBreakToken</c>, none of
/// which exist here - <c>BreakToken.cs</c>'s own doc comment confirms only forced <c>break-before</c>/
/// <c>break-after: page</c> ever produces a real cross-pass token in this port (everything else - overflow,
/// break-inside:avoid, keep-with-next, widows/orphans, table-row breaks - turned out to be same-pass local
/// corrections instead), so <see cref="BlockBreakToken"/> is the only concrete <see cref="BreakToken"/> to
/// test. Every test below that PeachPDF built over a different token kind is dropped as out of scope rather
/// than adapted; <c>Chain_...</c> is kept but rewritten to chain only <see cref="BlockBreakToken"/> links,
/// since that's the only concrete kind this port has to chain.
/// </remarks>
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

        // A record's compiler-generated equality is what lets a resumed pass compare "did this pass land
        // on the same resumption point as a previous one" without hand-written Equals/GetHashCode - two
        // independently-built tokens over the same field values must compare equal.
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
