using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// <c>@layer</c> rules apply as if they were unlayered, so the passing tests are the ones where
/// specificity and order of appearance give the same answer as layer order.
/// </summary>
[TestClass]
public sealed class CascadeLayerIntegrationTests
{
    [TestMethod]
    [Spec("css-cascade-5", "#layer-block")]
    public void LayerBlock_Rules_AreApplied_NotDropped()
    {
        var html = Html(
            "@layer base { p { color: #0000ff; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#layering")]
    public void LaterLayer_Wins_OverEarlierLayer_AtEqualSpecificity()
    {
        var html = Html(
            "@layer base { p { color: #ff0000; } } @layer components { p { color: #0000ff; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-cascade-5.yml")]
    [Spec("css-cascade-5", "#layering")]
    public void LaterLayer_Wins_EvenAgainstHigherSpecificityInEarlierLayer()
    {
        var html = Html(
            "@layer a { #x { color: #ff0000; } } @layer b { p { color: #0000ff; } }",
            "<p id='x'>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-cascade-5.yml")]
    [Spec("css-cascade-5", "#layer-ordering")]
    public void UnlayeredRule_Wins_OverAnyLayeredRule()
    {
        var html = Html(
            "@layer utilities { p#x { color: #ff0000; } } p { color: #0000ff; }",
            "<p id='x'>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-cascade-5.yml")]
    [Spec("css-cascade-5", "#layer-empty")]
    public void StatementDeclaresOrder_RegardlessOfBlockOrder()
    {
        var html = Html(
            "@layer base, utilities; @layer utilities { p { color: #0000ff; } } @layer base { p { color: #ff0000; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#cascade-sort")]
    public void WithinOneLayer_SpecificityStillBreaksTies()
    {
        var html = Html(
            "@layer base { p { color: #ff0000; } p.hi { color: #0000ff; } }",
            "<p class='hi'>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#layer-names")]
    public void DottedLayerName_IsParsedAndApplied()
    {
        var html = Html(
            "@layer framework.utilities { p { color: #0000ff; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#unnamed-layers")]
    public void AnonymousLayer_Rules_AreApplied_AndBeatenByUnlayered()
    {
        var html = Html(
            "@layer { p { color: #ff0000; } } p { color: #0000ff; }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-cascade-5.yml")]
    [Spec("css-cascade-5", "#layering")]
    public void Important_EarlierLayer_Wins_OverLaterLayer()
    {
        var html = Html(
            "@layer base { p { color: #ff0000 !important; } } @layer utilities { p { color: #0000ff !important; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(255, 0, 0)", box.Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-cascade-5.yml")]
    [Spec("css-cascade-5", "#layering")]
    public void Important_LayeredRule_Wins_OverUnlayeredImportant()
    {
        var html = Html(
            "@layer base { p { color: #0000ff !important; } } p { color: #ff0000 !important; }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#importance")]
    public void Important_WithinOneLayer_HigherSpecificityStillWins()
    {
        var html = Html(
            "@layer base { p { color: #ff0000 !important; } p.hi { color: #0000ff !important; } }",
            "<p class='hi'>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-cascade-5.yml")]
    [Spec("css-cascade-5", "#layer-ordering")]
    public void NestedSublayers_StayContiguous_WithinParentBand()
    {
        // a is ordered by its first appearance (a.b), so a.d still sorts below c
        var html = Html(
            "@layer a.b { p { color: #ff0000; } } @layer c { p { color: #00ff00; } } @layer a.d { p { color: #0000ff; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 255, 0)", box.Color);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#layering")]
    public void NestedSublayer_LaterWithinParent_Wins_OverEarlierSublayer()
    {
        var html = Html(
            "@layer a.b { p { color: #ff0000; } } @layer a.d { p { color: #0000ff; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-cascade-5.yml")]
    [Spec("css-cascade-5", "#revert-layer")]
    public void RevertLayer_RollsBackToLowerLayer_NotToOrigin()
    {
        var html = Html(
            "@layer base { p { color: #0000ff; } } @layer top { p { color: revert-layer; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#default")]
    public void Revert_StillRollsBackToOrigin_NotToLowerLayer()
    {
        var html = Html(
            "@layer base { p { color: #0000ff; } } @layer top { p { color: revert; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreNotEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-cascade-5.yml")]
    [Spec("css-cascade-5", "#revert-layer")]
    public void RevertLayer_OnCustomProperty_RollsBackToLowerLayer()
    {
        var html = Html(
            "@layer base { p { --c: #0000ff; } } @layer top { p { --c: revert-layer; } } p { color: var(--c); }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
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
