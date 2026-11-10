using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssEasingTests
{
    // ── 2 Easing functions ───────────────────────────────────────────────

    [TestMethod]
    [Spec("css-easing-1", "#the-linear-easing-function")]
    [Spec("css-easing-1", "#cubic-bezier-easing-functions")]
    [Spec("css-easing-1", "#step-easing-functions")]
    [DataRow("linear")]
    [DataRow("ease")]
    [DataRow("ease-in")]
    [DataRow("ease-out")]
    [DataRow("ease-in-out")]
    [DataRow("step-start")]
    [DataRow("step-end")]
    public void Keyword_IsAccepted(string keyword)
    {
        Assert.AreEqual(keyword, Easing(keyword));
    }

    // ── 2.2 Cubic Bézier ─────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-easing-1", "#cubic-bezier-easing-functions")]
    public void CubicBezier_AcceptsYValuesOutsideTheUnitRange()
    {
        Assert.AreEqual("cubic-bezier(0.1, -2, 0.3, 3)", Easing("cubic-bezier(0.1, -2, 0.3, 3)"));
    }

    [TestMethod]
    [Spec("css-easing-1", "#cubic-bezier-easing-functions")]
    public void CubicBezier_WithThreeNumbers_IsInvalid()
    {
        Assert.AreEqual(string.Empty, Easing("cubic-bezier(0.1, 0.2, 0.3)"));
    }

    [Ignore("Gap, see Spec/css-easing-1.yml")]
    [TestMethod]
    [Spec("css-easing-1", "#cubic-bezier-easing-functions")]
    [DataRow("cubic-bezier(2, 0, 0, 1)")]
    [DataRow("cubic-bezier(0, 0, -0.1, 1)")]
    public void CubicBezier_WithXOutsideTheUnitRange_IsInvalid(string value)
    {
        Assert.AreEqual(string.Empty, Easing(value));
    }

    [Ignore("Gap, see Spec/css-easing-1.yml")]
    [TestMethod]
    [Spec("css-easing-1", "#cubic-bezier-algo")]
    public void CubicBezier_EasesTheProgress()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>@keyframes k { from { margin-left: 0 } to { margin-left: 100px } } p { animation: k 2s ease-in -1s }</style><p id=x>a</p>"));

        Assert.AreEqual(31.5, LayoutHarness.FindById(root, "x")!.ActualMarginLeft, 0.5);
    }

    // ── 2.3 Steps ────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-easing-1", "#step-easing-functions")]
    [DataRow("steps(3)")]
    [DataRow("steps(3, start)")]
    [DataRow("steps(3, end)")]
    public void Steps_IsAccepted(string value)
    {
        Assert.AreNotEqual(string.Empty, Easing(value));
    }

    [Ignore("Gap, see Spec/css-easing-1.yml")]
    [TestMethod]
    [Spec("css-easing-1", "#step-easing-functions")]
    [DataRow("steps(3, jump-start)")]
    [DataRow("steps(3, jump-end)")]
    [DataRow("steps(3, jump-none)")]
    [DataRow("steps(3, jump-both)")]
    public void Steps_AcceptsTheJumpPositions(string value)
    {
        Assert.AreEqual(value, Easing(value));
    }

    [Ignore("Gap, see Spec/css-easing-1.yml")]
    [TestMethod]
    [Spec("css-easing-1", "#step-easing-functions")]
    [DataRow("steps(0)")]
    [DataRow("steps(-1, start)")]
    public void Steps_WithoutAPositiveCount_IsInvalid(string value)
    {
        Assert.AreEqual(string.Empty, Easing(value));
    }

    [Ignore("Gap, see Spec/css-easing-1.yml")]
    [TestMethod]
    [Spec("css-easing-1", "#step-easing-algo")]
    public void Steps_JumpTheProgress()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>@keyframes k { from { margin-left: 0 } to { margin-left: 100px } } p { animation: k 2s steps(4) -1.2s }</style><p id=x>a</p>"));

        Assert.AreEqual(50, LayoutHarness.FindById(root, "x")!.ActualMarginLeft, 0.5);
    }

    private static string Easing(string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations("transition-timing-function: " + value);
        return style.GetPropertyValue("transition-timing-function");
    }
}
