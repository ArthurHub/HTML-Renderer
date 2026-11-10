using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>Ported from PeachPDF.Tests/CSS/Gradient.cs.</summary>
[TestClass]
public sealed class GradientTests
{
    [TestMethod]
    public void InLinearGradient()
    {
        var source = "linear-gradient(135deg, red, blue)";
        var value = CssConstructionFunctions.ParseValue(source);
        Assert.AreEqual(1, value.Count);
        Assert.AreEqual("linear-gradient", value[0].Data);
    }

    [TestMethod]
    public void InRadialGradient()
    {
        var source = "radial-gradient(ellipse farthest-corner at 45px 45px , #00FFFF, rgba(0, 0, 255, 0) 50%, #0000FF 95%)";
        var value = CssConstructionFunctions.ParseValue(source);
        Assert.AreEqual("radial-gradient", value[0].Data);
    }

    [TestMethod]
    public void BackgroundImageLinearGradientWithAngle()
    {
        var source = "background-image: linear-gradient(135deg, red, blue)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageLinearGradientWithSide()
    {
        var source = "background-image: linear-gradient(to right, red, orange, yellow, green, blue, indigo, violet)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageLinearGradientWithCornerAndRgba()
    {
        var source = "background-image: linear-gradient(to bottom right, red, rgba(255,0,0,0))";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageLinearGradientWithSideAndHsl()
    {
        var source = "background-image: linear-gradient(to bottom, hsl(0, 80%, 70%), #bada55)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageLinearGradientNoAngle()
    {
        var source = "background-image: linear-gradient(yellow, blue 20%, #0f0)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientCircleFarthestCorner()
    {
        var source = "background-image: radial-gradient(circle farthest-corner at 45px 45px , #00FFFF 0%, rgba(0, 0, 255, 0) 50%, #0000FF 95%)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientEllipseFarthestCorner()
    {
        var source = "background-image: radial-gradient(ellipse farthest-corner at 470px 47px , #FFFF80 20%, rgba(204, 153, 153, 0.4) 30%, #E6E6FF 60%)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientFarthestCornerWithPoint()
    {
        var source = "background-image: radial-gradient(farthest-corner at 45px 45px , #FF0000 0%, #0000FF 100%)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientSingleSize()
    {
        var source = "background-image: radial-gradient(16px at 60px 50% , #000000 0%, #000000 14px, rgba(0, 0, 0, 0.3) 18px, rgba(0, 0, 0, 0) 19px)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientCircle()
    {
        var source = "background-image: radial-gradient(circle, yellow, green)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientOnlyGradientStops()
    {
        var source = "background-image: radial-gradient(yellow, green)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientEllipseAtCenter()
    {
        var source = "background-image: radial-gradient(ellipse at center, yellow 0%, green 100%)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientFarthestCornerWithoutPoint()
    {
        var source = "background-image: radial-gradient(farthest-corner, yellow, green)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientClosestSideWithPoint()
    {
        var source = "background-image: radial-gradient(closest-side at 20px 30px, red, yellow, green)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientSizeAndPoint()
    {
        var source = "background-image: radial-gradient(20px 30px at 20px 30px, red, yellow, green)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientClosestSideCircleShuffledWithPoint()
    {
        var source = "background-image: radial-gradient(closest-side circle at 20px 30px, red, yellow, green)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientFarthestSideLeftBottom()
    {
        var source = "background-image: radial-gradient(farthest-side at left bottom, red, yellow 50px, green);";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    // ── Percentage-based positions (used by ParseRadialGradient renderer parser) ──

    [TestMethod]
    public void BackgroundImageRadialGradientAtPercentPosition()
    {
        var source = "background-image: radial-gradient(at 30% 70%, red, blue)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientEllipseAtPercentPosition()
    {
        var source = "background-image: radial-gradient(ellipse at 25% 75%, yellow, green)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientCircleAtPercentPosition()
    {
        var source = "background-image: radial-gradient(circle at 80% 20%, orange, crimson)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientAtCenterKeyword()
    {
        var source = "background-image: radial-gradient(at center, gold, navy)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientThreeStopsNoShape()
    {
        var source = "background-image: radial-gradient(red, yellow 50%, blue)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRadialGradientRgbaTransparency()
    {
        var source = "background-image: radial-gradient(rgba(255,0,0,0), rgba(255,0,0,1))";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRepeatingLinearGradientRedBlue()
    {
        var source = "background-image: repeating-linear-gradient(red, blue 20px, red 40px)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRepeatingRadialGradientRedBlue()
    {
        var source = "background-image: repeating-radial-gradient(red, blue 20px, red 40px)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    [TestMethod]
    public void BackgroundImageRepeatingRadialGradientFunky()
    {
        var source = "background-image: repeating-radial-gradient(circle closest-side at 20px 30px, red, yellow, green 100%, yellow 150%, red 200%)";
        var property = CssConstructionFunctions.ParseDeclaration(source);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    // <color-stop-length> = <length-percentage>{1,2} (CSS Images 4 §3.5.1). Both positions must survive
    // serialization: the render layer (CssValueParser.ParseLinearGradient) reads the property's value
    // and expands two positions into two stops, so dropping one here collapsed the solid band. This
    // asserts the non-lossy value directly - the LinearGradientIntegrationTests only check that a
    // stitching function is present, which is true with or without the dropped position.
    [TestMethod]
    [DataRow("background-image: linear-gradient(red 0 50%, blue 50% 100%)",
        "linear-gradient(rgb(255, 0, 0) 0 50%, rgb(0, 0, 255) 50% 100%)")]
    [DataRow("background-image: linear-gradient(90deg, red 0 8px, blue)",
        "linear-gradient(90deg, rgb(255, 0, 0) 0 8px, rgb(0, 0, 255))")]
    [DataRow("background-image: radial-gradient(red 0 8px, blue)",
        "radial-gradient(rgb(255, 0, 0) 0 8px, rgb(0, 0, 255))")]
    public void GradientTwoPositionColorStopRoundTrips(string snippet, string expected)
    {
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.AreEqual(expected, backgroundImage.Value);
    }

    // Single-position and no-position stops, plus a bare-position colour hint, are unaffected; three
    // positions on one stop and two bare positions with no colour remain invalid.
    [TestMethod]
    [DataRow("background-image: linear-gradient(red 50%, blue)", true)]
    [DataRow("background-image: linear-gradient(red, 25%, blue)", true)]
    [DataRow("background-image: linear-gradient(red 0 25% 50%, blue)", false)]
    [DataRow("background-image: linear-gradient(0 50%, blue)", false)]
    public void GradientColorStopValidity(string snippet, bool valid)
    {
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual(valid, ((BackgroundImageProperty)property).HasValue);
    }

    // conic-gradient() is now validated at parse time (issue #244) instead of accepting anything.
    // Every form CssValueParser.ParseConicGradient renders must still be accepted here — these mirror
    // the values in ConicGradientIntegrationTests / ColorSpaceGradientIntegrationTests.
    [TestMethod]
    [DataRow("conic-gradient(red, blue)")]
    [DataRow("conic-gradient(red, yellow, blue)")]
    [DataRow("conic-gradient(from 90deg, red, blue)")]
    [DataRow("conic-gradient(at 25% 75%, red, green, blue)")]
    [DataRow("conic-gradient(from 45deg at 30% 70%, red, blue)")]
    [DataRow("conic-gradient(red 0deg, blue 180deg, green 360deg)")]
    [DataRow("conic-gradient(red 0%, blue 50%, green 100%)")]
    [DataRow("conic-gradient(red calc(1turn * 0.35), blue calc(1turn * 0.7), green 1turn)")]
    // A non-angle-typed calc() position (percentage here) is also accepted, matching the renderer's
    // TryParseConicAngle calc branch which resolves any calc()-family against a full turn.
    [DataRow("conic-gradient(red calc(50%), blue)")]
    [DataRow("conic-gradient(red 0 90deg, blue 90deg 180deg, green 180deg 360deg)")]
    [DataRow("conic-gradient(rgba(255,0,0,0), red)")]
    [DataRow("repeating-conic-gradient(red 0deg 30deg, blue 30deg 60deg)")]
    [DataRow("repeating-conic-gradient(from 45deg, red 0deg, blue 60deg)")]
    [DataRow("repeating-conic-gradient(#000 0 25%, #fff 25% 50%)")]
    [DataRow("conic-gradient(in oklch, red, blue)")]
    public void BackgroundImageConicGradientAccepted(string gradient)
    {
        var property = CssConstructionFunctions.ParseDeclaration($"background-image: {gradient}");
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    // Malformed conic gradients must now be DROPPED at parse time (HasValue == false) so a prior
    // background-image (or the initial value) wins, per CSS Cascade §4.1 — the bug in issue #244.
    [TestMethod]
    [DataRow("conic-gradient(!!! 5px \"garbage\")")]          // the issue's example
    [DataRow("conic-gradient(red 5px, blue)")]                // a <length> is not a valid angular position
    [DataRow("conic-gradient(from red, blue, green)")]        // "from" prelude needs an <angle>, not a colour
    [DataRow("conic-gradient(red 5, blue)")]                  // a bare non-zero number is not a valid position
    public void BackgroundImageConicGradientRejected(string gradient)
    {
        var property = CssConstructionFunctions.ParseDeclaration($"background-image: {gradient}");
        Assert.IsFalse(((BackgroundImageProperty)property).HasValue);
    }

    // A gradient's "in <color-interpolation-method>" prelude (CSS Images 4 §3.1) is now validated at
    // parse time (issue #245) instead of accepting any comma group that merely contains an "in" ident.
    // Every space PeachPDF supports - alone, with a polar hue method, and combined with a direction/
    // shape in either order - must stay accepted, across linear/radial/conic.
    [TestMethod]
    [DataRow("linear-gradient(in srgb, red, blue)")]
    [DataRow("linear-gradient(in srgb-linear, red, blue)")]
    [DataRow("linear-gradient(in display-p3, red, blue)")]
    [DataRow("linear-gradient(in lab, red, blue)")]
    [DataRow("linear-gradient(in oklab, red, blue)")]
    [DataRow("linear-gradient(in xyz, red, blue)")]
    [DataRow("linear-gradient(in xyz-d50, red, blue)")]
    [DataRow("linear-gradient(in hsl, red, blue)")]
    [DataRow("linear-gradient(in hwb, red, blue)")]
    [DataRow("linear-gradient(in lch, red, blue)")]
    [DataRow("linear-gradient(in oklch, red, blue)")]
    [DataRow("linear-gradient(in oklch shorter hue, red, blue)")]
    [DataRow("linear-gradient(in oklch longer hue, red, blue)")]
    [DataRow("linear-gradient(in hsl increasing hue, red, blue)")]
    [DataRow("linear-gradient(in oklab to right, red, blue)")]
    [DataRow("linear-gradient(to right in oklab, red, blue)")]
    [DataRow("linear-gradient(45deg in oklch longer hue, red, blue)")]
    [DataRow("radial-gradient(in oklch, red, blue)")]
    [DataRow("radial-gradient(in oklab circle at center, red, blue)")]
    [DataRow("conic-gradient(in oklab, red, blue)")]
    [DataRow("conic-gradient(in oklch from 45deg, red, blue)")]
    [DataRow("conic-gradient(from 45deg in oklch longer hue, red, blue)")]
    public void BackgroundImageGradientInterpolationMethodAccepted(string gradient)
    {
        var property = CssConstructionFunctions.ParseDeclaration($"background-image: {gradient}");
        Assert.IsInstanceOfType<BackgroundImageProperty>(property);
        var backgroundImage = (BackgroundImageProperty)property;
        Assert.IsTrue(backgroundImage.HasValue);
        Assert.IsFalse(backgroundImage.IsInitial);
    }

    // A malformed or unsupported "in <color-interpolation-method>" prelude must now be DROPPED at parse
    // time (HasValue == false), instead of being accepted unvalidated and silently ignored (issue #245).
    [TestMethod]
    [DataRow("linear-gradient(in nonsense garbage, red, blue)")]  // the issue's example
    [DataRow("linear-gradient(in, red, blue)")]                   // "in" with no color space
    [DataRow("linear-gradient(in oklab longer hue, red, blue)")]  // hue method on a rectangular space
    [DataRow("linear-gradient(in oklch longer, red, blue)")]      // hue direction without the "hue" keyword
    [DataRow("linear-gradient(in oklab to bogus, red, blue)")]    // the direction half is invalid
    [DataRow("linear-gradient(in a98-rgb, red, blue)")]           // valid CSS space, unsupported here
    [DataRow("linear-gradient(in prophoto-rgb, red, blue)")]
    [DataRow("linear-gradient(in rec2020, red, blue)")]
    [DataRow("radial-gradient(in bogus, red, blue)")]
    [DataRow("radial-gradient(in oklab nonsense, red, blue)")]    // in <space> then an invalid shape
    [DataRow("conic-gradient(in bogus, red, blue)")]
    [DataRow("conic-gradient(in oklab from red, red, blue)")]     // in <space> then an invalid from-angle
    public void BackgroundImageGradientInterpolationMethodRejected(string gradient)
    {
        var property = CssConstructionFunctions.ParseDeclaration($"background-image: {gradient}");
        Assert.IsFalse(((BackgroundImageProperty)property).HasValue);
    }

    [TestMethod]
    [DataRow("linear-gradient(in srgb, red, blue)", Math.PI)]
    [DataRow("linear-gradient(in oklab to right, red, blue)", Math.PI / 2)]
    [DataRow("linear-gradient(to right in oklab, red, blue)", Math.PI / 2)]
    [DataRow("linear-gradient(in hsl longer hue 90deg, red, blue)", Math.PI / 2)]
    [DataRow("linear-gradient(90deg in oklch increasing hue, red, blue)", Math.PI / 2)]
    [DataRow("repeating-linear-gradient(in srgb, red, blue)", Math.PI)]
    public void ParseImageSkipsTheInterpolationMethod(string gradient, double angleRad)
    {
        var image = new CssValueParser(new MockAdapter()).ParseImage(gradient);

        var parsed = ((CssImage.LinearGradient)image).Gradient;
        Assert.AreEqual(angleRad, parsed.AngleRad, 1e-6);
        CollectionAssert.AreEqual(
            new RColor?[] { RColor.FromArgb(255, 0, 0), RColor.FromArgb(0, 0, 255) },
            parsed.Stops.Select(s => s.Color).ToArray());
    }

    [TestMethod]
    [DataRow("linear-gradient(in bogus, red, blue)")]
    [DataRow("linear-gradient(in oklab longer hue, red, blue)")]
    public void ParseImageRejectsAnInvalidInterpolationMethod(string gradient)
    {
        Assert.IsNull(new CssValueParser(new MockAdapter()).ParseImage(gradient));
    }
}
