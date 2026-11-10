using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Parsing of string-set. content(text) serializes as content().
/// </summary>
[TestClass]
public sealed class StringSetPropertyTests
{
    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSetNoneLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: none");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSetSimpleTextLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: chapter content(text)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("chapter content()", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSetWithStringLiteral()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: header \"Page \" counter(page)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("header \"Page \" counter(page)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSetWithAttrFunction()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: title attr(title)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("title attr(title)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSetContentBefore()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: heading content(before) \":\" content(text)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("heading content(before) \":\" content()", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSetContentAfter()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: heading content(after)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("heading content(after)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSetContentFirstLetter()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: initial content(first-letter)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("initial content(first-letter)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSetMultiplePairs()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: header content(text), footer counter(page)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("header content(), footer counter(page)", concrete.Value);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSetInvalidNoContentListIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: header");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css-content-3", "#string-set")]
    public void StringSetInvalidNumberIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: 123 content(text)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }
}
