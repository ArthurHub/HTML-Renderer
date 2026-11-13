using HtmlRenderer.Test.CssEngineSupport;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssScrollbarsTests
{

    [TestMethod]
    [Spec("css-scrollbars-1", "#scrollbar-color")]
    public void ScrollbarColor_IsAccepted()
    {
        var style = CssConstructionFunctions.ParseDeclarations("scrollbar-color: red blue");

        Assert.AreEqual(1, style.Length);
    }

    [TestMethod]
    [DataRow("scrollbar-width: thin")]
    [DataRow("scrollbar-width: none")]
    [Spec("css-scrollbars-1", "#scrollbar-width")]
    public void ScrollbarWidth_IsAccepted(string declaration)
    {
        var style = CssConstructionFunctions.ParseDeclarations(declaration);

        Assert.AreEqual(1, style.Length);
    }
}
