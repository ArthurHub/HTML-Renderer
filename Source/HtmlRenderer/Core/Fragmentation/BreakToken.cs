using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// A resumption record: where layout stopped in one fragmentainer, so the next one can pick up from
    /// exactly that point (https://www.w3.org/TR/css-break-3/#breaking-controls, CSS Fragmentation Level 3
    /// §2/§4.4). Ported from PeachPDF's <c>BreakToken</c>, reduced to what this port's driver loop
    /// actually needs: only forced <c>break-before</c>/<c>break-after: page</c> ever produces a real
    /// cross-pass token here (see <see cref="BlockFragmentation.TryGetForcedBreakTarget"/>) - overflow,
    /// <c>break-inside:avoid</c>, keep-with-next, widows/orphans, and table-row breaks all turned out to
    /// be same-pass local corrections instead (confirmed empirically stage by stage while investigating
    /// the fragmentation-engine-parity plan's R2-R9), so PeachPDF's inline and table token kinds - and its
    /// per-token <c>FanOutContinuations</c> "parallel flows" mechanism, which only those kinds ever used -
    /// have no counterpart in this port and were never added.
    /// </summary>
    /// <remarks>
    /// Tokens form a chain, one link per ancestor between the fragmentation-context root and the box that
    /// actually stopped: each link names a box and where inside it to resume, and points at the deeper
    /// link for its own child. The driver hands the chain back to the root, which walks it down, so every
    /// ancestor on the path re-enters mid-flight while boxes off the path are untouched. A token records
    /// where to resume, never geometry: the box tree still holds the coordinates.
    /// </remarks>
    /// <param name="Box">the box this link of the chain resumes into</param>
    /// <param name="ResumeSlotIndex">
    /// the pagination slot to resume in. Derived from where the break actually fell, never from "the pass
    /// after this one": a box can be placed far down the document, so the fragmentainer it overflows is
    /// not in general the one after the fragmentainer the pass nominally started in.
    /// </param>
    internal abstract record BreakToken(CssBox Box, int ResumeSlotIndex);

    /// <summary>A block container stopped part-way through its in-flow children.</summary>
    /// <param name="Box">the block container to resume</param>
    /// <param name="ResumeSlotIndex">the pagination slot the resumed pass fills</param>
    /// <param name="ResumeChildIndex">the index into <see cref="CssBox.Boxes"/> to resume the child loop at</param>
    /// <param name="ChildToken">
    /// how to resume that child, or null when the child has not been entered at all (<see cref="IsBreakBefore"/>).
    /// </param>
    /// <param name="IsBreakBefore">
    /// whether the break falls before the child rather than inside it. A break before a box means the box
    /// was never entered, so it has no geometry in the earlier fragmentainer and produces no fragment
    /// there, as opposed to a box that was partially laid out and continues. A break-before child runs its
    /// full prologue on resume; a partially laid-out one must not.
    /// </param>
    /// <param name="ResumeTopOverride">
    /// the document Y to place a break-before child at, when it is not simply the next fragmentainer's
    /// band top. Set by the margin-truncation and keep-with-next paths, which have already computed an
    /// adjusted target and must not have it re-derived.
    /// </param>
    internal sealed record BlockBreakToken(
        CssBox Box,
        int ResumeSlotIndex,
        int ResumeChildIndex,
        BreakToken ChildToken,
        bool IsBreakBefore,
        double? ResumeTopOverride) : BreakToken(Box, ResumeSlotIndex);
}
