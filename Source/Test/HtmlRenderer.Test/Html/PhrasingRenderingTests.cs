using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class PhrasingRenderingTests
{
    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);

    /// <summary>Decorations are pushed down to the text, so they're read from the element's first child.</summary>
    private static string Decoration(string html) => Box(html).Boxes[0].TextDecoration;

    private static double WordLeft(string html, string word) =>
        Box(html).Boxes.SelectMany(b => b.Boxes.Prepend(b)).SelectMany(b => b.Words).First(w => w.Text == word).Left;

    [TestMethod]
    [Spec("html", "rendering.html#phrasing-content-3")]
    [DataRow("cite")]
    [DataRow("em")]
    [DataRow("i")]
    [DataRow("var")]
    public void Emphasis_IsItalic(string tag)
    {
        Assert.AreEqual("italic", Box($"<{tag} id=x>a</{tag}>").FontStyle);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#phrasing-content-3")]
    public void Dfn_IsItalic()
    {
        Assert.AreEqual("italic", Box("<dfn id=x>a</dfn>").FontStyle);
    }

    [TestMethod]
    [Spec("html", "rendering.html#phrasing-content-3")]
    [DataRow("b")]
    [DataRow("strong")]
    public void StrongImportance_IsBold(string tag)
    {
        Assert.IsTrue(Box($"<{tag} id=x>a</{tag}>").ActualFontWeight >= 700);
    }

    [TestMethod]
    [Spec("html", "rendering.html#phrasing-content-3")]
    public void BigAndSmall_ChangeTheFontSize()
    {
        var parent = Box("<div id=x style='font-size:16px'>a</div>").ActualFont.Size;

        Assert.IsTrue(Box("<div style='font-size:16px'><big id=x>a</big></div>").ActualFont.Size > parent);
        Assert.IsTrue(Box("<div style='font-size:16px'><small id=x>a</small></div>").ActualFont.Size < parent);
    }

    [TestMethod]
    [Spec("html", "rendering.html#phrasing-content-3")]
    [DataRow("sub")]
    [DataRow("sup")]
    public void SubAndSup_AreSmallerAndShifted(string tag)
    {
        var box = Box($"<div style='font-size:16px'><{tag} id=x>a</{tag}></div>");

        Assert.AreEqual(tag == "sub" ? "sub" : "super", box.VerticalAlign);
        Assert.IsTrue(box.ActualFont.Size < 12);
    }

    [TestMethod]
    [Spec("html", "rendering.html#phrasing-content-3")]
    [DataRow("u", "underline")]
    [DataRow("ins", "underline")]
    [DataRow("del", "line-through")]
    [DataRow("s", "line-through")]
    [DataRow("strike", "line-through")]
    [DataRow("a href=#", "underline")]
    public void Edits_AreDecorated(string tag, string decoration)
    {
        Assert.AreEqual(decoration, Decoration($"<{tag} id=x>a</{tag.Split(' ')[0]}>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#phrasing-content-3")]
    public void Links_AreBlueAndAnchorsWithoutHrefAreNot()
    {
        Assert.AreEqual(RColor.FromArgb(0, 0, 0xEE), Box("<a id=x href=#>a</a>").ActualColor);
        Assert.AreEqual(RColor.FromArgb(0, 0, 0), Box("<a id=x>a</a>").ActualColor);
        Assert.AreEqual("", Decoration("<a id=x>a</a>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#phrasing-content-3")]
    public void Mark_HasAYellowBackground()
    {
        Assert.AreEqual(RColor.FromArgb(255, 255, 0), Box("<mark id=x>a</mark>").ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#phrasing-content-3")]
    [DataRow("abbr")]
    [DataRow("acronym")]
    public void AbbreviationsWithATitle_AreUnderlined(string tag)
    {
        Assert.AreEqual("underline", Decoration($"<{tag} id=x title=t>a</{tag}>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#phrasing-content-3")]
    public void Q_IsQuoted()
    {
        var words = Box("<q id=x>a</q>").Boxes.SelectMany(b => b.Words).Select(w => w.Text);

        Assert.AreEqual("“a”", string.Concat(words));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#phrasing-content-3")]
    public void Nobr_DoesNotWrap()
    {
        Assert.AreEqual("nowrap", Box("<nobr id=x>a b</nobr>").WhiteSpace);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#phrasing-content-3")]
    [DataRow("left", "left")]
    [DataRow("right", "right")]
    [DataRow("all", "both")]
    [DataRow("both", "both")]
    public void BrClear_ClearsFloats(string value, string clear)
    {
        Assert.AreEqual(clear, Box($"<div style='float:left;width:10px;height:50px'></div>a<br id=x clear={value}>b").Clear);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#phrasing-content-3")]
    public void RubyText_IsParenthesizedWithoutRubySupport()
    {
        var words = Box("<div id=x><ruby>a<rt>b</rt></ruby></div>").Boxes.SelectMany(b => b.Boxes).SelectMany(b => b.Boxes.Prepend(b)).SelectMany(b => b.Words).Select(w => w.Text);

        Assert.AreEqual("a(b)", string.Concat(words));
    }

    [TestMethod]
    [Spec("html", "rendering.html#phrasing-content-3")]
    [DataRow("red")]
    [DataRow("#f00")]
    public void FontColor_SetsTheColor(string color)
    {
        Assert.AreEqual(Red, Box($"<font id=x color='{color}'>a</font>").ActualColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#phrasing-content-3")]
    [DataRow("1", "x-small")]
    [DataRow("2", "small")]
    [DataRow("6", "xx-large")]
    [DataRow("+1", "large")]
    [DataRow("-1", "small")]
    public void FontSize_IsALegacyFontSize(string size, string keyword)
    {
        Assert.AreEqual(Box($"<span id=x style='font-size:{keyword}'>a</span>").ActualFont.Size, Box($"<font id=x size='{size}'>a</font>").ActualFont.Size, 0.01);
    }

    [TestMethod]
    [Spec("html", "rendering.html#bidi-rendering")]
    [DataRow("ltr")]
    [DataRow("rtl")]
    public void Dir_SetsTheDirection(string dir)
    {
        Assert.AreEqual(dir, Box($"<p id=x dir={dir}>a</p>").Direction);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#bidi-rendering")]
    [DataRow("<p id=x dir=auto>&#1513;&#1500;&#1493;&#1501;</p>")]
    [DataRow("<bdi id=x>&#1513;&#1500;&#1493;&#1501;</bdi>")]
    public void DirAutoAndBdi_TakeTheDirectionOfTheirText(string html)
    {
        Assert.AreEqual("rtl", Box(html).Direction);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#bidi-rendering")]
    public void Bdo_OverridesTheDirectionOfItsText()
    {
        const string html = "<div id=x style='width:300px' dir=rtl>a <bdo dir=ltr>b c</bdo></div>";

        Assert.IsTrue(WordLeft(html, "b") < WordLeft(html, "c"));
    }
}
