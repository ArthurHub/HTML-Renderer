using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HtmlRenderer.SpecCompliance;

public static class SectionStatus
{
    public const string Unknown = "unknown";
    public const string Supported = "supported";
    public const string Partial = "partial";
    public const string ParsedOnly = "parsed-only";
    public const string Unsupported = "unsupported";
    public const string NotApplicable = "n/a";

    public static readonly string[] All = [Supported, Partial, ParsedOnly, Unsupported, NotApplicable, Unknown];
}

public sealed class ManifestSection
{
    public string Title { get; set; } = "";

    public string Status { get; set; } = SectionStatus.Unknown;

    public List<string>? Gaps { get; set; }
}

/// <summary>
/// The implementation status of one spec, <c>Spec/&lt;spec&gt;.yml</c>. Sections are keyed by their anchor
/// and kept in spec order.
/// </summary>
public sealed class Manifest
{
    public string Spec { get; set; } = "";

    public string Title { get; set; } = "";

    /// <summary>Published URL, anchors are appended to it for links in the report.</summary>
    public string Url { get; set; } = "";

    /// <summary>
    /// The version the sections were checked against: the commit of a living standard or the dated URL of a
    /// published draft, none for a fixed version.
    /// </summary>
    public string? Snapshot { get; set; }

    public Dictionary<string, ManifestSection> Sections { get; set; } = new(StringComparer.Ordinal);

    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .Build();

    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)
        .WithIndentedSequences()
        .Build();

    public static Manifest Load(string path)
    {
        var manifest = Deserializer.Deserialize<Manifest>(File.ReadAllText(path));
        manifest.Sections ??= new Dictionary<string, ManifestSection>(StringComparer.Ordinal);
        return manifest;
    }

    public void Save(string path) => File.WriteAllText(path, Serializer.Serialize(this).ReplaceLineEndings("\n"));
}
