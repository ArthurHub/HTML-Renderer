using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class TablesTests
{
    /// <summary>Lays out <paramref name="content"/> in a table without border spacing and returns the boxes with the given ids.</summary>
    private static CssBox[] Table(string content, params string[] ids)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<table style='border-spacing:0'>{content}</table>"));
        return ids.Select(id => LayoutHarness.FindById(root, id) ?? throw new AssertFailedException($"No box with id '{id}'")).ToArray();
    }

    [TestMethod]
    [Spec("html", "tables.html#the-table-element")]
    [Spec("html", "tables.html#the-colgroup-element")]
    [Spec("html", "tables.html#the-col-element")]
    [Spec("html", "tables.html#the-tbody-element")]
    [Spec("html", "tables.html#the-thead-element")]
    [Spec("html", "tables.html#the-tfoot-element")]
    [Spec("html", "tables.html#the-tr-element")]
    [Spec("html", "tables.html#the-td-element")]
    [Spec("html", "tables.html#the-th-element")]
    [DataRow("<table id=x><tr><td>a</td></tr></table>", "table")]
    [DataRow("<table><colgroup id=x><col></colgroup><tr><td>a</td></tr></table>", "table-column-group")]
    [DataRow("<table><colgroup><col id=x></colgroup><tr><td>a</td></tr></table>", "table-column")]
    [DataRow("<table><tbody id=x><tr><td>a</td></tr></tbody></table>", "table-row-group")]
    [DataRow("<table><thead id=x><tr><td>a</td></tr></thead></table>", "table-header-group")]
    [DataRow("<table><tfoot id=x><tr><td>a</td></tr></tfoot></table>", "table-footer-group")]
    [DataRow("<table><tr id=x><td>a</td></tr></table>", "table-row")]
    [DataRow("<table><tr><td id=x>a</td></tr></table>", "table-cell")]
    [DataRow("<table><tr><th id=x>a</th></tr></table>", "table-cell")]
    public void Element_TakesPartInTheTableModel(string body, string display)
    {
        Assert.AreEqual(display, Box(body).Display);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "tables.html#the-caption-element")]
    public void Caption_IsRenderedAboveTheRows()
    {
        var b = Table("<caption id=c>Title</caption><tr><td id=x>a</td></tr>", "c", "x");
        Assert.IsGreaterThan(0, b[0].Size.Height);
        Assert.IsLessThanOrEqualTo(b[1].Location.Y, b[0].ActualBottom);
    }

    [TestMethod]
    [Spec("html", "tables.html#the-tfoot-element")]
    [Spec("html", "tables.html#forming-a-table")]
    public void Tfoot_IsPlacedAfterAllBodies()
    {
        var b = Table("<tfoot><tr><td id=f>f</td></tr></tfoot><tbody><tr><td id=x>a</td></tr></tbody><tbody><tr><td id=y>b</td></tr></tbody>", "f", "x", "y");
        Assert.IsLessThan(b[2].Location.Y, b[1].Location.Y);
        Assert.IsLessThan(b[0].Location.Y, b[2].Location.Y);
    }

    [TestMethod]
    [Spec("html", "tables.html#attributes-common-to-td-and-th-elements")]
    [Spec("html", "tables.html#forming-a-table")]
    public void Colspan_CoversTheColumnsItSpans()
    {
        var b = Table("<tr><td id=a colspan=2>a</td><td id=x>b</td></tr><tr><td>c</td><td>d</td><td id=y>e</td></tr>", "a", "x", "y");
        Assert.AreEqual(b[2].Location.X, b[1].Location.X, 0.01);
        Assert.AreEqual(b[2].Location.X, b[0].ActualRight, 0.01);
    }

    [TestMethod]
    [Spec("html", "tables.html#attributes-common-to-td-and-th-elements")]
    [Spec("html", "tables.html#forming-a-table")]
    public void Rowspan_CoversTheRowsItSpans()
    {
        var b = Table("<tr><td id=a rowspan=2>a</td><td id=y>b</td></tr><tr><td id=x>c</td></tr>", "a", "x", "y");
        Assert.AreEqual(b[2].Location.X, b[1].Location.X, 0.01);
        Assert.AreEqual(b[1].ActualBottom, b[0].ActualBottom, 0.01);
    }

    private static string ColspanTable(string value) =>
        $"<tr><td id=a colspan='{value}'>a</td><td id=x>b</td></tr><tr><td>c</td><td id=y1>d</td><td id=y2>e</td></tr>";

    [TestMethod]
    [Spec("html", "tables.html#forming-a-table")]
    [DataRow("abc")]
    [DataRow("")]
    public void Colspan_Invalid_CountsAsOne(string value)
    {
        var b = Table(ColspanTable(value), "x", "y1");
        Assert.IsGreaterThan(b[0].Location.X, b[0].ActualRight);
        Assert.AreEqual(b[1].Location.X, b[0].Location.X, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "tables.html#forming-a-table")]
    [DataRow("0")]
    [DataRow("-1")]
    public void Colspan_ZeroOrNegative_CountsAsOne(string value)
    {
        var b = Table(ColspanTable(value), "x", "y1");
        Assert.IsGreaterThan(b[0].Location.X, b[0].ActualRight);
        Assert.AreEqual(b[1].Location.X, b[0].Location.X, 0.01);
    }

    [TestMethod]
    [Spec("html", "tables.html#forming-a-table")]
    public void Colspan_WithLeadingWhitespace_IsParsed()
    {
        var b = Table(ColspanTable(" 2"), "x", "y2");
        Assert.AreEqual(b[1].Location.X, b[0].Location.X, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "tables.html#forming-a-table")]
    public void Colspan_WithTrailingCharacters_UsesTheLeadingDigits()
    {
        var b = Table(ColspanTable("2px"), "x", "y2");
        Assert.AreEqual(b[1].Location.X, b[0].Location.X, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "tables.html#forming-a-table")]
    public void Colspan_AboveAThousand_IsClampedToAThousand()
    {
        var cells = string.Concat(Enumerable.Range(0, 1001).Select(i => $"<td id=c{i}></td>"));
        var b = Table($"<tr><td colspan=1001>a</td><td id=x>b</td></tr><tr>{cells}</tr>", "x", "c1000");
        Assert.AreEqual(b[1].Location.X, b[0].Location.X, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "tables.html#forming-a-table")]
    public void Colspan_AddsColumns()
    {
        var b = Table("<tr><td id=a colspan=2>a</td><td id=x>b</td></tr><tr><td>c</td><td id=y>d</td></tr>", "a", "x", "y");
        Assert.IsGreaterThan(b[1].Location.X, b[1].ActualRight);
        Assert.IsGreaterThanOrEqualTo(b[2].ActualRight, b[1].Location.X);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "tables.html#forming-a-table")]
    public void CellSpanningPastTheOtherRows_IsLaidOut()
    {
        var b = Table("<tr><td id=a colspan=3>a</td></tr><tr><td id=x>b</td><td id=y>c</td></tr>", "a", "x", "y");
        Assert.IsGreaterThan(b[0].Location.X, b[0].ActualRight);
        Assert.IsGreaterThanOrEqualTo(b[2].ActualRight, b[0].ActualRight);
    }

    private static string RowspanTable(string value) =>
        $"<tbody><tr><td id=a rowspan='{value}'>a</td><td id=y>b</td></tr><tr><td id=x>c</td></tr></tbody>";

    [TestMethod]
    [Spec("html", "tables.html#forming-a-table")]
    [DataRow("abc")]
    [DataRow("-1")]
    public void Rowspan_Invalid_CountsAsOne(string value)
    {
        var b = Table(RowspanTable(value), "a", "x");
        Assert.AreEqual(b[0].Location.X, b[1].Location.X, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "tables.html#forming-a-table")]
    public void Rowspan_WithTrailingCharacters_UsesTheLeadingDigits()
    {
        var b = Table(RowspanTable("2px"), "x", "y");
        Assert.AreEqual(b[1].Location.X, b[0].Location.X, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "tables.html#attributes-common-to-td-and-th-elements")]
    public void RowspanZero_SpansTheRestOfTheRowGroup()
    {
        var b = Table(RowspanTable("0") + "<tbody><tr><td id=z>d</td></tr></tbody>", "a", "x", "y", "z");
        Assert.AreEqual(b[2].Location.X, b[1].Location.X, 0.01);
        Assert.AreEqual(b[1].ActualBottom, b[0].ActualBottom, 0.01);
        Assert.AreEqual(b[0].Location.X, b[3].Location.X, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "tables.html#forming-a-table")]
    public void Rowspan_EndsWithItsRowGroup()
    {
        var b = Table(RowspanTable("3") + "<tbody><tr><td id=z>d</td><td>e</td></tr></tbody>", "a", "z");
        Assert.AreEqual(b[0].Location.X, b[1].Location.X, 0.01);
        Assert.IsLessThanOrEqualTo(b[1].Location.Y, b[0].ActualBottom);
    }

    private static CssBox[] ColumnTable(string columns) =>
        Table($"{columns}<tr><td id=a>a</td><td id=x>b</td><td id=y>c</td></tr>", "a", "x", "y");

    [TestMethod]
    [Spec("html", "tables.html#the-colgroup-element")]
    [Spec("html", "tables.html#the-col-element")]
    [Spec("html", "tables.html#forming-a-table")]
    [DataRow("<col span=2 style='width:50px'><col style='width:80px'>")]
    [DataRow("<colgroup span=2 style='width:50px'></colgroup><colgroup style='width:80px'></colgroup>")]
    public void Span_CoversThatManyColumns(string columns)
    {
        var b = ColumnTable(columns);
        Assert.AreEqual(50, b[0].ActualRight - b[0].Location.X, 0.01);
        Assert.AreEqual(50, b[1].ActualRight - b[1].Location.X, 0.01);
        Assert.AreEqual(80, b[2].ActualRight - b[2].Location.X, 0.01);
    }

    [TestMethod]
    [Spec("html", "tables.html#the-colgroup-element")]
    [Spec("html", "tables.html#forming-a-table")]
    public void ColgroupSpan_IsIgnoredWhenItHasCols()
    {
        var b = ColumnTable("<colgroup span=3><col style='width:50px'><col style='width:80px'></colgroup>");
        Assert.AreEqual(80, b[1].ActualRight - b[1].Location.X, 0.01);
    }

    [TestMethod]
    [Spec("html", "tables.html#forming-a-table")]
    [DataRow("0")]
    [DataRow("abc")]
    public void Span_Invalid_CountsAsOne(string value)
    {
        var b = ColumnTable($"<col span='{value}' style='width:50px'><col style='width:80px'><col>");
        Assert.AreEqual(80, b[1].ActualRight - b[1].Location.X, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "tables.html#forming-a-table")]
    public void Span_WithTrailingCharacters_UsesTheLeadingDigits()
    {
        var b = ColumnTable("<col span=2px style='width:50px'><col style='width:80px'>");
        Assert.AreEqual(80, b[2].ActualRight - b[2].Location.X, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "tables.html#forming-a-table")]
    public void Span_WithAFraction_IsCutOffAtTheDecimalPoint()
    {
        var b = ColumnTable("<col span=1.5 style='width:50px'><col style='width:80px'><col>");
        Assert.AreEqual(80, b[1].ActualRight - b[1].Location.X, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "tables.html#forming-a-table")]
    public void CellsPastTheDeclaredColumns_AreLaidOut()
    {
        var b = ColumnTable("<col style='width:50px'>");
        Assert.AreEqual(b[0].ActualRight, b[1].Location.X, 0.01);
        Assert.IsGreaterThan(b[1].Location.X, b[1].ActualRight);
        Assert.IsGreaterThan(b[2].Location.X, b[2].ActualRight);
    }
}
