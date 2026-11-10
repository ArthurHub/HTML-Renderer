using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

/// <summary>CSS Text Decoration Level 3 line styles and colors, text shadows and the painting order.</summary>
[DoNotParallelize]
[TestClass]
public sealed class CssTextDecor3PaintIntegrationTests
{
    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);

    [TestMethod]
    [Spec("css-text-decor-3", "#text-decoration-property")]
    public void Shorthand_WithStyleAndColor_SetsTheLine()
    {
        var lines = Log("<span style='text-decoration:dotted underline rgb(255,0,0)'>text</span>").OfType<RecordingGraphics.DrawLineCall>();

        Assert.AreEqual(1, lines.Count());
    }

    [Ignore("Gap, see Spec/css-text-decor-3.yml")]
    [TestMethod]
    [Spec("css-text-decor-3", "#text-decoration-property")]
    public void Shorthand_SetsTheColor()
    {
        var line = Log("<span style='color:rgb(0,0,255);text-decoration:underline rgb(255,0,0)'>text</span>").OfType<RecordingGraphics.DrawLineCall>().Single();

        Assert.AreEqual(Red, line.Color);
    }

    [Ignore("Gap, see Spec/css-text-decor-3.yml")]
    [TestMethod]
    [Spec("css-text-decor-3", "#text-decoration-color-property")]
    public void DecorationColor_IsTheColorOfTheLine()
    {
        var line = Log("<span style='color:rgb(0,0,255);text-decoration-line:underline;text-decoration-color:rgb(255,0,0)'>text</span>").OfType<RecordingGraphics.DrawLineCall>().Single();

        Assert.AreEqual(Red, line.Color);
    }

    [Ignore("Gap, see Spec/css-text-decor-3.yml")]
    [TestMethod]
    [Spec("css-text-decor-3", "#text-decoration-style-property")]
    public void Double_DrawsTwoLines()
    {
        var lines = Log("<span style='text-decoration-line:underline;text-decoration-style:double'>text</span>").OfType<RecordingGraphics.DrawLineCall>();

        Assert.AreEqual(2, lines.Count());
    }

    [Ignore("Gap, see Spec/css-text-decor-3.yml")]
    [TestMethod]
    [Spec("css-text-decor-3", "#text-shadow-property")]
    public void TextShadow_PaintsTheTextInTheShadowColor()
    {
        var strings = Log("<span style='color:black;text-shadow:2px 2px rgb(255,0,0)'>text</span>").OfType<RecordingGraphics.DrawStringCall>();

        Assert.IsTrue(strings.Any(s => s.Color == Red));
    }

    [Ignore("Gap, see Spec/css-text-decor-3.yml")]
    [TestMethod]
    [Spec("css-text-decor-3", "#painting-order")]
    [DataRow("underline")]
    [DataRow("overline")]
    public void UnderlineAndOverline_ArePaintedBelowTheText(string line)
    {
        var log = Log($"<span style='text-decoration:{line}'>text</span>");

        Assert.IsLessThan(IndexOf<RecordingGraphics.DrawStringCall>(log), IndexOf<RecordingGraphics.DrawLineCall>(log));
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#painting-order")]
    public void LineThrough_IsPaintedOverTheText()
    {
        var log = Log("<span style='text-decoration:line-through'>text</span>");

        Assert.IsGreaterThan(IndexOf<RecordingGraphics.DrawStringCall>(log), IndexOf<RecordingGraphics.DrawLineCall>(log));
    }

    [TestMethod]
    [Spec("css-text-decor-3", "#overflow")]
    public void Decorations_DontChangeTheLayout()
    {
        var (plain, _) = PaintHarness.Layout(PaintHarness.Wrap("<div id='d'>text</div>"));
        var (decorated, _) = PaintHarness.Layout(PaintHarness.Wrap("<div id='d' style='text-decoration:underline overline line-through'>text</div>"));

        Assert.AreEqual(PaintHarness.FindById(plain, "d")!.Size.Height, PaintHarness.FindById(decorated, "d")!.Size.Height, 0.01);
    }

    private static List<object> Log(string body)
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(body));
        return PaintHarness.PaintPage(container).Log.Cast<object>().ToList();
    }

    private static int IndexOf<T>(List<object> log) => log.FindIndex(call => call is T);
}
