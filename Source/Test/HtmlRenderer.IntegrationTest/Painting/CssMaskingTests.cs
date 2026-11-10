using System.Collections.Generic;
using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class CssMaskingTests
{
    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);

    [Ignore("Gap, see Spec/css-masking-1.yml")]
    [TestMethod]
    [Spec("css-masking-1", "#the-clip-path")]
    public void ClipPathInset_ClipsThePaintedBox()
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div style='clip-path:inset(10px 20px);width:100px;height:100px;background:rgb(255,0,0)'></div>"), 400, 400);
        var g = PaintHarness.PaintPage(container);

        var clip = ClipAt(g, e => e is RecordingGraphics.DrawRectCall r && r.Color == Red);
        Assert.AreEqual(20, clip.X, 0.5);
        Assert.AreEqual(10, clip.Y, 0.5);
        Assert.AreEqual(90, clip.Bottom, 0.5);
    }

    /// <summary>The intersection of all clips pushed and not yet popped when the first matching call was recorded.</summary>
    private static RRect ClipAt(RecordingGraphics g, System.Func<object, bool> match)
    {
        var stack = new List<RRect> { new RRect(0, 0, 400, 400) };
        foreach (var entry in g.Log)
        {
            switch (entry)
            {
                case RecordingGraphics.PushClipCall push:
                    stack.Add(push.Rect);
                    break;
                case RecordingGraphics.PopClipCall:
                    stack.RemoveAt(stack.Count - 1);
                    break;
                default:
                    if (match(entry))
                        return stack.Aggregate(RRect.Intersect);
                    break;
            }
        }

        Assert.Fail("No matching draw call was recorded");
        return RRect.Empty;
    }
}
