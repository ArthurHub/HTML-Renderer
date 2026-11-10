using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.CssModules;

[DoNotParallelize]
[TestClass]
public sealed class CssSizingTests
{
    private const double Delta = 0.5;

    [TestMethod]
    [Spec("css-sizing-3", "#box-sizing")]
    public void ContentBox_SizesTheContentBox()
    {
        var root = Layout("<div id='b' style='box-sizing:content-box;width:100px;height:50px;padding:10px;border:5px solid'></div>");
        var b = Box(root, "b");

        Assert.AreEqual(130, b.Size.Width, Delta);
        Assert.AreEqual(80, b.Size.Height, Delta);
    }

    [TestMethod]
    [Spec("css-sizing-3", "#box-sizing")]
    public void BorderBox_SizesTheBorderBox()
    {
        var root = Layout("<div id='b' style='box-sizing:border-box;width:100px;height:50px;padding:10px;border:5px solid'><div id='c' style='height:10px'></div></div>");
        var b = Box(root, "b");

        Assert.AreEqual(100, b.Size.Width, Delta);
        Assert.AreEqual(50, b.Size.Height, Delta);
        Assert.AreEqual(70, Box(root, "c").Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css-sizing-3", "#box-sizing")]
    public void BorderBox_PercentageWidth_SizesTheBorderBox()
    {
        var root = Layout("<div style='width:200px'><div id='b' style='box-sizing:border-box;width:50%;padding:0 10px;border:5px solid;height:10px'></div></div>");

        Assert.AreEqual(100, Box(root, "b").Size.Width, Delta);
    }

    [Ignore("Gap, see Spec/css-sizing-3.yml")]
    [TestMethod]
    [Spec("css-sizing-3", "#box-sizing")]
    public void BorderBox_ContentSizeIsFlooredAtZero()
    {
        var root = Layout("<div id='b' style='box-sizing:border-box;width:100px;height:10px;padding-left:60px;border-left:60px solid'></div>");

        Assert.AreEqual(120, Box(root, "b").Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css-sizing-3", "#box-sizing")]
    public void BorderBox_AppliesToMaxHeight()
    {
        var root = Layout("<div id='b' style='box-sizing:border-box;max-height:50px;padding:10px;border:5px solid'><div style='height:200px'></div></div>");

        Assert.AreEqual(50, Box(root, "b").Size.Height, Delta);
    }

    [Ignore("Gap, see Spec/css-sizing-3.yml")]
    [TestMethod]
    [DataRow("inline-size:100px", "width")]
    [DataRow("block-size:40px", "height")]
    [Spec("css-sizing-3", "#preferred-size-properties")]
    [Spec("css-logical-1", "#dimension-properties")]
    [Spec("css-sizing-4", "#sizing-properties")]
    public void FlowRelativePreferredSize_MapsToThePhysicalSize(string style, string axis)
    {
        var root = Layout($"<div style='width:300px'><div id='b' style='{style}'><div style='height:10px'></div></div></div>");
        var b = Box(root, "b");

        Assert.AreEqual(axis == "width" ? 100 : 40, axis == "width" ? b.Size.Width : b.Size.Height, Delta);
    }

    [Ignore("Gap, see Spec/css-sizing-3.yml")]
    [TestMethod]
    [Spec("css-sizing-3", "#min-size-properties")]
    [Spec("css-logical-1", "#dimension-properties")]
    [Spec("css-sizing-4", "#sizing-properties")]
    public void MinBlockSize_GrowsTheBox()
    {
        var root = Layout("<div id='b' style='min-block-size:40px'></div>");

        Assert.AreEqual(40, Box(root, "b").Size.Height, Delta);
    }

    [Ignore("Gap, see Spec/css-sizing-3.yml")]
    [TestMethod]
    [Spec("css-sizing-3", "#max-size-properties")]
    [Spec("css-logical-1", "#dimension-properties")]
    [Spec("css-sizing-4", "#sizing-properties")]
    public void MaxInlineSize_LimitsTheBox()
    {
        var root = Layout("<div style='width:300px'><div id='b' style='max-inline-size:100px;height:10px'></div></div>");

        Assert.AreEqual(100, Box(root, "b").Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css-sizing-3", "#sizing-values")]
    [Spec("css-sizing-3", "#percentage-sizing")]
    [Spec("css-sizing-4", "#percentage-sizing")]
    public void PercentageSizes_ResolveAgainstADefiniteContainingBlock()
    {
        var root = Layout("<div style='width:300px'><div id='b' style='width:50%;height:10px'></div></div>");

        Assert.AreEqual(150, Box(root, "b").Size.Width, Delta);
    }

    [Ignore("Gap, see Spec/css-sizing-3.yml")]
    [TestMethod]
    [Spec("css-sizing-3", "#max-size-properties")]
    public void PercentageMaxWidth_ClampsThePercentageWidth()
    {
        var root = Layout("<div style='width:300px'><div id='b' style='width:50%;max-width:40%;height:10px'></div></div>");

        Assert.AreEqual(120, Box(root, "b").Size.Width, Delta);
    }

    [Ignore("Gap, see Spec/css-sizing-3.yml")]
    [TestMethod]
    [Spec("css-sizing-3", "#percentage-sizing")]
    [Spec("css-sizing-4", "#percentage-sizing")]
    public void NestedPercentageHeights_ResolveAgainstTheResolvedPercentage()
    {
        var root = Layout("<div style='height:200px'><div id='f' style='height:50%'><div id='i' style='height:50%'></div></div></div>");

        Assert.AreEqual(100, Box(root, "f").Size.Height, Delta);
        Assert.AreEqual(50, Box(root, "i").Size.Height, Delta);
    }

    [Ignore("Gap, see Spec/css-sizing-3.yml")]
    [TestMethod]
    [DataRow("min-content")]
    [DataRow("max-content")]
    [DataRow("fit-content")]
    [Spec("css-sizing-3", "#sizing-values")]
    public void IntrinsicSizeKeywords_SizeABlockToItsContent(string keyword)
    {
        var root = Layout($"<div style='width:400px'><div id='b' style='width:{keyword}'><span style='display:inline-block;width:60px;height:10px'></span></div></div>");

        Assert.AreEqual(60, Box(root, "b").Size.Width, Delta);
    }

    [Ignore("Gap, see Spec/css-sizing-3.yml")]
    [TestMethod]
    [Spec("css-sizing-3", "#sizing-values")]
    [Spec("css-sizing-3", "#stretch-fit-sizing")]
    [Spec("css-sizing-4", "#extrinsic")]
    public void Stretch_FillsTheContainingBlock()
    {
        var root = Layout("<div style='width:300px'><span id='b' style='display:inline-block;width:stretch;margin:0 10px;height:10px'></span></div>");

        Assert.AreEqual(280, Box(root, "b").Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css-sizing-3", "#behave-auto")]
    public void PercentageHeight_AgainstAnIndefiniteHeight_BehavesAsAuto()
    {
        var root = Layout("<div><div id='b' style='height:50%'><div style='height:30px'></div></div></div>");

        Assert.AreEqual(30, Box(root, "b").Size.Height, Delta);
    }

    [TestMethod]
    [Spec("css-sizing-3", "#stretch-fit-sizing")]
    [Spec("css-sizing-4", "#extrinsic")]
    public void AutoWidthBlock_IsStretchFit()
    {
        var root = Layout("<div style='width:300px'><div id='b' style='margin:0 10px 0 20px;padding:0 5px;border:0 solid;border-left-width:3px;height:10px'></div></div>");
        var b = Box(root, "b");

        Assert.AreEqual(20, b.Location.X, Delta);
        Assert.AreEqual(270, b.Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css-sizing-3", "#intrinsic-sizes")]
    [Spec("css-sizing-4", "#intrinsic-sizes")]
    [Spec("css-sizing-3", "#intrinsic-contribution")]
    [Spec("css-sizing-4", "#intrinsic-contribution")]
    public void TableColumns_UseTheMinAndMaxContentOfTheirCells()
    {
        var root = Layout("<table id='t' style='border-spacing:0'><tr><td id='a' style='padding:0'><span style='display:inline-block;width:80px;height:10px'></span></td>"
                          + "<td id='b' style='padding:0'><span style='display:inline-block;width:30px;height:10px'></span></td></tr></table>");

        Assert.AreEqual(80, Box(root, "a").Size.Width, 1);
        Assert.AreEqual(30, Box(root, "b").Size.Width, 1);
    }

    [TestMethod]
    [Spec("css-sizing-3", "#intrinsic-sizes")]
    [Spec("css-sizing-4", "#intrinsic-sizes")]
    public void NarrowTable_ShrinksColumnsToTheirMinContent()
    {
        var root = Layout("<table id='t' style='width:10px;border-spacing:0'><tr><td id='a' style='padding:0'>aaaa bbbb cccc</td></tr></table>");
        var a = Box(root, "a");
        var words = LayoutHarness.Descendants(a).SelectMany(d => d.Words).Where(w => w.Text.Trim().Length > 0).ToList();

        Assert.AreEqual(3, words.Select(w => w.Top).Distinct().Count());
        Assert.AreEqual(words.Max(w => w.Width), a.Size.Width, 1);
    }

    [TestMethod]
    [Spec("css-sizing-3", "#cyclic-percentage-contribution")]
    [Spec("css-sizing-4", "#intrinsic-contribution")]
    public void CyclicPercentageWidth_IsAutoForTheContribution_ThenResolves()
    {
        var root = Layout("<div style='width:400px'><div id='f' style='float:left'><div id='c' style='width:50%'><span style='display:inline-block;width:100px;height:10px'></span></div></div></div>");

        Assert.AreEqual(100, Box(root, "f").Size.Width, Delta);
        Assert.AreEqual(50, Box(root, "c").Size.Width, Delta);
    }

    private static CssBox Layout(string body) => LayoutHarness.Layout("<!DOCTYPE html><html><head></head><body style='margin:0'>" + body + "</body></html>", maxWidth: 400).Root;

    private static CssBox Box(CssBox root, string id) => LayoutHarness.FindById(root, id)!;
}
