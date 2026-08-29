using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Paint.Content;

namespace HtmlRenderer.IntegrationTest.Painting;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/FragmentContentPainterTests.cs: the per-box-type paint dispatch -
/// <c>FragmentContentPainters.For</c> picks the painter, and the ones the other paint suites don't already
/// drive (<c>&lt;iframe&gt;</c>, <c>&lt;hr&gt;</c>) draw what they should.
/// </summary>
/// <remarks>
/// <c>FragmentContentPainters.For</c>'s <c>switch</c> (Core/Paint/Content/FragmentContentPainters.cs) lists
/// only 3 cases - <see cref="TheArtOfDev.HtmlRenderer.Core.Dom.CssBoxImage"/>,
/// <see cref="TheArtOfDev.HtmlRenderer.Core.Dom.CssBoxHr"/>,
/// <see cref="TheArtOfDev.HtmlRenderer.Core.Dom.CssBoxFrame"/> - confirmed by direct source read, HTML-Renderer
/// has no distinct box type for <c>&lt;object&gt;</c> or inline <c>&lt;svg&gt;</c> at all (no
/// <c>CssBoxObject</c>/<c>CssBoxSvg</c> anywhere in <c>Core/Dom/</c>), so PeachPDF's theory rows for those two
/// element types are dropped.
/// <para>
/// <c>Iframe_PaintsItsOwnBoxOnly_WithNoEmbeddedContent</c> ports for a different underlying reason than in
/// PeachPDF: this fork's <c>CssBoxFrame</c> is not a stub - it can render a YouTube/Vimeo video thumbnail/
/// title/play button for a matching <c>src</c> (see <c>CssBoxFrame</c>'s own doc comment). An ordinary
/// (non-video) <c>&lt;iframe&gt;</c> with no matching <c>src</c> still draws nothing beyond its own
/// background/border, though: <c>_isVideo</c> is false, so <c>DrawImage</c>/<c>DrawTitle</c>/<c>DrawPlay</c>
/// all no-op (confirmed by reading <c>CssBoxFrame.DrawFrameContent</c> and its three private helpers in
/// full), which happens to match PeachPDF's own "no embedded content" expectation for this fixture.
/// </para>
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class FragmentContentPainterTests
{
    [TestMethod]
    public void For_PlainBox_HasNoContentPainter()
    {
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap("<div id='el'>x</div>"));

        Assert.IsNull(FragmentContentPainters.For(PaintHarness.FindById(root, "el")!));
    }

    [TestMethod]
    [DataRow("<img id='el' src='missing.png'>", typeof(ImageFragmentPainter))]
    [DataRow("<iframe id='el' width='40' height='30'></iframe>", typeof(FrameFragmentPainter))]
    [DataRow("<hr id='el'>", typeof(HrFragmentPainter))]
    public void For_ReplacedBox_PicksItsOwnPainter(string body, System.Type expected)
    {
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap(body));

        Assert.IsInstanceOfType(FragmentContentPainters.For(PaintHarness.FindById(root, "el")!), expected);
    }

    [TestMethod]
    public void Iframe_PaintsItsOwnBoxOnly_WithNoEmbeddedContent()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<iframe id='el' style='display:block; width:40px; height:30px;"
            + " background:rgb(10,20,30); border:2px solid rgb(1,2,3)'></iframe>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "el")!);

        Assert.IsTrue(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColor.FromArgb(10, 20, 30)));
        Assert.IsFalse(g.DrawImageCalls.Any());
        Assert.IsFalse(g.DrawStringCalls.Any());
    }

    [TestMethod]
    public void Hr_TallerThanTheRule_FillsItsBackground()
    {
        // An <hr> tall enough to have an interior fills it with background-color before drawing the border
        // sides that make up the rule itself.
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<hr id='el' style='height: 20px; background: rgb(10,20,30); border: none'>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "el")!);

        Assert.IsTrue(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColor.FromArgb(10, 20, 30)));
    }
}
