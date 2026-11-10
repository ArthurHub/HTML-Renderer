using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Content;

[TestClass]
public sealed class GeneratedContentModuleTests
{
    [TestMethod]
    [Ignore("Gap, see Spec/css-content-3.yml")]
    [Spec("css-content-3", "#content-property")]
    public void ImageOnAnElement_ReplacesItsContents()
    {
        var words = LayoutHarness.Descendants(Box("<p id=x style='content:url(data:image/png;base64,iVBORw0KGgo=)'>abc</p>"))
            .SelectMany(b => b.Words).ToList();

        Assert.IsTrue(words.Any(w => w.IsImage));
        Assert.IsFalse(words.Any(w => w.Text == "abc"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-content-3.yml")]
    [Spec("css-content-3", "#alt")]
    public void AltText_LeavesTheVisibleContent()
    {
        Assert.AreEqual("a", Before("<p id=x>b</p><style>#x::before { content: 'a' / 'alt' }</style>").Text);
    }

    [TestMethod]
    [Spec("css-content-3", "#strings")]
    public void WhiteSpaceInStrings_CollapsesAcrossStrings()
    {
        var before = Before("<p id=x>a</p><style>#x::before { content: 'First  ' '  Second' }</style>");

        CollectionAssert.AreEqual(new[] { "First", "Second" }, before.Words.Select(w => w.Text).ToArray());
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-content-3.yml")]
    [Spec("css-content-3", "#quotes-property")]
    [DataRow("auto")]
    [DataRow("match-parent")]
    public void QuotesKeyword_IsAccepted(string keyword)
    {
        Assert.IsTrue(ParseDeclaration($"quotes: {keyword}").HasValue);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-content-3.yml")]
    [Spec("css-content-3", "#string-set")]
    [Spec("css-content-3", "#string-function")]
    public void StringFunction_InsertsTheNamedString()
    {
        var html = "<h1 style='string-set: title \"Loomings\"'>a</h1><p id=x>b</p><style>#x::before { content: '[' string(title) ']' }</style>";

        Assert.AreEqual("[Loomings]", Before(html).Text);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-content-3.yml")]
    [Spec("css-content-3", "#content-function")]
    public void ContentFunction_InsertsTheElementText()
    {
        Assert.AreEqual("[abc]", Before("<p id=x>abc</p><style>#x::before { content: '[' content(text) ']' }</style>").Text);
    }

    private static CssBox Before(string body, string id = "x")
    {
        var before = Box(body, id).Boxes.FirstOrDefault(b => b.IsBeforePseudoElement);
        Assert.IsNotNull(before, $"No ::before on '{id}'");
        return before;
    }
}
