using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// The &lt;basic-shape&gt; grammar of <see cref="BasicShapeGrammar"/> and the clip-path values built on it.
/// The <see cref="BasicShapeGrammar"/> and the <c>clip-path</c> values it validates.
/// </summary>
[TestClass]
public sealed class BasicShapeGrammarTests
{
    private static BasicShapeGrammar.ParsedBasicShape Parse(string value) =>
        BasicShapeGrammar.TryParse(CssValueParser.GetCssTokens(value));

    // ─── polygon() ─────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-shapes-1", "#supported-basic-shapes")]
    public void Polygon_ThreePoints_ParsesWithDefaultNonzeroFill()
    {
        var shape = Parse("polygon(0 0, 100% 0, 50% 100%)");

        Assert.IsNotNull(shape);
        Assert.AreEqual(BasicShapeGrammar.BasicShapeKind.Polygon, shape.Kind);
        Assert.AreEqual(BasicShapeGrammar.FillRule.Nonzero, shape.PolygonFillRule);
        Assert.AreEqual(3, shape.PolygonPoints.Count);
        Assert.AreEqual(("0", "0"), (shape.PolygonPoints[0].X, shape.PolygonPoints[0].Y));
        Assert.AreEqual(("100%", "0"), (shape.PolygonPoints[1].X, shape.PolygonPoints[1].Y));
        Assert.AreEqual(("50%", "100%"), (shape.PolygonPoints[2].X, shape.PolygonPoints[2].Y));
    }

    [TestMethod]
    [DataRow("polygon(evenodd, 0 0, 10px 0, 0 10px)", true)]
    [DataRow("polygon(nonzero, 0 0, 10px 0, 0 10px)", false)]
    public void Polygon_ExplicitFillRule_IsCaptured(string value, bool expectEvenodd)
    {
        var shape = Parse(value);

        Assert.IsNotNull(shape);
        var expected = expectEvenodd ? BasicShapeGrammar.FillRule.Evenodd : BasicShapeGrammar.FillRule.Nonzero;
        Assert.AreEqual(expected, shape.PolygonFillRule);
        Assert.AreEqual(3, shape.PolygonPoints.Count);
    }

    [TestMethod]
    public void Polygon_CalcComponent_IsAcceptedAndPreserved()
    {
        // A calc()-family expression is a valid <length-percentage> polygon vertex component
        // (resolved at render time). The whole shape must stay valid and the calc text preserved.
        var shape = Parse("polygon(0% calc(100% * 0.65), 100% 100%, 0% 100%)");

        Assert.IsNotNull(shape);
        Assert.AreEqual(BasicShapeGrammar.BasicShapeKind.Polygon, shape.Kind);
        Assert.AreEqual(3, shape.PolygonPoints.Count);
        Assert.AreEqual("0%", shape.PolygonPoints[0].X);
        Assert.IsTrue(shape.PolygonPoints[0].Y.StartsWith("calc("));
        Assert.IsTrue(shape.PolygonPoints[0].Y.Contains("0.65"));
        Assert.AreEqual(("100%", "100%"), (shape.PolygonPoints[1].X, shape.PolygonPoints[1].Y));
        Assert.AreEqual(("0%", "100%"), (shape.PolygonPoints[2].X, shape.PolygonPoints[2].Y));
    }

    [TestMethod]
    [DataRow("polygon(min(10px, 20px) 0, 100% 0, 0 100%)")]
    [DataRow("polygon(0 max(10%, 5px), 100% 0, 0 100%)")]
    [DataRow("polygon(clamp(0px, 50%, 100px) 0, 100% 0, 0 100%)")]
    public void Polygon_CalcFamilyFunctions_AreAccepted(string value)
    {
        var shape = Parse(value);
        Assert.IsNotNull(shape);
        Assert.AreEqual(3, shape.PolygonPoints.Count);
    }

    [TestMethod]
    public void Polygon_SinglePoint_IsValid()
    {
        var shape = Parse("polygon(50% 50%)");

        Assert.IsNotNull(shape);
        Assert.AreEqual(1, shape.PolygonPoints.Count);
    }

    [TestMethod]
    [DataRow("polygon()")]                      // no points
    [DataRow("polygon(0)")]                     // odd token count in a pair
    [DataRow("polygon(0 0, 100%)")]             // second pair incomplete
    [DataRow("polygon(banana, 0 0)")]           // bad fill-rule ident
    [DataRow("polygon(evenodd)")]               // fill-rule but no points
    [DataRow("polygon(0 0,, 10px 10px)")]       // empty middle group
    [DataRow("polygon(red 0, 10px 0)")]         // non-length component
    public void Polygon_Malformed_ReturnsNull(string value)
    {
        Assert.IsNull(Parse(value));
    }

    // ─── inset() ───────────────────────────────────────────────────────────

    [TestMethod]
    [DataRow("inset(10px)", "10px", "10px", "10px", "10px")]
    [DataRow("inset(10px 20px)", "10px", "20px", "10px", "20px")]
    [DataRow("inset(10px 20px 30px)", "10px", "20px", "30px", "20px")]
    [DataRow("inset(1px 2px 3px 4px)", "1px", "2px", "3px", "4px")]
    [Spec("css-shapes-1", "#supported-basic-shapes")]
    public void Inset_ShorthandFill_ExpandsToTopRightBottomLeft(string value, string top, string right, string bottom, string left)
    {
        var shape = Parse(value);

        Assert.IsNotNull(shape);
        Assert.AreEqual(BasicShapeGrammar.BasicShapeKind.Inset, shape.Kind);
        CollectionAssert.AreEqual(new[] { top, right, bottom, left }, shape.InsetEdges.ToArray());
        Assert.IsFalse(shape.InsetHasRound);
    }

    [TestMethod]
    public void Inset_WithRound_CapturesRadiusButFlagsRound()
    {
        var shape = Parse("inset(10px round 5px)");

        Assert.IsNotNull(shape);
        CollectionAssert.AreEqual(new[] { "10px", "10px", "10px", "10px" }, shape.InsetEdges.ToArray());
        Assert.IsTrue(shape.InsetHasRound);
        Assert.IsTrue(shape.InsetRoundRadius.Count > 0);
    }

    [TestMethod]
    public void Inset_PercentageEdges_AreValid()
    {
        var shape = Parse("inset(10% 20%)");

        Assert.IsNotNull(shape);
        CollectionAssert.AreEqual(new[] { "10%", "20%", "10%", "20%" }, shape.InsetEdges.ToArray());
    }

    [TestMethod]
    [DataRow("inset()")]                    // no offsets
    [DataRow("inset(1px 2px 3px 4px 5px)")] // more than 4 offsets
    [DataRow("inset(10px round)")]          // round with no radius
    [DataRow("inset(red)")]                 // non-length offset
    public void Inset_Malformed_ReturnsNull(string value)
    {
        Assert.IsNull(Parse(value));
    }

    // ─── circle() ──────────────────────────────────────────────────────────

    [TestMethod]
    public void Circle_Empty_DefaultsClosestSideAndCenter()
    {
        var shape = Parse("circle()");

        Assert.IsNotNull(shape);
        Assert.AreEqual(BasicShapeGrammar.BasicShapeKind.Circle, shape.Kind);
        Assert.AreEqual(BasicShapeGrammar.ShapeRadiusKind.ClosestSide, shape.RadiusX.Kind);
        Assert.AreEqual("50%", shape.CenterX);
        Assert.AreEqual("50%", shape.CenterY);
    }

    [TestMethod]
    public void Circle_FarthestSide_Keyword()
    {
        var shape = Parse("circle(farthest-side)");

        Assert.IsNotNull(shape);
        Assert.AreEqual(BasicShapeGrammar.ShapeRadiusKind.FarthestSide, shape.RadiusX.Kind);
    }

    [TestMethod]
    public void Circle_ExplicitRadius_AndPosition()
    {
        var shape = Parse("circle(50px at 10px 20px)");

        Assert.IsNotNull(shape);
        Assert.AreEqual(BasicShapeGrammar.ShapeRadiusKind.LengthPercentage, shape.RadiusX.Kind);
        Assert.AreEqual("50px", shape.RadiusX.Length);
        Assert.AreEqual("10px", shape.CenterX);
        Assert.AreEqual("20px", shape.CenterY);
    }

    [TestMethod]
    public void Circle_PositionKeywords_ResolveToPercentages()
    {
        var shape = Parse("circle(closest-side at left top)");

        Assert.IsNotNull(shape);
        Assert.AreEqual("0%", shape.CenterX);
        Assert.AreEqual("0%", shape.CenterY);
    }

    [TestMethod]
    public void Circle_PositionRightBottom_ResolveToHundredPercent()
    {
        var shape = Parse("circle(at right bottom)");

        Assert.IsNotNull(shape);
        Assert.AreEqual("100%", shape.CenterX);
        Assert.AreEqual("100%", shape.CenterY);
    }

    [TestMethod]
    [DataRow("circle(10px 20px)")]   // two radii (that's ellipse)
    [DataRow("circle(at)")]          // "at" with no position
    [DataRow("circle(banana)")]      // junk radius
    [DataRow("circle(-5px)")]        // negative <shape-radius> is invalid
    [DataRow("circle(-10% at center)")]
    [DataRow("ellipse(-5px 10px)")]  // a negative axis radius invalidates the whole shape
    [Spec("css-shapes-1", "#supported-basic-shapes")]
    public void Circle_Malformed_ReturnsNull(string value)
    {
        Assert.IsNull(Parse(value));
    }

    // ─── ellipse() ─────────────────────────────────────────────────────────

    [TestMethod]
    public void Ellipse_Empty_DefaultsBothRadiiClosestSide()
    {
        var shape = Parse("ellipse()");

        Assert.IsNotNull(shape);
        Assert.AreEqual(BasicShapeGrammar.BasicShapeKind.Ellipse, shape.Kind);
        Assert.AreEqual(BasicShapeGrammar.ShapeRadiusKind.ClosestSide, shape.RadiusX.Kind);
        Assert.AreEqual(BasicShapeGrammar.ShapeRadiusKind.ClosestSide, shape.RadiusY.Kind);
    }

    [TestMethod]
    [Spec("css-shapes-1", "#supported-basic-shapes")]
    public void Ellipse_TwoRadii_AndPosition()
    {
        var shape = Parse("ellipse(40px 20% at 25% 75%)");

        Assert.IsNotNull(shape);
        Assert.AreEqual("40px", shape.RadiusX.Length);
        Assert.AreEqual("20%", shape.RadiusY.Length);
        Assert.AreEqual("25%", shape.CenterX);
        Assert.AreEqual("75%", shape.CenterY);
    }

    [TestMethod]
    public void Ellipse_MixedKeywordRadii()
    {
        var shape = Parse("ellipse(closest-side farthest-side at right bottom)");

        Assert.IsNotNull(shape);
        Assert.AreEqual(BasicShapeGrammar.ShapeRadiusKind.ClosestSide, shape.RadiusX.Kind);
        Assert.AreEqual(BasicShapeGrammar.ShapeRadiusKind.FarthestSide, shape.RadiusY.Kind);
        Assert.AreEqual("100%", shape.CenterX);
        Assert.AreEqual("100%", shape.CenterY);
    }

    [TestMethod]
    [DataRow("ellipse(40px 20px 10px)")]
    [Spec("css-shapes-1", "#supported-basic-shapes")]
    public void Ellipse_Malformed_ReturnsNull(string value)
    {
        Assert.IsNull(Parse(value));
    }

    [Ignore("Gap, see Spec/css-shapes-1.yml")]
    [TestMethod]
    [DataRow("ellipse(40px)")]
    [DataRow("ellipse(closest-side)")]
    [Spec("css-shapes-1", "#supported-basic-shapes")]
    public void Ellipse_OneRadiusIsValid(string value)
    {
        Assert.IsNotNull(Parse(value));
    }

    [Ignore("Gap, see Spec/css-shapes-1.yml")]
    [TestMethod]
    [DataRow("rect(0 auto 10px 5%)")]
    [DataRow("xywh(10px 10px 50% 20px round 4px)")]
    [DataRow("polygon(round 5px, 0 0, 100% 0, 50% 100%)")]
    [DataRow("path(\"M 0 0 L 10 0 L 0 10 Z\")")]
    [Spec("css-shapes-1", "#supported-basic-shapes")]
    public void RectXywhPathAndRoundedPolygon_AreBasicShapes(string value)
    {
        Assert.IsNotNull(Parse(value));
    }

    [Ignore("Gap, see Spec/css-shapes-1.yml")]
    [TestMethod]
    [Spec("css-shapes-1", "#shape-function")]
    public void ShapeFunction_IsABasicShape()
    {
        Assert.IsNotNull(Parse("shape(from 0 0, line to 100% 0, line to 50% 100%, close)"));
    }

    [TestMethod]
    [DataRow("none")]
    [DataRow("banana")]
    [DataRow("url(#clip)")]
    public void NonBasicShape_ReturnsNull(string value)
    {
        Assert.IsNull(Parse(value));
    }

    // ─── clip-path ──────────────────────────────────────────────────────────

    [TestMethod]
    [DataRow("none")]
    [DataRow("polygon(0 0, 100% 0, 50% 100%)")]
    [DataRow("inset(10px 20px round 4px)")]
    [DataRow("circle(50px at center)")]
    [DataRow("ellipse(closest-side farthest-side)")]
    [Spec("css-masking-1", "#the-clip-path")]
    public void ClipPath_ValidValue_SurvivesParsing(string value)
    {
        var property = CssConstructionFunctions.ParseDeclaration($"clip-path: {value}");

        Assert.IsInstanceOfType<ClipPathProperty>(property);
        var concrete = (ClipPathProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual(value, concrete.Value);
    }

    [TestMethod]
    [DataRow("banana")]
    [DataRow("polygon(0)")]
    [DataRow("circle(10px 20px)")]
    [Spec("css-masking-1", "#the-clip-path")]
    public void ClipPath_InvalidValue_IsDropped(string value)
    {
        var property = CssConstructionFunctions.ParseDeclaration($"clip-path: {value}");

        Assert.IsInstanceOfType<ClipPathProperty>(property);
        Assert.IsFalse(((ClipPathProperty)property).HasValue);
    }

    [TestMethod]
    [Spec("css-masking-1", "#the-clip-path")]
    public void ClipPath_InStyleSheet_ValidSurvives_InvalidDropped()
    {
        var valid = CssConstructionFunctions.ParseStyleSheet("div{clip-path:polygon(0 0, 100% 0, 50% 100%)}");
        var validRule = valid.Rules.OfType<StyleRule>().Single();
        Assert.AreEqual("polygon(0 0, 100% 0, 50% 100%)", validRule.Style.GetPropertyValue("clip-path"));

        var invalid = CssConstructionFunctions.ParseStyleSheet("div{clip-path:banana}");
        var invalidRule = invalid.Rules.OfType<StyleRule>().Single();
        Assert.AreEqual(string.Empty, invalidRule.Style.GetPropertyValue("clip-path"));
    }
}
