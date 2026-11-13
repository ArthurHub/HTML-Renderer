using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssPosition4Tests
{
    private const double Delta = 0.5;

    private const string ScrollContainer = "<div id='c' style='position:relative;overflow:hidden;width:200px;height:100px;padding:10px;border:5px solid'>"
                                           + "<div style='height:300px'></div><div id='a' style='position:absolute;bottom:0;left:0;width:10px;height:10px'></div></div>";

    [TestMethod]
    [Spec("css-position-4", "#scrollable-cb")]
    public void AbsoluteBoxInAScrollContainer_IgnoresTheScrollableOverflow()
    {
        var root = Layout(ScrollContainer);

        Assert.IsTrue(Box(root, "a").Location.Y - Box(root, "c").Location.Y < 130);
    }

    [Ignore("Gap, see Spec/css-position-4.yml")]
    [TestMethod]
    [Spec("css-position-4", "#scrollable-cb")]
    public void AbsoluteBoxInAScrollContainer_UsesThePaddingBox()
    {
        var root = Layout(ScrollContainer);
        var c = Box(root, "c");
        var a = Box(root, "a");

        Assert.AreEqual(5, a.Location.X - c.Location.X, Delta);
        Assert.AreEqual(115, a.Location.Y - c.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css-position-4", "#backdrop")]
    public void BackdropInASelectorList_KeepsTheRuleValid()
    {
        var html = "<!DOCTYPE html><html><head><style>dialog::backdrop, #x { color: #0000ff }</style></head><body><p id='x'>a</p></body></html>";
        var (root, _) = LayoutHarness.Layout(html);

        Assert.AreEqual(RColor.FromArgb(255, 0, 0, 255), Box(root, "x").ActualColor);
    }

    private static CssBox Layout(string body) => LayoutHarness.Layout(LayoutHarness.Wrap(body)).Root;

    private static CssBox Box(CssBox root, string id) => LayoutHarness.FindById(root, id)!;
}
