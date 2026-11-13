using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.CssModules;

[DoNotParallelize]
[TestClass]
public sealed class CssDisplayTests
{
    private const double Delta = 1.0;
    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);

    [TestMethod]
    [Spec("css-display-3", "#outer-role")]
    [Spec("css-display-4", "#outer-role")]
    public void BlockAndInlineOuterRoles_StackOrShareTheLine()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div><span id='a' style='display:block'>first</span><span id='b' style='display:block'>second</span>"
            + "<div id='c' style='display:inline'>third</div> <div id='d' style='display:inline'>fourth</div></div>"));

        Assert.IsGreaterThanOrEqualTo(FindWord(root, "first")!.Bottom - Delta, FindWord(root, "second")!.Top);
        Assert.AreEqual(FindWord(root, "third")!.Top, FindWord(root, "fourth")!.Top, Delta);
    }

    [Ignore("Gap, see Spec/css-display-3.yml")]
    [TestMethod]
    [Spec("css-display-3", "#outer-role")]
    [Spec("css-display-4", "#outer-role")]
    [Spec("css-display-3", "#run-in-layout")]
    [Spec("css-display-4", "#run-in-layout")]
    public void RunIn_IsInsertedAtTheStartOfTheFollowingBlock()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:run-in'>head</div><p style='margin:0'>body</p>"));

        Assert.AreEqual(FindWord(root, "head")!.Top, FindWord(root, "body")!.Top, Delta);
        Assert.IsLessThan(FindWord(root, "body")!.Left, FindWord(root, "head")!.Right);
    }

    [Ignore("Gap, see Spec/css-display-3.yml")]
    [TestMethod]
    [DataRow("block flow", "block")]
    [DataRow("inline flow", "inline")]
    [DataRow("inline flow-root", "inline-block")]
    [DataRow("block table", "table")]
    [DataRow("inline table", "inline-table")]
    [DataRow("flow", "block")]
    [Spec("css-display-3", "#the-display-properties")]
    [Spec("css-display-4", "#the-display-properties")]
    public void TwoValueSyntax_ComputesToTheEquivalentShortValue(string value, string expected)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            $"<p style='display:{(expected == "block" ? "inline" : "block")};display:{value}' id='e'>x</p>"));

        Assert.AreEqual(expected, LayoutHarness.FindById(root, "e")!.Display);
    }

    [TestMethod]
    [Spec("css-display-3", "#inner-model")]
    [Spec("css-display-4", "#inner-model")]
    public void FlowInnerDisplay_LaysTheChildrenOutInNormalFlow()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='d' style='display:block'><div id='a' style='height:20px'></div><div id='b' style='height:30px'></div></div>"));

        var d = LayoutHarness.FindById(root, "d")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(20, b.Location.Y - d.Location.Y, Delta);
        Assert.AreEqual(50, d.Size.Height, Delta);
    }

    [TestMethod]
    [Spec("css-display-3", "#inner-model")]
    [Spec("css-display-4", "#inner-model")]
    public void FlowRoot_IsBlockLevel()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div>first <span id='fr' style='display:flow-root'>second</span></div>"));

        Assert.IsGreaterThan(FindWord(root, "first")!.Top + Delta, FindWord(root, "second")!.Top);
    }

    [Ignore("Gap, see Spec/css-display-3.yml")]
    [TestMethod]
    [Spec("css-display-3", "#inner-model")]
    [Spec("css-display-4", "#inner-model")]
    public void FlowRoot_IsBlockLevelAndContainsItsFloats()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<span id='fr' style='display:flow-root'><div style='float:left;width:50px;height:80px'></div>text</span><div id='after'>after</div>"));

        var fr = LayoutHarness.FindById(root, "fr")!;

        Assert.AreEqual(80, fr.Size.Height, Delta);
        Assert.IsGreaterThanOrEqualTo(80 - Delta, FindWord(root, "after")!.Top);
    }

    [Ignore("Gap, see Spec/css-display-3.yml")]
    [TestMethod]
    [Spec("css-display-3", "#inner-model")]
    [Spec("css-display-4", "#inner-model")]
    [Spec("css-display-3", "#layout-specific-display")]
    [Spec("css-display-4", "#layout-specific-display")]
    public void Ruby_KeepsTheAnnotationOffTheBaseLine()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div><span style='display:ruby'>base<span style='display:ruby-text'>anno</span></span></div>"));

        Assert.IsLessThan(FindWord(root, "base")!.Top - Delta, FindWord(root, "anno")!.Top);
    }

    [TestMethod]
    [Spec("css-display-3", "#list-items")]
    [Spec("css-display-4", "#list-items")]
    public void ListItem_GeneratesAMarker()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='li' style='display:list-item;margin-left:40px;list-style-type:square'>item</div>"));

        var li = LayoutHarness.FindById(root, "li")!;

        Assert.IsNotNull(li.ListItemBox);
    }

    [Ignore("Gap, see Spec/css-display-3.yml")]
    [TestMethod]
    [Spec("css-display-3", "#list-items")]
    [Spec("css-display-4", "#list-items")]
    public void InlineListItem_IsInlineAndGeneratesAMarker()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div>first <span id='li' style='display:inline list-item;list-style-position:inside'>second</span></div>"));

        var li = LayoutHarness.FindById(root, "li")!;

        Assert.AreEqual(FindWord(root, "first")!.Top, FindWord(root, "second")!.Top, Delta);
        Assert.IsNotNull(li.ListItemBox);
    }

    [TestMethod]
    [Spec("css-display-3", "#layout-specific-display")]
    [Spec("css-display-4", "#layout-specific-display")]
    public void TableInternalDisplayTypes_BuildATable()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:table'><div style='display:table-row'>"
            + "<div id='a' style='display:table-cell;width:50px'>a</div><div id='b' style='display:table-cell;width:50px'>b</div>"
            + "</div></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(a.Location.Y, b.Location.Y, Delta);
        Assert.IsGreaterThanOrEqualTo(a.Location.X + 50 - Delta, b.Location.X);
    }

    [TestMethod]
    [Spec("css-display-3", "#box-generation")]
    [Spec("css-display-4", "#box-generation")]
    public void DisplayNone_PaintsNothingOfTheSubtree()
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div style='display:none'><div style='display:block;height:50px;background:rgb(255,0,0)'>gone</div></div><div>after</div>"));
        var g = PaintHarness.PaintPage(container);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == Red));
        Assert.IsFalse(g.DrawStringCalls.Any(c => c.Text == "gone"));
        Assert.AreEqual(0, g.DrawStringCalls.Single(c => c.Text == "after").Point.Y, Delta);
    }

    [Ignore("Gap, see Spec/css-display-3.yml")]
    [TestMethod]
    [Spec("css-display-3", "#box-generation")]
    [Spec("css-display-4", "#box-generation")]
    public void DisplayContents_ReplacesTheElementByItsChildren()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div>first <div id='c' style='display:contents;padding-left:50px;border:5px solid red'>second</div> third</div>"));

        Assert.AreEqual(FindWord(root, "first")!.Top, FindWord(root, "second")!.Top, Delta);
        Assert.AreEqual(FindWord(root, "second")!.Top, FindWord(root, "third")!.Top, Delta);
    }

    [Ignore("Gap, see Spec/css-display-3.yml")]
    [TestMethod]
    [Spec("css-display-3", "#box-generation")]
    [Spec("css-display-4", "#box-generation")]
    public void DisplayContents_PaintsNoBoxOfItsOwn()
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div style='display:contents;background:rgb(255,0,0)'><div style='height:20px'>child</div></div>"));
        var g = PaintHarness.PaintPage(container);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == Red));
        Assert.IsTrue(g.DrawStringCalls.Any(c => c.Text == "child"));
    }

    [Ignore("Gap, see Spec/css-display-3.yml")]
    [TestMethod]
    [Spec("css-display-3", "#legacy-display")]
    [Spec("css-display-4", "#legacy-display")]
    public void InlineBlockWithBlockChildren_StaysOnTheLine()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div>first <span style='display:inline-block;width:80px'><div>a</div><div>b</div></span> second</div>"));

        Assert.IsGreaterThan(FindWord(root, "first")!.Right, FindWord(root, "a")!.Left);
        Assert.IsGreaterThan(FindWord(root, "a")!.Right, FindWord(root, "second")!.Left);
    }

    [Ignore("Gap, see Spec/css-display-3.yml")]
    [TestMethod]
    [DataRow("flex")]
    [DataRow("grid")]
    [DataRow("inline-flex")]
    [DataRow("inline-grid")]
    [Spec("css-display-3", "#inner-model")]
    [Spec("css-display-4", "#inner-model")]
    [Spec("css-display-3", "#legacy-display")]
    [Spec("css-flexbox-1", "#flex-containers")]
    [Spec("css-display-4", "#legacy-display")]
    public void FlexAndGridContainers_PaintTheirContent(string display)
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(
            $"<div style='display:{display}'><div>aaa</div><div>bbb</div></div>"));
        var g = PaintHarness.PaintPage(container);

        Assert.IsTrue(g.DrawStringCalls.Any(c => c.Text == "aaa"));
        Assert.IsTrue(g.DrawStringCalls.Any(c => c.Text == "bbb"));
    }

    [Ignore("Gap, see Spec/css-display-3.yml")]
    [TestMethod]
    [Spec("css-display-3", "#transformations")]
    [Spec("css-display-4", "#transformations")]
    public void FloatedInlineBlock_BlockifiesToBlock()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div><span id='f' style='display:inline-block;float:left;width:50px;height:30px'></span>text</div>"));

        Assert.AreEqual("block", LayoutHarness.FindById(root, "f")!.Display);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("display:inline")]
    [Spec("css-display-3", "#root")]
    [Spec("css-display-4", "#root")]
    public void RootElement_IsBlockLevelAndFillsTheViewport(string style)
    {
        var (root, _) = LayoutHarness.Layout($"<!DOCTYPE html><html id='h' style='{style}'><body style='margin:0'><div style='height:20px'></div></body></html>", maxWidth: 400);

        var html = LayoutHarness.FindById(root, "h")!;

        Assert.AreEqual("block", html.Display);
        Assert.AreEqual(400, html.Size.Width, Delta);
    }

    [Ignore("Gap, see Spec/css-display-3.yml")]
    [TestMethod]
    [DataRow("inline-block")]
    [DataRow("table-cell")]
    [Spec("css-display-3", "#root")]
    [Spec("css-display-4", "#root")]
    public void RootElement_IsBlockified(string display)
    {
        var (root, _) = LayoutHarness.Layout($"<!DOCTYPE html><html id='h' style='display:{display}'><body style='margin:0'><div style='height:20px'></div></body></html>", maxWidth: 400);

        var html = LayoutHarness.FindById(root, "h")!;

        Assert.AreEqual("block", html.Display);
        Assert.AreEqual(400, html.Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css-display-3", "#visibility")]
    [Spec("css-display-4", "#visibility")]
    public void VisibilityCollapse_OutsideTables_HidesLikeHidden()
    {
        var (_, collapsed) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div style='visibility:collapse;height:20px;background:rgb(255,0,0)'>gone</div><div>after</div>"));
        var g = PaintHarness.PaintPage(collapsed);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == Red));
        Assert.IsFalse(g.DrawStringCalls.Any(c => c.Text == "gone"));
        Assert.IsGreaterThanOrEqualTo(20 - Delta, g.DrawStringCalls.Single(c => c.Text == "after").Point.Y);
    }

    private static CssRect? FindWord(CssBox box, string text)
    {
        foreach (var word in box.Words)
        {
            if (word.Text == text)
                return word;
        }

        foreach (var child in box.Boxes)
        {
            var found = FindWord(child, text);
            if (found is not null)
                return found;
        }

        return null;
    }
}
