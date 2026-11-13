using System.Collections.Generic;
using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class GapPropertyTests
{
    [TestMethod]
    [DynamicData(nameof(GapTestValues))]
    public void GapLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<GapProperty>(PropertyNames.Gap, value);

    [TestMethod]
    [Spec("css-grid-2", "#gutters")]
    public void GapAcceptsNormalKeyword()
        => CssConstructionFunctions.TestForLegalValue<GapProperty>(PropertyNames.Gap, "normal");

    [TestMethod]
    [DynamicData(nameof(GapExpandedTestValues))]
    public void GapShorthandValueExpanded(string propertyValue, string expectedRowGap, string expectedColumnGap)
    {
        var source = $".test {{ gap: {propertyValue}; }}";
        var styleSheet = CssConstructionFunctions.ParseStyleSheet(source);
        var rule = (StyleRule)styleSheet.StyleRules.First();

        Assert.AreEqual(propertyValue, rule.Style.Gap);
        Assert.AreEqual(expectedRowGap, rule.Style.RowGap);
        Assert.AreEqual(expectedColumnGap, rule.Style.ColumnGap);
    }

    [TestMethod]
    [Spec("css-grid-2", "#gutters")]
    public void GapShorthandValueExpanded_Normal()
        => GapShorthandValueExpanded("normal", "normal", "normal");

    public static IEnumerable<object[]> GapTestValues =>
        new[]
        {
            new object[] { "20px 10px" },
            new object[] { "1em 0.5em" },
            new object[] { "3vmin 2vmax" },
            new object[] { "3vmin" },
            new object[] { "0.5cm" },
            new object[] { "0.5cm 2mm" },
            new object[] { "16% 100%" },
            new object[] { "21px 82%" }
        }.Union(CssConstructionFunctions.LengthOrPercentOrGlobalTestValues.Union(
            CssConstructionFunctions.GlobalKeywordTestValues.ToObjectArray(), ObjectArrayComparer.Instance),
            ObjectArrayComparer.Instance);

    public static IEnumerable<object[]> GapExpandedTestValues =>
        new[]
        {
            new object[] { "20px 10px", "20px", "10px" },
            new object[] { "1em 0.5em", "1em", "0.5em" },
            new object[] { "3vmin 2vmax", "3vmin", "2vmax" },
            new object[] { "3vmin", "3vmin", "3vmin" },
            new object[] { "0.5cm", "0.5cm", "0.5cm" },
            new object[] { "0.5cm 2mm", "0.5cm", "2mm" },
            new object[] { "16% 100%", "16%", "100%" },
            new object[] { "21px 82%", "21px", "82%" },
            new object[] { "initial inherit", "initial", "inherit" },
            new object[] { "unset revert-layer", "unset", "revert-layer" },
        };
}
