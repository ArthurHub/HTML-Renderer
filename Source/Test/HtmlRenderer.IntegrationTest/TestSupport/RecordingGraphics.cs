using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.TestSupport;

/// <summary>Records every point added to the path so tests can assert on the resulting geometry.</summary>
internal sealed class MockGraphicsPath : RGraphicsPath
{
    public List<RPoint> Points { get; } = [];

    public override void Start(double x, double y) => Points.Add(new RPoint(x, y));
    public override void LineTo(double x, double y) => Points.Add(new RPoint(x, y));
    public override void ArcTo(double x, double y, double radiusX, double radiusY, Corner corner) => Points.Add(new RPoint(x, y));
    public override void Dispose() { }
}

/// <summary>
/// Minimal <see cref="RGraphics"/> implementation that records paint calls so tests can verify layout/paint
/// behavior without a full rendering stack (WinForms). Mirrors HtmlRenderer.Test's
/// TestSupport/RecordingGraphics.cs (a sibling, non-referenceable project).
/// </summary>
internal class RecordingGraphics : RGraphics
{
    public sealed record DrawStringCall(string Text, RFont Font, RColor Color, RPoint Point, RSize Size, bool Rtl);
    public sealed record DrawRectCall(RColor Color, double X, double Y, double Width, double Height);
    public sealed record DrawLineCall(RColor Color, double X1, double Y1, double X2, double Y2);
    public sealed record DrawPolygonCall(RColor Color, RPoint[] Points);
    public sealed record DrawImageCall(RImage Image, RRect DestRect);
    public sealed record PushClipCall(RRect Rect);
    public sealed record PopClipCall;

    public List<object> Log { get; } = [];
    public List<DrawStringCall> DrawStringCalls { get; } = [];
    public List<DrawImageCall> DrawImageCalls { get; } = [];

    public RecordingGraphics() : this(new MockAdapter())
    {
    }

    public RecordingGraphics(RAdapter adapter) : base(adapter, new RRect(0, 0, double.MaxValue, double.MaxValue))
    {
    }

    public override void PopClip()
    {
        if (_clipStack.Count > 1) _clipStack.Pop();
        Log.Add(new PopClipCall());
    }

    public override void PushClip(RRect rect)
    {
        _clipStack.Push(rect);
        Log.Add(new PushClipCall(rect));
    }

    public override void PushClipExclude(RRect rect) { }

    public override object SetAntiAliasSmoothingMode() => new object();

    public override void ReturnPreviousSmoothingMode(object prevMode) { }

    public override RBrush GetTextureBrush(RImage image, RRect dstRect, RPoint translateTransformLocation) => new MockBrush(RColor.Empty);

    public override RGraphicsPath GetGraphicsPath() => new MockGraphicsPath();

    public override RSize MeasureString(string str, RFont font) => new((str?.Length ?? 0) * font.Size * 0.6, font.Height);

    public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
    {
        charFit = str?.Length ?? 0;
        charFitWidth = charFit * font.Size * 0.6;
    }

    public override void DrawString(string str, RFont font, RColor color, RPoint point, RSize size, bool rtl)
    {
        var call = new DrawStringCall(str, font, color, point, size, rtl);
        DrawStringCalls.Add(call);
        Log.Add(call);
    }

    public override void DrawLine(RPen pen, double x1, double y1, double x2, double y2)
    {
        var color = pen is MockPen mp ? mp.Color : RColor.Empty;
        Log.Add(new DrawLineCall(color, x1, y1, x2, y2));
    }

    public override void DrawRectangle(RPen pen, double x, double y, double width, double height)
    {
        var color = pen is MockPen mp ? mp.Color : RColor.Empty;
        Log.Add(new DrawRectCall(color, x, y, width, height));
    }

    public override void DrawRectangle(RBrush brush, double x, double y, double width, double height)
    {
        var color = brush is MockBrush mb ? mb.Color : RColor.Empty;
        Log.Add(new DrawRectCall(color, x, y, width, height));
    }

    public override void DrawImage(RImage image, RRect destRect, RRect srcRect)
    {
        var call = new DrawImageCall(image, destRect);
        DrawImageCalls.Add(call);
        Log.Add(call);
    }

    public override void DrawImage(RImage image, RRect destRect)
    {
        var call = new DrawImageCall(image, destRect);
        DrawImageCalls.Add(call);
        Log.Add(call);
    }

    public override void DrawPath(RPen pen, RGraphicsPath path) { }

    public override void DrawPath(RBrush brush, RGraphicsPath path) { }

    public override void DrawPolygon(RBrush brush, RPoint[] points)
    {
        var color = brush is MockBrush mb ? mb.Color : RColor.Empty;
        Log.Add(new DrawPolygonCall(color, (RPoint[])points.Clone()));
    }

    public override void Dispose() { }
}

internal sealed record Arc(RPoint End, double RadiusX, double RadiusY, RGraphicsPath.Corner Corner);

/// <summary>A path that records its points and arcs, and the pen or brush it was drawn with.</summary>
internal sealed class ShapePath : RGraphicsPath
{
    public List<RPoint> Points { get; } = [];
    public List<Arc> Arcs { get; } = [];
    public RColor? Brush { get; set; }
    public RColor? Pen { get; set; }
    public bool AntiAliased { get; set; }

    public override void Start(double x, double y) => Points.Add(new RPoint(x, y));
    public override void LineTo(double x, double y) => Points.Add(new RPoint(x, y));

    public override void ArcTo(double x, double y, double radiusX, double radiusY, Corner corner)
    {
        Points.Add(new RPoint(x, y));
        Arcs.Add(new Arc(new RPoint(x, y), radiusX, radiusY, corner));
    }

    public override void Dispose() { }
}

/// <summary>Also records the paths, which <see cref="RecordingGraphics"/> drops, and the anti-aliasing mode.</summary>
internal sealed class ShapeRecordingGraphics : RecordingGraphics
{
    public List<ShapePath> Paths { get; } = [];

    public bool AntiAlias { get; private set; }

    public override RGraphicsPath GetGraphicsPath() => new ShapePath();

    public override object SetAntiAliasSmoothingMode()
    {
        var prev = AntiAlias;
        AntiAlias = true;
        return prev;
    }

    public override void ReturnPreviousSmoothingMode(object prevMode)
    {
        if (prevMode is bool prev)
            AntiAlias = prev;
    }

    public override void DrawPath(RPen pen, RGraphicsPath path)
    {
        var shape = (ShapePath)path;
        shape.Pen = pen is MockPen p ? p.Color : RColor.Empty;
        shape.AntiAliased = AntiAlias;
        Paths.Add(shape);
    }

    public override void DrawPath(RBrush brush, RGraphicsPath path)
    {
        var shape = (ShapePath)path;
        shape.Brush = brush is MockBrush b ? b.Color : RColor.Empty;
        shape.AntiAliased = AntiAlias;
        Paths.Add(shape);
    }
}
