using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class PropertyContentTests
{
    [TestMethod]
    public void CssColorHexLegal()
    {
        var snippet = "color : #123456";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColorProperty>(property);
        var concrete = (ColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(18, 52, 86)", concrete.Value);
        Assert.AreEqual("#123456", concrete.Original);
    }

    [TestMethod]
    public void CssColorRgbLegal()
    {
        var snippet = "color : rgb(121, 181, 201)";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColorProperty>(property);
        var concrete = (ColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(121, 181, 201)", concrete.Value);
        Assert.AreEqual("rgb(121, 181, 201)", concrete.Original);
    }

    [TestMethod]
    public void CssColorRgbaLegal()
    {
        var snippet = "color : rgba(255, 255, 201, 0.7)";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColorProperty>(property);
        var concrete = (ColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgba(255, 255, 201, 0.7)", concrete.Value);
        Assert.AreEqual("rgba(255, 255, 201, 0.7)", concrete.Original);
    }

    [TestMethod]
    public void CssColorNameLegal()
    {
        var snippet = "color : red";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColorProperty>(property);
        var concrete = (ColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(255, 0, 0)", concrete.Value);
        Assert.AreEqual("red", concrete.Original);
    }

    [TestMethod]
    public void CssColorNameUppercaseLegal()
    {
        var snippet = "color : BLUE";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColorProperty>(property);
        var concrete = (ColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(0, 0, 255)", concrete.Value);
        Assert.AreEqual("BLUE", concrete.Original);
    }

    [TestMethod]
    public void CssColorNameIllegal()
    {
        var snippet = "color : horse";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColorProperty>(property);
        var concrete = (ColorProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "generate.html#content")]
    [Spec("css-content-3", "#content-property")]
    public void CssContentNormalLegal()
    {
        var snippet = "content : normal ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("content", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ContentProperty>(property);
        var concrete = (ContentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("normal", concrete.Value);
        Assert.AreEqual("normal", concrete.Original);
    }

    [TestMethod]
    [Spec("css21", "generate.html#content")]
    [Spec("css-content-3", "#content-property")]
    public void CssContentNoneLegalUppercaseN()
    {
        var snippet = "content : noNe ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("content", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ContentProperty>(property);
        var concrete = (ContentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
        Assert.AreEqual("noNe", concrete.Original);
    }

    [TestMethod]
    [Spec("css21", "generate.html#content")]
    [Spec("css-content-3", "#content-values")]
    public void CssContentStringLegal()
    {
        var snippet = "content : 'hi' ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("content", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ContentProperty>(property);
        var concrete = (ContentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("\"hi\"", concrete.Value);
        Assert.AreEqual("\"hi\"", concrete.Original);
    }

    [TestMethod]
    [Spec("css21", "generate.html#content")]
    [Spec("css-content-3", "#content-values")]
    public void CssContentNoOpenQuoteNoCloseQuoteLegal()
    {
        var snippet = "content : no-open-quote no-close-quote ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("content", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ContentProperty>(property);
        var concrete = (ContentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("no-open-quote no-close-quote", concrete.Value);
        Assert.AreEqual("no-open-quote no-close-quote", concrete.Original);
    }

    [TestMethod]
    [Spec("css21", "generate.html#content")]
    [Spec("css-content-3", "#content-values")]
    public void CssContentUrlLegal()
    {
        var snippet = "content : url(test.html) ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("content", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ContentProperty>(property);
        var concrete = (ContentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"test.html\")", concrete.Value);
        Assert.AreEqual("url(\"test.html\")", concrete.Original);
    }

    [TestMethod]
    [Spec("css21", "generate.html#content")]
    [Spec("css-content-3", "#content-values")]
    public void CssContentStringsLegal()
    {
        var snippet = "content : 'how' 'are' 'you' ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("content", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ContentProperty>(property);
        var concrete = (ContentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("\"how\" \"are\" \"you\"", concrete.Value);
        Assert.AreEqual("\"how\" \"are\" \"you\"", concrete.Original);
    }

    [TestMethod]
    [Spec("css21", "generate.html#quotes-specify")]
    [Spec("css-content-3", "#quotes-property")]
    public void CssQuoteStringIllegal()
    {
        var snippet = "quotes : '\"' ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("quotes", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<QuotesProperty>(property);
        var concrete = (QuotesProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "generate.html#quotes-specify")]
    [Spec("css-content-3", "#quotes-property")]
    public void CssQuoteStringsLegal()
    {
        var snippet = "quotes : '\"' '\"' ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("quotes", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<QuotesProperty>(property);
        var concrete = (QuotesProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("\"\\\"\" \"\\\"\"", concrete.Value);
        Assert.AreEqual("\"\\\"\" \"\\\"\"", concrete.Original);
    }

    [TestMethod]
    [Spec("css21", "generate.html#quotes-specify")]
    [Spec("css-content-3", "#quotes-property")]
    public void CssQuoteStringsIllegal()
    {
        var snippet = "quotes : \"'\"";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("quotes", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<QuotesProperty>(property);
        var concrete = (QuotesProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "generate.html#quotes-specify")]
    [Spec("css-content-3", "#quotes-property")]
    public void CssQuoteStringsMultipleLegal()
    {
        // The fourth quote mark is a literal U+FFFD on purpose
        var snippet = "quotes : '\"' '\"' '`' '�' ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("quotes", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<QuotesProperty>(property);
        var concrete = (QuotesProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("\"\\\"\" \"\\\"\" \"`\" \"�\"", concrete.Value);
        Assert.AreEqual("\"\\\"\" \"\\\"\" \"`\" \"�\"", concrete.Original);
    }

    [TestMethod]
    [Spec("css21", "generate.html#quotes-specify")]
    [Spec("css-content-3", "#quotes-property")]
    public void CssQuoteStringsMultipleIllegal()
    {
        var snippet = "quotes : '\"' '\"' '`' ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("quotes", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<QuotesProperty>(property);
        var concrete = (QuotesProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "generate.html#quotes-specify")]
    [Spec("css-content-3", "#quotes-property")]
    public void CssQuoteNoneLegal()
    {
        var snippet = "quotes : none";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("quotes", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<QuotesProperty>(property);
        var concrete = (QuotesProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
        Assert.AreEqual("none", concrete.Original);
    }

    [TestMethod]
    [Spec("css21", "generate.html#quotes-specify")]
    [Spec("css-content-3", "#quotes-property")]
    public void CssQuoteNoneStringIllegal()
    {
        var snippet = "quotes : 'none'";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("quotes", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<QuotesProperty>(property);
        var concrete = (QuotesProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "generate.html#quotes-specify")]
    [Spec("css-content-3", "#quotes-property")]
    public void CssQuoteNormalIllegal()
    {
        var snippet = "quotes : normal ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("quotes", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<QuotesProperty>(property);
        var concrete = (QuotesProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }
}
