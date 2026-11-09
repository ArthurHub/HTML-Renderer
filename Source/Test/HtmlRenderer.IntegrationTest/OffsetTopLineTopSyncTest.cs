using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest;

/// <summary>
/// <c>CssBox.OffsetTop</c> has to shift the line-side rectangles too, <c>EffectiveTop</c> of an
/// inline-only box reads from them.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class OffsetTopLineTopSyncTest
{
    [TestMethod]
    public async Task EffectiveTop_MatchesLocation_AfterOffsetTopOnAMultiLineBox()
    {
        using var wrapper = new HtmlContainer();
        await wrapper.SetHtml(
            """
            <html><body>
                <h2 style="margin:0;">Heading WordTwo WordThree WordFour WordFive WordSix WordSeven WordEight WordNine WordTen</h2>
            </body></html>
            """);

        wrapper.MaxSize = new SizeF(300, 0);
        using var bitmap = new Bitmap(300, 2000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var containerInt = (HtmlContainerInt)prop.GetValue(wrapper)!;

        CssBox? Walk(CssBox box) =>
            box.HtmlTag?.Name == "h2" ? box : box.Boxes.Select(Walk).FirstOrDefault(r => r != null);

        var heading = Walk(containerInt.Root);
        Assert.IsNotNull(heading, "expected an <h2> box in the laid-out tree");

        var effectiveTopProp = typeof(CssBox).GetProperty("EffectiveTop", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var offsetTopMethod = typeof(CssBox).GetMethod("OffsetTop", BindingFlags.NonPublic | BindingFlags.Instance)!;

        var beforeLocation = heading!.Location.Y;
        var beforeEffectiveTop = (double)effectiveTopProp.GetValue(heading)!;
        Assert.AreEqual(beforeLocation, beforeEffectiveTop, 0.01, "precondition: Location.Y and EffectiveTop must agree before any shift");
        Assert.IsGreaterThan(1, heading.LineBoxes.Count, "the heading must genuinely wrap to more than one line for this test to be meaningful");

        offsetTopMethod.Invoke(heading, new object[] { 50.0 });

        var afterLocation = heading.Location.Y;
        var afterEffectiveTop = (double)effectiveTopProp.GetValue(heading)!;

        Assert.AreEqual(beforeLocation + 50.0, afterLocation, 0.01, "OffsetTop must move Location.Y by the given amount");
        Assert.AreEqual(afterLocation, afterEffectiveTop, 0.01,
            "EffectiveTop must match Location.Y after OffsetTop - the line-side rectangle mirror must not go stale");
    }
}
