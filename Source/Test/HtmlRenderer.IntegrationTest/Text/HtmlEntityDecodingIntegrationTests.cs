using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Text;

/// <summary>
/// Character references as they end up in the rendered words. The text is decoded once by the tokenizer and
/// a second time when it's split into words, so an escaped reference such as <c>&amp;amp;lt;</c> doesn't
/// survive as literal text.
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class HtmlEntityDecodingIntegrationTests
{
    #region Double-Escaped Entities (Should Render as Literal Entity References)

    [Ignore("Gap, see Spec/html.yml")]
    [TestMethod]
    [Spec("html", "parsing.html#character-reference-state")]
    public void DoubleEscapedNbsp_RendersAsLiteralEntityReference()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Use &amp;nbsp; here</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(AllWords(p).Any(w => w.Text == "&nbsp;"));
    }

    [Ignore("Gap, see Spec/html.yml")]
    [TestMethod]
    [Spec("html", "parsing.html#character-reference-state")]
    public void DoubleEscapedAmp_RendersAsLiteralEntityReference()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Use &amp;amp; here</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(AllWords(p).Any(w => w.Text == "&amp;"));
    }

    [Ignore("Gap, see Spec/html.yml")]
    [TestMethod]
    [Spec("html", "parsing.html#character-reference-state")]
    public void DoubleEscapedLt_RendersAsLiteralEntityReference()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Use &amp;lt; here</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(AllWords(p).Any(w => w.Text == "&lt;"));
    }

    [Ignore("Gap, see Spec/html.yml")]
    [TestMethod]
    [Spec("html", "parsing.html#character-reference-state")]
    public void DoubleEscapedNumericEntity_RendersAsLiteralEntityReference()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Use &amp;#65; here</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(AllWords(p).Any(w => w.Text == "&#65;"));
    }

    #endregion

    #region Single-Escaped Entities (Should Render as Decoded Characters)

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#white-space-model")]
    public void SingleEscapedNbsp_RendersAsNonBreakingSpace()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Use&nbsp;here</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(AllWords(p).Any(w => w.Text != null && w.Text.Contains('\u00A0')));
    }

    [TestMethod]
    [Spec("html", "parsing.html#character-reference-state")]
    public void SingleEscapedAmp_RendersAsAmpersand()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Use &amp; here</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(AllWords(p).Any(w => w.Text == "&"));
    }

    [TestMethod]
    [Spec("html", "parsing.html#character-reference-state")]
    public void SingleEscapedLt_RendersAsLessThan()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Use &lt; here</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(AllWords(p).Any(w => w.Text == "<"));
    }

    [TestMethod]
    [Spec("html", "parsing.html#character-reference-state")]
    public void SingleEscapedNumericEntity_RendersAsCharacter()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Use &#65; here</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(AllWords(p).Any(w => w.Text == "A"));
    }

    #endregion

    #region Code Blocks and Pre Elements

    [Ignore("Gap, see Spec/html.yml")]
    [TestMethod]
    [Spec("html", "parsing.html#character-reference-state")]
    public void DoubleEscapedEntitiesInCodeBlock_RenderCorrectly()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<code id='c'>Use &amp;nbsp; for spaces</code>"));
        var c = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual("Use &nbsp; for spaces", JoinWordsNormal(c));
    }

    [Ignore("Gap, see Spec/html.yml")]
    [TestMethod]
    [Spec("html", "parsing.html#character-reference-state")]
    public void DoubleEscapedEntitiesInPreElement_RenderCorrectly()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<pre id='p'>&amp;lt;html&amp;gt;</pre>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual("&lt;html&gt;", JoinWordsPre(p));
    }

    [Ignore("Gap, see Spec/html.yml")]
    [TestMethod]
    [Spec("html", "parsing.html#character-reference-state")]
    public void MixedEntitiesInParagraph_RenderCorrectly()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>A &amp; B &amp;amp; C</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual("A & B &amp; C", JoinWordsNormal(p));
    }

    #endregion

    #region CSS Content Strings

    [TestMethod]
    public void CssContentWithCssEscape_RendersLiterally()
    {
        const string html = "<html><head><style>p::before { content: \"\\26\"; }</style></head>" +
                             "<body><p id='p'>text</p></body></html>";
        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;

        var beforeBox = p.Boxes.FirstOrDefault(b => b.HtmlTag == null && b.Text != null);
        Assert.IsNotNull(beforeBox);
        Assert.AreEqual("&", beforeBox!.Text);
    }

    [TestMethod]
    public void CssContentWithCssEscapeInString_RendersLiterally()
    {
        const string html = "<html><head><style>p::after { content: \" \\3C tag\\3E \"; }</style></head>" +
                             "<body><p id='p'>text</p></body></html>";
        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;

        var afterBox = p.Boxes.FirstOrDefault(b => b.IsAfterPseudoElement);
        Assert.IsNotNull(afterBox);
        Assert.IsTrue(afterBox!.Text!.Contains('<'));
        Assert.IsTrue(afterBox.Text!.Contains('>'));
    }

    #endregion

    #region Edge Cases

    [Ignore("Gap, see Spec/html.yml")]
    [TestMethod]
    [Spec("html", "parsing.html#character-reference-state")]
    public void WhitespacePreservation_WithEntities()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<p id='p' style='white-space:pre'>&amp;nbsp;  test</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual("&nbsp;  test", JoinWordsPre(p));
    }

    [Ignore("Gap, see Spec/html.yml")]
    [TestMethod]
    [Spec("html", "parsing.html#character-reference-state")]
    public void MultipleDoubleEscapedEntities_InSentence()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<p id='p'>Entities: &amp;lt;, &amp;gt;, &amp;amp;, &amp;nbsp;</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual("Entities: &lt;, &gt;, &amp;, &nbsp;", JoinWordsNormal(p));
    }

    [Ignore("Gap, see Spec/html.yml")]
    [TestMethod]
    [Spec("html", "parsing.html#character-reference-state")]
    public void TripleEscapedEntity_RendersWithTwoLevels()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>&amp;amp;nbsp;</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual("&amp;nbsp;", JoinWordsNormal(p));
    }

    [TestMethod]
    [Spec("html", "parsing.html#character-reference-state")]
    public void EntityInAttributeValue_DecodedCorrectly()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p' title='A &amp; B'>text</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        var title = p.HtmlTag?.TryGetAttribute("title", "");
        Assert.AreEqual("A & B", title);
    }

    #endregion

    // Text sits in anonymous child boxes, so the words are collected from the whole subtree.
    private static IEnumerable<CssRect> AllWords(CssBox box) => LayoutHarness.Descendants(box).SelectMany(b => b.Words);

    private static string JoinWordsNormal(CssBox box) => string.Join(" ", AllWords(box).Select(w => w.Text));

    private static string JoinWordsPre(CssBox box) => string.Concat(AllWords(box).Select(w => w.Text));
}
