using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class ListPropertyTests
{
    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
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
    public void CssListStyleTypeLowerAlphaLegal()
    {
        var property = ParseDeclaration("list-style-type: lower-ALPHA ");
        Assert.AreEqual("list-style-type", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ListStyleTypeProperty>(property);
        var concrete = (ListStyleTypeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        // Map.ListStyles resolves lower-alpha to the ListStyle.LowerLatin enum member, but the converter's
        // serialized text is the keyword that was actually written (lower-alpha), not a canonicalized
        // "lower-latin" - see Source/HtmlRenderer/Core/CssEngine/Model/Map.cs.
        Assert.AreEqual("lower-alpha", concrete.Value);
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
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
    public void CssListStyleTypeHebrewLegal()
    {
        // CSS 2.0 keyword, dropped from CSS 2.1, still defined in Map.ListStyles
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
    [Ignore("Gap: CSS Counter Styles Level 3's extended predefined counter-style keyword set (hiragana, " +
        "katakana, arabic-indic, cjk-decimal, devanagari, and friends, the lower-armenian/upper-armenian " +
        "split, disclosure-open/-closed, etc.) is not implemented - the ListStyle enum " +
        "(Source/HtmlRenderer/Core/CssEngine/Enumerations/ListStyle.cs) and Map.ListStyles " +
        "(Source/HtmlRenderer/Core/CssEngine/Model/Map.cs:166-185) only define " +
        "none/disc/circle/square/decimal/decimal-leading-zero/lower-roman/upper-roman/lower-greek/" +
        "lower-latin/upper-latin/armenian/georgian/hebrew/lower-alpha/upper-alpha, so any of these extra " +
        "keywords fails to parse (HasValue false) rather than resolving as PeachPDF's port claims.")]
    public void CssListStyleTypeExtendedKeywordSetUnsupported()
    {
        var property = ParseDeclaration("list-style-type: hiragana ");
        var concrete = (ListStyleTypeProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("hiragana", concrete.Value);
    }

    [TestMethod]
    [Ignore("Gap: list-style-type does not accept a CSS string literal as a custom counter-style glyph " +
        "(CSS Counter Styles Level 3 / CSS Lists 3 'symbols()'-adjacent shorthand form). " +
        "ListStyleTypeProperty's converter (Source/HtmlRenderer/Core/CssEngine/StyleProperties/List/" +
        "ListStyleTypeProperty.cs) is Converters.ListStyleConverter = Map.ListStyles.ToConverter() - a pure " +
        "keyword/identifier dictionary converter (Source/HtmlRenderer/Core/CssEngine/Extensions/" +
        "ValueConverterExtensions.cs:77 DictionaryValueConverter) with no string-literal branch, so a quoted " +
        "value like \"-> \" fails to convert entirely (DeclaredValue stays null - HasValue false, " +
        "IsInherited reads true only because IsInitial does, not because 'inherit' was written).")]
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
