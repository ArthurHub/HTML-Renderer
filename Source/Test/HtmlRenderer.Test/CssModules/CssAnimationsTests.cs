using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

/// <summary>
/// The static frame is the document at time zero of its timeline, the first frame a browser would paint.
/// </summary>
[TestClass]
public sealed class CssAnimationsTests
{
    private static readonly RColor Green = RColor.FromArgb(0, 128, 0);

    // ── 2 Animations ─────────────────────────────────────────────────────

    [Ignore("Gap, see Spec/css-animations-1.yml")]
    [TestMethod]
    [Spec("css-animations-1", "#animations")]
    [Spec("web-animations-1", "#the-effect-value-of-a-keyframe-animation-effect")]
    [Spec("web-animations-1", "#applying-the-composited-result")]
    public void RunningAnimation_ShowsItsFirstKeyframeAtTimeZero()
    {
        var x = Box("<style>@keyframes k { from, to { color: green } } p { color: red; animation: k 1s }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css-animations-1", "#animations")]
    public void AnimationWithoutKeyframes_ChangesNothing()
    {
        var x = Box("<style>p { color: green; animation: missing 1s both }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    // ── 3 Keyframes ──────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-animations-1", "#keyframes")]
    public void KeyframesRule_IsParsedWithItsKeyframes()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet(
            "@keyframes slide { from { margin-left: 0 } 50%, 75% { margin-left: 10px } to { margin-left: 20px } }");

        var rule = (KeyframesRule)sheet.Rules.Single();
        Assert.AreEqual("slide", rule.Name);
        CollectionAssert.AreEqual(
            new[] { "0%", "50%, 75%", "100%" },
            rule.Rules.OfType<KeyframeRule>().Select(r => r.KeyText).ToArray());
    }

    [TestMethod]
    [Spec("css-animations-1", "#keyframes")]
    public void KeyframesRule_DoesNotAffectOtherRules()
    {
        Assert.AreEqual(Green, Box("<style>@keyframes k { from { color: red } } p { color: green }</style><p id=x>a</p>").ActualColor);
        Assert.AreEqual(Green, Box("<style>p { color: green } @keyframes p { from { color: red } to { color: red } }</style><p id=x>a</p>").ActualColor);
    }

    [Ignore("Gap, see Spec/css-animations-1.yml")]
    [TestMethod]
    [Spec("css-animations-1", "#keyframes")]
    public void KeyframesRule_WithAStringName_IsParsed()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@keyframes \"initial\" { from { color: red } }");

        Assert.AreEqual("initial", ((KeyframesRule)sheet.Rules.Single()).Name);
    }

    [Ignore("Gap, see Spec/css-animations-1.yml")]
    [TestMethod]
    [Spec("css-animations-1", "#keyframes")]
    [DataRow("none")]
    [DataRow("initial")]
    public void KeyframesRule_WithAReservedName_IsInvalid(string name)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@keyframes " + name + " { from { color: red } }");

        Assert.AreEqual(0, sheet.Rules.Length);
    }

    [Ignore("Gap, see Spec/css-animations-1.yml")]
    [TestMethod]
    [Spec("css-animations-1", "#keyframes")]
    [DataRow("110%")]
    [DataRow("-10%")]
    public void KeyframeSelector_OutsideTheRange_DropsTheBlock(string selector)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@keyframes k { " + selector + " { color: red } to { color: blue } }");

        var rule = (KeyframesRule)sheet.Rules.Single();
        CollectionAssert.AreEqual(new[] { "100%" }, rule.Rules.OfType<KeyframeRule>().Select(r => r.KeyText).ToArray());
    }

    [TestMethod]
    [Spec("css-animations-1", "#keyframes")]
    public void KeyframeSelector_WithoutAUnit_DropsTheBlock()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@keyframes k { 0 { color: red } to { color: blue } }");

        var rule = (KeyframesRule)sheet.Rules.Single();
        CollectionAssert.AreEqual(new[] { "100%" }, rule.Rules.OfType<KeyframeRule>().Select(r => r.KeyText).ToArray());
    }

    [Ignore("Gap, see Spec/css-animations-1.yml")]
    [TestMethod]
    [Spec("css-animations-1", "#keyframes")]
    public void ImportantDeclarationInAKeyframe_IsIgnored()
    {
        var rule = CssConstructionFunctions.ParseKeyframeRule("from { color: red !important; margin-left: 1px }");

        CollectionAssert.AreEqual(new[] { "margin-left" }, rule.Style.Declarations.Select(d => d.Name).ToArray());
    }

    [TestMethod]
    [Spec("css-animations-1", "#timing-functions")]
    public void KeyframeBlock_AcceptsATimingFunction()
    {
        var rule = CssConstructionFunctions.ParseKeyframeRule("from { top: 100px; animation-timing-function: ease-out }");

        Assert.AreEqual("ease-out", rule.Style.GetPropertyValue("animation-timing-function"));
    }

    // ── 3.2 - 3.9 Animation properties ──────────────────────────────────

    [Ignore("Gap, see Spec/css-animations-1.yml")]
    [TestMethod]
    [Spec("css-animations-1", "#animation-name")]
    public void AnimationName_AcceptsAString()
    {
        var style = CssConstructionFunctions.ParseDeclarations("animation-name: \"initial\"");

        Assert.AreNotEqual(string.Empty, style.GetPropertyValue("animation-name"));
    }

    [Ignore("Gap, see Spec/css-animations-1.yml")]
    [TestMethod]
    [Spec("css-animations-1", "#animation-name")]
    public void AnimationName_WithACssWideKeywordInAList_IsInvalid()
    {
        var style = CssConstructionFunctions.ParseDeclarations("animation-name: foo, initial");

        Assert.AreEqual(string.Empty, style.GetPropertyValue("animation-name"));
    }

    [TestMethod]
    [Spec("css-animations-1", "#animation-name")]
    public void AnimationName_AcceptsNoneInAList()
    {
        var style = CssConstructionFunctions.ParseDeclarations("animation-name: none, foo");

        Assert.AreEqual("none, foo", style.GetPropertyValue("animation-name"));
    }

    [Ignore("Gap, see Spec/css-animations-1.yml")]
    [TestMethod]
    [Spec("css-animations-1", "#animation-duration")]
    [Spec("css-animations-1", "#animation-fill-mode")]
    [Spec("web-animations-1", "#calculating-the-overall-progress")]
    public void ZeroDuration_WithForwardsFill_ShowsTheLastKeyframe()
    {
        var x = Box("<style>@keyframes k { to { color: green } } p { color: red; animation: k 0s forwards }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [Ignore("Gap, see Spec/css-animations-1.yml")]
    [TestMethod]
    [Spec("css-animations-1", "#animation-duration")]
    public void AnimationDuration_Negative_IsInvalid()
    {
        var style = CssConstructionFunctions.ParseDeclarations("animation-duration: -1s");

        Assert.AreEqual(string.Empty, style.GetPropertyValue("animation-duration"));
    }

    [Ignore("Gap, see Spec/css-animations-1.yml")]
    [TestMethod]
    [Spec("css-animations-1", "#animation-timing-function")]
    public void StepStartTimingFunction_ShowsTheEndOfTheFirstStepAtTimeZero()
    {
        var x = Box("<style>@keyframes k { from { color: red } to { color: green } } p { color: red; animation: k 1s step-start }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [Ignore("Gap, see Spec/css-animations-1.yml")]
    [TestMethod]
    [Spec("css-animations-1", "#animation-direction")]
    [Spec("web-animations-1", "#calculating-the-directed-progress")]
    public void ReverseDirection_ShowsTheLastKeyframeAtTimeZero()
    {
        var x = Box("<style>@keyframes k { from { color: red } to { color: green } } p { color: red; animation: k 1s reverse }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [Ignore("Gap, see Spec/css-animations-1.yml")]
    [TestMethod]
    [Spec("css-animations-1", "#animation-delay")]
    [Spec("web-animations-1", "#calculating-the-active-time")]
    public void NegativeDelay_StartsPartwayThroughTheAnimation()
    {
        var x = Box("<style>@keyframes k { from { margin-left: 0 } to { margin-left: 100px } } p { animation: k 2s linear -1s }</style><p id=x>a</p>");

        Assert.AreEqual(50, x.ActualMarginLeft, 0.5);
    }

    [Ignore("Gap, see Spec/css-animations-1.yml")]
    [TestMethod]
    [Spec("css-animations-1", "#animation-fill-mode")]
    [Spec("css-animations-1", "#animation-delay")]
    [Spec("web-animations-1", "#fill-modes")]
    public void BackwardsFill_AppliesTheFirstKeyframeDuringTheDelay()
    {
        var x = Box("<style>@keyframes k { from { color: green } to { color: red } } p { color: red; animation: k 1s 1s backwards }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    [TestMethod]
    [Spec("css-animations-1", "#animation-fill-mode")]
    [Spec("css-animations-1", "#animation-delay")]
    public void NoFill_LeavesTheElementAloneDuringTheDelay()
    {
        var x = Box("<style>@keyframes k { from, to { color: red } } p { color: green; animation: k 1s 1s }</style><p id=x>a</p>");

        Assert.AreEqual(Green, x.ActualColor);
    }

    // ── 3.10 animation ───────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-animations-1", "#animation")]
    public void Shorthand_SetsEveryLonghand()
    {
        var style = CssConstructionFunctions.ParseDeclarations("animation: slide 1s ease-in 2s infinite alternate both paused");

        Assert.AreEqual("slide", style.GetPropertyValue("animation-name"));
        Assert.AreEqual("1s", style.GetPropertyValue("animation-duration"));
        Assert.AreEqual("ease-in", style.GetPropertyValue("animation-timing-function"));
        Assert.AreEqual("2s", style.GetPropertyValue("animation-delay"));
        Assert.AreEqual("infinite", style.GetPropertyValue("animation-iteration-count"));
        Assert.AreEqual("alternate", style.GetPropertyValue("animation-direction"));
        Assert.AreEqual("both", style.GetPropertyValue("animation-fill-mode"));
        Assert.AreEqual("paused", style.GetPropertyValue("animation-play-state"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-animations-1.yml")]
    [Spec("css-animations-1", "#animation")]
    public void Shorthand_ListItemWithoutADelay_UsesTheInitialDelay()
    {
        var style = CssConstructionFunctions.ParseDeclarations("animation: a 1s 2s, b 3s");

        Assert.AreEqual("2s, 0s", style.GetPropertyValue("animation-delay"));
    }

    [TestMethod]
    [Spec("css-animations-1", "#animation")]
    public void Shorthand_WithANameAndADuration_SetsOnlyThose()
    {
        var style = CssConstructionFunctions.ParseDeclarations("animation: slide 1s");

        Assert.AreEqual("slide", style.GetPropertyValue("animation-name"));
        Assert.AreEqual("1s", style.GetPropertyValue("animation-duration"));
        Assert.AreNotEqual("1s", style.GetPropertyValue("animation-delay"));
    }

    [TestMethod]
    [Spec("css-animations-1", "#animation")]
    public void Shorthand_GivesKeywordsToTheOtherPropertiesFirst()
    {
        var style = CssConstructionFunctions.ParseDeclarations("animation: 3s none backwards");

        Assert.AreEqual("none", style.GetPropertyValue("animation-fill-mode"));
        Assert.AreEqual("backwards", style.GetPropertyValue("animation-name"));
    }

    private static CssBox Box(string body, string id = "x")
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(body));
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box, $"No box with id '{id}'");
        return box;
    }
}
