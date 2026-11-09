using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class TextLevelSemanticsTests
{
    [TestMethod]
    [Spec("html", "text-level-semantics.html#the-a-element")]
    public void A_WithHref_IsAHyperlink()
    {
        Assert.IsTrue(Box("<a id=x href=page.html>a</a>").IsClickable);
        Assert.IsFalse(Box("<a id=x>a</a>").IsClickable);
    }

    [TestMethod]
    [Spec("html", "text-level-semantics.html#the-em-element")]
    [Spec("html", "text-level-semantics.html#the-cite-element")]
    [Spec("html", "text-level-semantics.html#the-var-element")]
    [Spec("html", "text-level-semantics.html#the-i-element")]
    [DataRow("em")]
    [DataRow("cite")]
    [DataRow("var")]
    [DataRow("i")]
    public void Element_IsItalic(string tag)
    {
        var box = Box($"<{tag} id=x>a</{tag}>");

        Assert.AreEqual("inline", box.Display);
        Assert.AreEqual("italic", box.FontStyle);
    }

    [TestMethod]
    [Spec("html", "text-level-semantics.html#the-strong-element")]
    [Spec("html", "text-level-semantics.html#the-b-element")]
    [DataRow("strong")]
    [DataRow("b")]
    public void Element_IsBold(string tag)
    {
        var box = Box($"<{tag} id=x>a</{tag}>");

        Assert.AreEqual("inline", box.Display);
        Assert.AreEqual("bolder", box.FontWeight);
    }

    [TestMethod]
    [Spec("html", "text-level-semantics.html#the-code-element")]
    [Spec("html", "text-level-semantics.html#the-samp-element")]
    [Spec("html", "text-level-semantics.html#the-kbd-element")]
    [DataRow("code")]
    [DataRow("samp")]
    [DataRow("kbd")]
    public void Element_IsMonospace(string tag)
    {
        var adapter = new MockAdapter();
        adapter.AddFontFamily(new MockFontFamily("Courier New"));
        adapter.AddFontFamilyMapping("monospace", "Courier New");
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<{tag} id=x>a</{tag}>"), adapter: adapter);
        var box = LayoutHarness.FindById(root, "x")!;

        Assert.AreEqual("inline", box.Display);
        Assert.AreEqual("monospace", box.FontFamily);
    }

    [TestMethod]
    [Spec("html", "text-level-semantics.html#the-small-element")]
    public void Small_IsSmaller()
    {
        var box = Box("<div style='font-size:20px'><small id=x>a</small></div>");

        Assert.AreEqual("inline", box.Display);
        Assert.IsTrue(box.ActualFont.Size < Box("<div style='font-size:20px'><span id=x>a</span></div>").ActualFont.Size);
    }

    [TestMethod]
    [Spec("html", "text-level-semantics.html#the-sub-and-sup-elements")]
    public void SubAndSup_AreSmallerAndShifted()
    {
        var normal = Box("<div style='font-size:20px'><span id=x>a</span></div>");
        var sub = Box("<div style='font-size:20px'><sub id=x>a</sub></div>");
        var sup = Box("<div style='font-size:20px'><sup id=x>a</sup></div>");

        Assert.AreEqual("sub", sub.VerticalAlign);
        Assert.AreEqual("super", sup.VerticalAlign);
        Assert.IsTrue(sub.ActualFont.Size < normal.ActualFont.Size);
        Assert.IsTrue(sup.ActualFont.Size < normal.ActualFont.Size);
    }

    [TestMethod]
    [Spec("html", "text-level-semantics.html#the-s-element")]
    [Spec("html", "text-level-semantics.html#the-u-element")]
    [DataRow("s", "line-through")]
    [DataRow("u", "underline")]
    public void Element_IsDecorated(string tag, string decoration)
    {
        Assert.AreEqual(decoration, TextDecoration($"<{tag} id=x>a</{tag}>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "text-level-semantics.html#the-q-element")]
    public void Q_IsQuoted()
    {
        Assert.AreEqual("\u201Ca\u201D", Text(Box("<q id=x>a</q>")));
    }

    [TestMethod]
    [Spec("html", "text-level-semantics.html#the-abbr-element")]
    public void Abbr_WithoutATitle_IsPlainText()
    {
        var box = Box("<abbr id=x>HTML</abbr>");

        Assert.AreEqual("inline", box.Display);
        Assert.AreEqual("HTML", Text(box));
        Assert.AreEqual("", TextDecoration("<abbr id=x>HTML</abbr>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "text-level-semantics.html#the-abbr-element")]
    public void Abbr_WithATitle_IsUnderlined()
    {
        StringAssert.Contains(TextDecoration("<abbr id=x title='HyperText Markup Language'>HTML</abbr>"), "underline");
    }

    [TestMethod]
    [Spec("html", "text-level-semantics.html#the-data-element")]
    [Spec("html", "text-level-semantics.html#the-time-element")]
    [Spec("html", "text-level-semantics.html#the-span-element")]
    [DataRow("<data id=x value=42>forty-two</data>")]
    [DataRow("<time id=x datetime=2026-11-09>forty-two</time>")]
    [DataRow("<span id=x>forty-two</span>")]
    public void Element_ShowsItsContentInline(string html)
    {
        var box = Box(html);

        Assert.AreEqual("inline", box.Display);
        Assert.AreEqual("forty-two", Text(box));
    }

    [TestMethod]
    [Spec("html", "text-level-semantics.html#the-bdo-element")]
    public void Bdo_Rtl_ReversesTheWordOrder()
    {
        var box = Box("<bdo id=x dir=rtl>ab cd</bdo>");
        var words = Words(box).ToList();

        Assert.AreEqual("rtl", box.Direction);
        Assert.AreEqual("ab", words[0].Text.Trim());
        Assert.IsTrue(words[1].Left < words[0].Left);
    }

    [TestMethod]
    [Spec("html", "text-level-semantics.html#the-br-element")]
    public void Br_EndsTheLine()
    {
        var box = Box("<div id=x>a<br>b</div>");
        var words = Words(box).Where(w => !w.IsLineBreak).ToList();

        Assert.AreEqual(words[0].Left, words[1].Left);
        Assert.IsTrue(words[1].Top > words[0].Top);
    }

    [TestMethod]
    [Spec("html", "text-level-semantics.html#the-wbr-element")]
    public void Wbr_IsALineBreakOpportunity()
    {
        var box = Box("<div id=x style='width:60px;font-size:10px'>aaaaaaa<wbr>bbbbbbb</div>");
        var words = Words(box).ToList();

        Assert.AreEqual(2, words.Count);
        Assert.AreEqual(words[0].Left, words[1].Left);
        Assert.IsTrue(words[1].Top > words[0].Top);
    }

    private static string Text(CssBox box) => string.Concat(Words(box).Select(w => w.Text));

    private static IEnumerable<CssRect> Words(CssBox box) => LayoutHarness.Descendants(box).SelectMany(b => b.Words);

    /// <summary>The decoration of the element's text, the cascade moves it from the element to its text boxes.</summary>
    private static string TextDecoration(string body) => Box(body).Boxes[0].TextDecoration;
}
