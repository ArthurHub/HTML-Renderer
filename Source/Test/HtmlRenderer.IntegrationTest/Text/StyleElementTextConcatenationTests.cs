using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Text;

// This fork's CssParser keeps a process-wide, non-thread-safe regex cache
// (RegexParserUtils.GetRegex's static Dictionary) that SetHtml/DefaultCssData populate lazily on first use per
// AppDomain; running HtmlContainerInt.SetHtml from more than one thread at once (as MSTestSettings.cs's
// assembly-wide [Parallelize(Scope = ExecutionScope.MethodLevel)] does by default) can corrupt it and throw
// "A concurrent update was performed on this collection". [DoNotParallelize] avoids tripping that pre-existing
// library race rather than masking it.
[DoNotParallelize]
[TestClass]
public sealed class StyleElementTextConcatenationTests
{
    [TestMethod]
    [Spec("html", "semantics.html#the-style-element")]
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
        Assert.AreEqual("green", b.Color);
    }
}
