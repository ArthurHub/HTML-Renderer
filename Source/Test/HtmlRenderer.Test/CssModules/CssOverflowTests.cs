using HtmlRenderer.Test.CssEngineSupport;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssOverflowTests
{
    [TestMethod]
    [Spec("css-overflow-3", "#smooth-scrolling")]
    public void ScrollBehavior_IsAccepted()
    {
        var style = CssConstructionFunctions.ParseDeclarations("scroll-behavior: smooth");

        Assert.AreEqual(1, style.Length);
    }

    [TestMethod]
    [Spec("css-overflow-3", "#scrollbar-gutter-property")]
    public void ScrollbarGutter_IsAccepted()
    {
        var style = CssConstructionFunctions.ParseDeclarations("scrollbar-gutter: stable both-edges");

        Assert.AreEqual(1, style.Length);
    }
}
