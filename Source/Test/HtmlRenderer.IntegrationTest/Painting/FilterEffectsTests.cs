using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class FilterEffectsTests
{
    [Ignore("Gap, see Spec/filter-effects-1.yml")]
    [TestMethod]
    [Spec("filter-effects-1", "#FilterProperty")]
    [Spec("filter-effects-1", "#opacityEquivalent")]
    public void OpacityFilter_MakesTheBoxTransparent()
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div style='filter:opacity(0);width:50px;height:50px;background:rgb(255,0,0)'></div>"), 400, 400);
        var g = PaintHarness.PaintPage(container);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColor.FromArgb(255, 0, 0)));
    }
}
