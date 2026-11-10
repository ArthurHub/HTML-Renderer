using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class FlexPropertyTests
{
    [TestMethod]
    [Spec("css-flexbox-1", "#alignment")]
    public void JustifyAlign_Parses()
    {
        const string css = """
                            html {
                                justify-content: center;
                                align-items: center;
                                align-content: center;
                                align-self: center;
                            }
                            """;

        var stylesheet = CssConstructionFunctions.ParseStyleSheet(css);
        var info = (StyleRule)stylesheet.StyleRules.First();

        Assert.AreEqual("center", info.Style.AlignItems);
        Assert.AreEqual("center", info.Style.AlignContent);
        Assert.AreEqual("center", info.Style.AlignSelf);
        Assert.AreEqual("center", info.Style.JustifyContent);
    }

    [TestMethod]
    [Spec("css-flexbox-1", "#flex-property")]
    public void FlexAuto_Parses()
    {
        const string css = """
                            html {
                                flex: auto;
                            }
                            """;

        var stylesheet = CssConstructionFunctions.ParseStyleSheet(css);
        var info = (StyleRule)stylesheet.StyleRules.First();

        Assert.AreEqual("1 1 auto", info.Style.Flex);
    }
}
