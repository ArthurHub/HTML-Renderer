using HtmlRenderer.Test.CssEngineSupport;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssDisplay4Tests
{
    [Ignore("Gap, see Spec/css-display-4.yml")]
    [TestMethod]
    [Spec("css-display-4", "#visibility")]
    public void ForceHidden_IsAccepted()
    {
        Assert.IsTrue(CssConstructionFunctions.ParseDeclarations("visibility: force-hidden").Length > 0);
    }
}
