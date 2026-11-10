using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssNamespacesTests
{
    private const string Green = "rgb(0, 128, 0)";
    private const string Html = "http://www.w3.org/1999/xhtml";
    private const string Svg = "http://www.w3.org/2000/svg";

    [TestMethod]
    [Spec("css-namespaces-3", "#syntax")]
    [DataRow("@namespace svg url(" + Svg + ");", "svg", Svg)]
    [DataRow("@namespace url(\"" + Svg + "\");", "", Svg)]
    [DataRow("@namespace /**/ svg /**/ url( " + Svg + " ) /**/;", "svg", Svg)]
    public void NamespaceRule_TakesAnOptionalPrefixAndAUrl(string css, string prefix, string uri)
    {
        var rule = ParseStyleSheet(css).NamespaceRules.Single();

        Assert.AreEqual(prefix, rule.Prefix);
        Assert.AreEqual(uri, rule.NamespaceUri);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-namespaces-3.yml")]
    [Spec("css-namespaces-3", "#syntax")]
    [DataRow("@namespace svg \"" + Svg + "\";", "svg", Svg)]
    [DataRow("@namespace \"" + Svg + "\";", "", Svg)]
    [DataRow("@namespace empty \"\";", "empty", "")]
    public void NamespaceRule_TakesAString(string css, string prefix, string uri)
    {
        var rule = ParseStyleSheet(css).NamespaceRules.Single();

        Assert.AreEqual(prefix, rule.Prefix);
        Assert.AreEqual(uri, rule.NamespaceUri);
    }

    [TestMethod]
    [Spec("css-namespaces-3", "#syntax")]
    public void NamespaceRule_DoesntAffectTheFollowingRules()
    {
        Assert.AreEqual(Green, Styled($"@namespace url({Html}); @namespace h url({Html}); #x {{ color: green }}").Color);
    }

    [TestMethod]
    [Spec("css-namespaces-3", "#syntax")]
    public void MisplacedNamespaceRule_IsIgnored()
    {
        Assert.AreEqual(Green, Styled($"#y {{ }} @namespace url({Svg}); div#x {{ color: green }}").Color);
    }

    [TestMethod]
    [Spec("css-namespaces-3", "#css-qnames")]
    public void AttributeWithoutANamespace_Matches()
    {
        Assert.AreEqual(Green, Styled("#x[|title] { color: green }").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-namespaces-3.yml")]
    [Spec("css-namespaces-3", "#css-qnames")]
    [DataRow("*|div#x")]
    [DataRow("#x[*|title]")]
    public void AnyNamespace_Matches(string selector)
    {
        Assert.AreEqual(Green, Styled($"{selector} {{ color: green }}").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-namespaces-3.yml")]
    [Spec("css-namespaces-3", "#css-qnames")]
    public void UndeclaredPrefix_InvalidatesTheSelector()
    {
        Assert.AreEqual(Green, Styled("#x { color: green } foo|div#x, #x { color: red }").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-namespaces-3.yml")]
    [Spec("css-namespaces-3", "#css-qnames")]
    public void ElementWithoutANamespace_IsntAnHtmlElement()
    {
        Assert.AreEqual(Green, Styled("#x { color: green } |div#x { color: red }").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-namespaces-3.yml")]
    [Spec("css-namespaces-3", "#css-qnames")]
    [Spec("css-namespaces-3", "#prefixes")]
    [Spec("html", "infrastructure.html#xml")]
    public void DeclaredPrefix_MatchesItsNamespace()
    {
        Assert.AreEqual(Green, Styled($"@namespace h url({Html}); h|div#x {{ color: green }}").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-namespaces-3.yml")]
    [Spec("css-namespaces-3", "#prefixes")]
    public void DefaultNamespace_AppliesToTypeSelectors()
    {
        Assert.AreEqual(Green, Styled($"@namespace url({Svg}); #x {{ color: green }} div#x {{ color: red }}").Color);
    }

    private static CssBox Styled(string css)
    {
        var (root, _) = LayoutHarness.Layout($"<html><head><style>{css}</style></head><body style='margin:0'><div id=x title=t>a</div></body></html>");
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box;
    }
}
