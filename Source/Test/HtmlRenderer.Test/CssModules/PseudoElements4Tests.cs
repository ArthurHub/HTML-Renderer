using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class PseudoElements4Tests
{
    private static readonly RColor Red = RColor.FromArgb(255, 255, 0, 0);
    private static readonly RColor Green = RColor.FromArgb(255, 0, 255, 0);

    [TestMethod]
    [Spec("css-pseudo-4", "#first-letter-pseudo")]
    [Spec("css-pseudo-4", "#first-line-pseudo")]
    [DataRow("p::first-letter, p")]
    [DataRow("p::first-line, p")]
    public void FirstLetterAndFirstLine_AreValidSelectors(string selector)
    {
        Assert.AreEqual(Red, Box($"{selector} {{ color: #ff0000 }}", "<p id='x'>a</p>").ActualColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-pseudo-4.yml")]
    [Spec("css-pseudo-4", "#first-letter-pseudo")]
    [DataRow("p::first-letter::prefix, p")]
    [DataRow("p::first-letter::suffix, p")]
    public void PrefixAndSuffix_AreValidSubPseudoElementsOfFirstLetter(string selector)
    {
        Assert.AreEqual(Red, Box($"{selector} {{ color: #ff0000 }}", "<p id='x'>a</p>").ActualColor);
    }

    [TestMethod]
    [Spec("css-pseudo-4", "#highlight-selectors")]
    [Spec("css-pseudo-4", "#highlight-styling")]
    public void Selection_SetsTheSelectionColors()
    {
        var (_, container) = LayoutHarness.Layout(
            Document("::selection { color: #ff0000; background-color: #00ff00 }", "<p id='x'>a</p>"));

        Assert.AreEqual(Red, container.SelectionForeColor);
        Assert.AreEqual(Green, container.SelectionBackColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-pseudo-4.yml")]
    [Spec("css-pseudo-4", "#highlight-selectors")]
    [DataRow("p::search-text, p")]
    [DataRow("p::target-text, p")]
    [DataRow("p::spelling-error, p")]
    [DataRow("p::grammar-error, p")]
    [DataRow("p::highlight(a), p")]
    public void OtherHighlightPseudoElements_AreValidSelectors(string selector)
    {
        Assert.AreEqual(Red, Box($"{selector} {{ color: #ff0000 }}", "<p id='x'>a</p>").ActualColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-pseudo-4.yml")]
    [Spec("css-pseudo-4", "#generated-content")]
    public void ReplacedElement_GetsNoBeforeOrAfter()
    {
        var (root, _) = LayoutHarness.Layout(Document("img::before, img::after { content: 'N' }",
            "<img id='x' src='x.png'>"));

        Assert.AreEqual("", GeneratedText(LayoutHarness.FindById(root, "x")!));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-pseudo-4.yml")]
    [Spec("css-pseudo-4", "#marker-pseudo")]
    public void Marker_IsStyledByMarkerRules()
    {
        var li = Box("li::marker { color: #ff0000 } li { color: #00ff00 }", "<ul><li id='x'>a</li></ul>");

        Assert.IsNotNull(li.ListItemBox);
        Assert.AreEqual(Red, li.ListItemBox.ActualColor);
    }

    [TestMethod]
    [Spec("css-pseudo-4", "#marker-pseudo")]
    public void Marker_IsAValidSelector()
    {
        Assert.AreEqual(Red, Box("li::marker, li { color: #ff0000 }", "<ul><li id='x'>a</li></ul>").ActualColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-pseudo-4.yml")]
    [Spec("css-pseudo-4", "#placeholder-pseudo")]
    [Spec("css-pseudo-4", "#file-selector-button-pseudo")]
    [Spec("css-pseudo-4", "#details-content-pseudo")]
    [DataRow("input::placeholder, p")]
    [DataRow("input::file-selector-button, p")]
    [DataRow("details::details-content, p")]
    public void FormAndDetailsPseudoElements_AreValidSelectors(string selector)
    {
        Assert.AreEqual(Red, Box($"{selector} {{ color: #ff0000 }}", "<p id='x'>a</p>").ActualColor);
    }

    private static string Document(string css, string body) =>
        $"<!DOCTYPE html><html><head><style>{css}</style></head><body>{body}</body></html>";

    private static CssBox Box(string css, string body)
    {
        var (root, _) = LayoutHarness.Layout(Document(css, body));
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box;
    }

    private static string GeneratedText(CssBox box) =>
        string.Concat(box.Boxes.Where(b => b.IsPseudoElement)
            .SelectMany(LayoutHarness.Descendants).SelectMany(b => b.Words).Select(w => w.Text.Trim()));
}
