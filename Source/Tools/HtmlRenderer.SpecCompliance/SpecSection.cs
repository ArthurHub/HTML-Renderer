namespace HtmlRenderer.SpecCompliance;

/// <summary>A numbered section of a spec, e.g. "8.3.1 Collapsing margins" at <c>box.html#collapsing-margins</c>.</summary>
public sealed record SpecSection(string Anchor, string Number, string Title)
{
    public string DisplayTitle => $"{Number} {Title}";

    public bool IsAncestorOf(SpecSection other) => other.Number.StartsWith(Number + ".", StringComparison.Ordinal);

    /// <summary>Splits a manifest title like "8.3.1 Collapsing margins" into number and title.</summary>
    public static SpecSection FromManifest(string anchor, string displayTitle)
    {
        var space = displayTitle.IndexOf(' ');
        return space < 0
            ? new SpecSection(anchor, displayTitle, "")
            : new SpecSection(anchor, displayTitle[..space], displayTitle[(space + 1)..]);
    }
}
