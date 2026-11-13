using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>Which box-shadow values the CSS engine accepts.</summary>
[TestClass]
public sealed class BoxShadowTests
{
    private static string Parse(string value) =>
        CssConstructionFunctions.ParseDeclarations($"box-shadow: {value}").GetPropertyValue("box-shadow");

    [TestMethod]
    [Spec("css-backgrounds-3", "#box-shadow")]
    [Spec("css-borders-4", "#box-shadow")]
    [DataRow("none")]
    [DataRow("2px 3px")]
    [DataRow("2px 2px 5px")]
    [DataRow("1px 1px 2px 3px")]
    [DataRow("inset 0 0 5px red")]
    [DataRow("2px 2px red")]
    [DataRow("2px 2px #fff")]
    [DataRow("2px 2px #08f")]
    [DataRow("2px 2px #000")]
    [DataRow("2px 2px #0088ff")]
    [DataRow("2px 2px rgba(0,0,0,.5)")]
    [DataRow("2px 2px currentColor")]
    [DataRow("red 2px 2px")]
    [DataRow("0.5em 0.5em 1em")]
    [DataRow("-2px -3px 4px -1px")]
    [DataRow("1px 1px 2px 3px rgba(0,0,0,.5), inset 0 0 0 1px blue")]
    public void ValidValue_IsAccepted(string value)
    {
        Assert.AreNotEqual(string.Empty, Parse(value));
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#box-shadow")]
    [DataRow("banana")]
    [DataRow("2px")]
    [DataRow("2px 2px 2px 2px 2px")]
    [DataRow("inset inset 2px 2px")]
    [DataRow("2px 2px -5px red")]
    [DataRow("2px red 2px")]
    [DataRow("2px 2px banana")]
    [DataRow("2px 2px #12")]
    [DataRow("2px 2px #")]
    [DataRow("2px 50%")]
    [DataRow("2px 2px 50%")]
    [DataRow("2px 2px red blue")]
    public void InvalidValue_IsDropped(string value)
    {
        Assert.AreEqual(string.Empty, Parse(value));
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#box-shadow")]
    public void MultipleLayers_KeepTheirOrder()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("div { box-shadow: 1px 1px red, inset 0 0 0 1px blue; }");

        var value = sheet.Rules.OfType<StyleRule>().Single().Style.GetPropertyValue("box-shadow");

        Assert.IsTrue(value.IndexOf("red") < value.IndexOf("inset"), value);
    }
}
