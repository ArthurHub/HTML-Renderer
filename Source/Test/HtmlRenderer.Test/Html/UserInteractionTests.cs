using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class UserInteractionTests
{
    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "interaction.html#the-hidden-attribute")]
    [DataRow("<div id=x hidden>a</div>")]
    [DataRow("<div id=x hidden=''>a</div>")]
    [DataRow("<div id=x hidden=foo>a</div>")]
    public void Hidden_IsNotRendered(string html)
    {
        Assert.AreEqual("none", Box(html).Display);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "interaction.html#the-hidden-attribute")]
    public void HiddenUntilFound_KeepsItsBoxButSkipsItsContent()
    {
        var box = Box("<div id=x hidden=until-found style='border:1px solid'>a</div>");

        Assert.AreEqual("block", box.Display);
        Assert.AreEqual(2, box.Size.Height, 0.01);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "interaction.html#the-inert-attribute")]
    public void InertLink_IsNotHit()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div inert><a id=x href=page.html>link</a></div>"));

        Assert.IsNull(DomUtils.GetLinkBox(root, Center(LayoutHarness.FindById(root, "x")!)));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "interaction.html#the-inert-attribute")]
    public void InertText_IsNotSelected()
    {
        var (_, container) = LayoutHarness.Layout(LayoutHarness.Wrap("<p>active</p><p inert>inert</p>"));

        container.HandleKeyDown(new MockControl(container.Adapter), new RKeyEvent(true, true, false));

        Assert.AreEqual("active", container.SelectedText.Trim());
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "interaction.html#the-autofocus-attribute")]
    public void Autofocus_FocusesTheElement()
    {
        var box = Box("<style>#x:focus { color: red }</style><div id=x tabindex=0 autofocus>a</div>");

        Assert.AreEqual(Red, box.ActualColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "interaction.html#contenteditable")]
    public void Contenteditable_MakesTheElementEditable()
    {
        var box = Box("<style>#x:read-write { color: red }</style><div id=x contenteditable>a</div>");

        Assert.AreEqual(Red, box.ActualColor);
    }

    [TestMethod]
    [Spec("html", "dnd.html#drag-and-drop-processing-model")]
    public void DraggingTheSelection_DragsItsHtmlAndText()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id=x>hello</p>"));
        var control = new MockControl(container.Adapter);
        container.HandleKeyDown(control, new RKeyEvent(true, true, false));

        var dragged = Drag(container, control, Center(LayoutHarness.FindById(root, "x")!));

        Assert.IsNotNull(dragged);
        var (html, text) = ((string, string))dragged;
        StringAssert.Contains(html, "hello");
        Assert.AreEqual("hello", text.Trim());
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "dnd.html#drag-and-drop-processing-model")]
    public void DraggingALink_DragsIt()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap("<a id=x href=page.html>link</a>"));

        Assert.IsNotNull(Drag(container, new MockControl(container.Adapter), Center(LayoutHarness.FindById(root, "x")!)));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "popover.html#the-popover-attribute")]
    [DataRow("<div id=x popover>a</div>")]
    [DataRow("<div id=x popover=manual>a</div>")]
    public void Popover_IsHiddenUntilShown(string html)
    {
        Assert.AreEqual("none", Box(html).Display);
    }

    /// <summary>The center of the first word inside the box.</summary>
    private static RPoint Center(CssBox box)
    {
        var word = LayoutHarness.Descendants(box).SelectMany(b => b.Words).First();
        return new RPoint(word.Left + word.Width / 2, word.Top + word.Height / 2);
    }

    /// <summary>Presses the left button at the location and moves the mouse, returns what got dragged, if anything.</summary>
    private static object? Drag(HtmlContainerInt container, MockControl control, RPoint location)
    {
        control.LeftMouseButtonDown = true;
        container.HandleMouseDown(control, location);
        // A drag only starts once the button has been held for 200ms.
        Thread.Sleep(250);
        container.HandleMouseMove(control, new RPoint(location.X + 5, location.Y));
        return control.Dragged;
    }

    private sealed class MockControl(RAdapter adapter) : RControl(adapter)
    {
        public bool LeftMouseButtonDown { get; set; }
        public object? Dragged { get; private set; }

        public override bool LeftMouseButton => LeftMouseButtonDown;
        public override bool RightMouseButton => false;
        public override RPoint MouseLocation => RPoint.Empty;
        public override void SetCursorDefault() { }
        public override void SetCursorHand() { }
        public override void SetCursorIBeam() { }
        public override void DoDragDropCopy(object dragDropData) => Dragged = dragDropData;

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            charFit = str.Length;
            charFitWidth = 0;
        }

        public override void Invalidate() { }
    }
}
