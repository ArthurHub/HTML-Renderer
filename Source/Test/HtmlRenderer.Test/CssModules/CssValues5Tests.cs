using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssValues5Tests
{
    private const string Green = "rgb(0, 128, 0)";
    private const double Delta = 0.01;

    // ── 3 Value Definition Syntax ────────────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#component-function-commas")]
    public void CommaContainingArgument_CanBeWrappedInBraces()
    {
        Assert.AreEqual("Times, serif", Styled("#x{font-family:var(--missing, {Times, serif})}").FontFamily);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#css-syntax")]
    public void RegisteredCustomProperty_ThatDoesNotMatchItsSyntax_UsesItsInitialValue()
    {
        Assert.AreEqual(7, Styled("@property --w { syntax: '<length>'; inherits: false; initial-value: 7px } #x{--w:red;margin-left:var(--w)}").ActualMarginLeft, Delta);
    }

    // ── 4 Extensions to Level 4 Value Types ──────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#request-url-modifiers")]
    public void RequestUrlModifier_DoesNotChangeTheUrl()
    {
        CollectionAssert.AreEqual(new[] { "a.png" }, RequestedImages("#x{background-image:url(\"a.png\" crossorigin(anonymous))}"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#position")]
    [DataRow("x-end y-end")]
    [DataRow("inline-end block-end")]
    [DataRow("end end")]
    public void LogicalPositionKeywords_AreAccepted(string value)
    {
        Assert.AreNotEqual("left top", Styled($"#x{{background-position:left top;background-position:{value}}}").BackgroundPosition);
    }

    // ── 5 Progress, 6 Mixing ─────────────────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#progress-func")]
    public void Progress_IsTheFractionBetweenStartAndEnd()
    {
        Assert.AreEqual(25, Styled("#x{margin-left:calc(100px * progress(25px, 0px, 100px))}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#media-progress-func")]
    public void MediaProgress_IsTheFractionOfTheMediaFeature()
    {
        Assert.AreEqual(50, Styled("#x{margin-left:calc(100px * media-progress(width, 0px, 1600px))}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#calc-mix")]
    public void CalcMix_InterpolatesBetweenTheValues()
    {
        Assert.AreEqual(25, Styled("#x{margin-left:calc-mix(25%, 0px, 100px)}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css-values-5", "#cross-fade")]
    public void CrossFade_IsAcceptedAsAnImage()
    {
        Assert.AreNotEqual("none", Styled("#x{background-image:cross-fade(url(a.png) 50%, url(b.png))}").BackgroundImage);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#mix")]
    public void Mix_InterpolatesBetweenTheValues()
    {
        Assert.AreEqual(25, Styled("#x{margin-left:mix(25%, 0px, 100px)}").ActualMarginLeft, Delta);
    }

    // ── 7 Value Substituting Functions ───────────────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#first-valid")]
    public void FirstValid_UsesTheFirstValidArgument()
    {
        Assert.AreEqual(Green, Styled("#x{color:red;color:first-valid(foo, green)}").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#if-notation")]
    public void If_UsesTheFirstTrueBranch()
    {
        Assert.AreEqual(Green, Styled("#x{color:red;color:if(media(print): red; else: green)}").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#toggle-notation")]
    public void Toggle_CyclesAgainstTheInheritedValue()
    {
        Assert.AreEqual("normal", Styled("#x{font-style:toggle(italic, normal)}", "<div style='font-style:italic'><div id=x>a</div></div>").FontStyle);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#inherit-notation")]
    public void Inherit_TakesTheParentsCustomProperty()
    {
        Assert.AreEqual(5, Styled("#x{--a:9px;margin-left:inherit(--a)}", "<div style='--a:5px'><div id=x>a</div></div>").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css-values-5", "#attr-notation")]
    [Spec("css-values-5", "#attr-substitution")]
    public void Attr_WithoutASyntax_IsTheLiteralAttributeValue()
    {
        Assert.AreEqual("a\\b", Before("#x::before{content:attr(data-v)}", "<div id=x data-v='a\\b'>a</div>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#attr-notation")]
    public void Attr_UsesTheFallback_WhenTheAttributeIsMissing()
    {
        Assert.AreEqual("fb", Before("#x::before{content:attr(data-missing, \"fb\")}"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#attr-notation")]
    public void Attr_WithASyntax_ParsesTheAttribute()
    {
        Assert.AreEqual("[q]", Before("#x::before{content:'[' attr(data-v *) ']'}", "<div id=x data-v='\"q\"'>a</div>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#attr-substitution")]
    public void Attr_WorksInAnyProperty()
    {
        Assert.AreEqual(12, Styled("#x{margin-left:3px;margin-left:attr(data-m <length>)}", "<div id=x data-m='12px'>a</div>").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css-values-5", "#attr-security")]
    public void Attr_IsNotUsedAsAUrl()
    {
        var requested = RequestedImages("#x{background-image:image(attr(data-u))}", "<div id=x data-u='evil.png'>a</div>");

        Assert.IsFalse(requested.Any(r => r.Contains("evil")));
    }

    // ── 8 Random, 9 Tree Counting, 10 calc-size() ────────────────────────

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#random")]
    public void Random_IsBetweenTheLimits()
    {
        Assert.AreEqual(10, Styled("#x{margin-left:random(10px, 10px)}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#random-item")]
    public void RandomItem_PicksOneOfTheItems()
    {
        Assert.AreEqual(Green, Styled("#x{color:red;color:random-item(--x, green, green)}").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#tree-counting")]
    public void SiblingIndex_IsTheElementsPosition()
    {
        Assert.AreEqual(30, Styled("#x{margin-left:calc(sibling-index() * 10px)}", "<div>a</div><div>b</div><div id=x>c</div>").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#tree-counting")]
    public void SiblingCount_IsTheNumberOfSiblings()
    {
        Assert.AreEqual(30, Styled("#x{margin-left:calc(sibling-count() * 10px)}", "<div>a</div><div>b</div><div id=x>c</div>").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-values-5.yml")]
    [Spec("css-values-5", "#calc-size")]
    public void CalcSize_CalculatesWithTheIntrinsicSize()
    {
        Assert.AreEqual(400, Styled("#x{width:50px;width:calc-size(auto, size * 0.5)}").ActualWidth, Delta);
    }

    private static List<string> RequestedImages(string css, string body = "<div id=x>a</div>")
    {
        var requested = new List<string>();
        var container = new HtmlContainerInt(new MockAdapter()) { MaxSize = new RSize(800, 600) };
        container.ImageLoad += (_, e) => requested.Add(e.Src);
        container.SetHtml($"<html><head><style>#x{{width:10px;height:10px}} {css}</style></head><body>{body}</body></html>");
        using var graphics = new RecordingGraphics();
        container.PerformLayout(graphics);
        container.PerformPaint(graphics);
        return requested;
    }

    private static CssBox Styled(string css, string body = "<div id=x>a</div>")
    {
        var (root, _) = LayoutHarness.Layout($"<html><head><style>{css}</style></head><body style='margin:0'>{body}</body></html>", 800, 600);
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box;
    }

    private static string Before(string css, string body = "<div id=x>a</div>")
    {
        var before = Styled(css, body).Boxes.FirstOrDefault(b => b.IsBeforePseudoElement);
        Assert.IsNotNull(before, "No ::before box");
        return before.Text ?? string.Concat(LayoutHarness.Descendants(before).SelectMany(b => b.Words).Select(w => w.Text));
    }
}
