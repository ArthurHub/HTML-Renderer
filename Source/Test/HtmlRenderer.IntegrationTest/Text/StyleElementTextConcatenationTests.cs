using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Text;

// RegexParserUtils caches its regexes in a static Dictionary, parallel SetHtml calls can corrupt it
[DoNotParallelize]
[TestClass]
public sealed class StyleElementTextConcatenationTests
{
    [TestMethod]
    [Spec("html", "semantics.html#the-style-element")]
    [Spec("html", "parsing.html#creating-and-inserting-nodes")]
    public void RuleAfterLessThanInDeclaration_StillApplies()
    {
        const string html = """
            <html><head><style>
              #a { --x: "<"; }
              #b { color: green; }
            </style></head><body style='margin:0'>
            <div id="a">a</div><div id="b">b</div>
            </body></html>
            """;

        var (root, _) = LayoutHarness.Layout(html);
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.IsNotNull(b);
        Assert.AreEqual("rgb(0, 128, 0)", b.Color);
    }
}
