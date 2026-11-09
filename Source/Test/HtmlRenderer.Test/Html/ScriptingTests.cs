using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class ScriptingTests
{
    private static string Text(CssBox box) =>
        string.Concat(LayoutHarness.Descendants(box).SelectMany(b => b.Words).Select(w => w.Text));

    [TestMethod]
    [Spec("html", "scripting.html#the-script-element")]
    public void Script_ContentIsNotRendered()
    {
        var box = Box("<div id=x><script>var a = '<b>x</b>';</script></div>");

        Assert.AreEqual(0, box.Size.Height);
        Assert.AreEqual("", Text(box));
    }

    [TestMethod]
    [Spec("html", "scripting.html#the-noscript-element")]
    public void Noscript_ContentIsParsedAsMarkup()
    {
        const string html = "<p><noscript><b id=x>a</b><i>b</i></noscript></p>";

        Assert.AreEqual("noscript", ParentTag(html));
        Assert.AreEqual("a", Text(Box(html)));
    }

    [TestMethod]
    [Spec("html", "scripting.html#the-noscript-element")]
    public void Noscript_TextIsRendered()
    {
        // Crashes the test host with a stack overflow
        Assert.AreEqual("a", Text(Box("<p id=x><noscript>a</noscript></p>")));
    }

    [TestMethod]
    [Spec("html", "scripting.html#the-noscript-element")]
    public void Noscript_ElementKeepsTheContentAfterItsFirstChildElement()
    {
        Assert.AreEqual("ab", Text(Box("<noscript><p id=x><b>a</b><i>b</i></p></noscript>")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "scripting.html#the-template-element")]
    public void Template_ContentIsNotRendered()
    {
        Assert.AreEqual(0, Box("<div id=x><template><p>a</p></template></div>").Size.Height);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "scripting.html#the-template-element")]
    public void Template_WithShadowRootMode_ReplacesTheHostsContent()
    {
        Assert.AreEqual("a", Text(Box("<div id=x><template shadowrootmode=open>a</template>b</div>")));
    }

    [TestMethod]
    [Spec("html", "scripting.html#the-slot-element")]
    public void Slot_OutsideOfAShadowTree_RendersItsContent()
    {
        Assert.AreEqual("a", Text(Box("<p id=x><slot>a</slot></p>")));
    }

    [TestMethod]
    [Spec("html", "canvas.html#the-canvas-element")]
    public void Canvas_RendersItsFallbackContent()
    {
        Assert.AreEqual("a", Text(Box("<canvas id=x width=50 height=20>a</canvas>")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "canvas.html#the-canvas-element")]
    public void Canvas_WithFallbackContent_IgnoresItsBitmapSize()
    {
        Assert.IsTrue(Box("<div id=x><canvas height=100>a</canvas></div>").Size.Height < 50);
    }
}
