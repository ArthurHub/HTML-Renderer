using System.Collections.Generic;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class ColumnGapPropertyTests
{
    [TestMethod]
    [DynamicData(nameof(ColumnGapTestDataValues))]
    [Spec("css-multicol-1", "#cg")]
    public void ColumnGapLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<ColumnGapProperty>(PropertyNames.ColumnGap, value);

    [Ignore("Gap, see Spec/css-multicol-1.yml")]
    [TestMethod]
    [Spec("css-multicol-1", "#cg")]
    public void ColumnGapAcceptsNormalKeyword()
        => CssConstructionFunctions.TestForLegalValue<ColumnGapProperty>(PropertyNames.ColumnGap, "normal");

    public static IEnumerable<object[]> ColumnGapTestDataValues => CssConstructionFunctions.LengthOrPercentOrGlobalTestValues;
}
