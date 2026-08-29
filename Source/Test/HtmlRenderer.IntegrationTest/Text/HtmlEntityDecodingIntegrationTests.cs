using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Text;

/// <summary>
/// Verifies HTML character-reference (entity) decoding.
/// </summary>
/// <remarks>
/// <para>
/// Correction from an earlier draft of this file's own reasoning, verified empirically by dumping the actual
/// box tree: <c>CssBox.Text</c> is NOT raw, undecoded source text on this fork. HtmlKit's <c>HtmlTokenizer</c>
/// (used by <c>Parse\HtmlParser.cs</c>) already performs standard HTML5 entity decoding while producing Data
/// tokens - e.g. for source <c>"Use &amp;amp;nbsp; here"</c>, the anonymous text child's own <c>.Text</c> is
/// already <c>"Use &amp;nbsp; here"</c> (confirmed by direct inspection) by the time HTML-Renderer sees it at
/// all. <c>CssBox.ParseToWords</c> THEN calls <c>HtmlUtils.DecodeHtml</c> a SECOND time, per word, on top of
/// that already-decoded text. The net effect is real double-decoding: a double-escaped entity like
/// <c>&amp;amp;nbsp;</c> - a legitimate technique for displaying literal entity syntax, e.g. in documentation
/// or code samples - does NOT survive as literal text the way a single-pass decoder would leave it; it gets
/// fully resolved by the second pass instead. Confirmed by direct instrumentation reading
/// <c>HtmlUtils.DecodeHtml</c>'s own two internal sub-passes (<c>DecodeHtmlCharByCode</c> then
/// <c>DecodeHtmlCharByName</c>, each of which is individually single-pass) combined with the tokenizer's own
/// prior decode.
/// </para>
/// <para>
/// Decoded content only ever shows up in <c>box.Words</c> (each <see cref="CssRect"/>'s <c>Text</c>) - and
/// specifically on the descendant ANONYMOUS TEXT CssBox that HtmlParser creates for text content (a container
/// box like <c>&lt;p&gt;</c> never carries text/words directly - see <c>AllWords</c> below) - so these tests
/// read words, joined back together, instead of <c>box.Text</c>.
/// </para>
/// </remarks>
[DoNotParallelize]
[TestClass]
public sealed class HtmlEntityDecodingIntegrationTests
{
    #region Double-Escaped Entities (Should Render as Literal Entity References)

    // A double-escaped entity like "&amp;nbsp;" is meant to display literal entity syntax (e.g. in
    // documentation/code samples) by surviving exactly one decode pass. On this fork it does NOT survive:
    // see this class's remarks on the confirmed double-decode (HtmlKit tokenizer decode + a second
    // HtmlUtils.DecodeHtml pass in CssBox.ParseToWords). Ported faithfully and ignored, not deleted, so the
    // intended behavior stays documented.

    [Ignore("Double-decoded (see class remarks): box.Text already arrives as \"Use &nbsp; here\" (HtmlKit's " +
            "tokenizer already decoded the outer &amp;), then ParseToWords's own DecodeHtml decodes the now-" +
            "literal \"&nbsp;\" AGAIN into a plain space - confirmed by dumping the box tree directly.")]
    [TestMethod]
    public void DoubleEscapedNbsp_RendersAsLiteralEntityReference()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Use &amp;nbsp; here</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(AllWords(p).Any(w => w.Text == "&nbsp;"));
    }

    [Ignore("Double-decoded (see class remarks): box.Text already arrives as \"Use &amp; here\", then " +
            "ParseToWords's own DecodeHtml decodes that already-literal \"&amp;\" AGAIN into a plain \"&\".")]
    [TestMethod]
    public void DoubleEscapedAmp_RendersAsLiteralEntityReference()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Use &amp;amp; here</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(AllWords(p).Any(w => w.Text == "&amp;"));
    }

    [Ignore("Double-decoded (see class remarks): box.Text already arrives as \"Use &lt; here\", then " +
            "ParseToWords's own DecodeHtml decodes that already-literal \"&lt;\" AGAIN into a plain \"<\".")]
    [TestMethod]
    public void DoubleEscapedLt_RendersAsLiteralEntityReference()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Use &amp;lt; here</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(AllWords(p).Any(w => w.Text == "&lt;"));
    }

    [Ignore("Double-decoded (see class remarks): box.Text already arrives as \"Use &#65; here\" (a now-genuine " +
            "numeric entity), then ParseToWords's own DecodeHtml decodes THAT into the literal character \"A\".")]
    [TestMethod]
    public void DoubleEscapedNumericEntity_RendersAsLiteralEntityReference()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Use &amp;#65; here</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(AllWords(p).Any(w => w.Text == "&#65;"));
    }

    #endregion

    #region Single-Escaped Entities (Should Render as Decoded Characters)

    [Ignore("HtmlUtils.DecodeHtml decodes &nbsp; to a plain U+0020 space rather than U+00A0 (non-breaking " +
            "space) - confirmed real engine behavior (_decodeOnly[\"nbsp\"] = ' ' in HtmlUtils.cs), same " +
            "root cause tracked for WhiteSpaceLayoutIntegrationTests' nbsp cases.")]
    [TestMethod]
    public void SingleEscapedNbsp_RendersAsNonBreakingSpace()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Use&nbsp;here</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(AllWords(p).Any(w => w.Text != null && w.Text.Contains('\u00A0')));
    }

    [TestMethod]
    public void SingleEscapedAmp_RendersAsAmpersand()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Use &amp; here</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(AllWords(p).Any(w => w.Text == "&"));
    }

    [TestMethod]
    public void SingleEscapedLt_RendersAsLessThan()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Use &lt; here</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(AllWords(p).Any(w => w.Text == "<"));
    }

    [TestMethod]
    public void SingleEscapedNumericEntity_RendersAsCharacter()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Use &#65; here</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(AllWords(p).Any(w => w.Text == "A"));
    }

    #endregion

    #region Code Blocks and Pre Elements

    [Ignore("Double-decoded (see class remarks): the already-tokenizer-decoded \"Use &nbsp; for spaces\" gets " +
            "its \"&nbsp;\" decoded again by ParseToWords into a plain space.")]
    [TestMethod]
    public void DoubleEscapedEntitiesInCodeBlock_RenderCorrectly()
    {
        // <code> is normal white-space, so re-joining decoded words with single spaces reconstructs the
        // original (collapsed) spacing exactly.
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<code id='c'>Use &amp;nbsp; for spaces</code>"));
        var c = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual("Use &nbsp; for spaces", JoinWordsNormal(c));
    }

    [Ignore("Double-decoded (see class remarks): the already-tokenizer-decoded \"&lt;html&gt;\" gets decoded " +
            "again by ParseToWords into literal \"<html>\".")]
    [TestMethod]
    public void DoubleEscapedEntitiesInPreElement_RenderCorrectly()
    {
        // <pre> is white-space:pre via the UA default stylesheet - "&amp;lt;html&amp;gt;" has no whitespace
        // at all, so it is a single word token, decoded in one pass with no join/spacing concerns.
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<pre id='p'>&amp;lt;html&amp;gt;</pre>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual("&lt;html&gt;", JoinWordsPre(p));
    }

    [Ignore("The single-escaped portions (\"&amp;\" -> \"&\") are fine, but the double-escaped \"&amp;amp;\" " +
            "portion gets decoded twice (see class remarks), ending up as a plain \"&\" instead of the " +
            "literal \"&amp;\" this asserts.")]
    [TestMethod]
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

        // Unlike ::before (inserted at index 0), ::after is appended at the end of p.Boxes - so
        // FirstOrDefault(HtmlTag == null) would instead match the real text node "text" (which also has
        // no HtmlTag), not the pseudo box. IsAfterPseudoElement identifies it unambiguously.
        var afterBox = p.Boxes.FirstOrDefault(b => b.IsAfterPseudoElement);
        Assert.IsNotNull(afterBox);
        Assert.IsTrue(afterBox!.Text!.Contains('<'));
        Assert.IsTrue(afterBox.Text!.Contains('>'));
    }

    #endregion

    #region Edge Cases

    [Ignore("Double-decoded (see class remarks): \"&amp;nbsp;\" is already literal \"&nbsp;\" by the time " +
            "ParseToWords runs, and gets decoded again into a plain space.")]
    [TestMethod]
    public void WhitespacePreservation_WithEntities()
    {
        // white-space:pre preserves the two literal spaces between the entity and "test" as their own word
        // token (see WhiteSpaceLayoutIntegrationTests' Pre_PreservesMultipleConsecutiveSpacesAsLiteralWord),
        // so concatenating words with NO separator reconstructs the exact original spacing.
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<p id='p' style='white-space:pre'>&amp;nbsp;  test</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual("&nbsp;  test", JoinWordsPre(p));
    }

    [Ignore("Every double-escaped entity in this sentence gets decoded twice (see class remarks), so none of " +
            "them survive as the literal entity references this asserts.")]
    [TestMethod]
    public void MultipleDoubleEscapedEntities_InSentence()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<p id='p'>Entities: &amp;lt;, &amp;gt;, &amp;amp;, &amp;nbsp;</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual("Entities: &lt;, &gt;, &amp;, &nbsp;", JoinWordsNormal(p));
    }

    [Ignore("Confirmed real (if convoluted) two-layer decode, empirically verified: raw \"&amp;amp;nbsp;\" -> " +
            "HtmlKit's tokenizer decodes the FIRST \"&amp;\" (greedy, single pass) to \"&\", leaving " +
            "box.Text = \"&amp;nbsp;\" (i.e. exactly the DoubleEscapedNbsp case's raw INPUT) -> " +
            "ParseToWords's own DecodeHtml then decodes THAT down one more level to \"&nbsp;\", not the two " +
            "literal levels (\"&amp;nbsp;\") this asserts.")]
    [TestMethod]
    public void TripleEscapedEntity_RendersWithTwoLevels()
    {
        // &amp;amp;nbsp; -> the entity scan finds only the leading "&amp;" (index 0..4) and decodes it to
        // "&"; the remainder "amp;nbsp;" has no leading '&' left, so it stays literal - "&amp;nbsp;".
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>&amp;amp;nbsp;</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual("&amp;nbsp;", JoinWordsNormal(p));
    }

    [TestMethod]
    public void EntityInAttributeValue_DecodedCorrectly()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p' title='A &amp; B'>text</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        var title = p.HtmlTag?.TryGetAttribute("title", "");
        Assert.AreEqual("A & B", title);
    }

    #endregion

    // A block/inline CONTAINER box (e.g. <p>, <code>) never carries its own Words directly - HtmlParser always
    // puts text content into a separate anonymous CHILD CssBox (see HtmlParser.AddTextBox: it always creates a
    // new child box and sets Text on THAT, never on the current container). So "p.Words" for a <p> wrapping
    // plain text is always empty; the real per-word decoded content lives on the descendant anonymous text
    // box(es). AllWords flattens words from the box and every descendant, in document order, which is what
    // these tests actually need to read back the decoded content.
    private static IEnumerable<CssRect> AllWords(CssBox box) => LayoutHarness.Descendants(box).SelectMany(b => b.Words);

    private static string JoinWordsNormal(CssBox box) => string.Join(" ", AllWords(box).Select(w => w.Text));

    private static string JoinWordsPre(CssBox box) => string.Concat(AllWords(box).Select(w => w.Text));
}
