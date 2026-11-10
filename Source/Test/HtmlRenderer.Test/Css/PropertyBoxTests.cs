using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/Property.cs (the BoxShadow/Clip/Cursor/BoxDecorationBreak cases).
///
/// box-shadow (StyleProperties/Box/BoxShadowProperty.cs) is parsed by
/// Source/HtmlRenderer/Core/CssEngine/BoxShadowGrammar.cs, which validates the
/// <c>none | [ inset? &amp;&amp; &lt;length&gt;{2,4} &amp;&amp; &lt;color&gt;? ]#</c> grammar but then
/// stores the token stream's own reconstructed text verbatim as <c>Value</c>/<c>Original</c>
/// (BoxShadowValueConverter.cs: <c>CssText => Original.Text</c>) rather than a normalized
/// representation - so authored casing like "NONE"/"INITIAL" survives unchanged in both Value and
/// Original, while multi-space runs between tokens still collapse to one space because the tokenizer
/// itself only ever emits a single whitespace token between significant tokens. These assertions are
/// parse-only per the port task's caveat: box-shadow is a layout/paint no-op in this renderer, but that
/// does not affect what the parser records here.
///
/// clip (StyleProperties/Visibility/ClipProperty.cs) uses <c>Converters.ShapeConverter</c>
/// (Model/Converters.cs line 122), a <c>rect(&lt;length&gt;{4})</c> function grammar via the shared
/// LengthConverter, which normalizes zero-with-unit lengths ("0cm") down to unitless "0" the same way
/// the rest of the length grammar does (see BorderPropertyTests.cs's border-width port), while non-zero
/// units and comma placement are preserved from the authored value.
///
/// cursor (StyleProperties/CursorProperty.cs) accepts a list of <c>url()</c> image sources (each
/// optionally followed by an x/y hotspot pair) that must end in a plain keyword from <c>Map.Cursors</c>
/// (Model/Map.cs) - a url()-only list with no trailing keyword fallback is invalid, matching PeachPDF.
///
/// box-decoration-break (StyleProperties/Box/BoxDecorationBreak.cs) is a
/// <c>Toggle(Keywords.Clone, Keywords.Slice)</c> (Model/Converters.cs line 543) plus the standard
/// CSS-wide keyword chain, both value sets confirmed identical to the source file's expectations.
/// </summary>
[TestClass]
public sealed class PropertyBoxTests
{
    [TestMethod]
    public void CssBoxShadowOffsetLegal()
    {
        var snippet = "box-shadow:  5px 4px";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-shadow", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxShadowProperty>(property);
        var concrete = (BoxShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("5px 4px", concrete.Value);
        Assert.AreEqual("5px 4px", concrete.Original);
    }

    [TestMethod]
    public void CssBoxShadowInsetOffsetLegal()
    {
        var snippet = "box-shadow: inset 5px 4px";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-shadow", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxShadowProperty>(property);
        var concrete = (BoxShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("inset 5px 4px", concrete.Value);
        Assert.AreEqual("inset 5px 4px", concrete.Original);
    }

    [TestMethod]
    public void CssBoxShadowNoneUppercaseLegal()
    {
        var snippet = "box-shadow: NONE";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-shadow", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxShadowProperty>(property);
        var concrete = (BoxShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("NONE", concrete.Value);
        Assert.AreEqual("NONE", concrete.Original);
    }

    [TestMethod]
    public void CssBoxShadowNormalTealLegal()
    {
        var snippet = "box-shadow: 60px -16px teal";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-shadow", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxShadowProperty>(property);
        var concrete = (BoxShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("60px -16px teal", concrete.Value);
        Assert.AreEqual("60px -16px teal", concrete.Original);
    }

    [TestMethod]
    public void CssBoxShadowNormalSpreadBlackLegal()
    {
        var snippet = "box-shadow: 10px 5px 5px black";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-shadow", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxShadowProperty>(property);
        var concrete = (BoxShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("10px 5px 5px black", concrete.Value);
        Assert.AreEqual("10px 5px 5px black", concrete.Original);
    }

    [TestMethod]
    public void CssBoxShadowOliveAndRedLegal()
    {
        var snippet = "box-shadow: 3px 3px red, -1em 0 0.4em olive";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-shadow", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxShadowProperty>(property);
        var concrete = (BoxShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("3px 3px red, -1em 0 0.4em olive", concrete.Value);
        Assert.AreEqual("3px 3px red, -1em 0 0.4em olive", concrete.Original);
    }

    [TestMethod]
    public void CssBoxShadowInsetGoldLegal()
    {
        var snippet = "box-shadow: inset 5em 1em gold";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-shadow", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxShadowProperty>(property);
        var concrete = (BoxShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("inset 5em 1em gold", concrete.Value);
        Assert.AreEqual("inset 5em 1em gold", concrete.Original);
    }

    [TestMethod]
    public void CssBoxShadowZeroGoldLegal()
    {
        var snippet = "box-shadow: 0 0 1em gold";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-shadow", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxShadowProperty>(property);
        var concrete = (BoxShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0 0 1em gold", concrete.Value);
        Assert.AreEqual("0 0 1em gold", concrete.Original);
    }

    [TestMethod]
    public void CssBoxShadowInsetZeroGoldLegal()
    {
        var snippet = "box-shadow: inset  0 0 1em gold";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-shadow", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxShadowProperty>(property);
        var concrete = (BoxShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("inset 0 0 1em gold", concrete.Value);
        Assert.AreEqual("inset 0 0 1em gold", concrete.Original);
    }

    [TestMethod]
    public void CssBoxShadowInsetZeroGoldAndNormalRedLegal()
    {
        var snippet = "box-shadow: inset  0 0 1em  gold   ,  0 0   1em   red !important";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-shadow", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<BoxShadowProperty>(property);
        var concrete = (BoxShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("inset 0 0 1em gold, 0 0 1em red", concrete.Value);
        Assert.AreEqual("inset 0 0 1em gold, 0 0 1em red", concrete.Original);
    }

    [TestMethod]
    public void CssBoxShadowOffsetColorLegal()
    {
        var snippet = "box-shadow:  5px 4px #000";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-shadow", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxShadowProperty>(property);
        var concrete = (BoxShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("5px 4px #000", concrete.Value);
        Assert.AreEqual("5px 4px #000", concrete.Original);
    }

    [TestMethod]
    public void CssBoxShadowOffsetBlurColorLegal()
    {
        var snippet = "box-shadow:  5px 4px 2px #000";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-shadow", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxShadowProperty>(property);
        var concrete = (BoxShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("5px 4px 2px #000", concrete.Value);
        Assert.AreEqual("5px 4px 2px #000", concrete.Original);
    }

    [TestMethod]
    public void CssBoxShadowInitialUppercaseLegal()
    {
        var snippet = "box-shadow:  INITIAL";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-shadow", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxShadowProperty>(property);
        var concrete = (BoxShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("initial", concrete.Value);
        Assert.AreEqual("INITIAL", concrete.Original);
    }

    [TestMethod]
    public void CssBoxShadowOffsetIllegal()
    {
        var snippet = "box-shadow:  5px 4px 2px 1px 3px #f00";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-shadow", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxShadowProperty>(property);
        var concrete = (BoxShadowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssClipShapeLegal()
    {
        var snippet = "clip: rect( 2px, 3em, 1in, 0cm )";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("clip", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ClipProperty>(property);
        var concrete = (ClipProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rect(2px, 3em, 1in, 0)", concrete.Value);
        Assert.AreEqual("rect( 2px, 3em, 1in, 0cm )", concrete.Original);
    }

    [TestMethod]
    public void CssClipShapeBackwards()
    {
        var snippet = "clip: rect( 2px 3em 1in 0cm )";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("clip", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ClipProperty>(property);
        var concrete = (ClipProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rect(2px 3em 1in 0)", concrete.Value);
        Assert.AreEqual("rect( 2px 3em 1in 0cm )", concrete.Original);
    }

    [TestMethod]
    public void CssClipShapeZerosLegal()
    {
        var snippet = "clip: rect(0, 0, 0, 0)";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("clip", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ClipProperty>(property);
        var concrete = (ClipProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rect(0, 0, 0, 0)", concrete.Value);
        Assert.AreEqual("rect(0, 0, 0, 0)", concrete.Original);
    }

    [TestMethod]
    public void CssClipShapeZerosIllegal()
    {
        var snippet = "clip: rect(0, 0, 0 0)";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("clip", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ClipProperty>(property);
        var concrete = (ClipProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssClipShapeNonZerosIllegal()
    {
        var snippet = "clip: rect(2px, 1cm, 5mm)";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("clip", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ClipProperty>(property);
        var concrete = (ClipProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssClipShapeSingleValueIllegal()
    {
        var snippet = "clip: rect(1em)";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("clip", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ClipProperty>(property);
        var concrete = (ClipProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssCursorDefaultUppercaseLegal()
    {
        var snippet = "cursor: DEFAULT";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("cursor", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<CursorProperty>(property);
        var concrete = (CursorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("default", concrete.Value);
        Assert.AreEqual("DEFAULT", concrete.Original);
    }

    [TestMethod]
    public void CssCursorAutoLegal()
    {
        var snippet = "cursor: auto";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("cursor", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<CursorProperty>(property);
        var concrete = (CursorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("auto", concrete.Value);
        Assert.AreEqual("auto", concrete.Original);
    }

    [TestMethod]
    public void CssCursorZoomOutLegal()
    {
        var snippet = "cursor  : zoom-out";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("cursor", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<CursorProperty>(property);
        var concrete = (CursorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("zoom-out", concrete.Value);
        Assert.AreEqual("zoom-out", concrete.Original);
    }

    [TestMethod]
    public void CssCursorUrlNoFallbackIllegal()
    {
        var snippet = "cursor  : url(foo.png)";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("cursor", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<CursorProperty>(property);
        var concrete = (CursorProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssCursorUrlLegal()
    {
        var snippet = "cursor  : url(foo.png), default";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("cursor", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<CursorProperty>(property);
        var concrete = (CursorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"foo.png\"), default", concrete.Value);
        Assert.AreEqual("url(\"foo.png\"), default", concrete.Original);
    }

    [TestMethod]
    public void CssCursorUrlShiftedLegal()
    {
        var snippet = "cursor  : url(foo.png) 0 5, auto";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("cursor", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<CursorProperty>(property);
        var concrete = (CursorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"foo.png\") 0 5, auto", concrete.Value);
        Assert.AreEqual("url(\"foo.png\") 0 5, auto", concrete.Original);
    }

    [TestMethod]
    public void CssCursorUrlShiftedNoFallbackIllegal()
    {
        var snippet = "cursor  : url(foo.png) 0 5";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("cursor", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<CursorProperty>(property);
        var concrete = (CursorProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssCursorUrlsLegal()
    {
        var snippet = "cursor  : url(foo.png), url(master.png), url(more.png), wait";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("cursor", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<CursorProperty>(property);
        var concrete = (CursorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"foo.png\"), url(\"master.png\"), url(\"more.png\"), wait", concrete.Value);
        Assert.AreEqual("url(\"foo.png\"), url(\"master.png\"), url(\"more.png\"), wait", concrete.Original);
    }

    [TestMethod]
    [Spec("css-break-3", "#break-decoration")]
    public void CssBoxDecorationBreakNumberIllegal()
    {
        var snippet = "box-decoration-break : 1.5 ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-decoration-break", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxDecorationBreak>(property);
        var concrete = (BoxDecorationBreak)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-break-3", "#break-decoration")]
    public void CssBoxDecorationBreakSliceLegal()
    {
        var snippet = "box-decoration-break : slice ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-decoration-break", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxDecorationBreak>(property);
        var concrete = (BoxDecorationBreak)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("slice", concrete.Value);
        Assert.AreEqual("slice", concrete.Original);
    }

    [TestMethod]
    [Spec("css-break-3", "#break-decoration")]
    public void CssBoxDecorationBreakClonePascalLegal()
    {
        var snippet = "box-decoration-break : Clone ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-decoration-break", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BoxDecorationBreak>(property);
        var concrete = (BoxDecorationBreak)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("clone", concrete.Value);
        Assert.AreEqual("Clone", concrete.Original);
    }

    [TestMethod]
    [Spec("css-break-3", "#break-decoration")]
    public void CssBoxDecorationBreakInheritLegal()
    {
        var snippet = "box-decoration-break : inherit!important ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("box-decoration-break", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<BoxDecorationBreak>(property);
        var concrete = (BoxDecorationBreak)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("inherit", concrete.Value);
        Assert.AreEqual("inherit", concrete.Original);
    }

    // The rest of the CSS-wide keywords on box-decoration-break.
    [TestMethod]
    [DataRow("initial")]
    [DataRow("unset")]
    [DataRow("revert")]
    [DataRow("revert-layer")]
    [Spec("css-break-3", "#break-decoration")]
    public void CssBoxDecorationBreakGlobalKeywordLegal(string keyword)
    {
        var property = ParseDeclaration($"box-decoration-break:{keyword}");
        Assert.AreEqual("box-decoration-break", property.Name);
        Assert.IsInstanceOfType<BoxDecorationBreak>(property);
        var concrete = (BoxDecorationBreak)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual(keyword, concrete.Value);
    }

    // slice/clone is the whole value set - anything else is invalid CSS and dropped at parse time.
    [TestMethod]
    [DataRow("none")]
    [DataRow("both")]
    [DataRow("slice clone")]
    [Spec("css-break-3", "#break-decoration")]
    public void CssBoxDecorationBreakRejectsNonSpecValue(string value)
    {
        var property = ParseDeclaration($"box-decoration-break:{value}");
        Assert.AreEqual("box-decoration-break", property.Name);
        Assert.IsInstanceOfType<BoxDecorationBreak>(property);
        var concrete = (BoxDecorationBreak)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }
}
