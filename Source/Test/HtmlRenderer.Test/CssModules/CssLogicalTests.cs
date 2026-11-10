using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssLogicalTests
{
    private const double Delta = 0.5;

    [Ignore("Gap, see Spec/css-logical-1.yml")]
    [TestMethod]
    [Spec("css-logical-1", "#float-clear")]
    [DataRow("float", "inline-start")]
    [DataRow("float", "inline-end")]
    [DataRow("clear", "inline-start")]
    [DataRow("clear", "inline-end")]
    public void FloatAndClear_FlowRelativeValues_AreParsed(string property, string value)
    {
        Assert.AreEqual(value, Value(property, value));
    }

    [Ignore("Gap, see Spec/css-logical-1.yml")]
    [TestMethod]
    [Spec("css-logical-1", "#float-clear")]
    public void FloatInlineEnd_FloatsToTheRightInLtr()
    {
        var root = Layout("<div style='width:200px'><div id='x' style='float:inline-end;width:50px;height:10px'></div></div>");

        Assert.AreEqual(150, LayoutHarness.FindById(root, "x")!.Location.X, Delta);
    }

    [Ignore("Gap, see Spec/css-logical-1.yml")]
    [TestMethod]
    [Spec("css-logical-1", "#page")]
    [DataRow("break-before", "recto")]
    [DataRow("break-after", "verso")]
    [DataRow("page-break-before", "recto")]
    public void RectoAndVerso_AreParsed(string property, string value)
    {
        Assert.AreEqual(value, Value(property, value));
    }

    [TestMethod]
    [Spec("css-logical-1", "#box")]
    public void FlowRelativeAndPhysicalDeclarations_CascadeTogetherInDeclarationOrder()
    {
        var box = Styled("#x{margin-inline-start:1px;margin-left:2px;margin-inline-end:3px;margin-right:4px;padding-block-start:5px}#x{padding-top:6px}");

        Assert.AreEqual(2, box.ActualMarginLeft, Delta);
        Assert.AreEqual(4, box.ActualMarginRight, Delta);
        Assert.AreEqual(6, box.ActualPaddingTop, Delta);
    }

    [TestMethod]
    [Spec("css-logical-1", "#margin-properties")]
    public void MarginShorthands_SetBothSidesOfTheirAxis()
    {
        var box = Styled("#x{margin-block:1px 2px;margin-inline:3px}");

        Assert.AreEqual(1, box.ActualMarginTop, Delta);
        Assert.AreEqual(2, box.ActualMarginBottom, Delta);
        Assert.AreEqual(3, box.ActualMarginLeft, Delta);
        Assert.AreEqual(3, box.ActualMarginRight, Delta);
    }

    [TestMethod]
    [Spec("css-logical-1", "#margin-properties")]
    public void MarginInlineAuto_SetsBothInlineMarginsToAuto()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet(".x { margin-inline: auto; }");
        var style = sheet.Rules.OfType<StyleRule>().Single().Style;

        Assert.AreEqual("auto", style.GetPropertyValue("margin-left"));
        Assert.AreEqual("auto", style.GetPropertyValue("margin-right"));
    }

    [TestMethod]
    [Spec("css-logical-1", "#padding-properties")]
    public void FlowRelativePadding_MapsToThePhysicalSidesInLtr()
    {
        var box = Styled("#x{padding-block-start:1px;padding-inline-end:2px;padding-block-end:3px;padding-inline-start:4px}");

        Assert.AreEqual(1, box.ActualPaddingTop, Delta);
        Assert.AreEqual(2, box.ActualPaddingRight, Delta);
        Assert.AreEqual(3, box.ActualPaddingBottom, Delta);
        Assert.AreEqual(4, box.ActualPaddingLeft, Delta);
    }

    [TestMethod]
    [Spec("css-logical-1", "#padding-properties")]
    public void PaddingShorthands_SetBothSidesOfTheirAxis()
    {
        var box = Styled("#x{padding-block:1px 2px;padding-inline:3px 4px}");

        Assert.AreEqual(1, box.ActualPaddingTop, Delta);
        Assert.AreEqual(2, box.ActualPaddingBottom, Delta);
        Assert.AreEqual(3, box.ActualPaddingLeft, Delta);
        Assert.AreEqual(4, box.ActualPaddingRight, Delta);
    }

    [Ignore("Gap, see Spec/css-logical-1.yml")]
    [TestMethod]
    [Spec("css-logical-1", "#padding-properties")]
    public void PaddingInlineStart_IsTheRightPaddingInRtl()
    {
        var box = Styled("#x{direction:rtl;padding-inline-start:4px}");

        Assert.AreEqual(4, box.ActualPaddingRight, Delta);
        Assert.AreEqual(0, box.ActualPaddingLeft, Delta);
    }

    [TestMethod]
    [Spec("css-logical-1", "#border-width")]
    public void FlowRelativeBorderWidths_MapToThePhysicalSidesInLtr()
    {
        var box = Styled("#x{border-style:solid;border-block-start-width:1px;border-inline-end-width:2px;border-block-end-width:3px;border-inline-start-width:4px}");

        Assert.AreEqual(1, box.ActualBorderTopWidth, Delta);
        Assert.AreEqual(2, box.ActualBorderRightWidth, Delta);
        Assert.AreEqual(3, box.ActualBorderBottomWidth, Delta);
        Assert.AreEqual(4, box.ActualBorderLeftWidth, Delta);
    }

    [TestMethod]
    [Spec("css-logical-1", "#border-width")]
    public void BorderWidthShorthands_SetBothSidesOfTheirAxis()
    {
        var box = Styled("#x{border-style:solid;border-block-width:1px 2px;border-inline-width:3px}");

        Assert.AreEqual(1, box.ActualBorderTopWidth, Delta);
        Assert.AreEqual(2, box.ActualBorderBottomWidth, Delta);
        Assert.AreEqual(3, box.ActualBorderLeftWidth, Delta);
        Assert.AreEqual(3, box.ActualBorderRightWidth, Delta);
    }

    [TestMethod]
    [Spec("css-logical-1", "#border-style")]
    public void FlowRelativeBorderStyles_MapToThePhysicalSidesInLtr()
    {
        var box = Styled("#x{border-block-style:solid dashed;border-inline-start-style:dotted;border-inline-end-style:double}");

        Assert.AreEqual("solid", box.BorderTopStyle);
        Assert.AreEqual("dashed", box.BorderBottomStyle);
        Assert.AreEqual("dotted", box.BorderLeftStyle);
        Assert.AreEqual("double", box.BorderRightStyle);
    }

    [TestMethod]
    [Spec("css-logical-1", "#border-color")]
    public void FlowRelativeBorderColors_MapToThePhysicalSidesInLtr()
    {
        var box = Styled("#x{border-style:solid;border-inline-color:red blue;border-block-start-color:lime;border-block-end-color:black}");

        Assert.AreEqual(255, box.ActualBorderLeftColor.R);
        Assert.AreEqual(255, box.ActualBorderRightColor.B);
        Assert.AreEqual(255, box.ActualBorderTopColor.G);
        Assert.AreEqual(0, box.ActualBorderBottomColor.R);
    }

    [TestMethod]
    [Spec("css-logical-1", "#border-shorthands")]
    public void FlowRelativeBorderShorthands_SetTheirSides()
    {
        var box = Styled("#x{border-block:2px solid red;border-inline-start:5px dashed blue}");

        Assert.AreEqual(2, box.ActualBorderTopWidth, Delta);
        Assert.AreEqual(2, box.ActualBorderBottomWidth, Delta);
        Assert.AreEqual(5, box.ActualBorderLeftWidth, Delta);
        Assert.AreEqual("dashed", box.BorderLeftStyle);
        Assert.AreEqual(0, box.ActualBorderRightWidth, Delta);
    }

    [Ignore("Gap, see Spec/css-logical-1.yml")]
    [TestMethod]
    [Spec("css-logical-1", "#border-shorthands")]
    public void BorderInlineStart_IsTheRightBorderInRtl()
    {
        var box = Styled("#x{direction:rtl;border-inline-start:5px solid red}");

        Assert.AreEqual(5, box.ActualBorderRightWidth, Delta);
        Assert.AreEqual(0, box.ActualBorderLeftWidth, Delta);
    }

    [Ignore("Gap, see Spec/css-logical-1.yml")]
    [TestMethod]
    [Spec("css-logical-1", "#border-radius-properties")]
    [DataRow("border-start-start-radius")]
    [DataRow("border-start-end-radius")]
    [DataRow("border-end-start-radius")]
    [DataRow("border-end-end-radius")]
    public void FlowRelativeCornerRadius_IsParsed(string property)
    {
        Assert.AreEqual("5px", Value(property, "5px"));
    }

    [Ignore("Gap, see Spec/css-logical-1.yml")]
    [TestMethod]
    [Spec("css-logical-1", "#logical-shorthand-keyword")]
    public void LogicalKeyword_AssignsTheValuesToTheFlowRelativeSides()
    {
        var box = Styled("#x{margin:logical 1px 2px 3px 4px}");

        Assert.AreEqual(1, box.ActualMarginTop, Delta);
        Assert.AreEqual(2, box.ActualMarginLeft, Delta);
        Assert.AreEqual(3, box.ActualMarginBottom, Delta);
        Assert.AreEqual(4, box.ActualMarginRight, Delta);
    }

    private static CssBox Layout(string body) =>
        LayoutHarness.Layout($"<html><head></head><body style='margin:0'>{body}</body></html>", 800, 600).Root;

    private static CssBox Styled(string css, string body = "<div id=x>a</div>")
    {
        var (root, _) = LayoutHarness.Layout($"<html><head><style>{css}</style></head><body style='margin:0'>{body}</body></html>", 800, 600);
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box;
    }

    private static string Value(string property, string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {property}: {value}; }}");
        return sheet.Rules.OfType<StyleRule>().Single().Style.GetPropertyValue(property);
    }
}
