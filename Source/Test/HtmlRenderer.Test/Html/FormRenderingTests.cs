using HtmlRenderer.Test.TestSupport;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class FormRenderingTests
{
    [TestMethod]
    [Spec("html", "rendering.html#form-controls")]
    [DataRow("<input id=x>")]
    [DataRow("<button id=x>a</button>")]
    public void FormControls_AreInlineBlocks(string html)
    {
        Assert.AreEqual("inline-block", Box(html).Display);
    }

    [TestMethod]
    [Spec("html", "rendering.html#the-select-element-2")]
    [Spec("html", "rendering.html#the-textarea-element-2")]
    [DataRow("<select id=x size=4><option>a</option></select>")]
    [DataRow("<textarea id=x>a</textarea>")]
    public void ListBoxSelectAndTextarea_AreInlineBlocks(string html)
    {
        Assert.AreEqual("inline-block", Box(html).Display);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#form-controls")]
    [DataRow("<button id=x>a</button>")]
    [DataRow("<input id=x type=submit value=a>")]
    public void Buttons_CenterTheirText(string html)
    {
        Assert.AreEqual("center", Box(html).TextAlign);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#form-controls")]
    public void Textarea_PreservesWhiteSpace()
    {
        Assert.AreEqual("pre-wrap", Box("<textarea id=x>a  b</textarea>").WhiteSpace);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#form-controls")]
    [DataRow("<button id=x>a</button>")]
    [DataRow("<select id=x><option>a</option></select>")]
    public void ButtonsAndSelects_AreBorderBox(string html)
    {
        Assert.AreEqual("border-box", Box(html).BoxSizing);
    }

    [TestMethod]
    [Spec("html", "rendering.html#the-fieldset-and-legend-elements")]
    public void Fieldset_IsABlock()
    {
        Assert.AreEqual("block", Box("<fieldset id=x>a</fieldset>").Display);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-fieldset-and-legend-elements")]
    public void Fieldset_HasAGrooveBorderAndPadding()
    {
        var box = Box("<fieldset id=x style='font-size:16px'>a</fieldset>");

        Assert.AreEqual("groove", box.BorderTopStyle);
        Assert.AreEqual(2, box.ActualBorderTopWidth);
        Assert.AreEqual(2, box.ActualMarginLeft);
        Assert.AreEqual(0, box.ActualMarginTop);
        Assert.AreEqual(12, box.ActualPaddingLeft, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-fieldset-and-legend-elements")]
    public void Legend_IsRenderedOverTheTopBorder()
    {
        const string html = "<fieldset id=f style='border:10px solid;padding:0;margin:0'><legend id=x style='padding:0'>L</legend>b</fieldset>";
        var legend = Box(html);

        Assert.AreEqual("block", legend.Display);
        Assert.IsTrue(legend.Location.Y < Box(html, "f").Location.Y + 10);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#native-appearance-2")]
    [DataRow("<input id=x value=abc>")]
    [DataRow("<input id=x type=checkbox>")]
    [DataRow("<meter id=x value=0.5></meter>")]
    [DataRow("<progress id=x value=0.5></progress>")]
    public void Widgets_HaveANativeAppearance(string html)
    {
        var box = Box($"<div>{html}</div>");

        Assert.IsTrue(box.Size.Width > 0 && box.Size.Height > 0);
    }

    [TestMethod]
    [Spec("html", "rendering.html#button-layout")]
    [Spec("html", "rendering.html#the-button-element-2")]
    public void Button_IsAnInlineBlockAroundItsContent()
    {
        var button = Box("<div><button id=x>Go</button></div>");

        Assert.AreEqual("inline-block", button.Display);
        Assert.AreEqual("Go", string.Concat(LayoutHarness.Descendants(button).SelectMany(b => b.Words).Select(w => w.Text)));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#button-layout")]
    public void BlockButton_IsFitContentWide()
    {
        Assert.IsTrue(Box("<div><button id=x style='display:block'>Go</button></div>").Size.Width < 100);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-button-element-2")]
    public void Button_IsDepictedAsAButton()
    {
        Assert.AreNotEqual("none", Box("<button id=x>Go</button>").BorderTopStyle);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-details-and-summary-elements")]
    [DataRow("<details id=x><summary>s</summary>b</details>", "block")]
    [DataRow("<details open><summary id=x>s</summary>b</details>", "list-item")]
    [DataRow("<details><summary>s</summary><p id=x>b</p></details>", "none")]
    public void Details_IsABlockThatHidesItsContentUntilOpened(string html, string display)
    {
        Assert.AreEqual(display, Box(html).Display);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-input-element-as-a-text-entry-widget")]
    [Spec("html", "rendering.html#the-input-element-as-a-button")]
    [DataRow("<input id=x value=abc>")]
    [DataRow("<input id=x type=submit value=abc>")]
    public void Input_ShowsItsValue(string html)
    {
        var words = LayoutHarness.Descendants(Box(html)).Prepend(Box(html)).SelectMany(b => b.Words).Select(w => w.Text);

        Assert.AreEqual("abc", string.Concat(words));
    }

    [TestMethod]
    [Spec("html", "rendering.html#the-marquee-element-2")]
    public void MarqueeAttributes_SetItsSizeAndMargins()
    {
        var box = Box("<marquee id=x width=50 height=20 hspace=5 vspace=6>a</marquee>");

        Assert.AreEqual("50px", box.Width);
        Assert.AreEqual("20px", box.Height);
        Assert.AreEqual(5, box.ActualMarginLeft);
        Assert.AreEqual(6, box.ActualMarginTop);
    }

    [TestMethod]
    [Spec("html", "rendering.html#the-marquee-element-2")]
    public void MarqueeBgcolor_SetsItsBackgroundColor()
    {
        Assert.AreEqual("red", Box("<marquee id=x bgcolor=red>a</marquee>").BackgroundColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-marquee-element-2")]
    public void Marquee_IsAnInlineBlockThatHidesItsOverflow()
    {
        var box = Box("<marquee id=x>a</marquee>");

        Assert.AreEqual("inline-block", box.Display);
        Assert.AreEqual("hidden", box.Overflow);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-select-element-2")]
    public void SelectOptions_AreBlocks()
    {
        Assert.AreEqual("block", Box("<select><option id=x>a</option></select>").Display);
        Assert.AreEqual("block", Box("<select><optgroup id=x label=g><option>a</option></optgroup></select>").Display);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#the-textarea-element-2")]
    public void TextareaWrapOff_DoesNotWrap()
    {
        Assert.AreEqual("pre", Box("<textarea id=x wrap=off>a</textarea>").WhiteSpace);
    }
}
