using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssUi4Tests
{
    [TestMethod]
    [Ignore("Gap, see Spec/css-ui-4.yml")]
    [Spec("css-ui-4", "#outline")]
    public void OutlineAuto_SetsStyleAndColorToAuto()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline: auto");

        Assert.IsTrue(property.HasValue);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-ui-4.yml")]
    [Spec("css-ui-4", "#outline-color")]
    public void OutlineColorAuto_IsAccepted()
    {
        var property = CssConstructionFunctions.ParseDeclaration("outline-color: auto");

        Assert.IsTrue(property.HasValue);
    }

    [TestMethod]
    [DataRow("cursor: url(example.svg), url(hyper.cur), url(hyper.png) 2 3, pointer")]
    [DataRow("cursor: zoom-in")]
    [DataRow("cursor: grabbing")]
    [Spec("css-ui-4", "#cursor")]
    [Spec("css-ui-4", "#predefined-cursors")]
    public void Cursor_IsAccepted(string declaration)
    {
        var property = CssConstructionFunctions.ParseDeclaration(declaration);

        Assert.IsTrue(property.HasValue);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-ui-4.yml")]
    [Spec("css-ui-4", "#content-selection")]
    public void UserSelectNone_ExcludesTheElementFromTheSelection()
    {
        var (_, container) = LayoutHarness.Layout(LayoutHarness.Wrap("<p>selectable</p><p style='user-select:none'>excluded</p>"));

        container.HandleKeyDown(new TestControl(container.Adapter), new RKeyEvent(true, true, false));

        StringAssert.Contains(container.SelectedText, "selectable");
        Assert.DoesNotContain("excluded", container.SelectedText);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-ui-4.yml")]
    [Spec("css-ui-4", "#pointer-events-control")]
    public void PointerEventsNone_TakesTheLinkOutOfHitTesting()
    {
        var (_, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<a href='a.html' style='display:block;height:50px;pointer-events:none'>link</a>"));

        Assert.IsNull(container.GetLinkAt(new RPoint(5, 5)));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-ui-4.yml")]
    [Spec("css-ui-4", "#inertness")]
    public void InteractivityInert_TakesTheLinkOutOfHitTesting()
    {
        var (_, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='interactivity:inert'><a href='a.html' style='display:block;height:50px'>link</a></div>"));

        Assert.IsNull(container.GetLinkAt(new RPoint(5, 5)));
    }

    [TestMethod]
    [DataRow("appearance: none")]
    public void UnsupportedProperties_AreDropped(string declaration)
    {
        var style = CssConstructionFunctions.ParseDeclarations(declaration);

        Assert.AreEqual(0, style.Length);
    }

    [TestMethod]
    [Spec("css-ui-4", "#widget-accent")]
    [DataRow("accent-color: red")]
    public void AccentColor_IsAccepted(string declaration)
    {
        var style = CssConstructionFunctions.ParseDeclarations(declaration);

        Assert.AreEqual(1, style.Length);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-ui-4.yml")]
    [Spec("css-ui-4", "#appearance-switching")]
    [DataRow("appearance: none")]
    [DataRow("-webkit-appearance: none")]
    public void Appearance_IsAccepted(string declaration)
    {
        var style = CssConstructionFunctions.ParseDeclarations(declaration);

        Assert.AreEqual(1, style.Length);
    }

    private sealed class TestControl(RAdapter adapter) : RControl(adapter)
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
