using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class EditsTests
{
    [TestMethod]
    [Spec("html", "edits.html#the-ins-element")]
    [Spec("html", "edits.html#the-del-element")]
    [DataRow("ins", "underline")]
    [DataRow("del", "line-through")]
    public void Edit_IsDecorated(string tag, string decoration)
    {
        var box = Box($"<{tag} id=x>a</{tag}>");

        Assert.AreEqual("inline", box.Display);
        Assert.AreEqual(decoration, box.Boxes[0].TextDecoration);
    }

    [TestMethod]
    [Spec("html", "edits.html#the-ins-element")]
    [Spec("html", "edits.html#the-del-element")]
    [DataRow("ins", "underline")]
    [DataRow("del", "line-through")]
    public void Edit_AroundAParagraph_DecoratesItsText(string tag, string decoration)
    {
        var box = Box($"<{tag}><p id=x>a</p></{tag}>");

        Assert.AreEqual(decoration, box.Boxes[0].TextDecoration);
    }
}
