using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class CssSpecificityOrderingTests
{
    [TestMethod]
    [Spec("css21", "cascade.html#specificity")]
    public void HigherSpecificityRule_WinsEvenWhenDeclaredEarlier()
    {
        var html = Html("#el { color: #0000ff; } div { color: #ff0000; }", "<div id='el'>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual(RColor.FromArgb(0, 0, 255), box.ActualColor);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#cascading-order")]
    public void EqualSpecificity_SameOrigin_StillResolvesByLastDeclared()
    {
        var html = Html("div { color: #ff0000; } div { color: #0000ff; }", "<div>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual(RColor.FromArgb(0, 0, 255), box.ActualColor);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#cascading-order")]
    public void MediaRuleDeclaredEarlier_LosesToEqualSpecificityPlainRuleDeclaredLater()
    {
        var html = Html(
            "@media print { div { color: #0000ff; } } div { color: #ff0000; }",
            "<div>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual(RColor.FromArgb(255, 0, 0), box.ActualColor);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#specificity")]
    public void CommaListRule_UsesOnlyTheMatchedBranchsSpecificity_NotASum()
    {
        // A selector list is equivalent to one rule per selector, so only ".a" counts here.
        var html = Html(
            ".a, #b { color: #0000ff; } .a.c { color: #ff0000; }",
            "<div class='a c'>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual(RColor.FromArgb(255, 0, 0), box.ActualColor);
    }

    private static string Html(string css, string body) =>
        $"<!DOCTYPE html><html><head><style>{css}</style></head><body>{body}</body></html>";

    private static CssBox FindBoxByTag(string html, string tag)
    {
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.Descendants(root).FirstOrDefault(b => b.HtmlTag != null && b.HtmlTag.Name == tag);
        Assert.IsNotNull(box);
        return box!;
    }
}
