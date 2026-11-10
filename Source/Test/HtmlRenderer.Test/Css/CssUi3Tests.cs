using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class CssUi3Tests
{
    [TestMethod]
    [Spec("css-ui-3", "#box-sizing")]
    [Spec("css-sizing-3", "#box-sizing")]
    public void BoxSizing_IsNotInherited()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div style='box-sizing:border-box'><p id=x>a</p></div>"));

        Assert.AreEqual("content-box", LayoutHarness.FindById(root, "x")!.BoxSizing);
    }

    [Ignore("Gap, see Spec/css-ui-3.yml")]
    [TestMethod]
    [Spec("css-ui-3", "#outline-style")]
    [Spec("css-ui-4", "#outline-style")]
    public void OutlineStyleAuto_IsAccepted()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline-style: auto");

        Assert.IsTrue(property.HasValue);
    }

    [Ignore("Gap, see Spec/css-ui-3.yml")]
    [TestMethod]
    [Spec("css-ui-3", "#outline-color")]
    [Spec("css-ui-4", "#outline-color")]
    public void OutlineColorInvert_IsRejectedWithoutColorInversion()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline-color: invert");

        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [DataRow("cursor: pointer")]
    [DataRow("cursor: url(hand.cur) 2 3, pointer")]
    [Spec("css-ui-3", "#cursor")]
    [Spec("css-ui-4", "#cursor")]
    public void Cursor_IsAccepted(string declaration)
    {
        var property = CssConstructionFunctions.ParseDeclaration(declaration);

        Assert.IsTrue(property.HasValue);
    }

    [Ignore("Gap, see Spec/css-ui-3.yml")]
    [TestMethod]
    [Spec("css-ui-3", "#cursor")]
    [Spec("css-ui-4", "#cursor")]
    public void Cursor_SetsThePointerOverTheElement()
    {
        var (_, container) = LayoutHarness.Layout(LayoutHarness.Wrap("<div style='cursor:pointer;height:100px'></div>"));
        var control = new CursorControl(container.Adapter);

        container.HandleMouseMove(control, new RPoint(50, 50));

        Assert.AreEqual("hand", control.Cursor);
    }

    [TestMethod]
    [DataRow("resize: both")]
    [DataRow("caret-color: red")]
    [DataRow("text-overflow: ellipsis")]
    [DataRow("outline-offset: 2px")]
    [DataRow("image-rendering: pixelated")]
    [DataRow("image-orientation: none")]
    public void UnsupportedProperties_AreDropped(string declaration)
    {
        var style = CssConstructionFunctions.ParseDeclarations(declaration);

        Assert.AreEqual(0, style.Length);
    }

    private sealed class CursorControl(RAdapter adapter) : RControl(adapter)
    {
        public string? Cursor { get; private set; }

        public override bool LeftMouseButton => false;
        public override bool RightMouseButton => false;
        public override RPoint MouseLocation => RPoint.Empty;
        public override void SetCursorDefault() => Cursor = "default";
        public override void SetCursorHand() => Cursor = "hand";
        public override void SetCursorIBeam() => Cursor = "ibeam";
        public override void DoDragDropCopy(object dragDropData) { }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            charFit = str.Length;
            charFitWidth = 0;
        }

        public override void Invalidate() { }
    }
}
