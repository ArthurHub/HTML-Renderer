using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragmentation;

namespace HtmlRenderer.Test.Fragmentation;

/// <summary>
/// Ported from PeachPDF.Tests/Html/Core/Fragmentation/MonolithicContentTests.cs (MonolithicContentTests).
/// </summary>
/// <remarks>
/// The monolithic classifier, asserted against css-break-3 §2's own set rather than the engine's prior
/// behaviour, the same way <see cref="BreakValuesTests"/> reads §3's value sets. Adapted for
/// <c>Core/Fragmentation/MonolithicContent.cs</c>'s own reduced scope (see its class doc comment): no flex/
/// grid/multi-column engine (so <c>PaginatesItsOwnContent</c> narrows to table/inline-table), and
/// HTML-Renderer's smaller replaced-element set - only <c>&lt;img&gt;</c>/<c>&lt;iframe&gt;</c> are replaced
/// (<c>CssBox.CreateBox</c>, Core/Dom/CssBox.cs), confirmed by direct source read: there is no
/// <c>CssBoxObject</c>/inline-SVG/form-widget box type anywhere in this fork, so PeachPDF's own
/// <c>&lt;svg&gt;</c>/<c>&lt;object&gt;</c> theory rows and its <c>UnresolvedObject_IsNotReplaced</c> test
/// (which exists specifically to probe PeachPDF's dynamic object-resolution behaviour) are dropped rather
/// than adapted - there is no dynamic resolution question to ask here. <c>FitsNoFragmentainer</c>/
/// <c>FitsInBand</c> also lost their <c>clonedStart</c>/<c>clonedEnd</c> parameters (box-decoration-break
/// clone insets do not exist in this port), so the theory rows that varied only those are dropped too.
/// </remarks>
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
    // Not in Map.OverflowModes (Core/CssEngine/Model/Map.cs), so it never converts and the box keeps
    // "visible" - which is the answer §2 wants for `clip` anyway, though by accident rather than by design
    // (matching PeachPDF's own identically-accidental behaviour here).
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

    // The narrowed scope itself (see class remarks): PeachPDF also recognizes flex/grid/multi-column as
    // self-paginating engines; none of the three exist in this fork, so none of them qualify here.
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
