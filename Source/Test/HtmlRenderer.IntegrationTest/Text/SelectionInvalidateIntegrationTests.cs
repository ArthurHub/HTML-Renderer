using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Text;

/// <summary>
/// Dragging a selection repaints the control only when the selection changes, every extra repaint shows up
/// as flicker over remote desktop (#271).
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class SelectionInvalidateIntegrationTests
{
    [TestMethod]
    public void MouseMove_WithinTheSameCharacter_DoesNotRepaint()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap("<pre id='p'>first second third</pre>"));
        var words = new List<CssRect>();
        CollectWords(LayoutHarness.FindById(root, "p")!, words);
        var first = words.First(w => w.Text == "first");
        var third = words.First(w => w.Text == "third");
        var control = new FakeControl(container.Adapter);

        var start = new RPoint(first.Left + 1, first.Top + 2);
        container.HandleMouseDown(control, start);
        control.Invalidations = 0;

        container.HandleMouseMove(control, start);
        Assert.AreEqual(1, control.Invalidations, "starting the selection must repaint");
        Assert.AreEqual("", container.SelectedText ?? "");

        var target = new RPoint(third.Left + third.Width / 2, third.Top + 2);
        container.HandleMouseMove(control, target);
        Assert.AreEqual(2, control.Invalidations, "selecting up to the target must repaint");
        var selected = container.SelectedText;
        StringAssert.StartsWith(selected, "first second t");
        Assert.AreNotEqual("first second third", selected);

        container.HandleMouseMove(control, target);
        container.HandleMouseMove(control, new RPoint(target.X + 0.1, target.Y + 1));
        Assert.AreEqual(2, control.Invalidations, "moving within the same character must not repaint");
        Assert.AreEqual(selected, container.SelectedText);

        container.HandleMouseMove(control, new RPoint(third.Right - 1, target.Y));
        Assert.AreEqual(3, control.Invalidations, "extending the selection must repaint");
        Assert.AreEqual("first second third", container.SelectedText);
    }

    private static void CollectWords(CssBox box, List<CssRect> words)
    {
        words.AddRange(box.Words);
        foreach (var child in box.Boxes)
            CollectWords(child, words);
    }

    /// <summary>A control with the left mouse button held down that counts its repaints.</summary>
    private sealed class FakeControl(RAdapter adapter) : RControl(adapter)
    {
        private const double CharWidth = 8;

        public int Invalidations { get; set; }

        public override bool LeftMouseButton => true;
        public override bool RightMouseButton => false;
        public override RPoint MouseLocation => RPoint.Empty;
        public override void SetCursorDefault() { }
        public override void SetCursorHand() { }
        public override void SetCursorIBeam() { }
        public override void DoDragDropCopy(object dragDropData) { }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            charFit = Math.Min(str.Length, (int)(maxWidth / CharWidth));
            charFitWidth = charFit * CharWidth;
        }

        public override void Invalidate() => Invalidations++;
    }
}
