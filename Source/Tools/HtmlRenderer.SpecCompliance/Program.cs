using System.Text;
using HtmlRenderer.SpecCompliance;

const string Usage = """
    Usage:
      check                                       Validate the manifests and the [Spec] references in the tests
      report [--output <file>] [--summary <file>]  Write the compliance report as Markdown, to stdout by default,
                                                  and append the overview table to the summary file
    """;

Console.OutputEncoding = new UTF8Encoding(false);

if (args.Length == 0)
{
    Console.Error.WriteLine(Usage);
    return 1;
}

var root = Compliance.FindRepositoryRoot();
string? Option(string name)
{
    var i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}

switch (args[0])
{
    case "check":
    {
        var compliance = Compliance.Load(root);
        var problems = compliance.CheckManifests().Concat(compliance.CheckReferences()).Concat(compliance.CheckEvidence()).ToList();
        foreach (var problem in problems)
            Console.WriteLine(problem);
        Console.WriteLine(problems.Count == 0 ? "No problems found." : $"{problems.Count} problem(s) found.");
        return problems.Count == 0 ? 0 : 1;
    }

    case "report":
    {
        var compliance = Compliance.Load(root);
        var markdown = Report.Render(compliance);
        var output = Option("--output");
        if (output == null)
            Console.Write(markdown);
        else
            File.WriteAllText(output, markdown);

        // Appends, as $GITHUB_STEP_SUMMARY is shared by all steps of a job
        var summary = Option("--summary");
        if (summary != null)
            File.AppendAllText(summary, Report.RenderOverview(compliance));
        return 0;
    }

    default:
        Console.Error.WriteLine(Usage);
        return 1;
}
