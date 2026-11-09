using System.Text.RegularExpressions;

namespace HtmlRenderer.SpecCompliance;

/// <summary>One spec with its manifest and the tests credited to each section.</summary>
public sealed class SpecResult
{
    public SpecResult(Manifest manifest)
    {
        Manifest = manifest;
        Sections = manifest.Sections.Select(s => SpecSection.FromManifest(s.Key, s.Value.Title)).ToList();
    }

    public Manifest Manifest { get; }

    /// <summary>The manifest's sections in spec order.</summary>
    public List<SpecSection> Sections { get; }

    /// <summary>Tests per section anchor. A test counts for the section it references and all its ancestors.</summary>
    public Dictionary<string, List<TestReference>> Tests { get; } = new(StringComparer.Ordinal);

    public IReadOnlyList<TestReference> TestsOf(string sectionAnchor) =>
        Tests.TryGetValue(sectionAnchor, out var tests) ? tests : [];

    internal void Credit(TestReference reference)
    {
        var section = Sections.FirstOrDefault(s => s.Anchor == reference.Anchor);
        if (section == null || reference.Ignored)
            return;

        foreach (var credited in Sections.Where(s => s == section || s.IsAncestorOf(section)))
        {
            if (!Tests.TryGetValue(credited.Anchor, out var tests))
                Tests[credited.Anchor] = tests = [];
            if (!tests.Any(t => t.Test == reference.Test))
                tests.Add(reference);
        }
    }
}

/// <summary>Everything under <c>Spec/</c> plus the <c>[Spec]</c> references in <c>Source/Test</c>.</summary>
public sealed partial class Compliance
{
    // "8.3.1", or an appendix like "E" or "G.2"
    [GeneratedRegex(@"^(\d+|[A-Z])(\.\d+)*$")]
    private static partial Regex SectionNumber();

    public required string RepositoryRoot { get; init; }

    public required List<SpecResult> Specs { get; init; }

    public required List<TestReference> References { get; init; }

    public static string SpecDirectory(string root) => Path.Combine(root, "Spec");

    public static string ManifestPath(string root, string spec) => Path.Combine(SpecDirectory(root), spec + ".yml");

    /// <summary>Walks up from <paramref name="start"/> to the folder holding <c>Source/HtmlRenderer.sln</c>.</summary>
    public static string FindRepositoryRoot(string? start = null)
    {
        for (var dir = new DirectoryInfo(start ?? Directory.GetCurrentDirectory()); dir != null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Source", "HtmlRenderer.sln")))
                return dir.FullName;
        }

        throw new DirectoryNotFoundException("Couldn't find the repository root, run this from within the HTML-Renderer checkout.");
    }

    public static Compliance Load(string root)
    {
        var specs = Directory.EnumerateFiles(SpecDirectory(root), "*.yml")
            .Order(StringComparer.Ordinal)
            .Select(path => new SpecResult(Manifest.Load(path)))
            .ToList();

        var references = TestReferences.Scan(Path.Combine(root, "Source", "Test"));
        foreach (var reference in references)
            specs.FirstOrDefault(s => s.Manifest.Spec == reference.Spec)?.Credit(reference);

        return new Compliance { RepositoryRoot = root, Specs = specs, References = references };
    }

    /// <summary>Every section has a title starting with its section number, a known status, and isn't an n/a entry its parent already covers.</summary>
    public List<string> CheckManifests()
    {
        var problems = new List<string>();
        foreach (var spec in Specs)
        {
            var name = spec.Manifest.Spec;
            foreach (var section in spec.Sections)
            {
                if (!SectionNumber().IsMatch(section.Number))
                    problems.Add($"{name}: {section.Anchor} needs a title starting with its section number");

                var status = spec.Manifest.Sections[section.Anchor].Status;
                if (!SectionStatus.All.Contains(status))
                    problems.Add($"{name}: {section.Anchor} has the unknown status \"{status}\", use one of {string.Join(", ", SectionStatus.All)}");

                var parent = spec.Sections.LastOrDefault(s => s.IsAncestorOf(section));
                if (status == SectionStatus.NotApplicable && spec.Manifest.Sections[section.Anchor].Gaps is not { Count: > 0 }
                    && parent != null && spec.Manifest.Sections[parent.Anchor].Status == SectionStatus.NotApplicable)
                    problems.Add($"{name}: {section.Anchor} is n/a like {parent.Anchor}, which already covers it, leave it out");
            }
        }

        return problems;
    }

    /// <summary>Every <c>[Spec]</c> in the tests points to a section of a known spec.</summary>
    public List<string> CheckReferences()
    {
        var problems = new List<string>();
        foreach (var reference in References)
        {
            var location = $"{reference.Test} ({Path.GetRelativePath(RepositoryRoot, reference.File)}:{reference.Line})";
            var spec = Specs.FirstOrDefault(s => s.Manifest.Spec == reference.Spec);
            if (spec == null)
                problems.Add($"{location}: unknown spec \"{reference.Spec}\"");
            else if (!spec.Manifest.Sections.ContainsKey(reference.Anchor))
                problems.Add($"{location}: \"{reference.Anchor}\" isn't a section of {reference.Spec}");
        }

        return problems;
    }

    /// <summary>A section claimed as (partially) supported has tests, and a partial one lists its gaps.</summary>
    public List<string> CheckEvidence()
    {
        var problems = new List<string>();
        foreach (var spec in Specs)
        {
            foreach (var (anchor, section) in spec.Manifest.Sections)
            {
                var claimed = section.Status is SectionStatus.Supported or SectionStatus.Partial;
                if (claimed && spec.TestsOf(anchor).Count == 0)
                    problems.Add($"{spec.Manifest.Spec}: {section.Title} is marked {section.Status}, but no test references it");

                if (section.Status == SectionStatus.Partial && !HasGaps(spec, anchor))
                    problems.Add($"{spec.Manifest.Spec}: {section.Title} is marked partial, but neither it nor its subsections list gaps");
            }
        }

        return problems;
    }

    private static bool HasGaps(SpecResult spec, string anchor)
    {
        var section = spec.Sections.First(s => s.Anchor == anchor);
        return spec.Sections
            .Where(s => s == section || section.IsAncestorOf(s))
            .Any(s => spec.Manifest.Sections[s.Anchor].Gaps is { Count: > 0 });
    }
}
