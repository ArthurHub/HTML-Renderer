using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class TableRenderingTests
{
    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);

    [TestMethod]
    [Spec("html", "rendering.html#tables-2")]
    [DataRow("<table id=x><tr><td>a</td></tr></table>", "table")]
    [DataRow("<table><caption id=x>a</caption><tr><td>a</td></tr></table>", "table-caption")]
    [DataRow("<table><colgroup id=x><col></colgroup><tr><td>a</td></tr></table>", "table-column-group")]
    [DataRow("<table><col id=x><tr><td>a</td></tr></table>", "table-column")]
    [DataRow("<table><thead id=x><tr><td>a</td></tr></thead></table>", "table-header-group")]
    [DataRow("<table><tbody id=x><tr><td>a</td></tr></tbody></table>", "table-row-group")]
    [DataRow("<table><tfoot id=x><tr><td>a</td></tr></tfoot></table>", "table-footer-group")]
    [DataRow("<table><tr id=x><td>a</td></tr></table>", "table-row")]
    [DataRow("<table><tr><td id=x>a</td></tr></table>", "table-cell")]
    [DataRow("<table><tr><th id=x>a</th></tr></table>", "table-cell")]
    public void TableElements_HaveTheirTableDisplay(string html, string display)
    {
        Assert.AreEqual(display, Box(html).Display);
    }

    [TestMethod]
    [Spec("html", "rendering.html#tables-2")]
    public void Table_HasTwoPixelBorderSpacing()
    {
        Assert.AreEqual(2, Box("<table id=x><tr><td>a</td></tr></table>").ActualBorderSpacingHorizontal);
    }

    [TestMethod]
    [Spec("html", "rendering.html#tables-2")]
    public void Th_IsBoldAndCentered()
    {
        var box = Box("<table><tr><th id=x>a</th></tr></table>");

        Assert.IsTrue(box.ActualFontWeight >= 700);
        Assert.AreEqual("center", box.TextAlign);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#tables-2")]
    public void Th_IsOnlyCenteredWhenTheRowHasTheInitialTextAlign()
    {
        Assert.AreEqual("right", Box("<div style='text-align:right'><table><tr><th id=x>a</th></tr></table></div>").TextAlign);
    }

    [TestMethod]
    [Spec("html", "rendering.html#tables-2")]
    public void RowsAndCells_AreVerticallyCentered()
    {
        Assert.AreEqual("middle", Box("<table><tbody id=x><tr><td>a</td></tr></tbody></table>").VerticalAlign);
        Assert.AreEqual("middle", Box("<table><tr><td id=x>a</td></tr></table>").VerticalAlign);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#tables-2")]
    public void Cells_HaveOnePixelPaddingAndVisibleOverflow()
    {
        var box = Box("<table><tr><td id=x>a</td></tr></table>");

        Assert.AreEqual(1, box.ActualPaddingTop);
        Assert.AreEqual(1, box.ActualPaddingLeft);
        Assert.AreEqual("visible", box.Overflow);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#tables-2")]
    public void TableBorders_UseTheTextColor()
    {
        var html = "<div style='color:red'><table id=x border=1><tr><td id=c>a</td></tr></table></div>";

        Assert.AreEqual(Red, Box(html).ActualBorderTopColor);
        Assert.AreEqual(Red, Box(html, "c").ActualBorderTopColor);
    }

    [TestMethod]
    [Spec("html", "rendering.html#tables-2")]
    public void Cellspacing_SetsTheBorderSpacing()
    {
        Assert.AreEqual(5, Box("<table id=x cellspacing=5><tr><td>a</td></tr></table>").ActualBorderSpacingHorizontal);
    }

    [TestMethod]
    [Spec("html", "rendering.html#tables-2")]
    [DataRow("<table cellpadding=7><tr><td id=x>a</td></tr></table>")]
    [DataRow("<table cellpadding=7><thead><tr><th id=x>a</th></tr></thead></table>")]
    [DataRow("<table cellpadding=7><tbody><tr><td id=x>a</td></tr></tbody></table>")]
    public void Cellpadding_SetsThePaddingOfTheCells(string html)
    {
        var box = Box(html);

        Assert.AreEqual(7, box.ActualPaddingTop);
        Assert.AreEqual(7, box.ActualPaddingLeft);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#tables-2")]
    public void Cellpadding_ReachesThCellsOutsideARowGroup()
    {
        Assert.AreEqual(7, Box("<table cellpadding=7><tr><th id=x>a</th></tr></table>").ActualPaddingTop);
    }

    [TestMethod]
    [Spec("html", "rendering.html#tables-2")]
    public void DimensionAttributes_SetTheSizes()
    {
        var table = Box("<table id=x width=300 height=100><tr><td>a</td></tr></table>");
        Assert.AreEqual("300px", table.Width);
        Assert.AreEqual("100px", table.Height);
        Assert.AreEqual("50%", Box("<table id=x width=50%><tr><td>a</td></tr></table>").Width);
        Assert.AreEqual("80px", Box("<table><col id=x width=80><tr><td>a</td></tr></table>").Width);
        Assert.AreEqual("40px", Box("<table><tr id=x height=40><td>a</td></tr></table>").Height);

        var cell = Box("<table><tr><td id=x width=120 height=40>a</td></tr></table>");
        Assert.AreEqual(120, cell.Size.Width, 0.01);
        Assert.AreEqual(40, cell.Size.Height, 0.01);
    }

    [TestMethod]
    [Spec("html", "rendering.html#tables-2")]
    [DataRow("<table id=x bgcolor=red><tr><td>a</td></tr></table>")]
    [DataRow("<table><tbody id=x bgcolor=red><tr><td>a</td></tr></tbody></table>")]
    [DataRow("<table><tr id=x bgcolor=red><td>a</td></tr></table>")]
    [DataRow("<table><tr><td id=x bgcolor=red>a</td></tr></table>")]
    [DataRow("<table><tr><th id=x bgcolor=red>a</th></tr></table>")]
    public void Bgcolor_SetsTheBackgroundColor(string html)
    {
        Assert.AreEqual(Red, Box(html).ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#tables-2")]
    public void Background_SetsTheBackgroundImage()
    {
        Assert.IsNotNull(Box("<table id=x background=a.png><tr><td>a</td></tr></table>").ActualBackgroundImage);
    }

    [TestMethod]
    [Spec("html", "rendering.html#tables-2")]
    public void Bordercolor_SetsTheBorderColor()
    {
        Assert.AreEqual(Red, Box("<table id=x bordercolor=red border=1><tr><td>a</td></tr></table>").ActualBorderTopColor);
    }

    [TestMethod]
    [Spec("html", "rendering.html#tables-2")]
    public void Border_SetsTheTableBorderWidthAndOnePixelCellBorders()
    {
        Assert.AreEqual(3, Box("<table id=x border=3><tr><td>a</td></tr></table>").ActualBorderTopWidth);
        Assert.AreEqual(1, Box("<table border=3><tr><td id=x>a</td></tr></table>").ActualBorderTopWidth);
        Assert.AreEqual(0, Box("<table border=0><tr><td id=x>a</td></tr></table>").ActualBorderTopWidth);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#tables-2")]
    public void Border_IsOutsetOnTheTableAndInsetOnTheCells()
    {
        Assert.AreEqual("outset", Box("<table id=x border=3><tr><td>a</td></tr></table>").BorderTopStyle);
        Assert.AreEqual("inset", Box("<table border=3><tr><td id=x>a</td></tr></table>").BorderTopStyle);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#tables-2")]
    public void BorderWithoutAValue_IsOnePixel()
    {
        Assert.AreEqual(1, Box("<table id=x border><tr><td>a</td></tr></table>").ActualBorderTopWidth);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#tables-2")]
    public void RulesAndFrame_SetTheBorders()
    {
        var rules = Box("<table rules=all><tr><td id=x>a</td></tr></table>");
        Assert.AreEqual("solid", rules.BorderTopStyle);
        Assert.AreEqual(1, rules.ActualBorderTopWidth);
        Assert.AreEqual("collapse", Box("<table id=x rules=all><tr><td>a</td></tr></table>").BorderCollapse);

        Assert.AreEqual("hidden", Box("<table id=x frame=void border=1><tr><td>a</td></tr></table>").BorderTopStyle);
    }

    [TestMethod]
    [Spec("html", "rendering.html#tables-2")]
    [DataRow("<table><tr id=x align=right><td>a</td></tr></table>", "right")]
    [DataRow("<table><tr><td id=x align=center>a</td></tr></table>", "center")]
    [DataRow("<table><tr><th id=x align=left>a</th></tr></table>", "left")]
    [DataRow("<table><tbody id=x align=justify><tr><td>a</td></tr></tbody></table>", "justify")]
    public void RowAndCellAlign_AlignsTheText(string html, string textAlign)
    {
        Assert.AreEqual(textAlign, Box(html).TextAlign);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#tables-2")]
    [DataRow("middle")]
    [DataRow("absmiddle")]
    public void RowAndCellAlignMiddle_CentersTheText(string align)
    {
        Assert.AreEqual("center", Box($"<table><tr><td id=x align={align}>a</td></tr></table>").TextAlign);
    }

    [TestMethod]
    [Spec("html", "rendering.html#tables-2")]
    [DataRow("<table><tr><td id=x valign=top>a</td></tr></table>", "top")]
    [DataRow("<table><tr id=x valign=bottom><td>a</td></tr></table>", "bottom")]
    [DataRow("<table><tbody id=x valign=baseline><tr><td>a</td></tr></tbody></table>", "baseline")]
    public void Valign_SetsTheVerticalAlignment(string html, string verticalAlign)
    {
        Assert.AreEqual(verticalAlign, Box(html).VerticalAlign);
    }

    [TestMethod]
    [Spec("html", "rendering.html#tables-2")]
    public void Nowrap_PreventsWrapping()
    {
        Assert.AreEqual("nowrap", Box("<table><tr><td id=x nowrap>a</td></tr></table>").WhiteSpace);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#tables-2")]
    public void TableAlignLeftAndRight_FloatTheTable()
    {
        Assert.AreEqual("left", Box("<table id=x align=left><tr><td>a</td></tr></table>").Float);
        Assert.AreEqual("right", Box("<table id=x align=right><tr><td>a</td></tr></table>").Float);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#tables-2")]
    public void TableAlignCenter_CentersTheTable()
    {
        Assert.AreEqual(100, Box("<div style='width:400px'><table id=x align=center style='width:200px'><tr><td>a</td></tr></table></div>").Location.X, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#tables-2")]
    public void Caption_IsRenderedAndCentered()
    {
        var box = Box("<table><caption id=x>a</caption><tr><td>a</td></tr></table>");

        Assert.AreEqual("center", box.TextAlign);
        Assert.IsTrue(box.Size.Height > 0);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#tables-2")]
    public void HiddenRows_AreCollapsed()
    {
        Assert.AreEqual("collapse", Box("<table><tr id=x hidden><td>a</td></tr></table>").Visibility);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "rendering.html#tables-2")]
    public void FormInATable_IsNotDisplayed()
    {
        Assert.AreEqual("none", Box("<table><form id=x></form><tr><td>a</td></tr></table>").Display);
    }
}
