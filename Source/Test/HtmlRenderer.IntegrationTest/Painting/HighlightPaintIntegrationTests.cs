using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class HighlightPaintIntegrationTests
{
    private static readonly RColor Text = RColor.FromArgb(10, 20, 30);
    private static readonly RColor Back = RColor.FromArgb(40, 50, 60);
    private static readonly RColor DefaultBack = RColor.FromArgb(0xa9, 0x33, 0x99, 0xff);

    [TestMethod]
    [Spec("css-pseudo-4", "#highlight-selectors")]
    [Spec("css-pseudo-4", "#highlight-styling")]
    [Spec("css-pseudo-4", "#highlight-backgrounds")]
    [Spec("css-pseudo-4", "#highlight-text")]
    public void Selection_PaintsTheBackgroundAndTheTextInItsColors()
    {
        var g = PaintSelected("::selection { color: rgb(10,20,30); background-color: rgb(40,50,60) }",
            "<div id='b'>x</div>");

        Assert.IsTrue(Rects(g).Any(r => r.Color == Back));
        Assert.AreEqual(Text, g.DrawStringCalls.Single().Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-pseudo-4.yml")]
    [Spec("css-pseudo-4", "#highlight-ua-styles")]
    public void WithoutSelectionRules_TheDefaultHighlightUsesHighlightAndHighlightText()
    {
        var (root, container) = Layout("", "<div id='b' style='background-color:Highlight; color:HighlightText'>x</div>");
        var reference = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "b")!);
        var highlight = Rects(reference).Single().Color;
        var highlightText = reference.DrawStringCalls.Single().Color;

        var g = PaintSelected("", "<div id='b' style='color:rgb(10,20,30)'>x</div>");

        Assert.IsTrue(Rects(g).Any(r => r.Color == highlight));
        Assert.AreEqual(highlightText, g.DrawStringCalls.Single().Color);
    }

    [TestMethod]
    [Spec("css-pseudo-4", "#highlight-bounds")]
    public void SelectionBackground_CoversTheWordUpToTheLineBoxEdges()
    {
        var (root, container) = Layout("::selection { background-color: rgb(40,50,60) }",
            "<div id='b' style='line-height:40px'>x</div>");
        SelectAll(container);
        var box = PaintHarness.FindById(root, "b")!;
        var g = PaintHarness.PaintBox(container, box);

        var rect = Rects(g).Single(r => r.Color == Back);
        var text = g.DrawStringCalls.Single();
        Assert.IsLessThanOrEqualTo(text.Point.X, rect.X);
        Assert.IsGreaterThanOrEqualTo(text.Point.X + text.Size.Width, rect.X + rect.Width);
        Assert.IsLessThanOrEqualTo(text.Point.Y, rect.Y);
        Assert.IsGreaterThanOrEqualTo(text.Point.Y + text.Size.Height, rect.Y + rect.Height);
        Assert.IsLessThanOrEqualTo(40, rect.Height);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-pseudo-4.yml")]
    [Spec("css-pseudo-4", "#paired-defaults")]
    public void AuthorSelectionColor_SuppressesTheDefaultBackground()
    {
        var g = PaintSelected("::selection { color: rgb(10,20,30) }", "<div id='b'>x</div>");

        Assert.IsFalse(Rects(g).Any(r => r.Color == DefaultBack));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-pseudo-4.yml")]
    [Spec("css-pseudo-4", "#highlight-cascade")]
    public void SelectionRule_OnlyAppliesToItsOriginatingElement()
    {
        var g = PaintSelected("p.w::selection { background-color: rgb(40,50,60) }",
            "<p id='b'>x</p><p class='w'>y</p>");

        Assert.IsFalse(Rects(g).Any(r => r.Color == Back));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-pseudo-4.yml")]
    [Spec("css-pseudo-4", "#highlight-text")]
    public void TextDecorations_AreRedrawnInTheSelectionColor()
    {
        var g = PaintSelected("::selection { color: rgb(10,20,30) }",
            "<div id='b' style='text-decoration:underline; color:rgb(200,0,0)'>x</div>");

        var lines = g.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();
        Assert.IsNotEmpty(lines);
        Assert.IsTrue(lines.All(l => l.Color == Text));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-pseudo-4.yml")]
    [Spec("css-pseudo-4", "#highlight-styling")]
    public void SelectionTextDecoration_IsPainted()
    {
        var g = PaintSelected("::selection { text-decoration: underline; text-decoration-color: rgb(10,20,30) }",
            "<div id='b'>x</div>");

        Assert.IsTrue(g.Log.OfType<RecordingGraphics.DrawLineCall>().Any(l => l.Color == Text));
    }

    [TestMethod]
    [Spec("css-pseudo-4", "#highlight-replaced")]
    public void SelectedImage_GetsASemiTransparentWash()
    {
        var (root, container) = Layout("::selection { background-color: rgb(40,50,60) }",
            $"<div id='b'><img src='{TestImages.Png(20, 20)}'></div>", images: true);
        SelectAll(container);
        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "b")!);

        var wash = Rects(g).Single(r => r.Color.R == 40 && r.Color.G == 50 && r.Color.B == 60);
        Assert.IsLessThan(255, (int)wash.Color.A);
    }

    private static RecordingGraphics PaintSelected(string css, string body)
    {
        var (root, container) = Layout(css, body);
        SelectAll(container);
        return PaintHarness.PaintBox(container, PaintHarness.FindById(root, "b")!);
    }

    private static (CssBox Root, HtmlContainerInt Container) Layout(string css, string body, bool images = false)
    {
        var container = new HtmlContainerInt(new MockAdapter())
        {
            MaxSize = new RSize(1000, 4000),
            Location = RPoint.Empty,
            PageSize = new RSize(1000, 4000),
            AvoidAsyncImagesLoading = images,
            AvoidImagesLateLoading = images
        };

        container.SetHtml($"<html><head><style>{css}</style></head><body style='margin:0'>{body}</body></html>");
        using var layoutGraphics = new RecordingGraphics();
        container.PerformLayout(layoutGraphics);

        return (container.Root!, container);
    }

    private static void SelectAll(HtmlContainerInt container) =>
        container.HandleKeyDown(new MockControl(new MockAdapter()), new RKeyEvent(true, true, false));

    private static List<RecordingGraphics.DrawRectCall> Rects(RecordingGraphics g) =>
        g.Log.OfType<RecordingGraphics.DrawRectCall>().ToList();

    private sealed class MockControl(RAdapter adapter) : RControl(adapter)
    {
        public override bool LeftMouseButton => false;
        public override bool RightMouseButton => false;
        public override RPoint MouseLocation => RPoint.Empty;
        public override void SetCursorDefault() { }
        public override void SetCursorHand() { }
        public override void SetCursorIBeam() { }
        public override void DoDragDropCopy(object dragDropData) { }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            charFit = str.Length;
            charFitWidth = 0;
        }

        public override void Invalidate() { }
    }
}
