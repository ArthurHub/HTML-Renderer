using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.CssModules;

/// <summary>Which will-change values the CSS engine accepts.</summary>
[TestClass]
public sealed class CssWillChangeTests
{
    [Ignore("Gap, see Spec/css-will-change-1.yml")]
    [TestMethod]
    [Spec("css-will-change-1", "#will-change")]
    [DataRow("auto")]
    [DataRow("scroll-position")]
    [DataRow("contents")]
    [DataRow("transform, opacity")]
    public void WillChange_IsParsed(string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ will-change: {value}; }}");

        Assert.AreEqual(value, sheet.Rules.OfType<StyleRule>().Single().Style.GetPropertyValue("will-change"));
    }
}
