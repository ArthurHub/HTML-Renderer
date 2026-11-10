using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssTransitionsTests
{
    private static readonly RColor Green = RColor.FromArgb(0, 128, 0);

    // ── 2 Transitions ────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-transitions-1", "#transitions")]
    public void Transition_DoesNotChangeTheFirstRendering()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>p { color: green; margin-left: 10px; transition: all 1s }</style><p id=x>a</p>"));
        var x = LayoutHarness.FindById(root, "x")!;

        Assert.AreEqual(Green, x.ActualColor);
        Assert.AreEqual(10, x.ActualMarginLeft);
    }

    // ── 2.1 transition-property ──────────────────────────────────────────

    [Ignore("Gap, see Spec/css-transitions-1.yml")]
    [TestMethod]
    [Spec("css-transitions-1", "#transition-property-property")]
    [DataRow("foo-bar")]
    [DataRow("--my-var")]
    [DataRow("color, foo-bar")]
    public void TransitionProperty_KeepsUnknownNames(string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations("transition-property: " + value);

        Assert.AreEqual(value, style.GetPropertyValue("transition-property"));
    }

    [TestMethod]
    [Spec("css-transitions-1", "#transition-property-property")]
    [DataRow("none, color")]
    [DataRow("color, initial")]
    public void TransitionProperty_WithAReservedKeywordInAList_IsInvalid(string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations("transition-property: " + value);

        Assert.AreEqual(string.Empty, style.GetPropertyValue("transition-property"));
    }

    // ── 2.2 transition-duration ──────────────────────────────────────────

    [Ignore("Gap, see Spec/css-transitions-1.yml")]
    [TestMethod]
    [Spec("css-transitions-1", "#transition-duration-property")]
    public void TransitionDuration_Negative_IsInvalid()
    {
        var style = CssConstructionFunctions.ParseDeclarations("transition-duration: -1s");

        Assert.AreEqual(string.Empty, style.GetPropertyValue("transition-duration"));
    }

    // ── 2.4 transition-delay ─────────────────────────────────────────────

    [TestMethod]
    [Spec("css-transitions-1", "#transition-delay-property")]
    public void TransitionDelay_Negative_IsValid()
    {
        var style = CssConstructionFunctions.ParseDeclarations("transition-delay: -1s, 2s");

        Assert.AreEqual("-1s, 2s", style.GetPropertyValue("transition-delay"));
    }

    // ── 2.5 transition ───────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-transitions-1", "#transition-shorthand-property")]
    public void Shorthand_WithAPropertyAndADuration_SetsOnlyThose()
    {
        var style = CssConstructionFunctions.ParseDeclarations("transition: color 1s");

        Assert.AreEqual("color", style.GetPropertyValue("transition-property"));
        Assert.AreEqual("1s", style.GetPropertyValue("transition-duration"));
        Assert.AreNotEqual("1s", style.GetPropertyValue("transition-delay"));
    }

    [TestMethod]
    [Spec("css-transitions-1", "#transition-shorthand-property")]
    public void Shorthand_AssignsTheSecondTimeToTheDelay()
    {
        var style = CssConstructionFunctions.ParseDeclarations("transition: 1s 2s");

        Assert.AreEqual("1s", style.GetPropertyValue("transition-duration"));
        Assert.AreEqual("2s", style.GetPropertyValue("transition-delay"));
    }

    [TestMethod]
    [Spec("css-transitions-1", "#transition-shorthand-property")]
    public void Shorthand_None_IsValid()
    {
        var style = CssConstructionFunctions.ParseDeclarations("transition: none");

        Assert.AreEqual("none", style.GetPropertyValue("transition-property"));
    }

    [TestMethod]
    [Spec("css-transitions-1", "#transition-shorthand-property")]
    public void Shorthand_WithNoneInAList_IsInvalid()
    {
        var style = CssConstructionFunctions.ParseDeclarations("transition: none, color 1s");

        Assert.AreEqual(string.Empty, style.GetPropertyValue("transition"));
        Assert.AreEqual(string.Empty, style.GetPropertyValue("transition-property"));
    }
}
