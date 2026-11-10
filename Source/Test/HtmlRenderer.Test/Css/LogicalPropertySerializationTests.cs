using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// The flow-relative properties expand to the physical longhands at parse time, so serialization must
/// not fold physical longhands back into a flow-relative shorthand.
/// </summary>
[TestClass]
public sealed class LogicalPropertySerializationTests
{
    [TestMethod]
    public void PhysicalLonghands_DoNotSerializeAsLogicalShorthand()
    {
        var sheet = ParseStyleSheet("div { margin-top: 5pt; margin-bottom: 5pt; }");
        var css = sheet.ToCss();

        Assert.IsFalse(css.Contains("margin-block"));
        Assert.IsTrue(css.Contains("margin-top"));
        Assert.IsTrue(css.Contains("margin-bottom"));
    }

    [TestMethod]
    public void LogicalShorthand_ParsesAndExpandsToPhysicalLonghands()
    {
        var sheet = ParseStyleSheet("div { margin-block: 3pt 4pt; }");
        var style = (StyleDeclaration)sheet.Rules.OfType<StyleRule>().Single().Style;

        Assert.AreEqual("3pt", style.GetPropertyValue("margin-top"));
        Assert.AreEqual("4pt", style.GetPropertyValue("margin-bottom"));
        Assert.AreEqual(string.Empty, style.GetPropertyValue("margin-block-start"));
        Assert.AreEqual(string.Empty, style.GetPropertyValue("margin-block-end"));
        Assert.IsFalse(sheet.ToCss().Contains("margin-block:"));
    }

    [TestMethod]
    public void PhysicalBorderStillReconstructs()
    {
        var sheet = ParseStyleSheet(
            "div { border-top: 1pt solid red; border-right: 1pt solid red; border-bottom: 1pt solid red; border-left: 1pt solid red; }");
        var css = sheet.ToCss();

        Assert.IsTrue(css.Contains("border:"));
    }
}
