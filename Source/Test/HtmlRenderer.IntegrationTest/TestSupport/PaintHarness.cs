using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragments;
using TheArtOfDev.HtmlRenderer.Core.Paint;

namespace HtmlRenderer.IntegrationTest.TestSupport;

/// <summary>
/// A layout+paint harness for tests that need to verify the actual paint calls a box makes (DrawLine,
/// DrawRectangle, colors, positions, ordering) rather than just resulting geometry. Uses the local
/// <see cref="MockAdapter"/>/<see cref="RecordingGraphics"/> pair (deterministic word metrics, every draw call
/// logged) instead of a real GDI+ surface, since real WinForms rendering has no way to intercept individual
/// draw calls. Mirrors the shape of PeachPDF.Tests' FragmentPaintHarness/TestRecordingGraphics.
/// </summary>
internal static class PaintHarness
{
    /// <summary>Lays <paramref name="html"/> out using the recording mock adapter.</summary>
    internal static (CssBox Root, HtmlContainerInt Container) Layout(
        string html,
        double maxWidth = 1000,
        double maxHeight = 4000)
    {
        var container = new HtmlContainerInt(new MockAdapter())
        {
            MaxSize = new RSize(maxWidth, maxHeight),
            Location = RPoint.Empty,
            PageSize = new RSize(maxWidth, maxHeight)
        };

        container.SetHtml(html);

        using var layoutGraphics = new RecordingGraphics();
        container.PerformLayout(layoutGraphics);

        Assert.IsNotNull(container.Root);

        return (container.Root!, container);
    }

    /// <summary>Wraps a body fragment in a minimal document, so a test can state only the markup it cares about.</summary>
    internal static string Wrap(string body) => $"<html><head></head><body style='margin:0'>{body}</body></html>";

    /// <summary>Depth-first search for the box carrying <c>id="<paramref name="id"/>"</c>.</summary>
    internal static CssBox? FindById(CssBox box, string id) => LayoutHarness.FindById(box, id);

    /// <summary>
    /// Paints a single box (and its descendants) the same way <see cref="HtmlContainerInt.PerformPaint"/> would
    /// paint the whole tree - establishes the same initial clip, then paints just <paramref name="box"/> - and
    /// returns a fresh <see cref="RecordingGraphics"/> with the resulting draw-call log.
    /// </summary>
    internal static RecordingGraphics PaintBox(HtmlContainerInt container, CssBox box)
    {
        var g = new RecordingGraphics();
        PaintBox(container, box, g);
        return g;
    }

    /// <summary>Same as <see cref="PaintBox(HtmlContainerInt, CssBox)"/> but reuses a caller-supplied graphics/log.</summary>
    internal static void PaintBox(HtmlContainerInt container, CssBox box, RecordingGraphics g)
    {
        if (container.MaxSize.Height > 0)
        {
            g.PushClip(new RRect(container.Location.X, container.Location.Y,
                Math.Min(container.MaxSize.Width, container.PageSize.Width),
                Math.Min(container.MaxSize.Height, container.PageSize.Height)));
        }
        else
        {
            g.PushClip(new RRect(container.MarginLeft, container.MarginTop, container.PageSize.Width, container.PageSize.Height));
        }

        var (fragment, bandTop) = FindFragment(container, box);
        new FragmentPainter(container).PaintFragmentSubtree(g, fragment, bandTop);

        g.PopClip();
    }

    /// <summary>
    /// Locates <paramref name="box"/>'s own <see cref="BoxFragment"/> in <paramref name="container"/>'s
    /// fragment tree (built by <see cref="HtmlContainerInt.PerformLayout"/>), searching every fragmentainer
    /// since a box relocated onto a later page won't be found on the first one. Paint now reads geometry
    /// from the fragment tree exclusively (<c>CssBox.Paint</c>/<c>PaintImp</c> were deleted once
    /// <see cref="FragmentPainter"/> became the only paint path), so a harness that wants "the draw calls
    /// for this one box" has to find its fragment first, the same way <see cref="FragmentPainter.Paint"/>
    /// itself starts from a fragmentainer's own <c>Root</c> fragment rather than a live <c>CssBox</c>.
    /// </summary>
    private static (BoxFragment Fragment, double BandTop) FindFragment(HtmlContainerInt container, CssBox box)
    {
        foreach (var fragmentainer in container.FragmentTree.Fragmentainers)
        {
            var found = FindFragment(fragmentainer.Root, box);
            if (found != null)
                return (found, fragmentainer.LocalOriginY);
        }

        throw new InvalidOperationException("No fragment found for the given box - is it display:none, or otherwise never laid out?");
    }

    private static BoxFragment? FindFragment(BoxFragment fragment, CssBox box)
    {
        if (ReferenceEquals(fragment.Box, box))
            return fragment;

        foreach (var child in fragment.Children)
        {
            var found = FindFragment(child, box);
            if (found != null)
                return found;
        }

        return fragment.MarkerFragment != null ? FindFragment(fragment.MarkerFragment, box) : null;
    }
}
