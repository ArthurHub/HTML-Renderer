using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// The css-break-3 <c>break-*</c> properties and the CSS 2.1 <c>page-break-*</c> ones, which accept
/// different value sets.
/// </summary>
[TestClass]
public sealed class PropertyBreakTests
{
    [TestMethod]
    public void CssBreakAfterLegalAvoid()
    {
        var snippet = "break-after:avoid";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("break-after", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BreakAfterProperty>(property);
        var concrete = (BreakAfterProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("avoid", concrete.Value);
        Assert.AreEqual("avoid", concrete.Original);
    }

    [TestMethod]
    [Spec("css21", "page.html#page-break-props")]
    public void CssPageBreakAfterLegalAvoid()
    {
        var snippet = "page-break-after:avoid";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("page-break-after", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PageBreakAfterProperty>(property);
        var concrete = (PageBreakAfterProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("avoid", concrete.Value);
        Assert.AreEqual("avoid", concrete.Original);
    }

    [TestMethod]
    public void CssBreakAfterLegalPageCapital()
    {
        var snippet = "break-after:Page";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("break-after", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BreakAfterProperty>(property);
        var concrete = (BreakAfterProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("page", concrete.Value);
        Assert.AreEqual("Page", concrete.Original);
    }

    [TestMethod]
    [Spec("css21", "page.html#page-break-props")]
    public void CssPageBreakAfterIllegalAvoidColumn()
    {
        var snippet = "page-break-after:avoid-column";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("page-break-after", property.Name);
        Assert.IsFalse(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PageBreakAfterProperty>(property);
        var concrete = (PageBreakAfterProperty)property;
        Assert.IsFalse(concrete.IsInherited);
    }

    [TestMethod]
    public void CssBreakAfterLegalAvoidColumn()
    {
        var snippet = "break-after:avoid-column";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("break-after", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BreakAfterProperty>(property);
        var concrete = (BreakAfterProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("avoid-column", concrete.Value);
        Assert.AreEqual("avoid-column", concrete.Original);
    }

    [TestMethod]
    public void CssBreakBeforeLegalAuto()
    {
        var snippet = "break-before:AUTO";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("break-before", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BreakBeforeProperty>(property);
        var concrete = (BreakBeforeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("auto", concrete.Value);
        Assert.AreEqual("AUTO", concrete.Original);
    }

    [TestMethod]
    [Spec("css21", "page.html#page-break-props")]
    public void CssPageBreakBeforeLegalAuto()
    {
        var snippet = "page-break-before:AUTO";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("page-break-before", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PageBreakBeforeProperty>(property);
        var concrete = (PageBreakBeforeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("auto", concrete.Value);
        Assert.AreEqual("AUTO", concrete.Original);
    }

    [TestMethod]
    [Spec("css21", "page.html#page-break-props")]
    public void CssPageBreakBeforeLegalLeft()
    {
        var snippet = "page-break-before:left";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("page-break-before", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PageBreakBeforeProperty>(property);
        var concrete = (PageBreakBeforeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("left", concrete.Value);
        Assert.AreEqual("left", concrete.Original);
    }

    [TestMethod]
    public void CssBreakBeforeIllegalValue()
    {
        var snippet = "break-before:whatever";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("break-before", property.Name);
        Assert.IsFalse(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BreakBeforeProperty>(property);
        var concrete = (BreakBeforeProperty)property;
        Assert.IsNotNull(concrete);
    }

    [TestMethod]
    public void CssBreakInsideIllegalPage()
    {
        var snippet = "break-inside:page";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("break-inside", property.Name);
        Assert.IsFalse(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BreakInsideProperty>(property);
        var concrete = (BreakInsideProperty)property;
        Assert.IsNotNull(concrete);
    }

    [TestMethod]
    public void CssBreakInsideLegalAvoidRegionUppercase()
    {
        var snippet = "break-inside:avoid-REGION";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("break-inside", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BreakInsideProperty>(property);
        var concrete = (BreakInsideProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("avoid-region", concrete.Value);
        Assert.AreEqual("avoid-REGION", concrete.Original);
    }

    [TestMethod]
    [Spec("css21", "page.html#page-break-props")]
    public void CssPageBreakInsideLegalAvoid()
    {
        var snippet = "page-break-inside:avoid";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("page-break-inside", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PageBreakInsideProperty>(property);
        var concrete = (PageBreakInsideProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("avoid", concrete.Value);
        Assert.AreEqual("avoid", concrete.Original);
    }

    [TestMethod]
    [Spec("css21", "page.html#page-break-props")]
    public void CssPageBreakInsideLegalAutoUppercase()
    {
        var snippet = "page-break-inside:AUTO";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("page-break-inside", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PageBreakInsideProperty>(property);
        var concrete = (PageBreakInsideProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("auto", concrete.Value);
        Assert.AreEqual("AUTO", concrete.Original);
    }

    [TestMethod]
    [DataRow("break-before")]
    [DataRow("break-after")]
    public void BreakBeforeAfter_AcceptsSupportedSpecValues(string name)
    {
        string[] supportedValues =
        [
            "auto", "avoid", "avoid-page", "page", "left", "right", "avoid-column", "column"
        ];

        foreach (var value in supportedValues)
        {
            var property = ParseDeclaration($"{name}:{value}");
            Assert.IsTrue(property.HasValue, $"{name}:{value} should be valid");
            Assert.AreEqual(value, property.Value);
        }
    }

    // css-break-3 values the engine doesn't implement.
    [TestMethod]
    [DataRow("break-before", "recto")]
    [DataRow("break-before", "verso")]
    [DataRow("break-before", "avoid-region")]
    [DataRow("break-before", "region")]
    [DataRow("break-after", "recto")]
    [DataRow("break-after", "verso")]
    [DataRow("break-after", "avoid-region")]
    [DataRow("break-after", "region")]
    public void BreakBeforeAfter_RejectsUnimplementedSpecValues(string name, string value)
    {
        var property = ParseDeclaration($"{name}:{value}");
        Assert.IsFalse(property.HasValue, $"{name}:{value} is not implemented by Map.BreakModes and should be rejected");
    }

    [TestMethod]
    [DataRow("break-before")]
    [DataRow("break-after")]
    public void BreakBeforeAfter_IsCaseInsensitiveAndKeepsAuthoredText(string name)
    {
        var property = ParseDeclaration($"{name}:AvOiD-PaGe");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("avoid-page", property.Value);
        Assert.AreEqual("AvOiD-PaGe", property.Original);
    }

    [TestMethod]
    [DataRow("break-before", "all")]
    [DataRow("break-after", "all")]
    [DataRow("break-before", "avoid-nothing")]
    public void BreakBeforeAfter_RejectsNonSpecValue(string name, string value)
    {
        var property = ParseDeclaration($"{name}:{value}");
        Assert.IsFalse(property.HasValue);
    }

    // css-break-3 only has "always" on the page-break-* aliases, the engine also takes it here.
    [TestMethod]
    [DataRow("break-before")]
    [DataRow("break-after")]
    public void BreakBeforeAfter_AcceptsAlwaysUnlikePeachPdf(string name)
    {
        var property = ParseDeclaration($"{name}:always");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("always", property.Value);
    }

    [TestMethod]
    [DataRow("auto")]
    [DataRow("avoid")]
    [DataRow("avoid-page")]
    [DataRow("avoid-column")]
    [DataRow("avoid-region")]
    public void BreakInside_AcceptsSpecValue(string value)
    {
        var property = ParseDeclaration($"break-inside:{value}");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual(value, property.Value);
    }

    [TestMethod]
    [DataRow("page")]
    [DataRow("column")]
    [DataRow("region")]
    [DataRow("left")]
    [DataRow("recto")]
    [DataRow("always")]
    public void BreakInside_RejectsNonSpecValue(string value)
    {
        var property = ParseDeclaration($"break-inside:{value}");
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [DataRow("page-break-before", "auto")]
    [DataRow("page-break-before", "always")]
    [DataRow("page-break-before", "avoid")]
    [DataRow("page-break-before", "left")]
    [DataRow("page-break-before", "right")]
    [DataRow("page-break-after", "auto")]
    [DataRow("page-break-after", "always")]
    [DataRow("page-break-after", "avoid")]
    [DataRow("page-break-after", "left")]
    [DataRow("page-break-after", "right")]
    [Spec("css21", "page.html#page-break-props")]
    public void PageBreakBeforeAfter_AcceptsLegacyValue(string name, string value)
    {
        var property = ParseDeclaration($"{name}:{value}");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual(value, property.Value);
    }

    [TestMethod]
    [DataRow("page-break-before", "page")]
    [DataRow("page-break-before", "recto")]
    [DataRow("page-break-before", "verso")]
    [DataRow("page-break-before", "column")]
    [DataRow("page-break-before", "region")]
    [DataRow("page-break-after", "page")]
    [DataRow("page-break-after", "avoid-page")]
    [DataRow("page-break-after", "verso")]
    [Spec("css21", "page.html#page-break-props")]
    public void PageBreakBeforeAfter_RejectsModernOnlyValue(string name, string value)
    {
        var property = ParseDeclaration($"{name}:{value}");
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [DataRow("auto")]
    [DataRow("avoid")]
    [Spec("css21", "page.html#page-break-props")]
    public void PageBreakInside_AcceptsLegacyValue(string value)
    {
        var property = ParseDeclaration($"page-break-inside:{value}");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual(value, property.Value);
    }

    [TestMethod]
    [DataRow("avoid-page")]
    [DataRow("avoid-column")]
    [DataRow("avoid-region")]
    [DataRow("page")]
    [Spec("css21", "page.html#page-break-props")]
    public void PageBreakInside_RejectsNonLegacyValue(string value)
    {
        var property = ParseDeclaration($"page-break-inside:{value}");
        Assert.IsFalse(property.HasValue);
    }
}
