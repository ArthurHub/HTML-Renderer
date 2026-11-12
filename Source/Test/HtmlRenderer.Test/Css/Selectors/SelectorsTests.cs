using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css.Selectors;

/// <summary>
/// Runs selectors against the bootstrap.css fixture, embedded as a resource.
/// </summary>
[TestClass]
public sealed class SelectorsTests
{
    [TestMethod]
    public void FindAllStyleRulesForAnElement()
    {
        var sheet = ParseBootstrap();

        var list = sheet.StyleRules
            .Where(r =>
                r.Selector is TypeSelector { Name: "input" }
                || (r.Selector is CompoundSelector selector && selector.First() is TypeSelector { Name: "input" })
            );

        Assert.AreEqual(6, list.Count());
    }

    [TestMethod]
    public void FindAllStyleRulesElementsWithMoreThanTwoCompoundSelectors()
    {
        var sheet = ParseBootstrap();

        var list = sheet.StyleRules
            .Where(r => (r.Selector as CompoundSelector)?.Length > 2);

        Assert.AreEqual(2, list.Count());
    }

    [TestMethod]
    public void FindAllStyleRulesWithCompoundSelector()
    {
        var sheet = ParseBootstrap();

        var list = sheet.StyleRules
            .Where(r => r.Selector is CompoundSelector selector && selector.Last().Text.StartsWith("["));

        Assert.AreEqual(7, list.Count());
    }

    private static readonly string[] StandardPseudoElementNames =
    [
        PseudoElementNames.After,
        PseudoElementNames.Before,
        PseudoElementNames.Content,
        PseudoElementNames.FirstLetter,
        PseudoElementNames.FirstLine,
        PseudoElementNames.Selection
    ];

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void FindAllStandardPseudoElementSelectors(bool allowInvalidSelectors)
    {
        Assert.AreEqual(277, CountPseudoElementRules(allowInvalidSelectors, nonStandard: false));
    }

    [TestMethod]
    public void FindAllNonStandardPseudoElementSelectors_WhenToleratingInvalidSelectors()
    {
        Assert.AreEqual(6, CountPseudoElementRules(allowInvalidSelectors: true, nonStandard: true));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/selectors-4.yml")]
    [Spec("selectors-4", "#invalid")]
    public void FindAllNonStandardPseudoElementSelectors_KeepsOnlyTheWebkitOnes()
    {
        Assert.AreEqual(3, CountPseudoElementRules(allowInvalidSelectors: false, nonStandard: true));
    }

    private static int CountPseudoElementRules(bool allowInvalidSelectors, bool nonStandard)
    {
        var sheet = ParseBootstrap(allowInvalidSelectors);

        return sheet.StyleRules.Count(r => HasStandardPseudoElementSelector(r.Selector, negate: nonStandard));
    }

    [TestMethod]
    [DataRow(":nth-child(10n + 1)", 10, 1)]
    [DataRow(":nth-child(10n - 1)", 10, -1)]
    [DataRow(":nth-child(-2n + 3)", -2, 3)]
    [DataRow(":nth-child( 10n  +  1 )", 10, 1)]
    [DataRow(":nth-child(10n+1)", 10, 1)]
    [DataRow(":nth-child(odd)", 2, 1)]
    [DataRow(":nth-last-child(3n + 2)", 3, 2)]
    [DataRow(":nth-of-type(3n + 2)", 3, 2)]
    [DataRow(":nth-last-of-type(3n + 2)", 3, 2)]
    public void NthChildAcceptsAnBWithAndWithoutSpaces(string selector, int step, int offset)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet(selector + " { color: red }");

        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<ChildSelector>(((StyleRule)sheet.Rules[0]).Selector);
        var child = (ChildSelector)((StyleRule)sheet.Rules[0]).Selector;
        Assert.AreEqual(step, child.Step);
        Assert.AreEqual(offset, child.Offset);
    }

    [TestMethod]
    [DataRow(":nth-child(3n + -6)")]
    [DataRow(":nth-child(10n + +1)")]
    [DataRow(":nth-child(10n + + 1)")]
    [DataRow(":nth-child(10n + n)")]
    [DataRow(":nth-child(3 n)")]
    [DataRow(":nth-child(+ 2n)")]
    [DataRow(":nth-child(+ 2)")]
    public void NthChildRejectsMalformedAnB(string selector)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet(selector + " { color: red }");

        Assert.AreEqual(0, sheet.Rules.Length);
    }

    private static bool HasStandardPseudoElementSelector(ISelector selector, bool negate = false)
    {
        if (selector is PseudoElementSelector pes)
            return negate ^ StandardPseudoElementNames.Contains(pes.Name);
        else if (selector is CompoundSelector comp)
            return HasStandardPseudoElementSelector(comp.Last(), negate);
        else if (selector is ListSelector list)
            return list.Any(s => HasStandardPseudoElementSelector(s, negate));
        else if (selector is ComplexSelector complex)
            return HasStandardPseudoElementSelector(complex.Last().Selector, negate);
        return false;
    }

    private static Stylesheet ParseBootstrap(bool tolerateInvalidSelectors = false)
    {
        using var stream = GetBootstrapCssStream();
        var parser = new StylesheetParser(tolerateInvalidSelectors: tolerateInvalidSelectors);
        return parser.Parse(stream);
    }

    private static Stream GetBootstrapCssStream()
    {
        const string resourceName = "HtmlRenderer.Test.CssEngineSupport.bootstrap.css";
        var assembly = typeof(SelectorsTests).Assembly;
        return assembly.GetManifestResourceStream(resourceName)
               ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
    }
}
