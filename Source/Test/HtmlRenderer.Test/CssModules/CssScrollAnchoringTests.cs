using HtmlRenderer.Test.CssEngineSupport;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssScrollAnchoringTests
{
    [TestMethod]
    public void OverflowAnchor_IsDropped()
    {
        var style = CssConstructionFunctions.ParseDeclarations("overflow-anchor: none");

        Assert.AreEqual(0, style.Length);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-scroll-anchoring-1.yml")]
    [Spec("css-scroll-anchoring-1", "#exclusion-api")]
    public void OverflowAnchor_IsAccepted()
    {
        var style = CssConstructionFunctions.ParseDeclarations("overflow-anchor: none");

        Assert.AreEqual(1, style.Length);
    }
}
