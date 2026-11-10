using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class LinksTests
{
    [TestMethod]
    [Spec("html", "links.html#links-created-by-a-and-area-elements")]
    public void AWithHref_CreatesAHyperlink()
    {
        var (_, container) = LayoutHarness.Layout(LayoutHarness.Wrap("<a href=page.html>a</a> <a name=n>b</a>"));

        CollectionAssert.AreEqual(new[] { "page.html" }, container.GetLinks().Select(l => l.Href).ToArray());
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "links.html#links-created-by-a-and-area-elements")]
    public void AreaWithHref_CreatesAHyperlink()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<map name=m><area id=x shape=rect coords=0,0,10,10 href=page.html></map><img usemap=#m style='width:10px;height:10px'>"));

        Assert.IsTrue(LayoutHarness.FindById(root, "x")!.IsClickable);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "links.html#links-created-by-a-and-area-elements")]
    public void IsmapImage_AppendsTheClickCoordinates()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<a id=x href=map.cgi><img ismap style='display:block;width:10px;height:10px'></a>"));

        Assert.AreEqual("map.cgi?3,4", Click(container, LayoutHarness.FindById(root, "x")!, new RPoint(3, 4)));
    }

    [TestMethod]
    [Spec("html", "links.html#following-hyperlinks")]
    public void Click_RaisesLinkClickedWithTheHref()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap("<a id=x href=https://example.com/>a</a>"));

        Assert.AreEqual("https://example.com/", Click(container, LayoutHarness.FindById(root, "x")!));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "links.html#following-hyperlinks")]
    public void Click_ResolvesTheHrefAgainstTheBaseUrl()
    {
        var (root, container) = LayoutHarness.Layout(
            "<html><head><base href=https://example.com/dir/></head><body><a id=x href=page.html>a</a></body></html>");

        Assert.AreEqual("https://example.com/dir/page.html", Click(container, LayoutHarness.FindById(root, "x")!));
    }

    [TestMethod]
    [Spec("html", "links.html#following-hyperlinks")]
    [Spec("html", "iana.html#text/html")]
    public void FragmentLink_ScrollsToTheElement()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<a id=x href=#target>a</a><p id=target style='margin-top:100px'>b</p>"));

        Assert.AreEqual(LayoutHarness.FindById(root, "target")!.Location.Y, ScrollTarget(container, LayoutHarness.FindById(root, "x")!));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "links.html#following-hyperlinks")]
    [Spec("html", "iana.html#text/html")]
    public void FragmentLink_MatchesTheIdCaseSensitively()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<a id=x href=#Target>a</a><p id=target style='margin-top:100px'>b</p><p id=Target style='margin-top:100px'>c</p>"));

        Assert.AreEqual(LayoutHarness.FindById(root, "Target")!.Location.Y, ScrollTarget(container, LayoutHarness.FindById(root, "x")!));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "links.html#following-hyperlinks")]
    [Spec("html", "iana.html#text/html")]
    public void FragmentLink_FallsBackToANamedAnchor()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<a id=x href=#n>a</a><p id=p style='margin-top:100px'><a name=n>b</a></p>"));

        Assert.AreEqual(LayoutHarness.FindById(root, "p")!.Location.Y, ScrollTarget(container, LayoutHarness.FindById(root, "x")!));
    }

    [TestMethod]
    [Spec("html", "links.html#link-type-stylesheet")]
    [DataRow("stylesheet")]
    [DataRow("StyleSheet")]
    public void Stylesheet_IsApplied(string rel)
    {
        Assert.AreEqual("rgb(255, 0, 0)", ColorWithStylesheet($"<link rel={rel} href=a.css>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "links.html#link-type-stylesheet")]
    public void Stylesheet_AmongOtherLinkTypes_IsApplied()
    {
        Assert.AreEqual("rgb(255, 0, 0)", ColorWithStylesheet("<link rel='preload stylesheet' href=a.css>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "links.html#link-type-stylesheet")]
    public void DisabledStylesheet_IsNotApplied()
    {
        Assert.AreEqual("black", ColorWithStylesheet("<link rel=stylesheet href=a.css disabled>"));
    }

    [TestMethod]
    [Spec("html", "links.html#rel-alternate")]
    [Spec("cssom-1", "#css-style-sheet-collections")]
    public void AlternateStylesheet_IsNotApplied()
    {
        Assert.AreEqual("black", ColorWithStylesheet("<link rel='alternate stylesheet' title=Red href=a.css>"));
    }

    /// <summary>The color of a paragraph when every linked stylesheet makes it red.</summary>
    private static string ColorWithStylesheet(string link)
    {
        var container = new HtmlContainerInt(new MockAdapter()) { MaxSize = new RSize(1000, 1000) };
        container.StylesheetLoad += (_, e) => e.SetStyleSheet = "p { color: red }";
        container.SetHtml($"<html><head>{link}</head><body><p id=x>a</p></body></html>");
        using var graphics = new RecordingGraphics();
        container.PerformLayout(graphics);

        return LayoutHarness.FindById(container.Root!, "x")!.Color;
    }

    /// <summary>Clicks the link and returns the link the LinkClicked event reports, handling it so nothing gets opened.</summary>
    private static string? Click(HtmlContainerInt container, CssBox link, RPoint location = default)
    {
        string? clicked = null;
        container.LinkClicked += (_, e) =>
        {
            clicked = e.Link;
            e.Handled = true;
        };
        container.HandleLinkClicked(new MockControl(container.Adapter), location, link);
        return clicked;
    }

    /// <summary>Clicks the fragment link and returns where the container asks the host to scroll to.</summary>
    private static double? ScrollTarget(HtmlContainerInt container, CssBox link)
    {
        double? y = null;
        container.ScrollChange += (_, e) => y = e.Y;
        container.HandleLinkClicked(new MockControl(container.Adapter), RPoint.Empty, link);
        return y;
    }

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
