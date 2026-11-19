using TheArtOfDev.HtmlRenderer.Core.Fragments;

namespace HtmlRenderer.IntegrationTest.TestSupport;

internal static class Fragments
{
    /// <summary><paramref name="fragment"/> and all its descendants, depth first.</summary>
    internal static IEnumerable<BoxFragment> Flatten(BoxFragment fragment)
    {
        yield return fragment;
        foreach (var child in fragment.Children)
            foreach (var descendant in Flatten(child))
                yield return descendant;
    }

    /// <summary>Like <see cref="Flatten"/>, plus each fragment's list marker after its children.</summary>
    internal static IEnumerable<BoxFragment> FlattenWithMarkers(BoxFragment fragment)
    {
        yield return fragment;
        foreach (var child in fragment.Children)
            foreach (var descendant in FlattenWithMarkers(child))
                yield return descendant;
        if (fragment.MarkerFragment != null)
            foreach (var descendant in FlattenWithMarkers(fragment.MarkerFragment))
                yield return descendant;
    }
}
