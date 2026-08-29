using System.Drawing;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest;

// See StageD2FragmentBucketingSmokeTest for why this opts out of this assembly's default
// method-level parallelization (MSTestSettings.cs).
[TestClass]
[DoNotParallelize]
public sealed class StageD4RepeatedHeaderTest
{
    [TestMethod]
    public void ThreadRepeatsOnEveryPageTheTableSpans()
    {
        // break-inside: avoid is explicit here rather than relied on from the UA default stylesheet's
        // "@media print { thead, tfoot { break-inside: avoid } }" - this test renders via WinForms,
        // whose adapter reports a "screen" media type, so that print-scoped rule never matches here
        // (confirmed intentional: only PdfSharpAdapter overrides DefaultMediaType to "print").
        var sb = new StringBuilder("<html><body><table border='1'><thead style='break-inside:avoid;'><tr><th>Col A</th><th>Col B</th></tr></thead><tbody>");
        for (var i = 0; i < 60; i++)
        {
            sb.Append($"<tr><td style='height:20px;'>row {i} a</td><td>row {i} b</td></tr>");
        }
        sb.Append("</tbody></table></body></html>");

        using var wrapper = new HtmlContainer();
        wrapper.SetHtml(sb.ToString()).GetAwaiter().GetResult();

        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var container = (HtmlContainerInt)prop.GetValue(wrapper)!;
        container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(500, 700);
        container.MarginTop = 0;
        wrapper.MaxSize = new SizeF(500, 0);

        using var bitmap = new Bitmap(500, 5000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        var table = DomUtils.GetBoxByTagName(container.Root, "table");
        Assert.IsNotNull(table);

        // The table must genuinely span multiple pages for this test to be meaningful.
        Assert.IsTrue(container.PageIndexOf(table.ActualBottom - 0.01) > container.PageIndexOf(table.Location.Y),
            "expected the table to span more than one page");

        Assert.IsNotNull(table.RepeatedHeaderRows, "expected at least one repeated header row set");
        Assert.IsTrue(table.RepeatedHeaderRows.Count > 0);

        // Each repeated row's text content should match the original header's text (Col A / Col B).
        var headerRow = table.RepeatedHeaderRows[0];
        var text = string.Join(" ", CollectWords(headerRow));
        StringAssert.Contains(text, "Col A");
        StringAssert.Contains(text, "Col B");

        // Every repeated header must land at the top of a page slot the table's body actually spans,
        // and must not be positioned on the table's own first page (it's already there once, in flow).
        // The row itself carries no Location (only its cells do - see CssLayoutEngineTable's row loop),
        // so the first cell is the reference point.
        var firstSlot = container.PageIndexOf(table.Location.Y);
        var slot = container.PageIndexOf(headerRow.Boxes[0].Location.Y);
        Assert.IsTrue(slot > firstSlot, "repeated header should not land back on the table's own first page");
        Assert.AreEqual(container.PageTopOf(slot), headerRow.Boxes[0].Location.Y, 0.5, "repeated header should sit flush at its page's content top");
    }

    private static System.Collections.Generic.IEnumerable<string> CollectWords(TheArtOfDev.HtmlRenderer.Core.Dom.CssBox box)
    {
        foreach (var word in box.Words)
        {
            if (!string.IsNullOrWhiteSpace(word.Text))
                yield return word.Text;
        }
        foreach (var child in box.Boxes)
        {
            foreach (var w in CollectWords(child))
                yield return w;
        }
    }
}
