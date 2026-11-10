using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssMulticolTests
{
    private const double Delta = 0.5;

    [Ignore("Gap, see Spec/css-multicol-1.yml")]
    [TestMethod]
    [Spec("css-multicol-1", "#the-multi-column-model")]
    [Spec("css-multicol-1", "#cc")]
    [Spec("css-multicol-1", "#cg")]
    public void ColumnCount_FlowsTheContentIntoColumns()
    {
        var root = Layout("<div id='m' style='width:400px;column-count:2;column-gap:0;column-fill:auto;height:50px'>"
                          + "<div id='a' style='height:50px'></div><div id='b' style='height:50px'></div></div>");

        Assert.AreEqual(200, Box(root, "a").Size.Width, Delta);
        Assert.AreEqual(Box(root, "a").Location.Y, Box(root, "b").Location.Y, Delta);
        Assert.AreEqual(200, Box(root, "b").Location.X - Box(root, "a").Location.X, Delta);
    }

    [Ignore("Gap, see Spec/css-multicol-1.yml")]
    [TestMethod]
    [Spec("css-multicol-1", "#cw")]
    [Spec("css-multicol-1", "#pseudo-algorithm")]
    public void ColumnWidth_DeterminesTheColumnCount()
    {
        var root = Layout("<div style='width:400px;column-width:150px;column-gap:0'><div id='a' style='height:10px'></div></div>");

        Assert.AreEqual(200, Box(root, "a").Size.Width, Delta);
    }

    private static CssBox Layout(string body) => LayoutHarness.Layout(LayoutHarness.Wrap(body)).Root;

    private static CssBox Box(CssBox root, string id) => LayoutHarness.FindById(root, id)!;
}
