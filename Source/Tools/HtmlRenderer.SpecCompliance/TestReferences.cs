using System.Text.RegularExpressions;

namespace HtmlRenderer.SpecCompliance;

/// <summary>
/// A <c>[Spec("css21", "box.html#collapsing-margins")]</c> on a test method. An ignored test documents a gap
/// and doesn't count as evidence.
/// </summary>
public sealed record TestReference(string Spec, string Anchor, string Test, string File, int Line, bool Ignored);

/// <summary>
/// Finds the <c>[Spec]</c> attributes in the test sources. Reading the sources instead of the built test
/// assemblies keeps the check independent of which test projects were built for which platform.
/// </summary>
public static partial class TestReferences
{
    [GeneratedRegex("""\bSpec\(\s*"([^"]+)"\s*,\s*"([^"]+)"\s*\)""")]
    private static partial Regex SpecAttribute();

    [GeneratedRegex(@"\b(?:void|Task)\s+(\w+)\s*\(")]
    private static partial Regex MethodName();

    // A declaration at the start of a line, so "class" inside a string literal doesn't count
    [GeneratedRegex(@"^\s*(?:(?:public|internal|private|protected|sealed|static|abstract|partial|file)\s+)*class\s+(\w+)", RegexOptions.Multiline)]
    private static partial Regex ClassName();

    public static List<TestReference> Scan(string testDirectory)
    {
        var references = new List<TestReference>();
        foreach (var file in Directory.EnumerateFiles(testDirectory, "*.cs", SearchOption.AllDirectories))
        {
            var separator = Path.DirectorySeparatorChar;
            if (file.Contains($"{separator}obj{separator}") || file.Contains($"{separator}bin{separator}"))
                continue;

            var source = File.ReadAllText(file);
            foreach (Match match in SpecAttribute().Matches(source))
            {
                // The attribute class itself and its usage examples aren't references
                var before = source.LastIndexOf('\n', match.Index) + 1;
                var linePrefix = source[before..match.Index].TrimStart();
                if (linePrefix.StartsWith("//", StringComparison.Ordinal) || linePrefix.StartsWith("///", StringComparison.Ordinal))
                    continue;

                var method = MethodName().Match(source, match.Index);
                var attributes = AttributeBlock(source, match.Index, method.Success ? method.Index : match.Index);
                var ignored = Regex.IsMatch(attributes, @"[\[,]\s*Ignore\b");
                var type = ClassName().Matches(source[..match.Index]).LastOrDefault();
                var test = $"{type?.Groups[1].Value}.{(method.Success ? method.Groups[1].Value : "?")}";
                var line = source.AsSpan(0, match.Index).Count('\n') + 1;
                references.Add(new TestReference(match.Groups[1].Value, match.Groups[2].Value, test, file, line, ignored));
            }
        }

        return references;
    }

    /// <summary>The attribute lines above a method, up to the previous blank line or brace.</summary>
    private static string AttributeBlock(string source, int attributeIndex, int methodIndex)
    {
        var start = source.LastIndexOf('\n', attributeIndex) + 1;
        while (start > 1)
        {
            var previousStart = source.LastIndexOf('\n', start - 2) + 1;
            var previous = source[previousStart..(start - 1)].Trim();
            if (previous.Length == 0 || previous == "}" || previous.EndsWith('{'))
                break;
            start = previousStart;
        }

        return source[start..methodIndex];
    }
}
