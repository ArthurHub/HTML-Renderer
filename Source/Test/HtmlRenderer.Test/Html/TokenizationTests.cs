using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class TokenizationTests
{
    [TestMethod]
    [Spec("html", "parsing.html#preprocessing-the-input-stream")]
    public void CrLf_InPre_BreaksTheLine()
    {
        Assert.AreEqual("a\nb", Words(Box("<pre id=x>a\r\nb</pre>")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#preprocessing-the-input-stream")]
    public void CarriageReturns_AreNormalizedToLineFeeds()
    {
        Assert.AreEqual("a\nb\nc", Text(Box("<p id=x>a\r\nb\rc</p>")));
        Assert.AreEqual("a\nb", Words(Box("<pre id=x>a\rb</pre>")));
    }

    [TestMethod]
    [Spec("html", "parsing.html#data-state")]
    [Spec("html", "parsing.html#tag-open-state")]
    [Spec("html", "parsing.html#end-tag-open-state")]
    [Spec("html", "parsing.html#tag-name-state")]
    public void LessThanSign_ThatDoesNotStartATag_IsText()
    {
        Assert.AreEqual("a < b <3 de", Text(Box("<p id=x>a < b <3 </ c>d</>e</p>")));
    }

    [TestMethod]
    [Spec("html", "parsing.html#tag-name-state")]
    public void TagNames_AreCaseInsensitive()
    {
        var p = Box("<P id=x><SPAN>a</Span></p >");
        Assert.AreEqual("block", p.Display);
        Assert.AreEqual("a", Text(p));
    }

    [TestMethod]
    [Spec("html", "parsing.html#before-attribute-name-state")]
    [Spec("html", "parsing.html#attribute-name-state")]
    [Spec("html", "parsing.html#after-attribute-name-state")]
    [Spec("html", "parsing.html#before-attribute-value-state")]
    [Spec("html", "parsing.html#attribute-value-(double-quoted)-state")]
    [Spec("html", "parsing.html#attribute-value-(single-quoted)-state")]
    [Spec("html", "parsing.html#attribute-value-(unquoted)-state")]
    [Spec("html", "parsing.html#after-attribute-value-(quoted)-state")]
    public void Attributes_AreParsed()
    {
        var tag = Box("<p id=x a=1 b = 2 c='3'd=\"4\" e f=\"\" g=\"a\nb\" class=\"c1\" class=c2>h</p>").HtmlTag!;
        Assert.AreEqual("1", tag.TryGetAttribute("a"));
        Assert.AreEqual("2", tag.TryGetAttribute("b"));
        Assert.AreEqual("3", tag.TryGetAttribute("c"));
        Assert.AreEqual("4", tag.TryGetAttribute("d"));
        Assert.IsTrue(tag.HasAttribute("e"));
        Assert.AreEqual("", tag.TryGetAttribute("f"));
        Assert.AreEqual("a\nb", tag.TryGetAttribute("g"));
        Assert.AreEqual("c1", tag.TryGetAttribute("class"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#attribute-name-state")]
    public void AttributeNames_AreLowercased()
    {
        var tag = Box("<p id=x CLASS=a title=b TITLE=c>d</p>").HtmlTag!;
        Assert.AreEqual("a", tag.TryGetAttribute("class"));
        Assert.AreEqual(2, tag.Attributes.Count - 1);
    }

    [TestMethod]
    [Spec("html", "parsing.html#self-closing-start-tag-state")]
    public void SelfClosingSlash_OnAVoidElement_IsAccepted()
    {
        var p = Box("<p id=x>a<input/>b<img / >c</p>");
        Assert.IsTrue(p.Boxes.Any(b => b.HtmlTag?.Name == "input"));
        Assert.AreEqual("abc", Text(p));
    }

    [TestMethod]
    [Spec("html", "parsing.html#markup-declaration-open-state")]
    [Spec("html", "parsing.html#comment-start-state")]
    [Spec("html", "parsing.html#comment-start-dash-state")]
    [Spec("html", "parsing.html#comment-state")]
    [Spec("html", "parsing.html#comment-less-than-sign-state")]
    [Spec("html", "parsing.html#comment-less-than-sign-bang-state")]
    [Spec("html", "parsing.html#comment-less-than-sign-bang-dash-state")]
    [Spec("html", "parsing.html#comment-less-than-sign-bang-dash-dash-state")]
    [Spec("html", "parsing.html#comment-end-dash-state")]
    [Spec("html", "parsing.html#comment-end-state")]
    [Spec("html", "parsing.html#comment-end-bang-state")]
    public void Comments_AreNotRendered()
    {
        Assert.AreEqual("abcdefgh", Text(Box(
            "<p id=x>a<!-- <b>c</b> -->b<!---->c<!--->d<!-->e<!-- <!-- x -->f<!-- a --!>g<!-- a -- b --->h</p>")));
    }

    [TestMethod]
    [Spec("html", "parsing.html#bogus-comment-state")]
    [Spec("html", "parsing.html#processing-instruction-open-state")]
    [Spec("html", "parsing.html#processing-instruction-target-state")]
    [Spec("html", "parsing.html#after-processing-instruction-target-state")]
    [Spec("html", "parsing.html#processing-instruction-data-state")]
    [Spec("html", "parsing.html#processing-instruction-questionable-state")]
    public void BogusCommentsAndProcessingInstructions_AreNotRendered()
    {
        Assert.AreEqual("abcdef", Text(Box(
            "<p id=x>a<!x>b<?php echo 1 ?>c<?xml version=\"1.0\"?>d</ b>e<![CDATA[ x<y ]]>f</p>")));
    }

    [TestMethod]
    [Spec("html", "parsing.html#doctype-state")]
    [Spec("html", "parsing.html#before-doctype-name-state")]
    [Spec("html", "parsing.html#doctype-name-state")]
    [Spec("html", "parsing.html#after-doctype-name-state")]
    [Spec("html", "parsing.html#after-doctype-public-keyword-state")]
    [Spec("html", "parsing.html#before-doctype-public-identifier-state")]
    [Spec("html", "parsing.html#doctype-public-identifier-(double-quoted)-state")]
    [Spec("html", "parsing.html#doctype-public-identifier-(single-quoted)-state")]
    [Spec("html", "parsing.html#after-doctype-public-identifier-state")]
    [Spec("html", "parsing.html#between-doctype-public-and-system-identifiers-state")]
    [Spec("html", "parsing.html#after-doctype-system-keyword-state")]
    [Spec("html", "parsing.html#before-doctype-system-identifier-state")]
    [Spec("html", "parsing.html#doctype-system-identifier-(double-quoted)-state")]
    [Spec("html", "parsing.html#doctype-system-identifier-(single-quoted)-state")]
    [Spec("html", "parsing.html#after-doctype-system-identifier-state")]
    [Spec("html", "parsing.html#bogus-doctype-state")]
    [DataRow("<!DOCTYPE html>")]
    [DataRow("<!doctype HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">")]
    [DataRow("<!DOCTYPE html PUBLIC '-//W3C//DTD HTML 4.01//EN' 'x'>")]
    [DataRow("<!DOCTYPE html SYSTEM \"about:legacy-compat\">")]
    [DataRow("<!DOCTYPE html SYSTEM 'about:legacy-compat'>")]
    [DataRow("<!DOCTYPE>")]
    [DataRow("<!DOCTYPE html bogus \"x\">")]
    public void Doctype_IsNotRendered(string doctype)
    {
        var (root, _) = LayoutHarness.Layout(doctype + LayoutHarness.Wrap("a"));
        Assert.AreEqual("a", Text(root));
    }

    [TestMethod]
    [Spec("html", "parsing.html#rcdata-state")]
    [Spec("html", "parsing.html#rcdata-less-than-sign-state")]
    [Spec("html", "parsing.html#rcdata-end-tag-open-state")]
    [Spec("html", "parsing.html#rcdata-end-tag-name-state")]
    public void Rcdata_KeepsTagsAndDecodesReferences()
    {
        Assert.AreEqual("a</b> < </textareax> ", Text(Box("<textarea id=x>a</b> &lt; </textareax> </TEXTAREA>")));
    }

    [TestMethod]
    [Spec("html", "parsing.html#rawtext-state")]
    [Spec("html", "parsing.html#rawtext-less-than-sign-state")]
    [Spec("html", "parsing.html#rawtext-end-tag-open-state")]
    [Spec("html", "parsing.html#rawtext-end-tag-name-state")]
    public void Rawtext_KeepsTagsAndReferences()
    {
        Assert.AreEqual("a</b> &lt; </xmpx> ", Text(Box("<xmp id=x>a</b> &lt; </xmpx> </XMP>")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#rawtext-state")]
    public void Rawtext_RendersReferencesLiterally()
    {
        Assert.AreEqual("&lt;", Words(Box("<xmp id=x>&lt;</xmp>")));
    }

    [TestMethod]
    [Spec("html", "parsing.html#script-data-state")]
    [Spec("html", "parsing.html#script-data-less-than-sign-state")]
    [Spec("html", "parsing.html#script-data-end-tag-open-state")]
    [Spec("html", "parsing.html#script-data-end-tag-name-state")]
    [Spec("html", "parsing.html#script-data-escape-start-state")]
    [Spec("html", "parsing.html#script-data-escape-start-dash-state")]
    [Spec("html", "parsing.html#script-data-escaped-state")]
    [Spec("html", "parsing.html#script-data-escaped-dash-state")]
    [Spec("html", "parsing.html#script-data-escaped-dash-dash-state")]
    [Spec("html", "parsing.html#script-data-escaped-less-than-sign-state")]
    [Spec("html", "parsing.html#script-data-escaped-end-tag-open-state")]
    [Spec("html", "parsing.html#script-data-escaped-end-tag-name-state")]
    [Spec("html", "parsing.html#script-data-double-escape-start-state")]
    [Spec("html", "parsing.html#script-data-double-escaped-state")]
    [Spec("html", "parsing.html#script-data-double-escaped-dash-state")]
    [Spec("html", "parsing.html#script-data-double-escaped-dash-dash-state")]
    [Spec("html", "parsing.html#script-data-double-escaped-less-than-sign-state")]
    [Spec("html", "parsing.html#script-data-double-escape-end-state")]
    public void ScriptData_EndsAtTheScriptEndTag()
    {
        Assert.AreEqual("abcd", Text(Box(
            "<p id=x>a<script>if (a<b) x = \"</p>\";</script>b<script><!--<script>x</script>y-->z</script>c<script><!-- x </script>d</p>")));
    }

    [TestMethod]
    [Spec("html", "parsing.html#plaintext-state")]
    public void Plaintext_RunsToTheEndOfTheInput()
    {
        var (root, _) = LayoutHarness.Layout("<plaintext id=x>a <b></plaintext>");
        Assert.AreEqual("a <b></plaintext>", Text(LayoutHarness.FindById(root, "x")!));
    }

    [TestMethod]
    [Spec("html", "parsing.html#character-reference-state")]
    [Spec("html", "parsing.html#named-character-reference-state")]
    [Spec("html", "parsing.html#ambiguous-ampersand-state")]
    [Spec("html", "parsing.html#numeric-character-reference-state")]
    [Spec("html", "parsing.html#hexadecimal-character-reference-start-state")]
    [Spec("html", "parsing.html#hexadecimal-character-reference-state")]
    [Spec("html", "parsing.html#decimal-character-reference-state")]
    [Spec("html", "parsing.html#numeric-character-reference-end-state")]
    public void CharacterReferences_InText_AreDecoded()
    {
        Assert.AreEqual("& <> © ∉ ¬it; A B C � € � � &; &foo; a&b &#",
            Text(Box("<p id=x>&amp; &lt;&gt; &copy &notin; &notit; &#65; &#x42; &#X43; &#0; &#128; &#x110000; &#xD800; &; &foo; a&b &#</p>")));
    }

    [TestMethod]
    [Spec("html", "parsing.html#character-reference-state")]
    [Spec("html", "parsing.html#named-character-reference-state")]
    [Spec("html", "parsing.html#ambiguous-ampersand-state")]
    public void CharacterReferences_InAttributes_AreDecoded()
    {
        var tag = Box("<p id=x title=\"&amp;&copy;&#65;\" lang='&copyx' dir=&notit; data-a=\"?a=1&lang=2\">b</p>").HtmlTag!;
        Assert.AreEqual("&©A", tag.TryGetAttribute("title"));
        Assert.AreEqual("&copyx", tag.TryGetAttribute("lang"));
        Assert.AreEqual("&notit;", tag.TryGetAttribute("dir"));
        Assert.AreEqual("?a=1&lang=2", tag.TryGetAttribute("data-a"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#decimal-character-reference-state")]
    [Spec("html", "parsing.html#hexadecimal-character-reference-state")]
    public void NumericCharacterReference_WithoutSemicolon_IsDecoded()
    {
        Assert.AreEqual("ABx", Box("<p id=x title=\"&#65&#x42x\">a</p>").HtmlTag!.TryGetAttribute("title"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "parsing.html#numeric-character-reference-end-state")]
    public void NumericCharacterReference_ToCarriageReturn_IsDecoded()
    {
        Assert.AreEqual("a\rb", Box("<p id=x title=\"a&#13;b\">a</p>").HtmlTag!.TryGetAttribute("title"));
    }

    [TestMethod]
    [Spec("html", "named-characters.html#named-character-references")]
    public void NamedCharacterReferences_CoverTheWholeTable()
    {
        Assert.AreEqual("✓fj∾̳\n‌Áá&AAcute;", Text(Box("<p id=x>&check;&fjlig;&acE;&NewLine;&zwnj;&Aacute;&aacute;&AAcute;</p>")));
    }

    private static string Text(CssBox box) =>
        string.Concat(LayoutHarness.Descendants(box).Where(b => b.HtmlTag == null).Select(b => b.Text));

    private static string Words(CssBox box) =>
        string.Concat(LayoutHarness.Descendants(box).SelectMany(b => b.Words).Select(w => w.Text));
}
