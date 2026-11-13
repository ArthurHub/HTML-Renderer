using System.Drawing;
using System.Drawing.Imaging;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.WinForms.Adapters;

namespace HtmlRenderer.IntegrationTest.TestSupport;

/// <summary>
/// The shared lightweight layout harness for pure layout/geometry assertions: builds an
/// <see cref="HtmlContainerInt"/> over the real <see cref="WinFormsAdapter"/> (so font metrics/word wrapping
/// reflect actual rendering, not a mock approximation), runs layout, and hands back the laid-out box tree plus
/// the container. Requires <c>InternalsVisibleTo("HtmlRenderer.IntegrationTest")</c> on both HtmlRenderer.csproj
/// (for the internal <see cref="CssBox"/>/<see cref="HtmlContainerInt.Root"/> types) and
/// HtmlRenderer.WinForms.csproj (for the internal <see cref="WinFormsAdapter"/>/GraphicsAdapter types).
/// </summary>
/// <remarks>
/// For tests that need to verify paint calls (DrawLine/DrawRectangle/etc. order, color, position) rather than
/// just geometry, use <see cref="PaintHarness"/> instead, which uses a recording mock adapter.
/// </remarks>
internal static class LayoutHarness
{
    /// <summary>
    /// Lays <paramref name="html"/> out at <paramref name="maxWidth"/> x <paramref name="maxHeight"/> pixels.
    /// </summary>
    /// <param name="prepare">
    /// Optional: run against the parsed box tree's root after <c>SetHtml</c> and before layout, for a test that
    /// has to put something in the tree the parser cannot produce.
    /// </param>
    /// <param name="margin">
    /// Optional: uniform offset applied as the container's <see cref="HtmlContainerInt.Location"/> (and mirrored
    /// onto <see cref="HtmlContainerInt.MarginTop"/>/Bottom/Left/Right for paint-clip purposes) - the root box is
    /// placed at <c>(margin, margin)</c>, standing in for a page margin since this engine has no page/margin
    /// concept of its own that feeds into layout.
    /// </param>
    internal static (CssBox Root, HtmlContainerInt Container) Layout(
        string html,
        double maxWidth = 1000,
        double maxHeight = 4000,
        Action<CssBox>? prepare = null,
        double margin = 0)
    {
        var container = new HtmlContainerInt(WinFormsAdapter.Instance)
        {
            MaxSize = new RSize(maxWidth, maxHeight),
            Location = new RPoint(margin, margin),
            PageSize = new RSize(maxWidth, maxHeight)
        };
        container.SetMargins((int)margin);

        container.SetHtml(html);

        if (prepare is not null)
        {
            Assert.IsNotNull(container.Root);
            prepare(container.Root!);
        }

        PerformLayout(container);

        Assert.IsNotNull(container.Root);

        return (container.Root!, container);
    }

    /// <summary>Runs one layout pass of <paramref name="container"/>, e.g. a second one after <see cref="Layout"/>.</summary>
    internal static void PerformLayout(HtmlContainerInt container)
    {
        using var bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bitmap);
        using var graphics = new GraphicsAdapter(g, false);
        container.PerformLayout(graphics);
    }

    /// <summary>Wraps a body fragment in a minimal document, so a test can state only the markup it cares about.</summary>
    internal static string Wrap(string body) => $"<html><head></head><body style='margin:0'>{body}</body></html>";

    /// <summary>Depth-first search for the box carrying <c>id="<paramref name="id"/>"</c>.</summary>
    internal static CssBox? FindById(CssBox box, string id)
    {
        if (box.HtmlTag?.TryGetAttribute("id") == id)
            return box;

        foreach (var childBox in box.Boxes)
        {
            var found = FindById(childBox, id);
            if (found is not null) return found;
        }

        return null;
    }

    /// <summary>Every box in the tree, in document order.</summary>
    internal static IEnumerable<CssBox> Descendants(CssBox box)
    {
        yield return box;

        foreach (var childBox in box.Boxes)
        {
            foreach (var descendant in Descendants(childBox))
            {
                yield return descendant;
            }
        }
    }
}
