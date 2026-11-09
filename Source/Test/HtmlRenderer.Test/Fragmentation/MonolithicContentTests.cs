using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragmentation;

namespace HtmlRenderer.Test.Fragmentation;

/// <summary>
/// The monolithic content classifier against css-break-3 §2.
/// </summary>
[TestClass]
public sealed class MonolithicContentTests
{
    // ── replaced elements ─────────────────────────────────────────────────

    [TestMethod]
    [DataRow("<img id='t' src='x.png' style='width:10pt;height:10pt'>")]
    [DataRow("<iframe id='t' style='width:10pt;height:10pt'></iframe>")]
    public void ReplacedElement_IsMonolithic(string markup)
    {
        var box = BoxOf(markup);

        Assert.IsTrue(MonolithicContent.IsReplaced(box));
        Assert.IsTrue(MonolithicContent.IsMonolithic(box));
    }

    [TestMethod]
    public void OrdinaryBlock_IsNotMonolithic()
    {
        var box = BoxOf("<div id='t'>text</div>");

        Assert.IsFalse(MonolithicContent.IsReplaced(box));
        Assert.IsFalse(MonolithicContent.IsMonolithic(box));
    }

    // ── scroll containers ─────────────────────────────────────────────────

    [TestMethod]
    [DataRow("hidden", true)]
    [DataRow("scroll", true)]
    [DataRow("auto", true)]
    [DataRow("visible", false)]
    // 'clip' isn't a known overflow value, the box stays 'visible', which is also what §2 wants.
    [DataRow("clip", false)]
    public void Overflow_DecidesScrollContainer(string overflow, bool expected)
    {
        var box = BoxOf($"<div id='t' style='overflow:{overflow}'>text</div>");

        Assert.AreEqual(expected, MonolithicContent.IsScrollContainer(box));
        Assert.AreEqual(expected, MonolithicContent.IsMonolithic(box));
    }

    // CSS Overflow 3 §3.3: the root's overflow propagates to the viewport, and <body>'s does when the
    // root's is visible, so neither is itself a scroll container. Without this the near-universal
    // `html { overflow: hidden }` idiom would declare an entire document unbreakable.
    [TestMethod]
    [DataRow("html")]
    [DataRow("body")]
    public void ViewportPropagationSource_IsNotAScrollContainer(string tag)
    {
        var box = BoxOfTag($"{tag} {{ overflow: hidden }}", tag);

        Assert.AreEqual("hidden", box.Overflow);
        Assert.IsFalse(MonolithicContent.IsScrollContainer(box));
        Assert.IsFalse(MonolithicContent.IsMonolithic(box));
    }

    // The other half of §3.3, which the theory above cannot see because it never sets both: the body's
    // value propagates only while the root's own is `visible`. Once the root has declared one it took
    // the propagation, and the body is a scroll container in its own right.
    [TestMethod]
    public void Body_UnderARootThatAlreadyDeclaredOverflow_IsAScrollContainer()
    {
        var box = BoxOfTag("html { overflow: hidden } body { overflow: auto }", "body");

        Assert.IsTrue(MonolithicContent.IsScrollContainer(box));
        Assert.IsTrue(MonolithicContent.IsMonolithic(box));
    }

    // ...and the companion direction, so the test above is not passing merely because `auto` is set.
    [TestMethod]
    public void Body_UnderAVisibleRoot_PropagatesAndIsNotAScrollContainer()
    {
        var box = BoxOfTag("html { overflow: visible } body { overflow: auto }", "body");

        Assert.IsFalse(MonolithicContent.IsScrollContainer(box));
    }

    // A stray element that happens to be named "body" but is not the root's own child gets no
    // propagation - §3.3 is about the document's body element, not the tag name.
    [TestMethod]
    public void NestedElementNamedBody_IsAnOrdinaryScrollContainer()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div><body id='t' style='overflow:hidden'>text</body></div>"));

        var box = LayoutHarness.FindById(root, "t");

        // The parser may or may not keep such an element; the assertion only means anything if it did.
        if (box is null || box.ParentBox is null || box.ParentBox.HtmlTag?.Name is "html") return;

        Assert.IsTrue(MonolithicContent.IsScrollContainer(box));
    }

    // ── the engine constraint, which is a different question ──────────────

    [TestMethod]
    [DataRow("display:table")]
    [DataRow("display:inline-table")]
    public void EngineThatPaginatesItself_IsNotBySpecMonolithic(string style)
    {
        var box = BoxOf($"<div id='t' style='{style}'><span>text</span></div>");

        Assert.IsTrue(MonolithicContent.PaginatesItsOwnContent(box));

        // The whole point of separating the two: this box is suppressed for an implementation reason,
        // and §2 says nothing about it.
        Assert.IsFalse(MonolithicContent.IsMonolithic(box));
    }

    // Flex, grid and multi-column layout don't exist, so they don't paginate their own content.
    [TestMethod]
    [DataRow("display:flex")]
    [DataRow("display:grid")]
    [DataRow("column-count:2")]
    public void UnsupportedEngineDisplay_DoesNotPaginateItsOwnContent(string style)
    {
        var box = BoxOf($"<div id='t' style='{style}'><span>text</span></div>");

        Assert.IsFalse(MonolithicContent.PaginatesItsOwnContent(box));
    }

    [TestMethod]
    public void OrdinaryBlock_DoesNotPaginateItsOwnContent()
    {
        var box = BoxOf("<div id='t'>text</div>");

        Assert.IsFalse(MonolithicContent.PaginatesItsOwnContent(box));
    }

    // ── the fitting question ──────────────────────────────────────────────

    [TestMethod]
    // Band is 160pt here (200pt page less two 20pt margins) - LayoutHarness's own pageHeight parameter is
    // already the content band (see its doc comment), so 160 is passed directly.
    [DataRow(100.0, false)]
    [DataRow(160.0, true)]
    [DataRow(200.0, true)]
    public void FitsNoFragmentainer_ComparesAgainstThePageContentBand(double height, bool expected)
    {
        var (_, container) = LayoutHarness.Layout(LayoutHarness.Wrap("<div>text</div>"), pageHeight: 160, margin: 20);

        Assert.AreEqual(expected, MonolithicContent.FitsNoFragmentainer(height, container));
    }

    // The companion question, and deliberately not the negation of the one above: "will it fit *there*"
    // is asked of one specific band, where a box exactly as tall as the band plainly does fit. The
    // relocation asks this one, so a band-tall box has somewhere to go.
    [TestMethod]
    [DataRow(100.0, 160.0, true)]
    [DataRow(160.0, 160.0, true)]
    [DataRow(161.0, 160.0, false)]
    public void FitsInBand_TreatsAnExactFitAsFitting(double height, double bandHeight, bool expected) =>
        Assert.AreEqual(expected, MonolithicContent.FitsInBand(height, bandHeight));

    // ── helpers ───────────────────────────────────────────────────────────

    private static CssBox BoxOfTag(string css, string tag)
    {
        var html = $"<!DOCTYPE html><html><head><style>{css}</style></head><body><div>text</div></body></html>";

        var (root, _) = LayoutHarness.Layout(html);

        return LayoutHarness.Descendants(root).First(b =>
            string.Equals(b.HtmlTag?.Name, tag, System.StringComparison.OrdinalIgnoreCase));
    }

    private static CssBox BoxOf(string markup)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(markup));
        var box = LayoutHarness.FindById(root, "t");

        Assert.IsNotNull(box);
        return box!;
    }
}
