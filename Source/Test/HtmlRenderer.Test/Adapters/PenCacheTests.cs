using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;

namespace HtmlRenderer.Test.Adapters;

[TestClass]
public sealed class PenCacheTests
{
    [TestMethod]
    public void PaintingADashedBorder_LeavesThePlainPenOfItsColorUnchanged()
    {
        var adapter = new MockAdapter();
        var container = new HtmlContainerInt(adapter) { MaxSize = new RSize(800, 600), PageSize = new RSize(800, 600) };
        container.SetHtml("<html><head></head><body style='margin:0'><div style='border:3px dashed red;height:10px'></div></body></html>");
        var graphics = new RecordingGraphics(adapter);
        container.PerformLayout(graphics);
        container.PerformPaint(graphics);

        var pen = (MockPen)adapter.GetPen(RColor.FromArgb(255, 255, 0, 0));

        Assert.AreEqual(1, pen.Width);
        Assert.AreEqual(RDashStyle.Solid, pen.RecordedDashStyle);
    }

    [TestMethod]
    public void PensOfTheSameColor_WithDifferentWidthsOrDashes_AreDifferentInstances()
    {
        var adapter = new MockAdapter();
        var red = RColor.FromArgb(255, 255, 0, 0);

        var thin = adapter.GetPen(red);
        var thick = adapter.GetPen(red, 3, RDashStyle.Solid);
        var dashed = adapter.GetPen(red, 3, RDashStyle.Dash);

        Assert.AreNotSame(thin, thick);
        Assert.AreNotSame(thick, dashed);
        Assert.AreSame(thick, adapter.GetPen(red, 3, RDashStyle.Solid));
        Assert.AreEqual(3, ((MockPen)thick).Width);
        Assert.AreEqual(RDashStyle.Dash, ((MockPen)dashed).RecordedDashStyle);
    }
}
