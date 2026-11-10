using System.Drawing;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.Test.TestSupport;

/// <summary>
/// A minimal, non-sealed <see cref="RAdapter"/> for tests that need to construct an
/// <see cref="TheArtOfDev.HtmlRenderer.Core.HtmlContainerInt"/> and lay it out without depending on any concrete
/// UI/PDF platform. Named-color resolution reuses <see cref="Color.FromName"/> (pure managed lookup, no GDI+),
/// so color-dependent assertions still see real values; everything else is a deterministic, non-null stub.
/// </summary>
internal sealed class MockAdapter : RAdapter
{
    /// <summary>
    /// The CSS media type this adapter reports for <c>@media</c> evaluation (see
    /// <see cref="TheArtOfDev.HtmlRenderer.Core.MediaQueryContext.FromAdapter"/>). Defaults to "screen",
    /// like the base <see cref="RAdapter.DefaultMediaType"/>; settable so a test can exercise "@media print"
    /// (or any other media type) deterministically without depending on a real platform adapter.
    /// </summary>
    public string MediaType { get; set; } = "screen";

    public override string DefaultMediaType => MediaType;

    public RColorScheme ColorScheme { get; set; } = RColorScheme.Light;

    public override RColorScheme SystemColorScheme => ColorScheme;

    protected override RColor GetColorInt(string colorName)
    {
        var color = Color.FromName(colorName);
        return RColor.FromArgb(color.A, color.R, color.G, color.B);
    }

    protected override RPen CreatePen(RColor color) => new MockPen(color);

    protected override RBrush CreateSolidBrush(RColor color) => new MockBrush(color);

    protected override RBrush CreateLinearGradientBrush(RPoint p1, RPoint p2, (RColor Color, double Position)[] stops) =>
        new MockBrush(stops.Length > 0 ? stops[0].Color : RColor.Black);

    protected override RImage ConvertImageInt(object image) => image as RImage ?? new MockImage(0, 0);

    protected override RImage ImageFromStreamInt(System.IO.Stream memoryStream) => new MockImage(40, 30);

    protected override RFont CreateFontInt(string family, double size, RFontStyle style) => new MockFont(size);

    protected override RFont CreateFontInt(RFontFamily family, double size, RFontStyle style) => new MockFont(size);

    protected override RFontFamily LoadFontFaceFontInt(byte[] fontBytes, string filePath) => new MockFontFamily(filePath);

    protected override object GetClipboardDataObjectInt(string html, string plainText) => (html, plainText);
}

/// <summary>A pen that remembers the color it was created with.</summary>
internal sealed class MockPen(RColor color) : RPen
{
    public RColor Color { get; } = color;
    public override double Width { get; set; }
    public RDashStyle RecordedDashStyle { get; private set; }
    public override RDashStyle DashStyle { set => RecordedDashStyle = value; }
}

/// <summary>A solid-color brush that remembers the color it was created with.</summary>
internal sealed class MockBrush(RColor color) : RBrush
{
    public RColor Color { get; } = color;
    public override void Dispose() { }
}

/// <summary>A fixed-size image, independent of any real pixel decoding.</summary>
internal sealed class MockImage(double width, double height) : RImage
{
    public override double Width => width;
    public override double Height => height;
    public override void Dispose() { }
}

/// <summary>A deterministic fixed-metric font, independent of any real font file/rasterizer.</summary>
internal sealed class MockFont(double size) : RFont
{
    public override double Size => size;
    public override double Height => size * 1.2;
    public override double UnderlineOffset => size * 0.9;
    public override double LeftPadding => size * 0.2;
    public override double GetWhitespaceWidth(RGraphics graphics) => size * 0.25;
}

/// <summary>A font family stand-in for @font-face loading, independent of any real font file parsing.</summary>
internal sealed class MockFontFamily(string name) : RFontFamily
{
    public override string Name => name;
}
