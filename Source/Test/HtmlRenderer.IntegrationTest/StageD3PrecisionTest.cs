using System.Drawing;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest;

// See StageD2FragmentBucketingSmokeTest for why this opts out of this assembly's default
// method-level parallelization (MSTestSettings.cs).
[TestClass]
[DoNotParallelize]
public sealed class StageD3PrecisionTest
{
    private static HtmlContainerInt Layout(string html, int pageWidth, int pageHeight, out HtmlContainer wrapper, out Bitmap bitmap)
    {
        wrapper = new HtmlContainer();
        wrapper.SetHtml(html).GetAwaiter().GetResult();

        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var container = (HtmlContainerInt)prop.GetValue(wrapper)!;
        container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(pageWidth, pageHeight);
        container.MarginTop = 0;
        wrapper.MaxSize = new SizeF(pageWidth, 0);

        bitmap = new Bitmap(pageWidth, 8000);
        var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);
        g.Dispose();

        return container;
    }

    [TestMethod]
    public void NoLine_EverStraddlesAPageBoundary()
    {
        var sentence = "one two three four five six seven eight nine ten eleven twelve thirteen fourteen fifteen sixteen seventeen eighteen nineteen twenty ";
        var html = $"<html><body><p>{string.Concat(Enumerable.Repeat(sentence, 60))}</p></body></html>";

        var container = Layout(html, 500, 700, out var wrapper, out var bitmap);
        try
        {
            var p = DomUtils.GetBoxByTagName(container.Root, "p");
            Assert.IsTrue(p.LineBoxes.Count > 5, "expected many lines to make this test meaningful");

            foreach (var line in p.LineBoxes)
            {
                var top = line.LineTop;
                var bottom = line.LineBottom;
                if (bottom <= top) continue;

                var topSlot = container.PageIndexOf(top);
                var bottomSlot = container.PageIndexOf(System.Math.Max(top, bottom - 0.01));
                Assert.AreEqual(topSlot, bottomSlot, $"line [{top:F1},{bottom:F1}) straddles a page boundary");
            }
        }
        finally
        {
            bitmap.Dispose();
            wrapper.Dispose();
        }
    }

    [TestMethod]
    public void Widows_NeverLeavesFewerThanMinimumLinesAtTopOfPage()
    {
        var filler = string.Concat(Enumerable.Repeat("<p style='margin:0;'>filler line of text</p>", 53));
        var sentence = "one two three four five six seven eight nine ten eleven twelve thirteen fourteen fifteen sixteen seventeen eighteen nineteen twenty ";
        var html = $"""
            <html><body>
                {filler}
                <p style="margin:0; widows: 3;">{string.Concat(Enumerable.Repeat(sentence, 6))}</p>
            </body></html>
            """;

        var container = Layout(html, 500, 700, out var wrapper, out var bitmap);
        try
        {
            // Find the widowed <p> specifically (the last <p>, since filler <p>s come first).
            var body = DomUtils.GetBoxByTagName(container.Root, "body");
            var target = body.Boxes[body.Boxes.Count - 1];

            AssertNoStraddleAndWidowsHonored(container, target, minWidows: 3);
        }
        finally
        {
            bitmap.Dispose();
            wrapper.Dispose();
        }
    }

    private static void AssertNoStraddleAndWidowsHonored(HtmlContainerInt container, CssBox box, int minWidows)
    {
        var lines = box.LineBoxes;
        var breakLineIndex = -1;
        for (var i = 1; i < lines.Count; i++)
        {
            if (container.PageIndexOf(lines[i].LineTop) != container.PageIndexOf(lines[i - 1].LineTop))
            {
                breakLineIndex = i;
                break;
            }
        }

        if (breakLineIndex < 0) return; // whole box fit on one page - nothing to check

        var linesAfterBreak = lines.Count - breakLineIndex;
        Assert.IsTrue(linesAfterBreak >= minWidows,
            $"only {linesAfterBreak} lines after the break, expected at least {minWidows}");
    }
}
