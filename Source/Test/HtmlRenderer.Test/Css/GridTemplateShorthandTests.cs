using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class GridTemplateShorthandTests
{
    private static StyleDeclaration Style(string declaration) =>
        (StyleDeclaration)CssConstructionFunctions.ParseStyleSheet($"div {{ {declaration} }}").Rules.OfType<StyleRule>().Single().Style;

    // ── grid-template ────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-grid-2", "#explicit-grid-shorthand")]
    public void GridTemplate_None_ResetsAllThree()
    {
        var style = Style("grid-template: none;");
        Assert.AreEqual("none", style.GetPropertyValue("grid-template-rows"));
        Assert.AreEqual("none", style.GetPropertyValue("grid-template-columns"));
        Assert.AreEqual("none", style.GetPropertyValue("grid-template-areas"));
    }

    [TestMethod]
    [Spec("css-grid-2", "#explicit-grid-shorthand")]
    public void GridTemplate_RowsSlashColumns_SetsBothAxesAreasNone()
    {
        var style = Style("grid-template: 1fr 2fr / 100px 200px;");
        Assert.AreEqual("1fr 2fr", style.GetPropertyValue("grid-template-rows"));
        Assert.AreEqual("100px 200px", style.GetPropertyValue("grid-template-columns"));
        // An omitted axis is reset to its initial value (via the CSS-wide `initial` keyword).
        Assert.AreEqual("initial", style.GetPropertyValue("grid-template-areas"));
    }

    [TestMethod]
    [Spec("css-grid-2", "#explicit-grid-shorthand")]
    public void GridTemplate_AreasForm_SynthesizesRowsAndColumns()
    {
        var style = Style("grid-template: \"a a\" 40px \"b b\" / 1fr 1fr;");
        Assert.AreEqual("\"a a\" \"b b\"", style.GetPropertyValue("grid-template-areas"));
        Assert.AreEqual("40px auto", style.GetPropertyValue("grid-template-rows"));
        Assert.AreEqual("1fr 1fr", style.GetPropertyValue("grid-template-columns"));
    }

    [TestMethod]
    [Spec("css-grid-2", "#explicit-grid-shorthand")]
    public void GridTemplate_AreasForm_WithLineNames_PreservesThem()
    {
        var style = Style("grid-template: [r1] \"a\" 10px [r2] \"b\" [r3];");
        Assert.AreEqual("\"a\" \"b\"", style.GetPropertyValue("grid-template-areas"));
        Assert.AreEqual("[r1] 10px [r2] auto [r3]", style.GetPropertyValue("grid-template-rows"));
        // No explicit column track list → columns reset to its initial value.
        Assert.AreEqual("initial", style.GetPropertyValue("grid-template-columns"));
    }

    [TestMethod]
    [Spec("css-grid-2", "#explicit-grid-shorthand")]
    // `none` is a valid <grid-template-rows>/<grid-template-columns> value on either side of the slash.
    [DataRow("grid-template: none / 1fr 1fr;", "none", "1fr 1fr")]
    [DataRow("grid-template: 1fr 2fr / none;", "1fr 2fr", "none")]
    public void GridTemplate_NoneOnOneAxis_IsAccepted(string declaration, string expectedRows, string expectedColumns)
    {
        var style = Style(declaration);
        Assert.AreEqual(expectedRows, style.GetPropertyValue("grid-template-rows"));
        Assert.AreEqual(expectedColumns, style.GetPropertyValue("grid-template-columns"));
    }

    // ── grid ─────────────────────────────────────────────────────────────

    [TestMethod]
    [Spec("css-grid-2", "#grid-shorthand")]
    public void Grid_TemplateForm_ResetsAutoProperties()
    {
        var style = Style("grid: \"a\" / 1fr;");
        Assert.AreEqual("\"a\"", style.GetPropertyValue("grid-template-areas"));
        Assert.AreEqual("auto", style.GetPropertyValue("grid-template-rows"));
        Assert.AreEqual("1fr", style.GetPropertyValue("grid-template-columns"));
        // The grid-auto-* longhands are reset to their initial values (via the CSS-wide `initial`).
        Assert.AreEqual("initial", style.GetPropertyValue("grid-auto-flow"));
        Assert.AreEqual("initial", style.GetPropertyValue("grid-auto-rows"));
        Assert.AreEqual("initial", style.GetPropertyValue("grid-auto-columns"));
    }

    [TestMethod]
    [Spec("css-grid-2", "#grid-shorthand")]
    public void Grid_ColumnAutoFlowForm_SetsRowsFlowAndAutoColumns()
    {
        var style = Style("grid: 1fr / auto-flow 2fr;");
        Assert.AreEqual("1fr", style.GetPropertyValue("grid-template-rows"));
        Assert.AreEqual("column", style.GetPropertyValue("grid-auto-flow"));
        Assert.AreEqual("2fr", style.GetPropertyValue("grid-auto-columns"));
        Assert.AreEqual("initial", style.GetPropertyValue("grid-template-columns"));
    }

    [TestMethod]
    [Spec("css-grid-2", "#grid-shorthand")]
    public void Grid_RowAutoFlowForm_WithDense_SetsColumnsFlowAndAutoRows()
    {
        var style = Style("grid: auto-flow dense 10px / 1fr;");
        Assert.AreEqual("1fr", style.GetPropertyValue("grid-template-columns"));
        Assert.AreEqual("row dense", style.GetPropertyValue("grid-auto-flow"));
        Assert.AreEqual("10px", style.GetPropertyValue("grid-auto-rows"));
    }

    [TestMethod]
    [Spec("css-grid-2", "#grid-shorthand")]
    public void Grid_NoneRows_WithColumnAutoFlow_IsAccepted()
    {
        // The <grid-template-rows> side of the auto-flow form also accepts `none`.
        var style = Style("grid: none / auto-flow 1fr;");
        Assert.AreEqual("none", style.GetPropertyValue("grid-template-rows"));
        Assert.AreEqual("column", style.GetPropertyValue("grid-auto-flow"));
        Assert.AreEqual("1fr", style.GetPropertyValue("grid-auto-columns"));
    }

    // ── rejection (whole declaration dropped) ────────────────────────────

    [TestMethod]
    [Spec("css-grid-2", "#explicit-grid-shorthand")]
    [Spec("css-grid-2", "#grid-shorthand")]
    [DataRow("grid-template: 10px \"a\";")]            // track-size before the first string
    [DataRow("grid-template: \"a\" repeat(2, 1fr);")]  // repeat() is not a valid row <track-size>
    [DataRow("grid-template: \"a\" \"b\" / repeat(2, 1fr);")] // trailing columns are an <explicit-track-list> (no repeat())
    [DataRow("grid-template: \"a a\" \"b\";")]         // ragged area rows
    [DataRow("grid-template: 1fr / 2fr / 3fr;")]       // two top-level slashes
    [DataRow("grid-template: 1fr 2fr;")]               // a bare track list is not a valid grid-template
    [DataRow("grid: auto-flow / auto-flow;")]          // auto-flow on both sides
    [DataRow("grid: dense 10px / 1fr;")]               // dense without auto-flow
    public void InvalidValue_IsDropped(string declaration)
    {
        var style = Style(declaration);
        Assert.AreEqual(string.Empty, style.GetPropertyValue("grid-template-rows"));
        Assert.AreEqual(string.Empty, style.GetPropertyValue("grid-template-columns"));
    }

    // ── serialization: never reconstructed ───────────────────────────────

    [TestMethod]
    public void Expanded_DoesNotReconstructMegaShorthand()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("div { grid: auto-flow dense 10px / 1fr; }");
        var css = sheet.ToCss();

        Assert.IsFalse(css.Contains("grid:"));
        Assert.IsFalse(css.Contains("grid-template:"));
        Assert.IsTrue(css.Contains("grid-auto-flow"));
        Assert.IsTrue(css.Contains("grid-template-columns"));
    }

    // ── var() is kept whole and deferred to cascade time ─────────────────

    [TestMethod]
    public void GridTemplate_WithVar_IsNotExpandedAtParseTime()
    {
        var style = Style("grid-template: var(--t);");
        // The shorthand stays whole (var() can't be sliced at parse time), so the longhands are not set.
        Assert.AreEqual(string.Empty, style.GetPropertyValue("grid-template-rows"));
    }
}
