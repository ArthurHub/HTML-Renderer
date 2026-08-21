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
/// Verifies a real, previously-undocumented bug found while investigating
/// <see cref="ContainerLeftBehindKeepWithNextTest"/>: <c>CssBox.OffsetTop</c> kept the box's own
/// <c>Rectangles</c> dictionary in sync with a shift, but never the corresponding entry in the line's OWN
/// mirror dictionary (<c>CssLineBox.Rectangles</c>, keyed the other way around) that
/// <c>CssLineBox.LineTop</c>/<c>LineBottom</c> - and therefore <c>CssBox.EffectiveTop</c> for any
/// inline-only box - read from. <c>Location.Y</c> (updated by <c>OffsetTop</c>'s own last statement) was
/// correct immediately after the call, while <c>EffectiveTop</c> silently kept reporting the pre-shift
/// position - confirmed directly by inspecting both dictionaries on a real shifted heading before the fix.
/// Exercised directly via reflection here (rather than only through whichever fragmentation mechanism
/// happens to call <c>OffsetTop</c> at a given filler count - <c>EnforceKeepWithNext</c>'s run-pull and
/// <c>InlineFragmentation</c>'s own orphans-driven push are both live callers, and only the former uses
/// <c>OffsetTop</c>, so a test gated only on "the heading visibly moved" can't reliably tell which path it
/// hit) since <c>OffsetTop</c>'s own contract - keep every derived position getter consistent after a
/// shift - should hold regardless of which caller invokes it.
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
