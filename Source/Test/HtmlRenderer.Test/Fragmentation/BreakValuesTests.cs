using TheArtOfDev.HtmlRenderer.Core.Fragmentation;

namespace HtmlRenderer.Test.Fragmentation;

/// <summary>
/// Ported from PeachPDF.Tests/Html/Core/Fragmentation/BreakValuesTests.cs (BreakValuesTests).
/// </summary>
/// <remarks>
/// PeachPDF's <c>BreakValues</c> answers several questions - a full css-break-3 §3.1 forced-break value
/// set (including the four directional values), a <c>RequiredSide</c>/<c>PageSide</c> resolver for them, a
/// two-context (<c>page</c>/<c>column</c>) <c>AvoidsBreak</c>/<c>IsForcedBreak</c>, a <c>SlotIsOn</c> parity
/// helper, and <c>PageRuleResolver.IsRightPage</c>. This port's own <see cref="BreakValues"/> is reduced to
/// exactly what this port's engine (pages only, no multi-column, no directional/<c>@page :left</c>/<c>:right</c>
/// matching - see its own class doc comment) needs: <see cref="BreakValues.IsForcedBreak"/> and
/// <see cref="BreakValues.AvoidsBreak"/>, each single-argument (no <c>FragmentationContext</c> - there is
/// only ever one context, the page) and single-context-valued (only <c>page</c>/<c>always</c> force a break;
/// only <c>avoid</c>/<c>avoid-page</c> forbid one). Every PeachPDF theory row naming a directional
/// (<c>left</c>/<c>right</c>/<c>recto</c>/<c>verso</c>), <c>region</c>/<c>avoid-region</c>, or
/// <c>column</c>/<c>avoid-column</c> value is dropped per the port plan's scope decision, and with them the
/// entire <c>RequiredSide</c>/<c>PageSide</c>/<c>SlotIsOn</c>/<c>PageRuleResolver</c> surface, which has no
/// counterpart here at all.
///
/// One real divergence from PeachPDF found while porting: PeachPDF's classifier rejects the legacy
/// <c>always</c> spelling outright (it only ever reaches a box through the legacy <c>page-break-*</c> alias,
/// which PeachPDF's own CssUtils rewrites to <c>page</c> before the classifier ever sees it). This port's
/// <see cref="BreakValues.IsForcedBreak"/> (Core/Fragmentation/BreakValues.cs) accepts <c>always</c> directly
/// instead, per its own doc comment: "HTML-Renderer's CSS engine accepts directly on the modern properties
/// too... rather than normalizing it away at parse time - so both spellings are classified here." Confirmed
/// independently by <c>PropertyBreakTests.BreakBeforeAfter_AcceptsAlwaysUnlikePeachPdf</c>
/// (Css/PropertyBreakTests.cs), which documents the same divergence at the CSS-parsing layer.
/// </remarks>
[TestClass]
public sealed class BreakValuesTests
{
    [TestMethod]
    [DataRow("page", true)]
    [DataRow("always", true)]
    [DataRow("auto", false)]
    [DataRow("avoid", false)]
    [DataRow("avoid-page", false)]
    [DataRow(null, false)]
    public void IsForcedBreak_MatchesThisPortsReducedValueSet(string value, bool expected) =>
        Assert.AreEqual(expected, BreakValues.IsForcedBreak(value));

    // The divergence from PeachPDF documented in the class remarks: unlike PeachPDF's
    // IsForcedPageBreak_RejectsTheLegacyAlwaysSpelling, this port's classifier accepts "always" directly.
    [TestMethod]
    public void IsForcedBreak_AcceptsTheLegacyAlwaysSpellingUnlikePeachPdf() =>
        Assert.IsTrue(BreakValues.IsForcedBreak("always"));

    [TestMethod]
    [DataRow("avoid", true)]
    [DataRow("avoid-page", true)]
    [DataRow("auto", false)]
    [DataRow("page", false)]
    [DataRow("always", false)]
    [DataRow(null, false)]
    public void AvoidsBreak_MatchesThisPortsReducedValueSet(string value, bool expected) =>
        Assert.AreEqual(expected, BreakValues.AvoidsBreak(value));
}
