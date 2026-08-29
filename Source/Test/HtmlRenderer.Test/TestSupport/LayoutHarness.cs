using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.TestSupport;

/// <summary>
/// The shared lightweight layout harness: builds an <see cref="HtmlContainerInt"/> over a <see cref="MockAdapter"/>,
/// runs layout, and hands back the laid-out box tree plus the container. Prefer this over hand-rolling another
/// per-file box-tree setup.
/// </summary>
internal static class LayoutHarness
{
    /// <summary>
    /// Lays <paramref name="html"/> out at <paramref name="maxWidth"/> × <paramref name="maxHeight"/> pixels.
    /// </summary>
    /// <param name="prepare">
    /// Optional: run against the parsed box tree's root after <c>SetHtml</c> and before layout, for a test that
    /// has to put something in the tree the parser cannot produce.
    /// </param>
    /// <param name="adapter">
    /// Optional: a caller-supplied <see cref="MockAdapter"/> (e.g. with a non-default <c>MediaType</c> for
    /// <c>@media</c> tests). Defaults to a plain <c>new MockAdapter()</c>.
    /// </param>
    /// <param name="pageHeight">
    /// Optional: enables a real page grid (<see cref="HtmlContainerInt.HasRealPageGrid"/>) for fragmentation
    /// tests. Assigned directly to <see cref="HtmlContainerInt.PageSize"/>'s height - i.e. this is already the
    /// per-page CONTENT BAND, not a sheet height margins are subtracted from, matching this fork's own
    /// <see cref="HtmlContainerInt.PageTopOf"/>/<see cref="HtmlContainerInt.PageIndexOf"/> convention (a
    /// caller wanting a 300px sheet with 20px margins passes <c>pageHeight: 260</c>). <paramref name="margin"/>
    /// pixels of top/bottom margin are applied on top - the root box is placed at <c>(margin, margin)</c>, and
    /// <c>MaxSize.Height</c> is left unbounded (0), matching this branch's own <c>StageR1DriverLoopTest</c>-style
    /// convention, since fragmentation content commonly spans many multiples of one page. Left null (the
    /// default) leaves <see cref="HtmlContainerInt.PageSize"/> unset - <c>HasRealPageGrid</c> false - which is
    /// required to keep every pre-existing non-fragmentation caller of this method behaving exactly as before.
    /// </param>
    /// <param name="margin">Only meaningful when <paramref name="pageHeight"/> is given - see its own doc.</param>
    internal static (CssBox Root, HtmlContainerInt Container) Layout(
        string html,
        double maxWidth = 1000,
        double maxHeight = 4000,
        Action<CssBox>? prepare = null,
        MockAdapter? adapter = null,
        double? pageHeight = null,
        double margin = 20)
    {
        var container = new HtmlContainerInt(adapter ?? new MockAdapter())
        {
            MaxSize = new RSize(maxWidth, pageHeight.HasValue ? 0 : maxHeight),
            Location = pageHeight.HasValue ? new RPoint(margin, margin) : RPoint.Empty
        };

        if (pageHeight.HasValue)
        {
            container.SetMargins((int)margin);
            container.PageSize = new RSize(maxWidth, pageHeight.Value);
        }

        container.SetHtml(html);

        if (prepare is not null)
        {
            Assert.IsNotNull(container.Root);
            prepare(container.Root!);
        }

        using var graphics = new RecordingGraphics();
        container.PerformLayout(graphics);

        Assert.IsNotNull(container.Root);

        return (container.Root!, container);
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
