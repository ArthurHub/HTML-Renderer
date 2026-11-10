using System.Collections.Generic;
using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class BoxShadowGrammarTests
{
    private static List<BoxShadowGrammar.ShadowLayer> Parse(string value) =>
        BoxShadowGrammar.TryParse(CssValueParser.GetCssTokens(value));

    [TestMethod]
    [Spec("css-backgrounds-3", "#box-shadow")]
    public void None_ReturnsEmptyList()
    {
        var layers = Parse("none");
        Assert.IsNotNull(layers);
        Assert.AreEqual(0, layers.Count);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#box-shadow")]
    public void TwoLengths_OffsetsOnly()
    {
        var layers = Parse("2px 3px");
        Assert.AreEqual(1, layers.Count);
        var layer = layers[0];
        Assert.IsFalse(layer.Inset);
        Assert.AreEqual("2px", layer.OffsetX);
        Assert.AreEqual("3px", layer.OffsetY);
        Assert.AreEqual("0", layer.Blur);
        Assert.AreEqual("0", layer.Spread);
        Assert.IsNull(layer.Color);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#box-shadow")]
    public void ThreeLengths_HasBlur()
    {
        var layers = Parse("2px 2px 5px");
        Assert.AreEqual(1, layers.Count);
        var layer = layers[0];
        Assert.AreEqual("5px", layer.Blur);
        Assert.AreEqual("0", layer.Spread);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#box-shadow")]
    public void FourLengths_HasBlurAndSpread()
    {
        var layers = Parse("1px 1px 2px 3px");
        Assert.AreEqual(1, layers.Count);
        var layer = layers[0];
        Assert.AreEqual("2px", layer.Blur);
        Assert.AreEqual("3px", layer.Spread);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#box-shadow")]
    public void Inset_WithColor()
    {
        var layers = Parse("inset 0 0 5px red");
        Assert.AreEqual(1, layers.Count);
        var layer = layers[0];
        Assert.IsTrue(layer.Inset);
        Assert.AreEqual("0", layer.OffsetX);
        Assert.AreEqual("0", layer.OffsetY);
        Assert.AreEqual("5px", layer.Blur);
        Assert.AreEqual("red", layer.Color);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#box-shadow")]
    [DataRow("2px 2px red", "red")]
    [DataRow("2px 2px #fff", "#fff")]        // letter-leading hex (Hash token)
    [DataRow("2px 2px #08f", "#08f")]        // digit-leading short hex (Delim + dimension)
    [DataRow("2px 2px #000", "#000")]        // digit-leading hex, all digits (Delim + number)
    [DataRow("2px 2px #0088ff", "#0088ff")]  // digit-leading long hex
    [DataRow("2px 2px rgba(0,0,0,.5)", "rgba(0,0,0,.5)")]
    [DataRow("2px 2px currentColor", "currentColor")]
    public void Color_IsCaptured(string value, string expectedColor)
    {
        var layers = Parse(value);
        Assert.AreEqual(1, layers.Count);
        Assert.AreEqual(expectedColor, layers[0].Color);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#box-shadow")]
    public void ColorBeforeLengths_IsAccepted()
    {
        // The && grammar allows the color in any position, including before the lengths.
        var layers = Parse("red 2px 2px");
        Assert.AreEqual(1, layers.Count);
        var layer = layers[0];
        Assert.AreEqual("red", layer.Color);
        Assert.AreEqual("2px", layer.OffsetX);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#box-shadow")]
    public void EmValidLengthsAreKeptAsAuthoredStrings()
    {
        var layers = Parse("0.5em 0.5em 1em");
        Assert.AreEqual(1, layers.Count);
        var layer = layers[0];
        Assert.AreEqual("0.5em", layer.OffsetX);
        Assert.AreEqual("1em", layer.Blur);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#box-shadow")]
    public void NegativeOffsetsAndSpread_AreValid()
    {
        var layers = Parse("-2px -3px 4px -1px");
        Assert.AreEqual(1, layers.Count);
        var layer = layers[0];
        Assert.AreEqual("-2px", layer.OffsetX);
        Assert.AreEqual("-3px", layer.OffsetY);
        Assert.AreEqual("-1px", layer.Spread);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#box-shadow")]
    public void MultipleLayers_ParseInOrder()
    {
        var layers = Parse("1px 1px 2px 3px rgba(0,0,0,.5), inset 0 0 0 1px blue");
        Assert.AreEqual(2, layers.Count);
        Assert.IsFalse(layers[0].Inset);
        Assert.AreEqual("rgba(0,0,0,.5)", layers[0].Color);
        Assert.IsTrue(layers[1].Inset);
        Assert.AreEqual("blue", layers[1].Color);
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#box-shadow")]
    [DataRow("")]
    [DataRow("banana")]              // no lengths
    [DataRow("2px")]                 // only one length
    [DataRow("2px 2px 2px 2px 2px")] // five lengths
    [DataRow("inset inset 2px 2px")] // inset twice
    [DataRow("2px 2px -5px red")]    // negative blur radius
    [DataRow("2px red 2px")]         // non-contiguous lengths
    [DataRow("2px 2px banana")]      // invalid color keyword
    [DataRow("2px 2px #12")]         // "#" + a 2-digit number: not a valid hex length
    [DataRow("2px 2px #")]           // a bare "#" delimiter with nothing after it
    [DataRow("2px 50%")]             // percentage is not a valid length
    [DataRow("2px 2px 50%")]         // percentage where a color/length is expected
    [DataRow("2px 2px red blue")]    // two colors
    public void Invalid_ReturnsNull(string value)
    {
        Assert.IsNull(Parse(value));
    }

    [TestMethod]
    [Spec("css-backgrounds-3", "#box-shadow")]
    [DataRow("box-shadow: none", true)]
    [DataRow("box-shadow: 2px 2px", true)]
    [DataRow("box-shadow: inset 0 0 5px red", true)]
    [DataRow("box-shadow: 1px 1px 2px 3px rgba(0,0,0,.5), 0 0 0 1px blue", true)]
    [DataRow("box-shadow: banana", false)]
    [DataRow("box-shadow: 2px 50%", false)]
    public void LayerA_AcceptsValid_RejectsInvalid(string declaration, bool shouldApply)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($"div {{ {declaration}; }}");
        var style = sheet.Rules.OfType<StyleRule>().Single().Style;
        var applied = !string.IsNullOrEmpty(style.GetPropertyValue("box-shadow"));
        Assert.AreEqual(shouldApply, applied);
    }
}
