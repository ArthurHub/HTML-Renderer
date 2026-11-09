using ComplianceData = HtmlRenderer.SpecCompliance.Compliance;

namespace HtmlRenderer.Test.Compliance;

/// <summary>
/// Keeps <c>Spec/*.yml</c> honest: every section has a numbered title, a known status and no n/a entry its parent
/// already covers, every <c>[Spec]</c> in the tests points to a listed section, and every section claimed as
/// supported is backed by a test.
/// </summary>
[TestClass]
public sealed class SpecComplianceTests
{
    private static readonly Lazy<ComplianceData> Data = new(() =>
        ComplianceData.Load(ComplianceData.FindRepositoryRoot(AppContext.BaseDirectory)));

    [TestMethod]
    public void Manifests_AreWellFormed() => AssertNone(Data.Value.CheckManifests());

    [TestMethod]
    public void SpecReferences_PointToExistingAnchors() => AssertNone(Data.Value.CheckReferences());

    [TestMethod]
    public void SupportedSections_AreBackedByTests() => AssertNone(Data.Value.CheckEvidence());

    private static void AssertNone(List<string> problems) =>
        Assert.IsEmpty(problems, Environment.NewLine + string.Join(Environment.NewLine, problems));
}
