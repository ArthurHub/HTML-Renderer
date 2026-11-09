using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class FormsTests
{
    private static string Text(CssBox box) =>
        string.Concat(LayoutHarness.Descendants(box).SelectMany(b => b.Words).Select(w => w.Text));

    [TestMethod]
    [Spec("html", "forms.html#the-form-element")]
    [Spec("html", "form-elements.html#the-fieldset-element")]
    [DataRow("form")]
    [DataRow("fieldset")]
    public void Element_IsABlock(string tag)
    {
        Assert.AreEqual("block", Box($"<{tag} id=x>a</{tag}>").Display);
    }

    [TestMethod]
    [Spec("html", "forms.html#the-label-element")]
    [Spec("html", "form-elements.html#the-output-element")]
    [DataRow("label")]
    [DataRow("output")]
    public void Element_RendersItsContentInline(string tag)
    {
        var box = Box($"<p><{tag} id=x>a</{tag}></p>");

        Assert.AreEqual("inline", box.Display);
        Assert.AreEqual("a", Text(box));
    }

    [TestMethod]
    public void HiddenInput_TakesNoSpace()
    {
        Assert.AreEqual(0, Box("<div id=x><input type=hidden value=a></div>").Size.Height);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "input.html#states-of-the-type-attribute")]
    [DataRow("text")]
    [DataRow("checkbox")]
    [DataRow("submit")]
    public void Input_RendersAWidget(string type)
    {
        Assert.IsTrue(Box($"<input id=x type={type} value=a>").Size.Width > 0);
    }

    [TestMethod]
    [Spec("html", "form-elements.html#the-button-element")]
    [Spec("html", "form-elements.html#the-textarea-element")]
    [DataRow("button")]
    [DataRow("textarea")]
    public void Element_RendersItsContent(string tag)
    {
        Assert.AreEqual("a", Text(Box($"<{tag} id=x>a</{tag}>")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "form-elements.html#the-button-element")]
    [Spec("html", "form-elements.html#the-textarea-element")]
    [DataRow("button")]
    [DataRow("textarea")]
    public void Element_HasAFrame(string tag)
    {
        Assert.AreNotEqual("none", Box($"<{tag} id=x>a</{tag}>").BorderTopStyle);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "form-elements.html#the-select-element")]
    public void Select_ShowsOnlyTheSelectedOption()
    {
        Assert.AreEqual("b", Text(Box("<select id=x><option>a</option><option selected>b</option></select>")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "form-elements.html#the-datalist-element")]
    public void Datalist_IsNotRendered()
    {
        Assert.AreEqual(0, Box("<div id=x><datalist><option>a</option></datalist></div>").Size.Height);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "form-elements.html#the-progress-element")]
    [Spec("html", "form-elements.html#the-meter-element")]
    [DataRow("progress")]
    [DataRow("meter")]
    public void Gauge_IsAnInlineBlockWidget(string tag)
    {
        Assert.AreEqual("inline-block", Box($"<{tag} id=x value=0.5>50%</{tag}>").Display);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "form-elements.html#the-fieldset-element")]
    public void Fieldset_HasAGrooveBorder()
    {
        Assert.AreEqual("groove", Box("<fieldset id=x>a</fieldset>").BorderTopStyle);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "form-elements.html#the-legend-element")]
    public void Legend_IsABlock()
    {
        Assert.AreEqual("block", Box("<fieldset><legend id=x>a</legend>b</fieldset>").Display);
    }
}
