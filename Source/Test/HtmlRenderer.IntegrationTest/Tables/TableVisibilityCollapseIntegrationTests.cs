using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Tables;

[DoNotParallelize]
[TestClass]
public sealed class TableVisibilityCollapseIntegrationTests
{
    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#dynamic-effects")]
    public void CollapsedRow_DirectTableChild_TakesNoSpace()
    {
        // <tr> as a direct child of <table>, no row group
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tr><td class="a" style="height:20px">A</td></tr>
              <tr style="visibility:collapse"><td class="b" style="height:20px">B</td></tr>
              <tr><td class="c" style="height:20px">C</td></tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tr><td class="a" style="height:20px">A</td></tr>
              <tr><td class="c" style="height:20px">C</td></tr>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "c")!, FindByClass(experiment, "c")!);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#dynamic-effects")]
    public void CollapsedRow_InsideRowGroup_TakesNoSpace()
    {
        // A <tr> inside a <tbody>
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tbody>
                <tr><td class="a" style="height:20px">A</td></tr>
                <tr style="visibility:collapse"><td class="b" style="height:20px">B</td></tr>
                <tr><td class="c" style="height:20px">C</td></tr>
              </tbody>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tbody>
                <tr><td class="a" style="height:20px">A</td></tr>
                <tr><td class="c" style="height:20px">C</td></tr>
              </tbody>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "c")!, FindByClass(experiment, "c")!);
    }

    [TestMethod]
    public void RowspanCrossingCollapsedRow_DoesNotMisalignLaterRow()
    {
        // A rowspan cell opening before a collapsed row and extending into/past it must not have its
        // placeholder land in the wrong row - the unrelated row after the span must keep its own two cells
        // in their own two columns rather than being pushed over by a leaked placeholder.
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tr><td rowspan="3" style="height:60px">Span</td><td style="height:20px">R1C2</td></tr>
              <tr style="visibility:collapse"><td style="height:20px">R2C2</td></tr>
              <tr><td style="height:20px">R3C2</td></tr>
              <tr><td class="after1" style="height:20px">After1</td><td class="after2" style="height:20px">After2</td></tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tr><td rowspan="2" style="height:60px">Span</td><td style="height:20px">R1C2</td></tr>
              <tr><td style="height:20px">R3C2</td></tr>
              <tr><td class="after1" style="height:20px">After1</td><td class="after2" style="height:20px">After2</td></tr>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "after1")!, FindByClass(experiment, "after1")!);
        AssertSameLocation(FindByClass(control, "after2")!, FindByClass(experiment, "after2")!);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#dynamic-effects")]
    public void CollapsedRowGroup_CollapsesEveryRowInsideIt()
    {
        // visibility is inherited, so the rows collapse with their group
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tr><td class="a" style="height:20px">A</td></tr>
              <tbody style="visibility:collapse">
                <tr><td style="height:20px">B1</td></tr>
                <tr><td style="height:20px">B2</td></tr>
              </tbody>
              <tr><td class="c" style="height:20px">C</td></tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tr><td class="a" style="height:20px">A</td></tr>
              <tr><td class="c" style="height:20px">C</td></tr>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "c")!, FindByClass(experiment, "c")!);
    }

    [TestMethod]
    public void HiddenRow_StillReservesSpace_UnlikeCollapse()
    {
        // visibility:hidden keeps the row's space, unlike collapse
        var (hidden, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tr><td class="a" style="height:20px">A</td></tr>
              <tr style="visibility:hidden"><td style="height:20px">B</td></tr>
              <tr><td class="c" style="height:20px">C</td></tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tr><td class="a" style="height:20px">A</td></tr>
              <tr><td class="c" style="height:20px">C</td></tr>
            </table>
            </body></html>
            """);

        var hiddenC = FindByClass(hidden, "c")!;
        var controlC = FindByClass(control, "c")!;
        Assert.IsTrue(hiddenC.Location.Y > controlC.Location.Y,
            $"a visibility:hidden row must still reserve its space, but hiddenC.Y={hiddenC.Location.Y} controlC.Y={controlC.Location.Y}");
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#dynamic-effects")]
    public void CollapsedRow_InsideDetachedHeader_TakesNoSpace()
    {
        // The header group is laid out separately from the body rows
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <thead>
                <tr><td style="height:20px">H1</td></tr>
                <tr style="visibility:collapse"><td style="height:20px">H2</td></tr>
              </thead>
              <tr><td class="body" style="height:20px">Body</td></tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <thead>
                <tr><td style="height:20px">H1</td></tr>
              </thead>
              <tr><td class="body" style="height:20px">Body</td></tr>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "body")!, FindByClass(experiment, "body")!);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#dynamic-effects")]
    public void CollapsedRow_InsideDetachedFooter_TakesNoSpace()
    {
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:0">
              <tr><td style="height:20px">Body</td></tr>
              <tfoot>
                <tr><td style="height:20px">F1</td></tr>
                <tr style="visibility:collapse"><td style="height:20px">F2</td></tr>
              </tfoot>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:0">
              <tr><td style="height:20px">Body</td></tr>
              <tfoot>
                <tr><td style="height:20px">F1</td></tr>
              </tfoot>
            </table>
            </body></html>
            """);

        Assert.AreEqual(FindByClass(control, "t")!.ActualBottom, FindByClass(experiment, "t")!.ActualBottom, 3);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#dynamic-effects")]
    public void CollapsedColumn_TakesNoWidth()
    {
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:0; width:300px">
              <colgroup>
                <col style="width:100px">
                <col style="width:100px; visibility:collapse">
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td class="b">B</td>
                <td class="c">C</td>
              </tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:0; width:200px">
              <colgroup>
                <col style="width:100px">
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td class="c">C</td>
              </tr>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "c")!, FindByClass(experiment, "c")!);

        var experimentTable = FindByClass(experiment, "t")!;
        var controlTable = FindByClass(control, "t")!;
        Assert.AreEqual(controlTable.ActualRight, experimentTable.ActualRight, 3);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#dynamic-effects")]
    public void CollapsedColumn_WideContentDoesNotStarveNextColumn()
    {
        // The collapsed column's unbreakable content must not take width from the visible column
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <div style="width:100px">
            <table class="t" style="border-spacing:0">
              <colgroup>
                <col style="width:50px">
                <col style="visibility:collapse">
                <col>
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td style="white-space:nowrap">ThisIsOneVeryLongUnbreakableWordFarWiderThanOneHundredPixels</td>
                <td class="c">CC CC</td>
              </tr>
            </table>
            </div>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <div style="width:100px">
            <table class="t" style="border-spacing:0">
              <colgroup>
                <col style="width:50px">
                <col>
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td class="c">CC CC</td>
              </tr>
            </table>
            </div>
            </body></html>
            """);

        var experimentC = FindByClass(experiment, "c")!;
        var controlC = FindByClass(control, "c")!;
        AssertSameLocation(controlC, experimentC);
        Assert.AreEqual(controlC.ActualWidth, experimentC.ActualWidth, 3);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#dynamic-effects")]
    public void CollapsedColumn_WithBorderSpacing_LeavesNoResidualGap()
    {
        // A collapsed column must not leave its border-spacing behind
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:5px; width:300px">
              <colgroup>
                <col style="width:100px">
                <col style="width:100px; visibility:collapse">
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td>B</td>
                <td class="c">C</td>
              </tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:5px; width:205px">
              <colgroup>
                <col style="width:100px">
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td class="c">C</td>
              </tr>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "c")!, FindByClass(experiment, "c")!);
        Assert.AreEqual(FindByClass(control, "t")!.ActualRight, FindByClass(experiment, "t")!.ActualRight, 3);
    }

    [TestMethod]
    public void CollapsedColumnGroup_CollapsesEveryColumnInsideIt()
    {
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0; width:200px">
              <colgroup>
                <col style="width:100px">
              </colgroup>
              <colgroup style="visibility:collapse">
                <col style="width:50px">
                <col style="width:50px">
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td>B1</td>
                <td>B2</td>
              </tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0; width:100px">
              <colgroup>
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a">A</td>
              </tr>
            </table>
            </body></html>
            """);

        var experimentA = FindByClass(experiment, "a")!;
        var controlA = FindByClass(control, "a")!;
        Assert.AreEqual(controlA.ActualWidth, experimentA.ActualWidth, 3);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#dynamic-effects")]
    public void ColspanCell_StraddlingVisibleThenCollapsedColumn_NoResidualBorderSpacing()
    {
        // A colspan cell ending in a collapsed column must not keep the border-spacing after it
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:5px; width:305px">
              <colgroup>
                <col style="width:100px">
                <col style="width:100px; visibility:collapse">
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a" colspan="2">AB</td>
                <td class="c">C</td>
              </tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:5px; width:205px">
              <colgroup>
                <col style="width:100px">
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td class="c">C</td>
              </tr>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "c")!, FindByClass(experiment, "c")!);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "tables.html#dynamic-effects")]
    public void ColspanCell_StraddlingCollapsedThenVisibleColumn_NoExtraInteriorSpacing()
    {
        // A colspan cell starting in a collapsed column must not keep the border-spacing inside it
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:5px; width:305px">
              <colgroup>
                <col style="width:100px; visibility:collapse">
                <col style="width:100px">
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a" colspan="2">AB</td>
                <td class="c">C</td>
              </tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:5px; width:205px">
              <colgroup>
                <col style="width:100px">
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td class="c">C</td>
              </tr>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "c")!, FindByClass(experiment, "c")!);
    }

    [TestMethod]
    public void ColspanCell_StraddlingCollapsedColumn_ContentDoesNotShareWidthWithIt()
    {
        // The spanning cell's min-width must not be shared with the collapsed column
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <div style="width:80px">
            <table class="t" style="border-spacing:0">
              <colgroup>
                <col style="visibility:collapse">
                <col>
              </colgroup>
              <tr><td class="a" colspan="2" style="white-space:nowrap">ThisIsOneVeryLongUnbreakableWordFarWiderThanEightyPixels</td></tr>
            </table>
            </div>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <div style="width:80px">
            <table class="t" style="border-spacing:0">
              <colgroup>
                <col>
              </colgroup>
              <tr><td class="a" style="white-space:nowrap">ThisIsOneVeryLongUnbreakableWordFarWiderThanEightyPixels</td></tr>
            </table>
            </div>
            </body></html>
            """);

        var experimentA = FindByClass(experiment, "a")!;
        var controlA = FindByClass(control, "a")!;
        Assert.AreEqual(controlA.ActualWidth, experimentA.ActualWidth, 3);
    }

    private static void AssertSameLocation(CssBox expected, CssBox actual)
    {
        Assert.AreEqual(expected.Location.X, actual.Location.X, 3);
        Assert.AreEqual(expected.Location.Y, actual.Location.Y, 3);
    }

    private static CssBox? FindByClass(CssBox box, string className)
    {
        var val = box.HtmlTag?.TryGetAttribute("class", "");
        if (val != null && val.Split(' ').Contains(className))
            return box;

        foreach (var child in box.Boxes)
        {
            var found = FindByClass(child, className);
            if (found != null) return found;
        }

        return null;
    }
}
