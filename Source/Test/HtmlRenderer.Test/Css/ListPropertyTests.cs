using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class ListPropertyTests
{
    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#list-style-position-property")]
    public void CssListStylePositionOutsideLegal()
    {
        var property = ParseDeclaration("list-style-position: outside ");
        Assert.AreEqual("list-style-position", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStylePositionProperty>(property);
        var concrete = (ListStylePositionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("outside", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#list-style-position-property")]
    public void CssListStylePositionOutsideIllegal()
    {
        var property = ParseDeclaration("list-style-position: out-side ");
        Assert.AreEqual("list-style-position", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStylePositionProperty>(property);
        var concrete = (ListStylePositionProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#list-style-position-property")]
    public void CssListStylePositionNoneIllegal()
    {
        var property = ParseDeclaration("list-style-position: none ");
        Assert.AreEqual("list-style-position", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStylePositionProperty>(property);
        var concrete = (ListStylePositionProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#list-style-position-property")]
    public void CssListStylePositionInsideLegal()
    {
        var property = ParseDeclaration("list-style-position: insiDe ");
        Assert.AreEqual("list-style-position", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStylePositionProperty>(property);
        var concrete = (ListStylePositionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("inside", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#image-markers")]
    public void CssListStyleImageNoneLegal()
    {
        var property = ParseDeclaration("list-style-image: none ");
        Assert.AreEqual("list-style-image", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStyleImageProperty>(property);
        var concrete = (ListStyleImageProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#image-markers")]
    public void CssListStyleImageUrlLegal()
    {
        var property = ParseDeclaration("list-style-image: url(http://www.example.com/images/list.png)");
        Assert.AreEqual("list-style-image", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStyleImageProperty>(property);
        var concrete = (ListStyleImageProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"http://www.example.com/images/list.png\")", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#text-markers")]
    public void CssListStyleTypeDiscLegal()
    {
        var property = ParseDeclaration("list-style-type: disc ");
        Assert.AreEqual("list-style-type", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStyleTypeProperty>(property);
        var concrete = (ListStyleTypeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("disc", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#text-markers")]
    public void CssListStyleTypeLowerAlphaLegal()
    {
        var property = ParseDeclaration("list-style-type: lower-ALPHA ");
        Assert.AreEqual("list-style-type", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStyleTypeProperty>(property);
        var concrete = (ListStyleTypeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        // Serialized as written, not as the lower-latin it maps to
        Assert.AreEqual("lower-alpha", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#text-markers")]
    public void CssListStyleTypeGeorgianLegal()
    {
        var property = ParseDeclaration("list-style-type: georgian ");
        Assert.AreEqual("list-style-type", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStyleTypeProperty>(property);
        var concrete = (ListStyleTypeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("georgian", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#text-markers")]
    public void CssListStyleTypeDecimalLeadingZeroLegal()
    {
        var property = ParseDeclaration("list-style-type: decimal-leading-zerO ");
        Assert.AreEqual("list-style-type", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStyleTypeProperty>(property);
        var concrete = (ListStyleTypeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("decimal-leading-zero", concrete.Value);
    }

    [TestMethod]
    [Spec("css-lists-3", "#text-markers")]
    public void CssListStyleTypeHebrewLegal()
    {
        // Dropped from CSS 2.1, predefined again in CSS Counter Styles 3
        var property = ParseDeclaration("list-style-type: HEbrew ");
        Assert.AreEqual("list-style-type", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStyleTypeProperty>(property);
        var concrete = (ListStyleTypeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("hebrew", concrete.Value);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-lists-3.yml")]
    [Spec("css-lists-3", "#text-markers")]
    public void CssListStyleTypeExtendedKeywordSetLegal()
    {
        var property = ParseDeclaration("list-style-type: hiragana ");
        var concrete = (ListStyleTypeProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("hiragana", concrete.Value);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-lists-3.yml")]
    [Spec("css-lists-3", "#text-markers")]
    public void CssListStyleTypeStringLegal()
    {
        var property = ParseDeclaration("list-style-type: \"-> \" ");
        Assert.AreEqual("list-style-type", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStyleTypeProperty>(property);
        var concrete = (ListStyleTypeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("\"-> \"", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    public void CssListStyleTypeNumberIllegal()
    {
        var property = ParseDeclaration("list-style-type: number ");
        Assert.AreEqual("list-style-type", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStyleTypeProperty>(property);
        var concrete = (ListStyleTypeProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#list-style-property")]
    public void CssListStyleCircleLegal()
    {
        var property = ParseDeclaration("list-style: circle ");
        Assert.AreEqual("list-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStyleProperty>(property);
        var concrete = (ListStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("circle", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#list-style-property")]
    public void CssListStyleNone()
    {
        var property = ParseDeclaration("list-style: none");
        Assert.AreEqual("list-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStyleProperty>(property);
        var concrete = (ListStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#list-style-property")]
    public void CssListStyleSquareInsideLegal()
    {
        var property = ParseDeclaration("list-style: square inside ");
        Assert.AreEqual("list-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStyleProperty>(property);
        var concrete = (ListStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("square inside", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#list-style-property")]
    public void CssListStyleSquareImageInsideLegal()
    {
        var property = ParseDeclaration("list-style: square url('image.png') inside ");
        Assert.AreEqual("list-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStyleProperty>(property);
        var concrete = (ListStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("square inside url(\"image.png\")", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counters")]
    [Spec("css-lists-3", "#counter-reset")]
    public void CssCounterResetLegal()
    {
        var property = ParseDeclaration("counter-reset: chapter section 1 page;");
        Assert.AreEqual("counter-reset", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<CounterResetProperty>(property);
        var concrete = (CounterResetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("chapter section 1 page", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counters")]
    [Spec("css-lists-3", "#counter-reset")]
    public void CssCounterResetSingleLegal()
    {
        var property = ParseDeclaration("counter-reset: counter-name");
        Assert.AreEqual("counter-reset", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<CounterResetProperty>(property);
        var concrete = (CounterResetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("counter-name", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counters")]
    [Spec("css-lists-3", "#counter-reset")]
    public void CssCounterResetNoneLegal()
    {
        var property = ParseDeclaration("counter-reset: none");
        Assert.AreEqual("counter-reset", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<CounterResetProperty>(property);
        var concrete = (CounterResetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counters")]
    [Spec("css-lists-3", "#counter-reset")]
    public void CssCounterResetNumberIllegal()
    {
        var property = ParseDeclaration("counter-reset: 3");
        Assert.AreEqual("counter-reset", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<CounterResetProperty>(property);
        var concrete = (CounterResetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counters")]
    [Spec("css-lists-3", "#counter-reset")]
    public void CssCounterResetNegativeLegal()
    {
        var property = ParseDeclaration("counter-reset  :  counter-name   -1");
        Assert.AreEqual("counter-reset", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<CounterResetProperty>(property);
        var concrete = (CounterResetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("counter-name -1", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counters")]
    [Spec("css-lists-3", "#counter-reset")]
    public void CssCounterResetTwoCountersExplicitLegal()
    {
        var property = ParseDeclaration("counter-reset  :  counter1   1   counter2   4  ");
        Assert.AreEqual("counter-reset", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<CounterResetProperty>(property);
        var concrete = (CounterResetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("counter1 1 counter2 4", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counters")]
    [Spec("css-lists-3", "#increment-set")]
    public void CssCounterIncrementNoneLegal()
    {
        var property = ParseDeclaration("counter-increment: none");
        Assert.AreEqual("counter-increment", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<CounterIncrementProperty>(property);
        var concrete = (CounterIncrementProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#counters")]
    [Spec("css-lists-3", "#increment-set")]
    public void CssCounterIncrementLegal()
    {
        var property = ParseDeclaration("counter-increment: chapter section 2 page");
        Assert.AreEqual("counter-increment", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<CounterIncrementProperty>(property);
        var concrete = (CounterIncrementProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("chapter section 2 page", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#list-style-property")]
    public void CssListStyleNoneSquareLegal()
    {
        var property = ParseDeclaration("list-style: none square");
        Assert.AreEqual("list-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStyleProperty>(property);
        var concrete = (ListStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("square none", concrete.Value); // Canonical order: type, image, position
    }
}
