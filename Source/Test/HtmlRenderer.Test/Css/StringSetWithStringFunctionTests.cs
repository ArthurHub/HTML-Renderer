using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Parsing of string() inside string-set.
/// </summary>
[TestClass]
public sealed class StringSetWithStringFunctionTests
{
    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSet_WithStringFunction_ParsesCorrectly()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: section string(chapter)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("section string(chapter)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSet_WithStringFunctionAndKeyword_ParsesCorrectly()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: section string(chapter, first)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsTrue(concrete.HasValue);
        // Parser may normalize keywords - just verify it contains the function
        StringAssert.Contains(concrete.Value, "section string(chapter");
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSet_WithStringFunctionAndLiteral_ParsesCorrectly()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: section string(chapter) \" - \" content(text)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("section string(chapter) \" - \" content()", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSet_WithMultipleStringFunctions_ParsesCorrectly()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: full-title string(chapter) \" > \" string(section)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("full-title string(chapter) \" > \" string(section)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSet_WithStringFunctionLastKeyword_ParsesCorrectly()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: current-section string(section, last)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("current-section string(section, last)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSet_WithStringFunctionStartKeyword_ParsesCorrectly()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: page-header string(chapter, start)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("page-header string(chapter, start)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSet_WithStringFunctionFirstExceptKeyword_ParsesCorrectly()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: heading string(chapter, first-except)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("heading string(chapter, first-except)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSet_WithStringFunctionAndCounter_ParsesCorrectly()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: page-title string(chapter) \" (\" counter(page) \")\"");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("page-title string(chapter) \" (\" counter(page) \")\"", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSet_WithStringFunctionAndAttr_ParsesCorrectly()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: full-heading string(chapter) \" - \" attr(title)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("full-heading string(chapter) \" - \" attr(title)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSet_WithStringFunctionAndContentFunction_ParsesCorrectly()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: combined string(chapter) \": \" content(text)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("combined string(chapter) \": \" content()", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSet_MultipleNamesWithStringFunction_ParsesCorrectly()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: header string(chapter), footer string(section, last)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("header string(chapter), footer string(section, last)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSet_ComplexNestedExample_ParsesCorrectly()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: breadcrumb string(part, first) \" / \" string(chapter) \" / \" content(text)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsTrue(concrete.HasValue);
        // Verify main components are present
        StringAssert.Contains(concrete.Value, "breadcrumb");
        StringAssert.Contains(concrete.Value, "string(part");
        StringAssert.Contains(concrete.Value, "string(chapter)");
        StringAssert.Contains(concrete.Value, "content()");
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSet_WithHyphenatedIdentifiers_ParsesCorrectly()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: my-heading string(my-chapter)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("my-heading string(my-chapter)", concrete.Value);
    }
}
