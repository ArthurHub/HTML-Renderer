using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.CssModules;

/// <summary>Which Compositing and Blending values the CSS engine accepts.</summary>
[TestClass]
public sealed class CompositingTests
{
    [Ignore("Gap, see Spec/compositing-1.yml")]
    [TestMethod]
    [Spec("compositing-1", "#mix-blend-mode")]
    [Spec("compositing-1", "#isolation")]
    [Spec("compositing-1", "#background-blend-mode")]
    [DataRow("mix-blend-mode", "multiply")]
    [DataRow("mix-blend-mode", "luminosity")]
    [DataRow("isolation", "isolate")]
    [DataRow("background-blend-mode", "screen, normal")]
    public void BlendingProperties_AreParsed(string property, string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {property}: {value}; }}");

        Assert.AreEqual(value, sheet.Rules.OfType<StyleRule>().Single().Style.GetPropertyValue(property));
    }
}
