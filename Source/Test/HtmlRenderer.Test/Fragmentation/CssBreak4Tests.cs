using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Fragmentation;

[TestClass]
public sealed class CssBreak4Tests
{
    private const double Delta = 0.5;
    private const double PageHeight = 300;

    [TestMethod]
    [Spec("css-break-4", "#break-between")]
    public void BreakAfterAlways_ForcesAPageBreak()
    {
        var (root, container) = Paginate("<div style='height:50px;break-after:always'></div><div id='b' style='height:10px'></div>");

        Assert.AreEqual(1, container.PageIndexOf(Find(root, "b").Location.Y));
    }

    [Ignore("Gap, see Spec/css-break-4.yml")]
    [TestMethod]
    [Spec("css-break-4", "#break-between")]
    public void BreakBeforeAll_ForcesAPageBreak()
    {
        var (root, container) = Paginate("<div style='height:50px'></div><div id='b' style='break-before:all;height:10px'></div>");

        Assert.AreEqual(1, container.PageIndexOf(Find(root, "b").Location.Y));
    }

    [Ignore("Gap, see Spec/css-break-4.yml")]
    [TestMethod]
    [DataRow("margin-break: keep")]
    [DataRow("margin-break: discard")]
    [Spec("css-break-4", "#break-margins")]
    public void MarginBreak_IsAccepted(string declaration)
    {
        Assert.IsTrue(CssConstructionFunctions.ParseDeclarations(declaration).Length > 0);
    }

    [Ignore("Gap, see Spec/css-break-4.yml")]
    [TestMethod]
    [Spec("css-break-4", "#break-margins")]
    public void MarginBreakKeep_KeepsTheMarginAfterAnUnforcedBreak()
    {
        var (root, _) = Paginate("<div style='height:290px'></div><div id='b' style='margin-break:keep;margin-top:20px;height:50px'></div>");

        Assert.AreEqual(PageHeight + 20, Find(root, "b").Location.Y, Delta);
    }

    private static (CssBox Root, HtmlContainerInt Container) Paginate(string body) =>
        LayoutHarness.Layout($"<html><head></head><body style='margin:0'>{body}</body></html>", 400, pageHeight: PageHeight, margin: 0);

    private static CssBox Find(CssBox root, string id) => LayoutHarness.FindById(root, id)!;
}
