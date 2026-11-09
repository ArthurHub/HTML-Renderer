using System.Text;

namespace HtmlRenderer.SpecCompliance;

/// <summary>Renders the compliance as Markdown, for the GitHub Actions job summary or a file.</summary>
public static class Report
{
    private static readonly string[] Columns = [SectionStatus.Supported, SectionStatus.Partial, SectionStatus.ParsedOnly, SectionStatus.Unsupported, SectionStatus.NotApplicable, SectionStatus.Unknown];

    public static string Render(Compliance compliance)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Spec compliance").AppendLine();
        sb.AppendLine("| Spec | Sections | Supported | Partial | Parsed only | Unsupported | N/A | Unknown | Tested |");
        sb.AppendLine("|---|--:|--:|--:|--:|--:|--:|--:|--:|");
        foreach (var spec in compliance.Specs)
        {
            var sections = spec.Manifest.Sections;
            var counts = Columns.Select(c => sections.Values.Count(s => s.Status == c).ToString());
            var tested = sections.Keys.Count(a => spec.TestsOf(a).Count > 0);
            sb.AppendLine($"| [{spec.Manifest.Title}]({spec.Manifest.Url}) | {sections.Count} | {string.Join(" | ", counts)} | {tested} |");
        }

        foreach (var spec in compliance.Specs)
        {
            sb.AppendLine().AppendLine($"### {spec.Manifest.Title}").AppendLine();
            foreach (var chapter in Chapters(spec))
                RenderChapter(sb, spec, chapter);
        }

        return sb.ToString();
    }

    private static void RenderChapter(StringBuilder sb, SpecResult spec, List<SpecSection> sections)
    {
        var entries = sections.Select(s => spec.Manifest.Sections.GetValueOrDefault(s.Anchor) ?? new ManifestSection()).ToList();
        var summary = Columns
            .Select(c => (Status: c, Count: entries.Count(e => e.Status == c)))
            .Where(c => c.Count > 0)
            .Select(c => $"{c.Count} {c.Status}");

        sb.AppendLine($"<details><summary>{Escape(sections[0].DisplayTitle)}: {string.Join(", ", summary)}</summary>").AppendLine();
        sb.AppendLine("| Section | Status | Tests | Gaps |");
        sb.AppendLine("|---|---|--:|---|");
        foreach (var section in sections)
        {
            var entry = spec.Manifest.Sections.GetValueOrDefault(section.Anchor) ?? new ManifestSection();
            var gaps = entry.Gaps == null ? "" : string.Join("<br>", entry.Gaps.Select(Escape));
            var link = spec.Manifest.Url.TrimEnd('/') + "/" + section.Anchor;
            sb.AppendLine($"| [{Escape(section.DisplayTitle)}]({link}) | {entry.Status} | {spec.TestsOf(section.Anchor).Count} | {gaps} |");
        }

        sb.AppendLine().AppendLine("</details>").AppendLine();
    }

    /// <summary>Groups the sections by their top-level number, i.e. chapter or appendix.</summary>
    private static IEnumerable<List<SpecSection>> Chapters(SpecResult spec)
    {
        List<SpecSection>? chapter = null;
        foreach (var section in spec.Sections)
        {
            if (!section.Number.Contains('.') || chapter == null)
            {
                if (chapter != null)
                    yield return chapter;
                chapter = [];
            }

            chapter.Add(section);
        }

        if (chapter != null)
            yield return chapter;
    }

    private static string Escape(string text) => text.Replace("|", "\\|").Replace("<", "&lt;").Replace(">", "&gt;");
}
