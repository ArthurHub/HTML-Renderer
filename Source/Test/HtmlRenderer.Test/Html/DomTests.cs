using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class DomTests
{
    [TestMethod]
    [Spec("html", "dom.html#global-attributes")]
    public void GlobalAttributes_ApplyToUnknownElements()
    {
        var box = Box("<style>.a { color: #00ff00 }</style><foo id=x class=a style='background-color: #0000ff'>a</foo>");

        AssertColor(0, 255, 0, box.ActualColor);
        AssertColor(0, 0, 255, box.ActualBackgroundColor);
    }

    [TestMethod]
    [Spec("html", "dom.html#global-attributes")]
    public void Id_CanStartWithADigit()
    {
        AssertColor(0, 255, 0, Box("<style>#\\31 a { color: #00ff00 }</style><p id=1a>a</p>", "1a").ActualColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "dom.html#the-lang-and-xml:lang-attributes")]
    public void Language_IsInheritedFromTheParent()
    {
        AssertColor(0, 255, 0, Box("<style>:lang(fr) { color: #00ff00 }</style><div lang=fr><p id=x>a</p></div>").ActualColor);
    }

    [TestMethod]
    [Spec("html", "dom.html#the-dir-attribute")]
    [DataRow("<p id=x dir=rtl>a</p>", "rtl")]
    [DataRow("<p id=x dir=RTL>a</p>", "rtl")]
    [DataRow("<div dir=rtl><p id=x dir=ltr>a</p></div>", "ltr")]
    public void Dir_SetsTheDirectionality(string html, string direction)
    {
        Assert.AreEqual(direction, Box(html).Direction);
    }

    [TestMethod]
    [Spec("html", "dom.html#the-dir-attribute")]
    public void ElementWithoutDir_HasItsParentsDirectionality()
    {
        Assert.AreEqual("rtl", Box("<div dir=rtl><p id=x>a</p></div>").Direction);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "dom.html#the-dir-attribute")]
    [DataRow("<p id=x dir=auto>שלום abc</p>", "rtl")]
    [DataRow("<p id=x dir=auto>123 שלום</p>", "rtl")]
    [DataRow("<div dir=rtl><p id=x dir=auto>abc שלום</p></div>", "ltr")]
    [DataRow("<div dir=rtl><p id=x dir=auto>123</p></div>", "ltr")]
    public void DirAuto_TakesTheDirectionOfTheFirstStrongCharacter(string html, string direction)
    {
        Assert.AreEqual(direction, Box(html).Direction);
    }

    [TestMethod]
    [Spec("html", "dom.html#the-style-attribute")]
    [Spec("css-style-attr", "#interpret")]
    public void StyleAttribute_OverridesStylesheets()
    {
        AssertColor(0, 255, 0, Box("<style>#x { color: #ff0000 }</style><p id=x style='color: #00ff00'>a</p>").ActualColor);
    }

    [TestMethod]
    [Spec("html", "dom.html#the-style-attribute")]
    [Spec("css-style-attr", "#syntax")]
    public void StyleAttribute_SkipsInvalidDeclarations()
    {
        var box = Box("<p id=x style='color: nonsense; background-color: #0000ff; : ; color: #00ff00'>a</p>");

        AssertColor(0, 255, 0, box.ActualColor);
        AssertColor(0, 0, 255, box.ActualBackgroundColor);
    }

    [TestMethod]
    [Spec("html", "dom.html#embedding-custom-non-visible-data-with-the-data-*-attributes")]
    public void DataAttribute_CanBeUsedFromCss()
    {
        var box = Box("<style>[data-state=on] { color: #00ff00 } #x::before { content: attr(data-label) }</style>" +
                      "<p id=x data-state=on data-label=Hi>a</p>");

        AssertColor(0, 255, 0, box.ActualColor);
        Assert.AreEqual("Hi", string.Concat(box.Boxes.Single(b => b.IsBeforePseudoElement).Words.Select(w => w.Text)));
    }

    [TestMethod]
    [Spec("html", "dom.html#embedding-custom-non-visible-data-with-the-data-*-attributes")]
    public void DataAttributeName_IsLowercased()
    {
        AssertColor(0, 255, 0, Box("<style>[data-state] { color: #00ff00 }</style><p id=x data-State=on>a</p>").ActualColor);
    }

    private static void AssertColor(int r, int g, int b, RColor actual)
    {
        Assert.AreEqual((r, g, b), (actual.R, actual.G, actual.B), $"Actual {actual}");
    }
}
