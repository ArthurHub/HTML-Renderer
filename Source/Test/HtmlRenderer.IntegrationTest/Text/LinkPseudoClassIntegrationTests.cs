using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Text;

/// <summary>
/// :link matches the hyperlinks, :visited and :active never match.
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class LinkPseudoClassIntegrationTests
{
    [TestMethod]
    [Spec("html", "links.html#links-created-by-a-and-area-elements")]
    [Spec("html", "semantics-other.html#pseudo-classes")]
    public void Link_MatchesAnchorWithHref()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>a:link { color: rgb(10,20,30) }</style><a href='https://example.com'>link</a>"));
        var a = FindByTag(root, "a")!;

        Assert.AreEqual("rgb(10, 20, 30)", a.Color);
    }

    [TestMethod]
    [Spec("html", "links.html#links-created-by-a-and-area-elements")]
    [Spec("html", "semantics-other.html#pseudo-classes")]
    public void Link_DoesNotMatchAnchorWithoutHref()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>a:link { color: rgb(10,20,30) }</style><a>not a link</a>"));
        var a = FindByTag(root, "a")!;

        Assert.AreNotEqual("rgb(10, 20, 30)", a.Color);
    }

    [TestMethod]
    [Spec("html", "semantics-other.html#pseudo-classes")]
    public void Link_DoesNotMatchNonAnchorElement()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>:link { color: rgb(10,20,30) }</style><span id='s' href='https://example.com'>not an anchor</span>"));
        var s = LayoutHarness.FindById(root, "s")!;

        Assert.AreNotEqual("rgb(10, 20, 30)", s.Color);
    }

    [TestMethod]
    [Spec("html", "semantics-other.html#pseudo-classes")]
    public void Visited_NeverMatches_ByDesign()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>a:visited { color: rgb(10,20,30) }</style><a href='https://example.com'>link</a>"));
        var a = FindByTag(root, "a")!;

        Assert.AreNotEqual("rgb(10, 20, 30)", a.Color);
    }

    [TestMethod]
    [Spec("html", "semantics-other.html#pseudo-classes")]
    public void Active_NeverMatches_ByDesign()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>a:active { color: rgb(10,20,30) }</style><a href='https://example.com'>link</a>"));
        var a = FindByTag(root, "a")!;

        Assert.AreNotEqual("rgb(10, 20, 30)", a.Color);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static CssBox? FindByTag(CssBox box, string tag)
    {
        if (box.HtmlTag?.Name.Equals(tag, System.StringComparison.OrdinalIgnoreCase) == true)
            return box;
        foreach (var child in box.Boxes)
        {
            var found = FindByTag(child, tag);
            if (found != null) return found;
        }
        return null;
    }
}
