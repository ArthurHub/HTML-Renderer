using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class MediaQueryTests
{
    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);

    private static CssBox LayoutAndFindTag(string css, string bodyHtml, string tag, string mediaType)
    {
        var adapter = new MockAdapter { MediaType = mediaType };
        var html = $"<!DOCTYPE html><html><head><style>{css}</style></head><body>{bodyHtml}</body></html>";
        var (root, _) = LayoutHarness.Layout(html, adapter: adapter);
        var box = LayoutHarness.Descendants(root).FirstOrDefault(b => b.HtmlTag != null && b.HtmlTag.Name == tag);
        Assert.IsNotNull(box);
        return box!;
    }

    // ── basic single media type + nested rules ──────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css21", "media.html#at-media-rule")]
    [Spec("css-conditional-3", "#at-media")]
    public void AtMedia_Print_NestedRuleAppliesUnderPrintMedia()
    {
        var box = LayoutAndFindTag("@media print { p { color: red; } }", "<p>text</p>", "p", "print");
        Assert.AreEqual(Red, box.ActualColor);
    }

    [TestMethod]
    [Spec("css21", "media.html#at-media-rule")]
    [Spec("css-conditional-3", "#processing")]
    public void AtMedia_Print_DoesNotApplyUnderScreenMedia()
    {
        // the rule is scoped to the "print" media only - it must not leak into "screen".
        var box = LayoutAndFindTag("@media print { p { color: red; } }", "<p>text</p>", "p", "screen");
        Assert.AreNotEqual(Red, box.ActualColor);
    }

    [TestMethod]
    [Spec("css21", "media.html#at-media-rule")]
    public void AtMedia_Screen_NestedRuleAppliesUnderScreenMedia()
    {
        var box = LayoutAndFindTag("@media screen { p { color: red; } }", "<p>text</p>", "p", "screen");
        Assert.AreEqual(Red, box.ActualColor);
    }

    [TestMethod]
    [Spec("css21", "media.html#at-media-rule")]
    public void AtMedia_All_NestedRuleAppliesUnderAllMedia()
    {
        var box = LayoutAndFindTag("@media all { p { color: red; } }", "<p>text</p>", "p", "screen");
        Assert.AreEqual(Red, box.ActualColor);
    }

    [TestMethod]
    [Spec("css21", "media.html#at-media-rule")]
    [Spec("css-conditional-3", "#at-media")]
    public void AtMedia_Print_MultipleNestedRulesAreParsed()
    {
        const string css = "@media print { p { color: red; } div { font-size: 12pt; } }";
        const string body = "<p>text</p><div>text</div>";

        var pBox = LayoutAndFindTag(css, body, "p", "print");
        Assert.AreEqual(Red, pBox.ActualColor);

        var divBox = LayoutAndFindTag(css, body, "div", "print");
        Assert.AreEqual("12pt", divBox.FontSize);
    }

    // ── "not" / "only" modifiers ─────────────────────────────────────────────────────────────────────────

    [TestMethod]
    public void AtMedia_NotPrint_ExcludesPrintButAppliesElsewhere()
    {
        const string css = "@media not print { p { color: red; } }";

        var underPrint = LayoutAndFindTag(css, "<p>text</p>", "p", "print");
        Assert.AreNotEqual(Red, underPrint.ActualColor);

        var underScreen = LayoutAndFindTag(css, "<p>text</p>", "p", "screen");
        Assert.AreEqual(Red, underScreen.ActualColor);
    }

    [TestMethod]
    public void AtMedia_OnlyPrint_AppliesOnlyUnderPrint()
    {
        const string css = "@media only print { p { color: red; } }";

        var underPrint = LayoutAndFindTag(css, "<p>text</p>", "p", "print");
        Assert.AreEqual(Red, underPrint.ActualColor);

        var underScreen = LayoutAndFindTag(css, "<p>text</p>", "p", "screen");
        Assert.AreNotEqual(Red, underScreen.ActualColor);
    }

    // ── comma-separated media list ─────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css21", "media.html#at-media-rule")]
    public void AtMedia_PrintCommaScreen_AppliesUnderBoth()
    {
        const string css = "@media print, screen { p { color: red; } }";

        var underPrint = LayoutAndFindTag(css, "<p>text</p>", "p", "print");
        Assert.AreEqual(Red, underPrint.ActualColor);

        var underScreen = LayoutAndFindTag(css, "<p>text</p>", "p", "screen");
        Assert.AreEqual(Red, underScreen.ActualColor);
    }
}
