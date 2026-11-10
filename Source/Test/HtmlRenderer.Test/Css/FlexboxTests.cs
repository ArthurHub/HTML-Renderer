using System.Collections.Generic;
using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>Ported from PeachPDF.Tests/CSS/Flexbox.cs. Pure CSSOM parse tests - no layout involved.</summary>
[TestClass]
public sealed class FlexboxTests
{
    [TestMethod]
    [DynamicData(nameof(FlexDirectionTestDataValues))]
    public void FlexDirectionLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<FlexDirectionProperty>(PropertyNames.FlexDirection, value);

    [TestMethod]
    [DynamicData(nameof(FlexWrapTestDataValues))]
    public void FlexWrapLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<FlexWrapProperty>(PropertyNames.FlexWrap, value);

    [TestMethod]
    [DynamicData(nameof(OrderTestDataValues))]
    public void OrderLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<OrderProperty>(PropertyNames.Order, value);

    [TestMethod]
    [DynamicData(nameof(FlexBasisTestDataValues))]
    public void FlexBasisLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<FlexBasisProperty>(PropertyNames.FlexBasis, value);

    [TestMethod]
    [DynamicData(nameof(FlexGrowShrinkTestDataValues))]
    public void FlexGrowLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<FlexGrowProperty>(PropertyNames.FlexGrow, value);

    [TestMethod]
    [DynamicData(nameof(FlexGrowShrinkTestDataValues))]
    public void FlexShrinkLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<FlexShrinkProperty>(PropertyNames.FlexShrink, value);

    [TestMethod]
    [DynamicData(nameof(AlignContentTestDataValues))]
    public void AlignContentLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<AlignContentProperty>(PropertyNames.AlignContent, value);

    [TestMethod]
    [DynamicData(nameof(AlignItemsTestDataValues))]
    public void AlignItemsLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<AlignItemsProperty>(PropertyNames.AlignItems, value);

    [TestMethod]
    [DynamicData(nameof(AlignSelfTestDataValues))]
    public void AlignSelfLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<AlignSelfProperty>(PropertyNames.AlignSelf, value);

    [TestMethod]
    [DynamicData(nameof(JustifyContentTestDataValues))]
    public void JustifyContentLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<JustifyContentProperty>(PropertyNames.JustifyContent, value);

    [TestMethod]
    [DynamicData(nameof(FlexFlowTestDataValues))]
    public void FlexFlowLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<FlexFlowProperty>(PropertyNames.FlexFlow, value);

    [TestMethod]
    [DynamicData(nameof(FlexTestDataValues))]
    public void FlexLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<FlexProperty>(PropertyNames.Flex, value);

    [TestMethod]
    [DynamicData(nameof(AlignContentInvalidPrefixTestDataValues))]
    public void AlignContentIllegalPrefixValues(string value)
    {
        var snippet = $"align-content: {value}";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("align-content", property.Name);
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [DynamicData(nameof(AlignItemsInvalidPrefixTestDataValues))]
    public void AlignItemsIllegalPrefixValues(string value)
    {
        var snippet = $"align-items: {value}";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("align-items", property.Name);
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [DynamicData(nameof(JustifyContentInvalidPrefixTestDataValues))]
    public void JustifyContentIllegalPrefixValues(string value)
    {
        var snippet = $"align-items: {value}";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("align-items", property.Name);
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [DataRow("align-items: safe")]
    [DataRow("align-self: unsafe")]
    [DataRow("align-content: first")]
    [DataRow("align-items: last")]
    [DataRow("justify-content: first baseline")]
    public void AlignmentPrefixWithoutKeywordIsRejected(string declaration)
    {
        var property = CssConstructionFunctions.ParseDeclaration(declaration);
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [DataRow("align-items: safe")]
    [DataRow("justify-self: safe right")]
    [DataRow("place-self: center left")]
    [DataRow("place-content: baseline")]
    [DataRow("place-items: start legacy")]
    public void AlignmentValueDoesntBreakTheDocument(string declaration)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<div id='x' style='{declaration}'>text</div>"));
        Assert.IsNotNull(LayoutHarness.FindById(root, "x"));
    }

    [TestMethod]
    [DynamicData(nameof(FlexFlowExpandedTestValues))]
    public void FlexFlowShorthandValueExpanded(string propertyValue, string expectedDirection, string expectedWrap)
    {
        var source = $".test {{ flex-flow: {propertyValue}; }}";
        var styleSheet = CssConstructionFunctions.ParseStyleSheet(source);
        var rule = (StyleRule)styleSheet.StyleRules.First();

        Assert.AreEqual(expectedDirection, rule.Style.FlexDirection);
        Assert.AreEqual(expectedWrap, rule.Style.FlexWrap);
    }

    [TestMethod]
    [DataRow("1")]
    [DataRow("2")]
    public void FlexShorthandOneValueExpanded(string propertyValue)
    {
        var source = $".test {{ flex: {propertyValue}; }}";
        var styleSheet = CssConstructionFunctions.ParseStyleSheet(source);
        var rule = (StyleRule)styleSheet.StyleRules.First();

        Assert.AreEqual(propertyValue, rule.Style.FlexGrow);
        Assert.AreEqual("1", rule.Style.FlexShrink);
        Assert.AreEqual("0", rule.Style.FlexBasis);
    }

    [TestMethod]
    [DataRow("1 30px", "1", "1", "30px")]
    [DataRow("2 2", "2", "2", "0")]
    public void FlexShorthandTwoValuesExpanded(string propertyValue,
        string expectedFlexGrow,
        string expectedFlexShrink,
        string expectedFlexBasis)
    {
        var source = $".test {{ flex: {propertyValue}; }}";
        var styleSheet = CssConstructionFunctions.ParseStyleSheet(source);
        var rule = (StyleRule)styleSheet.StyleRules.First();

        Assert.AreEqual(expectedFlexGrow, rule.Style.FlexGrow);
        Assert.AreEqual(expectedFlexShrink, rule.Style.FlexShrink);
        Assert.AreEqual(expectedFlexBasis, rule.Style.FlexBasis);
    }

    [TestMethod]
    [DataRow("2 2 10%", "2", "2", "10%")]
    [DataRow("1 2 20em", "1", "2", "20em")]
    [DataRow("2 1 min-content", "2", "1", "min-content")]
    public void FlexShorthandThreeValuesExpanded(string propertyValue,
        string expectedFlexGrow,
        string expectedFlexShrink,
        string expectedFlexBasis)
    {
        var source = $".test {{ flex: {propertyValue}; }}";
        var styleSheet = CssConstructionFunctions.ParseStyleSheet(source);
        var rule = (StyleRule)styleSheet.StyleRules.First();

        Assert.AreEqual(propertyValue, rule.Style.Flex);
        Assert.AreEqual(expectedFlexGrow, rule.Style.FlexGrow);
        Assert.AreEqual(expectedFlexShrink, rule.Style.FlexShrink);
        Assert.AreEqual(expectedFlexBasis, rule.Style.FlexBasis);
    }

    public static IEnumerable<object[]> FlexDirectionTestDataValues =>
        new[]
        {
            Keywords.Row,
            Keywords.RowReverse,
            Keywords.Column,
            Keywords.ColumnReverse,
        }.Union(CssConstructionFunctions.GlobalKeywordTestValues).ToObjectArray();

    public static IEnumerable<object[]> FlexWrapTestDataValues =>
        new[]
        {
            Keywords.Nowrap,
            Keywords.Wrap,
            Keywords.WrapReverse,
        }.Union(CssConstructionFunctions.GlobalKeywordTestValues).ToObjectArray();

    public static IEnumerable<object[]> FlexFlowTestDataValues =>
        new[]
        {
            new object[] { "row" },
            new object[] { "row-reverse" },
            new object[] { "column" },
            new object[] { "column-reverse" },
            new object[] { "nowrap" },
            new object[] { "wrap" },
            new object[] { "wrap-reverse" },
            new object[] { "row nowrap" },
            new object[] { "column wrap" },
            new object[] { "column-reverse wrap-reverse" },
        }.Union(CssConstructionFunctions.GlobalKeywordTestValues.ToObjectArray(), ObjectArrayComparer.Instance);

    public static IEnumerable<object[]> FlexFlowExpandedTestValues =>
        new[]
        {
            new object[] { "row nowrap", "row", "nowrap" },
            new object[] { "column wrap", "column", "wrap" },
            new object[] { "column-reverse wrap-reverse", "column-reverse", "wrap-reverse" },
            // flex-flow is "<'flex-direction'> || <'flex-wrap'>" (CSS Flexbox 1 §5.1), so the two
            // values are equally valid in the reverse order.
            new object[] { "nowrap row", "row", "nowrap" },
            new object[] { "wrap column", "column", "wrap" },
            new object[] { "wrap-reverse column-reverse", "column-reverse", "wrap-reverse" },
        };

    public static IEnumerable<object[]> FlexTestDataValues =>
        new[]
        {
            new object[] { "auto" },
            new object[] { "initial" },
            new object[] { "none" },
            new object[] { "2" },
            new object[] { "10em" },
            new object[] { "30%" },
            new object[] { "min-content" },
            new object[] { "1 30px" },
            new object[] { "2 2" },
            new object[] { "2 2 10%" },
        }.Union(CssConstructionFunctions.GlobalKeywordTestValues.ToObjectArray(), ObjectArrayComparer.Instance);

    public static IEnumerable<object[]> AlignContentTestDataValues =>
        new[]
        {
            new object[] { Keywords.Center },
            new object[] { Keywords.Start },
            new object[] { Keywords.End },
            new object[] { Keywords.FlexStart },
            new object[] { Keywords.FlexEnd },
            new object[] { Keywords.Normal },
            new object[] { Keywords.Baseline },
            new object[] { $"{Keywords.First} {Keywords.Baseline}" },
            new object[] { $"{Keywords.Last} {Keywords.Baseline}" },
            new object[] { Keywords.SpaceBetween },
            new object[] { Keywords.SpaceAround },
            new object[] { Keywords.SpaceEvenly },
            new object[] { Keywords.Stretch },
            new object[] { $"{Keywords.Safe} {Keywords.Center}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.Center}" },
        }.Union(CssConstructionFunctions.GlobalKeywordTestValues.ToObjectArray(), ObjectArrayComparer.Instance);

    public static IEnumerable<object[]> AlignSelfTestDataValues =>
        new[]
        {
            new object[] { Keywords.Auto },
            new object[] { Keywords.Stretch },
            new object[] { Keywords.Center },
            new object[] { Keywords.Start },
            new object[] { Keywords.End },
            new object[] { Keywords.FlexStart },
            new object[] { Keywords.FlexEnd },
            new object[] { Keywords.Normal },
            new object[] { Keywords.Baseline },
            new object[] { $"{Keywords.First} {Keywords.Baseline}" },
            new object[] { $"{Keywords.Last} {Keywords.Baseline}" },
            new object[] { $"{Keywords.Safe} {Keywords.Center}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.Center}" },
        }.Union(CssConstructionFunctions.GlobalKeywordTestValues.ToObjectArray(), ObjectArrayComparer.Instance);

    public static IEnumerable<object[]> JustifyContentTestDataValues =>
        new[]
        {
            new object[] { Keywords.Center },
            new object[] { Keywords.Start },
            new object[] { Keywords.End },
            new object[] { Keywords.FlexStart },
            new object[] { Keywords.FlexEnd },
            new object[] { Keywords.Left },
            new object[] { Keywords.Right },
            new object[] { Keywords.Normal },
            new object[] { Keywords.SpaceBetween },
            new object[] { Keywords.SpaceAround },
            new object[] { Keywords.SpaceEvenly },
            new object[] { Keywords.Stretch },
            new object[] { $"{Keywords.Safe} {Keywords.Center}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.Center}" },
        }.Union(CssConstructionFunctions.GlobalKeywordTestValues.ToObjectArray(), ObjectArrayComparer.Instance);

    public static IEnumerable<object[]> AlignItemsTestDataValues =>
        new[]
        {
            new object[] { Keywords.Normal },
            new object[] { Keywords.Stretch },
            new object[] { Keywords.Center },
            new object[] { Keywords.Start },
            new object[] { Keywords.End },
            new object[] { Keywords.FlexStart },
            new object[] { Keywords.FlexEnd },
            new object[] { Keywords.SelfStart },
            new object[] { Keywords.SelfEnd },
            new object[] { Keywords.Baseline },
            new object[] { $"{Keywords.First} {Keywords.Baseline}" },
            new object[] { $"{Keywords.Last} {Keywords.Baseline}" },
            new object[] { $"{Keywords.Safe} {Keywords.Center}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.Center}" },
        }.Union(CssConstructionFunctions.GlobalKeywordTestValues.ToObjectArray(), ObjectArrayComparer.Instance);

    public static IEnumerable<object[]> AlignContentInvalidPrefixTestDataValues =>
        new[]
        {
            new object[] { $"{Keywords.Safe} {Keywords.Start}" },
            new object[] { $"{Keywords.Safe} {Keywords.End}" },
            new object[] { $"{Keywords.Safe} {Keywords.FlexStart}" },
            new object[] { $"{Keywords.Safe} {Keywords.FlexEnd}" },

            new object[] { $"{Keywords.Unsafe} {Keywords.Start}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.End}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.FlexStart}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.FlexEnd}" },

            new object[] { $"{Keywords.First} {Keywords.Start}" },
            new object[] { $"{Keywords.First} {Keywords.End}" },
            new object[] { $"{Keywords.First} {Keywords.FlexStart}" },
            new object[] { $"{Keywords.First} {Keywords.FlexEnd}" },

            new object[] { $"{Keywords.Last} {Keywords.Start}" },
            new object[] { $"{Keywords.Last} {Keywords.End}" },
            new object[] { $"{Keywords.Last} {Keywords.FlexStart}" },
            new object[] { $"{Keywords.Last} {Keywords.FlexEnd}" },
        };

    public static IEnumerable<object[]> AlignItemsInvalidPrefixTestDataValues =>
        new[]
        {
            new object[] { $"{Keywords.Safe} {Keywords.Start}" },
            new object[] { $"{Keywords.Safe} {Keywords.End}" },
            new object[] { $"{Keywords.Safe} {Keywords.FlexStart}" },
            new object[] { $"{Keywords.Safe} {Keywords.FlexEnd}" },
            new object[] { $"{Keywords.Safe} {Keywords.SelfStart}" },
            new object[] { $"{Keywords.Safe} {Keywords.SelfEnd}" },

            new object[] { $"{Keywords.Unsafe} {Keywords.Start}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.End}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.FlexStart}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.FlexEnd}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.SelfStart}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.SelfEnd}" },

            new object[] { $"{Keywords.First} {Keywords.Start}" },
            new object[] { $"{Keywords.First} {Keywords.End}" },
            new object[] { $"{Keywords.First} {Keywords.FlexStart}" },
            new object[] { $"{Keywords.First} {Keywords.FlexEnd}" },
            new object[] { $"{Keywords.First} {Keywords.SelfStart}" },
            new object[] { $"{Keywords.First} {Keywords.SelfEnd}" },

            new object[] { $"{Keywords.Last} {Keywords.Start}" },
            new object[] { $"{Keywords.Last} {Keywords.End}" },
            new object[] { $"{Keywords.Last} {Keywords.FlexStart}" },
            new object[] { $"{Keywords.Last} {Keywords.FlexEnd}" },
            new object[] { $"{Keywords.Last} {Keywords.SelfStart}" },
            new object[] { $"{Keywords.Last} {Keywords.SelfEnd}" }
        };

    public static IEnumerable<object[]> JustifyContentInvalidPrefixTestDataValues =>
        new[]
        {
            new object[] { $"{Keywords.Safe} {Keywords.Start}" },
            new object[] { $"{Keywords.Safe} {Keywords.End}" },
            new object[] { $"{Keywords.Safe} {Keywords.FlexStart}" },
            new object[] { $"{Keywords.Safe} {Keywords.FlexEnd}" },
            new object[] { $"{Keywords.Safe} {Keywords.SelfStart}" },
            new object[] { $"{Keywords.Safe} {Keywords.SelfEnd}" },

            new object[] { $"{Keywords.Unsafe} {Keywords.Start}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.End}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.FlexStart}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.FlexEnd}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.SelfStart}" },
            new object[] { $"{Keywords.Unsafe} {Keywords.SelfEnd}" },
        };

    public static IEnumerable<object[]> FlexGrowShrinkTestDataValues =>
        new[]
        {
            new object[] { "3" },
            new object[] { "0.6" }
        }.Union(CssConstructionFunctions.GlobalKeywordTestValues.ToObjectArray(), ObjectArrayComparer.Instance);

    public static IEnumerable<object[]> FlexBasisTestDataValues =>
        new[]
        {
            new object[] { "10em" },
            new object[] { "3px" },
            new object[] { "50%" },
            new object[] { Keywords.MinContent },
            new object[] { Keywords.MaxContent },
            new object[] { Keywords.FitContent },
            new object[] { Keywords.Content },
            new object[] { Keywords.Auto }
        }.Union(CssConstructionFunctions.GlobalKeywordTestValues.ToObjectArray(), ObjectArrayComparer.Instance);

    public static IEnumerable<object[]> OrderTestDataValues =>
        new[]
        {
            new object[] { "-1" },
            new object[] { "1" },
        }.Union(CssConstructionFunctions.GlobalKeywordTestValues.ToObjectArray(), ObjectArrayComparer.Instance);
}
