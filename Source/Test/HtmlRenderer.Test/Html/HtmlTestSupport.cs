using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Html;

internal static class HtmlTestSupport
{
    /// <summary>Lays out <paramref name="body"/> and returns the box with the given id.</summary>
    public static CssBox Box(string body, string id = "x")
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(body));
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box, $"No box with id '{id}'");
        return box;
    }

    /// <summary>The tag name of the element the parser put the box with the given id into.</summary>
    public static string? ParentTag(string body, string id = "x") => Box(body, id).ParentBox?.HtmlTag?.Name;

    /// <summary>The list marker of the list item with the given id, e.g. "3.".</summary>
    public static string Marker(string body, string id = "x") =>
        string.Concat(Box(body, id).ListItemBox?.Words.Select(w => w.Text) ?? []);
}
