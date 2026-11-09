using HtmlRenderer.Test.TestSupport;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class CustomElementsTests
{
    [TestMethod]
    [Spec("html", "custom-elements.html#custom-elements-core-concepts")]
    public void UndefinedCustomElement_RendersItsContentInline()
    {
        var box = Box("<p><my-element id=x>a</my-element></p>");

        Assert.AreEqual("inline", box.Display);
        Assert.AreEqual("a", string.Concat(LayoutHarness.Descendants(box).SelectMany(b => b.Words).Select(w => w.Text)));
    }
}
