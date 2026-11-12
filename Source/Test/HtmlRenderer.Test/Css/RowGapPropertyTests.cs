using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class RowGapPropertyTests
{
    [TestMethod]
    [DataRow("0")]
    [DataRow("20px")]
    [DataRow("1em")]
    [DataRow("3vmin")]
    [DataRow("0.5cm")]
    [DataRow("10%")]
    [DataRow("inherit")]
    [DataRow("initial")]
    [DataRow("revert")]
    [DataRow("revert-layer")]
    [DataRow("unset")]
    public void RowGapLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<RowGapProperty>(PropertyNames.RowGap, value);

    [TestMethod]
    [Ignore("Gap, see Spec/css-grid-2.yml")]
    [Spec("css-grid-2", "#gutters")]
    public void RowGapAcceptsNormalKeyword()
        => CssConstructionFunctions.TestForLegalValue<RowGapProperty>(PropertyNames.RowGap, "normal");
}
