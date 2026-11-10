using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class WebAppApisTests
{
    private static string Text(CssBox box) =>
        string.Concat(LayoutHarness.Descendants(box).SelectMany(b => b.Words).Select(w => w.Text));

    [TestMethod]
    [Spec("html", "webappapis.html#enabling-and-disabling-scripting")]
    public void ScriptingIsDisabled_NoscriptIsRenderedAndScriptIsNot()
    {
        var box = Box("<div id=x><script>document.write('b')</script><noscript><span>a</span></noscript></div>");

        Assert.AreEqual("a", Text(box));
    }

    [TestMethod]
    [Spec("html", "webappapis.html#event-handlers-on-elements,-document-objects,-and-window-objects")]
    public void EventHandlerContentAttributes_AreIgnored()
    {
        const string html =
            "<div id=x onclick=\"this.style.display='none'\" onmouseover=\"alert(1)\">" +
            "<a href=\"#\" onclick=\"return false\">a</a><img onerror=\"alert(1)\"><b onload=x>b</b></div>";

        var box = Box(html);

        Assert.AreEqual("ab", Text(box));
        Assert.IsTrue(box.Size.Height > 0);
    }

    [TestMethod]
    [Spec("html", "webappapis.html#event-handlers-on-elements,-document-objects,-and-window-objects")]
    public void BodyEventHandlerContentAttributes_AreIgnored()
    {
        var (root, _) = LayoutHarness.Layout("<html><body onload=\"document.body.innerHTML=''\" onunload=x><p id=x>a</p></body></html>");

        Assert.AreEqual("a", Text(LayoutHarness.FindById(root, "x")!));
    }
}
