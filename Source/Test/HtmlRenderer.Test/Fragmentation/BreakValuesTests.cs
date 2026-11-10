using TheArtOfDev.HtmlRenderer.Core.Fragmentation;

namespace HtmlRenderer.Test.Fragmentation;

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
    [Spec("css21", "page.html#page-break-props")]
    [Spec("css-break-3", "#break-between")]
    [Spec("css-break-4", "#break-between")]
    public void IsForcedBreak_MatchesThisPortsReducedValueSet(string value, bool expected) =>
        Assert.AreEqual(expected, BreakValues.IsForcedBreak(value));

    [TestMethod]
    [Spec("css21", "page.html#page-break-props")]
    [Spec("css-break-3", "#break-between")]
    [Spec("css-break-4", "#break-between")]
    [DataRow("avoid", true)]
    [DataRow("avoid-page", true)]
    [DataRow("auto", false)]
    [DataRow("page", false)]
    [DataRow("always", false)]
    [DataRow(null, false)]
    public void AvoidsBreak_MatchesThisPortsReducedValueSet(string value, bool expected) =>
        Assert.AreEqual(expected, BreakValues.AvoidsBreak(value));
}
